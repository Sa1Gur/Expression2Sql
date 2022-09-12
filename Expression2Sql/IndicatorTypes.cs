using System.Runtime.Serialization;

namespace RestApi.Data.Models;

public enum IndicatorTypes
{
    /// <summary>
    /// Индикатор на один источник.
    /// </summary>
    [EnumMember(Value = "source")]
    Source = 1,

    /// <summary>
    /// Индикатор на пару истчоников.
    /// </summary>
    [EnumMember(Value = "sourcePair")]
    SourcePair = 2,

    /// <summary>
    /// Индикатор на индикатор.
    /// </summary>
    [EnumMember(Value = "aggregative")]
    Indicator = 3,

    /// <summary>
    /// Группа индикаторов.
    /// </summary>
    [EnumMember(Value = "group")]
    GroupIndicator = 4,

    /// <summary>
    /// Скрытый, порождаемый группой.
    /// </summary>
    [EnumMember(Value = "groupItem")]
    GroupItemIndicator = 5
}
