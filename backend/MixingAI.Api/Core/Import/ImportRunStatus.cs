namespace MixingAI.Api.Core.Import;

public enum ImportRunStatus
{
    Queued = 0,
    Extracting = 1,
    NeedsReview = 2,
    Approved = 3,
    Published = 4,
    Archived = 5,
    Failed = 10,
    Rejected = 11,
    NeedsRework = 12,
}
