namespace AlertManager.Models.AllEnums
{
    [System.Serializable]
    public enum AlertActionType
    {
        //These can be renumbered and reorganized without needing elastic reindexing
        Unresolve = 0,
        Resolve, Resolution = 1, //Different wordings for same action type. To change this, we would have to reindex some alerts
        Snooze = 2,
        Mute = 3,
        AutoResolve = 4,
        Comment = 5, //Not a status changing action
    }


    [System.Serializable]
    //Used to map AlertActionTypes to a better wording for status messages.
    //int values should match for action-status pairings.
    //If an int value is skipped in AlertStatus but not AlertActionType, that action does not affect status
    public enum AlertStatus
    {
        Unresolved = 0,
        Resolved = 1,
        Snoozed = 2,
        Muted = 3,
        AutoResolving = 4,
        //5 is skipped
    }
}