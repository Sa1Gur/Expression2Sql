using System.Runtime.Serialization;

namespace RestApi.Data.Models;

public enum ReportTemplateColumnTypes
{
    [EnumMember(Value = "attribute")]
    Attribute,
    [EnumMember(Value = "indicator")]
    Indicator,
    [EnumMember(Value = "userfield")]
    UserField,
    [EnumMember(Value = "aggregation")]
    Aggregation
}
