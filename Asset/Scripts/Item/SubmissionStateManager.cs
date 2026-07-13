using System.Collections.Generic;

/// <summary>
/// Runtime-only in-memory submission flags.
/// Key建议用 submissionId（每个提交点唯一）
/// </summary>
public static class SubmissionStateManager
{
    private static readonly HashSet<string> _submitted = new HashSet<string>();

    public static bool IsSubmitted(string submissionId)
    {
        return !string.IsNullOrEmpty(submissionId) && _submitted.Contains(submissionId);
    }

    public static void MarkSubmitted(string submissionId)
    {
        if (string.IsNullOrEmpty(submissionId)) return;
        _submitted.Add(submissionId);
    }

    public static void ClearAll()
    {
        _submitted.Clear();
    }
}