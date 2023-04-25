namespace AlertManager.Models.AllEnums
{
    [System.Serializable]
    public enum AlertActionType
    {
        Unresolve = 0,
        Resolve = 1,
        Resolution = 1,
        Snooze = 2,
        Mute = 3,
        AutoResolve = 4
    }

    [System.Serializable]
    public enum AlertStatus
    {
        Unresolved = 0,
        Resolved = 1,
        Snoozed = 2,
        Muted = 3,
        AutoResolving = 4
    }
}