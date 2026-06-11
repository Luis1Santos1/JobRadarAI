namespace JobRadar.Infrastructure.AI;

public sealed class LocalAiSettings
{
    public bool Enabled { get; init; } = true;

    public string BaseUrl { get; init; } = "http://localhost:11434";

    public string Model { get; init; } = "llama3.1:8b";

    public int TimeoutSeconds { get; init; } = 180;
}