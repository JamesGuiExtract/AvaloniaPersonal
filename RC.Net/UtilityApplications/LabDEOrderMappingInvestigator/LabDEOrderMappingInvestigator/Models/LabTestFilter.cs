using System;

namespace LabDEOrderMappingInvestigator.Models
{
    /// <summary>
    /// Lab test match filter (which tests to show in the main list)
    /// </summary>
    [Flags]
    public enum LabTestFilter
    {
        None = 0,
        All = 1 << 0,
        UnMapped = 1 << 1,
        RecentlyChanged = 1 << 2,
        MissedTests = 1 << 3,
        IncorrectTests = 1 << 4,
    }
}
