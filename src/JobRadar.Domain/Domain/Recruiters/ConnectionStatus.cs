namespace JobRadar.Domain.Recruiters;

public enum ConnectionStatus
{
    Unknown = 0,
    Discovered = 1,
    NotConnected = 2,
    InviteSentManually = 3,
    Connected = 4,
    Discarded = 5
}