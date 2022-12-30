namespace IndexConverterV2.Models
{
    public record AttributeListItem(
        string Name,
        string Value,
        string Type,
        FileListItem File,
        bool IsConditional,
        bool? ConditionType = null,
        string? LeftCondition = null,
        string? RightCondition = null);
}
