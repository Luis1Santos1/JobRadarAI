namespace JobRadar.Domain.JobPosts;

public enum JobPostAnalysisStatus
{
    NotRequested = 0,
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4
}