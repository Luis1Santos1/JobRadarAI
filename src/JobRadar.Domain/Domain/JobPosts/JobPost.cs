using JobRadar.Domain.Recruiters;
using JobRadar.SharedKernel;

namespace JobRadar.Domain.JobPosts;

public sealed class JobPost : BaseEntity
{
    private JobPost()
    {
    }

    public JobPost(
        Guid userId,
        string title,
        string company,
        string? location,
        string? sourceUrl,
        string? normalizedSourceUrl,
        Guid? recruiterId,
        string originalText,
        string? notes)
    {
        UserId = userId;
        Title = title;
        Company = company;
        Location = location;
        SourceUrl = sourceUrl;
        NormalizedSourceUrl = normalizedSourceUrl;
        RecruiterId = recruiterId;
        OriginalText = originalText;
        Notes = notes;
        AnalysisStatus = JobPostAnalysisStatus.NotRequested;
    }

    public Guid UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Company { get; private set; } = string.Empty;

    public string? Location { get; private set; }

    public string? SourceUrl { get; private set; }

    public string? NormalizedSourceUrl { get; private set; }

    public Guid? RecruiterId { get; private set; }

    public Recruiter? Recruiter { get; private set; }

    public string OriginalText { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public JobPostAnalysisStatus AnalysisStatus { get; private set; }

    public DateTimeOffset? AnalysisRequestedAt { get; private set; }

    public DateTimeOffset? AnalysisStartedAt { get; private set; }

    public DateTimeOffset? AnalysisCompletedAt { get; private set; }

    public string? AnalysisError { get; private set; }

    public void Update(
        string title,
        string company,
        string? location,
        string? sourceUrl,
        string? normalizedSourceUrl,
        Guid? recruiterId,
        string originalText,
        string? notes)
    {
        var originalTextChanged = !string.Equals(
            OriginalText,
            originalText,
            StringComparison.Ordinal);

        Title = title;
        Company = company;
        Location = location;
        SourceUrl = sourceUrl;
        NormalizedSourceUrl = normalizedSourceUrl;
        RecruiterId = recruiterId;
        OriginalText = originalText;
        Notes = notes;

        if (originalTextChanged)
        {
            AnalysisStatus = JobPostAnalysisStatus.NotRequested;
            AnalysisRequestedAt = null;
            AnalysisStartedAt = null;
            AnalysisCompletedAt = null;
            AnalysisError = null;
        }

        MarkAsUpdated();
    }

    public void LinkRecruiter(Guid recruiterId)
    {
        RecruiterId = recruiterId;
        MarkAsUpdated();
    }

    public void ClearRecruiter()
    {
        RecruiterId = null;
        MarkAsUpdated();
    }

    public void RequestAnalysis()
    {
        AnalysisStatus = JobPostAnalysisStatus.Pending;
        AnalysisRequestedAt = DateTimeOffset.UtcNow;
        AnalysisStartedAt = null;
        AnalysisCompletedAt = null;
        AnalysisError = null;

        MarkAsUpdated();
    }

    public void MarkAnalysisInProgress()
    {
        AnalysisStatus = JobPostAnalysisStatus.InProgress;
        AnalysisStartedAt = DateTimeOffset.UtcNow;

        MarkAsUpdated();
    }

    public void MarkAnalysisCompleted()
    {
        AnalysisStatus = JobPostAnalysisStatus.Completed;
        AnalysisCompletedAt = DateTimeOffset.UtcNow;
        AnalysisError = null;

        MarkAsUpdated();
    }

    public void MarkAnalysisFailed(string error)
    {
        AnalysisStatus = JobPostAnalysisStatus.Failed;
        AnalysisCompletedAt = DateTimeOffset.UtcNow;
        AnalysisError = error;

        MarkAsUpdated();
    }
}