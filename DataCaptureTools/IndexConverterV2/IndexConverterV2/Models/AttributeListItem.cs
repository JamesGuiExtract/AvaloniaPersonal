using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IndexConverterV2.Models
{
    public record class AttributeListItem(
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
