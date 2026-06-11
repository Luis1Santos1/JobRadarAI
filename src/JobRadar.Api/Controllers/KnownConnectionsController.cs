using JobRadar.Api.Extensions;
using JobRadar.Domain.Recruiters;
using JobRadar.Infrastructure.Normalization;
using JobRadar.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobRadar.Api.Controllers;

[ApiController]
[Route("api/v1/known-connections")]
[Authorize]
public sealed class KnownConnectionsController : ControllerBase
{
    private readonly JobRadarDbContext _dbContext;
    private readonly ContactNormalizationService _normalizer;

    public KnownConnectionsController(
        JobRadarDbContext dbContext,
        ContactNormalizationService normalizer)
    {
        _dbContext = dbContext;
        _normalizer = normalizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] ConnectionStatus? status,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var query = _dbContext.KnownConnections
            .Where(item => item.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = _normalizer.NormalizeText(search);

            query = query.Where(item =>
                item.NormalizedName.Contains(normalizedSearch) ||
                item.NormalizedCompany!.Contains(normalizedSearch));
        }

        if (status.HasValue)
        {
            query = query.Where(item => item.ConnectionStatus == status.Value);
        }

        var connections = await query
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => ToResponse(item, false))
            .ToListAsync(cancellationToken);

        return Ok(connections);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var connection = await _dbContext.KnownConnections
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (connection is null)
        {
            return NotFound();
        }

        var duplicates = await FindRecruiterDuplicatesAsync(
            userId,
            connection.NormalizedName,
            connection.NormalizedCompany,
            connection.NormalizedLinkedInUrl,
            _normalizer.NormalizeEmail(connection.Email),
            cancellationToken);

        return Ok(ToResponse(connection, duplicates.Any(), duplicates));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateKnownConnectionRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var normalizedName = _normalizer.NormalizeText(request.Name);
        var normalizedCompany = _normalizer.NormalizeText(request.Company);
        var normalizedLinkedInUrl = _normalizer.NormalizeLinkedInUrl(request.LinkedInUrl);

        var alreadyExists = await AlreadyExistsAsync(
            userId,
            normalizedName,
            normalizedCompany,
            normalizedLinkedInUrl,
            cancellationToken);

        if (alreadyExists)
        {
            return Conflict(new
            {
                message = "Known connection already exists.",
                matchReason = !string.IsNullOrWhiteSpace(normalizedLinkedInUrl)
                    ? "LinkedInUrl"
                    : "NameCompany"
            });
        }

        var normalizedEmail = _normalizer.NormalizeEmail(request.Email);

        var duplicates = await FindRecruiterDuplicatesAsync(
            userId,
            normalizedName,
            normalizedCompany,
            normalizedLinkedInUrl,
            normalizedEmail,
            cancellationToken);

        var connection = new KnownConnection(
            userId,
            request.Name.Trim(),
            normalizedName,
            request.LinkedInUrl?.Trim(),
            normalizedLinkedInUrl,
            request.Company?.Trim(),
            normalizedCompany,
            request.Title?.Trim(),
            request.Email?.Trim(),
            request.Location?.Trim(),
            request.ConnectionStatus,
            request.ImportedFrom.Trim());

        _dbContext.KnownConnections.Add(connection);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = connection.Id },
            ToResponse(connection, duplicates.Any(), duplicates));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateKnownConnectionRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var connection = await _dbContext.KnownConnections
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (connection is null)
        {
            return NotFound();
        }

        connection.Update(
            request.Name.Trim(),
            _normalizer.NormalizeText(request.Name),
            request.LinkedInUrl?.Trim(),
            _normalizer.NormalizeLinkedInUrl(request.LinkedInUrl),
            request.Company?.Trim(),
            _normalizer.NormalizeText(request.Company),
            request.Title?.Trim(),
            request.Email?.Trim(),
            request.Location?.Trim());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Known connection updated successfully."
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var connection = await _dbContext.KnownConnections
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (connection is null)
        {
            return NotFound();
        }

        _dbContext.KnownConnections.Remove(connection);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Known connection deleted successfully."
        });
    }

    [HttpPost("{id:guid}/change-connection-status")]
    public async Task<IActionResult> ChangeConnectionStatus(Guid id, ChangeConnectionStatusRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var connection = await _dbContext.KnownConnections
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (connection is null)
        {
            return NotFound();
        }

        connection.ChangeConnectionStatus(request.Status);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Known connection status changed successfully."
        });
    }

    [HttpPost("import-csv")]
    [RequestSizeLimit(2_000_000)]
    public async Task<IActionResult> ImportCsv(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (file.Length == 0)
        {
            return BadRequest(new
            {
                message = "CSV file is empty."
            });
        }

        var imported = 0;
        var skippedDuplicates = 0;
        var errors = new List<string>();
        var skippedItems = new List<ImportSkippedDuplicateResponse>();
        var importedItems = new List<KnownConnectionResponse>();

        var existingKeys = await LoadKnownConnectionDuplicateKeysAsync(userId, cancellationToken);
        var fileKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        var headerLine = await reader.ReadLineAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return BadRequest(new
            {
                message = "CSV header is required."
            });
        }

        var headers = SplitCsvLine(headerLine)
            .Select((name, index) => new
            {
                Name = _normalizer.NormalizeText(name),
                Index = index
            })
            .ToDictionary(item => item.Name, item => item.Index);

        var lineNumber = 1;

        while (!reader.EndOfStream)
        {
            lineNumber++;

            var line = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = SplitCsvLine(line);

            try
            {
                var name = GetColumn(headers, columns, "NAME", "NOME", "FULL NAME");
                var linkedInUrl = GetColumn(headers, columns, "LINKEDINURL", "LINKEDIN URL", "PROFILE URL", "URL");
                var company = GetColumn(headers, columns, "COMPANY", "EMPRESA");
                var title = GetColumn(headers, columns, "TITLE", "CARGO", "POSITION");
                var email = GetColumn(headers, columns, "EMAIL", "E-MAIL");
                var location = GetColumn(headers, columns, "LOCATION", "LOCALIZACAO", "LOCALIZAÇÃO");

                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add($"Line {lineNumber}: name is required.");
                    continue;
                }

                var normalizedName = _normalizer.NormalizeText(name);
                var normalizedCompany = _normalizer.NormalizeText(company);
                var normalizedLinkedInUrl = _normalizer.NormalizeLinkedInUrl(linkedInUrl);
                var normalizedEmail = _normalizer.NormalizeEmail(email);

                var duplicateKeys = BuildDuplicateKeys(
                    normalizedLinkedInUrl,
                    normalizedEmail,
                    normalizedName,
                    normalizedCompany);

                if (duplicateKeys.Count == 0)
                {
                    errors.Add($"Line {lineNumber}: at least LinkedInUrl, Email or Company is required to deduplicate.");
                    continue;
                }

                var duplicatedKey = duplicateKeys.FirstOrDefault(key =>
                    existingKeys.Contains(key) ||
                    fileKeys.Contains(key));

                if (!string.IsNullOrWhiteSpace(duplicatedKey))
                {
                    skippedDuplicates++;

                    skippedItems.Add(new ImportSkippedDuplicateResponse(
                        lineNumber,
                        name,
                        linkedInUrl,
                        email,
                        duplicatedKey));

                    continue;
                }

                var recruiterDuplicates = await FindRecruiterDuplicatesAsync(
                    userId,
                    normalizedName,
                    normalizedCompany,
                    normalizedLinkedInUrl,
                    normalizedEmail,
                    cancellationToken);

                var connection = new KnownConnection(
                    userId,
                    name.Trim(),
                    normalizedName,
                    linkedInUrl?.Trim(),
                    normalizedLinkedInUrl,
                    company?.Trim(),
                    normalizedCompany,
                    title?.Trim(),
                    email?.Trim(),
                    location?.Trim(),
                    ConnectionStatus.Connected,
                    "CsvImport");

                _dbContext.KnownConnections.Add(connection);

                foreach (var key in duplicateKeys)
                {
                    fileKeys.Add(key);
                }

                importedItems.Add(ToResponse(
                    connection,
                    recruiterDuplicates.Any(),
                    recruiterDuplicates));

                imported++;
            }
            catch (Exception exception)
            {
                errors.Add($"Line {lineNumber}: {exception.Message}");
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            imported,
            skippedDuplicates,
            errors,
            skippedItems,
            items = importedItems
        });
    }

    private async Task<HashSet<string>> LoadKnownConnectionDuplicateKeysAsync(
    Guid userId,
    CancellationToken cancellationToken)
    {
        var connections = await _dbContext.KnownConnections
            .Where(item => item.UserId == userId)
            .Select(item => new
            {
                item.NormalizedLinkedInUrl,
                item.Email,
                item.NormalizedName,
                item.NormalizedCompany
            })
            .ToListAsync(cancellationToken);

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var connection in connections)
        {
            var connectionKeys = BuildDuplicateKeys(
                connection.NormalizedLinkedInUrl,
                _normalizer.NormalizeEmail(connection.Email),
                connection.NormalizedName,
                connection.NormalizedCompany);

            foreach (var key in connectionKeys)
            {
                keys.Add(key);
            }
        }

        return keys;
    }

    private static List<string> BuildDuplicateKeys(
        string? normalizedLinkedInUrl,
        string? normalizedEmail,
        string normalizedName,
        string? normalizedCompany)
    {
        var keys = new List<string>();

        if (!string.IsNullOrWhiteSpace(normalizedLinkedInUrl))
        {
            keys.Add($"linkedin:{normalizedLinkedInUrl}");
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            keys.Add($"email:{normalizedEmail}");
        }

        if (!string.IsNullOrWhiteSpace(normalizedName) &&
            !string.IsNullOrWhiteSpace(normalizedCompany))
        {
            keys.Add($"name-company:{normalizedName}|{normalizedCompany}");
        }

        return keys;
    }

    private async Task<bool> AlreadyExistsAsync(
        Guid userId,
        string normalizedName,
        string? normalizedCompany,
        string? normalizedLinkedInUrl,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(normalizedLinkedInUrl))
        {
            var existsByUrl = await _dbContext.KnownConnections
                .AnyAsync(item =>
                    item.UserId == userId &&
                    item.NormalizedLinkedInUrl == normalizedLinkedInUrl,
                    cancellationToken);

            if (existsByUrl)
            {
                return true;
            }
        }

        return await _dbContext.KnownConnections
            .AnyAsync(item =>
                item.UserId == userId &&
                item.NormalizedName == normalizedName &&
                item.NormalizedCompany == normalizedCompany,
                cancellationToken);
    }

    private async Task<List<DuplicateCandidateResponse>> FindRecruiterDuplicatesAsync(
    Guid userId,
    string normalizedName,
    string? normalizedCompany,
    string? normalizedLinkedInUrl,
    string? normalizedEmail,
    CancellationToken cancellationToken)
    {
        var duplicates = new List<DuplicateCandidateResponse>();

        if (!string.IsNullOrWhiteSpace(normalizedLinkedInUrl))
        {
            var recruiterByUrl = await _dbContext.Recruiters
                .Where(item =>
                    item.UserId == userId &&
                    item.NormalizedLinkedInUrl == normalizedLinkedInUrl)
                .Select(item => new DuplicateCandidateResponse(
                    "Recruiter",
                    item.Id,
                    item.Name,
                    item.Company,
                    item.LinkedInUrl,
                    "LinkedInUrl"))
                .ToListAsync(cancellationToken);

            duplicates.AddRange(recruiterByUrl);
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            var recruiterByEmail = await _dbContext.Recruiters
                .Where(item =>
                    item.UserId == userId &&
                    item.Email != null &&
                    item.Email.ToLower() == normalizedEmail)
                .Select(item => new DuplicateCandidateResponse(
                    "Recruiter",
                    item.Id,
                    item.Name,
                    item.Company,
                    item.LinkedInUrl,
                    "Email"))
                .ToListAsync(cancellationToken);

            duplicates.AddRange(recruiterByEmail);
        }

        if (!string.IsNullOrWhiteSpace(normalizedName) &&
            !string.IsNullOrWhiteSpace(normalizedCompany))
        {
            var recruiterByName = await _dbContext.Recruiters
                .Where(item =>
                    item.UserId == userId &&
                    item.NormalizedName == normalizedName &&
                    item.NormalizedCompany == normalizedCompany)
                .Select(item => new DuplicateCandidateResponse(
                    "Recruiter",
                    item.Id,
                    item.Name,
                    item.Company,
                    item.LinkedInUrl,
                    "NameCompany"))
                .ToListAsync(cancellationToken);

            duplicates.AddRange(recruiterByName);
        }

        return duplicates
            .GroupBy(item => new { item.Type, item.Id })
            .Select(group => group.First())
            .ToList();
    }

    private static string? GetColumn(Dictionary<string, int> headers, IReadOnlyList<string> columns, params string[] possibleNames)
    {
        foreach (var possibleName in possibleNames)
        {
            if (!headers.TryGetValue(possibleName, out var index))
            {
                continue;
            }

            if (index >= columns.Count)
            {
                return null;
            }

            return columns[index];
        }

        return null;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var values = new List<string>();
        var current = new List<char>();
        var insideQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];

            if (character == '"')
            {
                insideQuotes = !insideQuotes;
                continue;
            }

            if ((character == ',' || character == ';') && !insideQuotes)
            {
                values.Add(new string(current.ToArray()).Trim());
                current.Clear();
                continue;
            }

            current.Add(character);
        }

        values.Add(new string(current.ToArray()).Trim());

        return values;
    }

    private static KnownConnectionResponse ToResponse(
        KnownConnection connection,
        bool hasPossibleDuplicate,
        IReadOnlyCollection<DuplicateCandidateResponse>? duplicates = null)
    {
        return new KnownConnectionResponse(
            connection.Id,
            connection.Name,
            connection.LinkedInUrl,
            connection.Company,
            connection.Title,
            connection.Email,
            connection.Location,
            connection.ConnectionStatus,
            connection.ImportedFrom,
            connection.ImportedAt,
            connection.LastConfirmedAt,
            hasPossibleDuplicate,
            duplicates ?? []);
    }
}

public sealed record CreateKnownConnectionRequest(
    string Name,
    string? LinkedInUrl,
    string? Company,
    string? Title,
    string? Email,
    string? Location,
    ConnectionStatus ConnectionStatus,
    string ImportedFrom);

public sealed record UpdateKnownConnectionRequest(
    string Name,
    string? LinkedInUrl,
    string? Company,
    string? Title,
    string? Email,
    string? Location);

public sealed record KnownConnectionResponse(
    Guid Id,
    string Name,
    string? LinkedInUrl,
    string? Company,
    string? Title,
    string? Email,
    string? Location,
    ConnectionStatus ConnectionStatus,
    string ImportedFrom,
    DateTimeOffset ImportedAt,
    DateTimeOffset? LastConfirmedAt,
    bool HasPossibleDuplicate,
    IReadOnlyCollection<DuplicateCandidateResponse> PossibleDuplicates);

public sealed record ImportSkippedDuplicateResponse(
    int LineNumber,
    string Name,
    string? LinkedInUrl,
    string? Email,
    string MatchKey);