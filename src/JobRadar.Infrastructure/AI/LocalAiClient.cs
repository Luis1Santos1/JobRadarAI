using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace JobRadar.Infrastructure.AI;

public sealed class LocalAiClient
{
    private readonly HttpClient _httpClient;
    private readonly LocalAiSettings _settings;

    public LocalAiClient(HttpClient httpClient, IOptions<LocalAiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            throw new InvalidOperationException("Local AI is disabled.");
        }

        var request = new OllamaChatRequest(
            _settings.Model,
            [
                new OllamaMessage("user", prompt)
            ],
            false,
            new OllamaOptions(0.1m));

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/chat",
            request,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Local AI request failed. Status: {(int)response.StatusCode}. Body: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
            cancellationToken: cancellationToken);

        if (string.IsNullOrWhiteSpace(result?.Message?.Content))
        {
            throw new InvalidOperationException("Local AI returned an empty message.");
        }

        return result.Message.Content;
    }

    private sealed record OllamaChatRequest(
        string Model,
        IReadOnlyCollection<OllamaMessage> Messages,
        bool Stream,
        OllamaOptions Options);

    private sealed record OllamaMessage(
        string Role,
        string Content);

    private sealed record OllamaOptions(
        decimal Temperature);

    private sealed record OllamaChatResponse(
        OllamaMessageResponse? Message);

    private sealed record OllamaMessageResponse(
        string Role,
        string Content);
}