using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RestApi.Data.Models;
using Microsoft.CSharp;

namespace QueryingCore;

public class Types
{
    public enum Undefined
    {
    }
    
    static readonly BiDictionary<DataTypes, Type> _typeMap = new ()
    {
        {DataTypes.String, typeof(string)},
        {DataTypes.Integer, typeof(int?)},
        {DataTypes.Integer, typeof(int)},
        {DataTypes.Date, typeof(DateTime?)},
        {DataTypes.Date, typeof(DateTime)},
        {DataTypes.Real, typeof(float?)},
        {DataTypes.Real, typeof(float)},
        {DataTypes.Number, typeof(float?)},
        {DataTypes.Number, typeof(decimal)},
        {DataTypes.Number, typeof(decimal?)},
        {DataTypes.Number, typeof(long)},
        {DataTypes.Number, typeof(long?)},
        {DataTypes.Bool, typeof(bool?)},
        {DataTypes.Bool, typeof(bool)},
        {DataTypes.Money, typeof(decimal?)},
        {DataTypes.Bigint, typeof(long?)},
        {DataTypes.Double, typeof(double?)},
        {DataTypes.Double, typeof(double)},
        {DataTypes.Duration, typeof(string)},
        {DataTypes.OutlineCode, typeof(string)},
        {DataTypes.IntegerList, typeof(string)},
        {DataTypes.TextList, typeof(string)},
        {DataTypes.Indicator, typeof(string)},
        {DataTypes.CurrencyRate, typeof(string)},
        {DataTypes.Rtf, typeof(string)},
        {DataTypes.Percentage, typeof(double?)},
        {DataTypes.Predecessors, typeof(string)},
        {DataTypes.Unknown, typeof(Undefined)},
        {DataTypes.Unknown, typeof(Guid)},
        {DataTypes.Unknown, typeof(Guid?)}
    };

    public static string GetDbType(DataTypes type)
    {
        return type switch
        {
            DataTypes.String => "varchar",
            DataTypes.Integer => "integer",
            DataTypes.Date => "timestamp without time zone",
            DataTypes.Number => "integer",
            DataTypes.Bool => "boolean",
            DataTypes.Money => "money",
            DataTypes.Bigint => "bigint",
            DataTypes.Double => "double precision",
            DataTypes.Real => "float4",

            DataTypes.Duration => "json",
            DataTypes.OutlineCode => "json",
            DataTypes.IntegerList => "json",
            DataTypes.TextList => "json",
            DataTypes.Indicator => "json",
            DataTypes.CurrencyRate => "json",
            DataTypes.Rtf => "json",

            DataTypes.Unknown => throw new NotImplementedException(),
            DataTypes.Percentage => "double precision",
            DataTypes.Predecessors => "varchar",
            _ => throw new NotImplementedException()
        };
    }

    public static DataTypes GetDataType(Type type) => _typeMap[type].Any() ? _typeMap[type].First() : DataTypes.Unknown;

    public static Type GetNetType(DataTypes dataType) =>
        _typeMap[dataType].Any() ? _typeMap[dataType].First() : typeof(Undefined);

    public static string GetNetTypeString(Type type)
    {
        var nullableType = Nullable.GetUnderlyingType(type);
        var isNullableType = nullableType != null;

        return isNullableType ? $"{GetFriendlyTypeString(nullableType)}?" : GetFriendlyTypeString(type);
    }

    static string GetFriendlyTypeString(Type? type)
    {
        using var provider = new CSharpCodeProvider();
        var typeRef = new CodeTypeReference(type);
        return provider.GetTypeOutput(typeRef);
    }
}

class BiDictionary<TFirst, TSecond> : IEnumerable
{
    readonly IDictionary<TFirst, IList<TSecond>> _firstToSecond = new Dictionary<TFirst, IList<TSecond>>();
    readonly IDictionary<TSecond, IList<TFirst>> _secondToFirst = new Dictionary<TSecond, IList<TFirst>>();

    static readonly IList<TFirst> _emptyFirstList = Array.Empty<TFirst>();
    static readonly IList<TSecond> _emptySecondList = Array.Empty<TSecond>();

    public void Add(TFirst first, TSecond second)
    {
        if (!_firstToSecond.TryGetValue(first, out var seconds))
        {
            seconds = new List<TSecond>();
            _firstToSecond[first] = seconds;
        }

        if (!_secondToFirst.TryGetValue(second, out var firsts))
        {
            firsts = new List<TFirst>();
            _secondToFirst[second] = firsts;
        }

        seconds.Add(second);
        firsts.Add(first);
    }

    public IList<TSecond> this[TFirst first] => GetByFirst(first);

    public IList<TFirst> this[TSecond second] => GetBySecond(second);

    public IList<TSecond> GetByFirst(TFirst first)
    {
        if (!_firstToSecond.TryGetValue(first, out var list))
            return _emptySecondList;

        return new List<TSecond>(list);
    }

    public IList<TFirst> GetBySecond(TSecond second)
    {
        if (!_secondToFirst.TryGetValue(second, out var list))
            return _emptyFirstList;

        return new List<TFirst>(list);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator GetEnumerator() => _firstToSecond.GetEnumerator();
}
