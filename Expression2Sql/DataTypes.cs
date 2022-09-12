using System;
using System.Runtime.Serialization;

namespace RestApi.Data.Models;

public enum DataTypes
{
    [EnumMember(Value = "string")]
    String,

    [EnumMember(Value = "integer")]
    Integer,

    [EnumMember(Value = "date")]
    Date,

    [EnumMember(Value = "number")]
    Number,

    [EnumMember(Value = "bool")]
    Bool,

    [EnumMember(Value = "double")]
    Double,

    [Obsolete("Not used any more", false)]
    [EnumMember(Value = "money")]
    Money,

    [Obsolete("Not used any more", false)]
    [EnumMember(Value = "bigint")]
    Bigint,

    [Obsolete("Not used any more", false)]
    [EnumMember(Value = "real")]
    Real,

    [EnumMember(Value = "textList")]
    TextList,

    [EnumMember(Value = "rtf")]
    Rtf,

    [EnumMember(Value = "outlineCode")]
    OutlineCode,

    [EnumMember(Value = "integerList")]
    IntegerList,

    [EnumMember(Value = "indicator")]
    Indicator,

    [EnumMember(Value = "duration")]
    Duration,

    [EnumMember(Value = "currencyRate")]
    CurrencyRate,
    
    [EnumMember(Value = "Percentage")]
    Percentage,

    [EnumMember(Value = "Predecessors")]
    Predecessors,

    [EnumMember(Value = "unknown")]
    Unknown
}
