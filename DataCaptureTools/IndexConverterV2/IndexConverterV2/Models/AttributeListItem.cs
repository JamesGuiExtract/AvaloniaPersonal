namespace IndexConverterV2.Models
{
    public record AttributeListItem(
        string Name,
        string Value,
        string Type,
        FileListItem File,
        string OutputFileName,
        bool IsConditional,
        bool? ConditionType = null,
        string? LeftCondition = null,
        string? RightCondition = null);
}
