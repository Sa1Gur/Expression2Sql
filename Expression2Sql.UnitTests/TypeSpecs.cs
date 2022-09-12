using System;
using QueryingCore;
using Npgsql.PostgresTypes;
using NUnit.Framework;
using RestApi.Data.Models;

namespace QueryingCoreTests;

public class TypeSpecs
{
    [TestCase]
    public void ShouldCreateTypesMap()
    {
        var typeTst = new Types();
        typeTst.ToString();
    }

    [TestCase(typeof(string), "string")]
    [TestCase(typeof(int), "int")]
    [TestCase(typeof(int?), "int?")]
    [TestCase(typeof(DateTime), "System.DateTime")]
    [TestCase(typeof(DateTime?), "System.DateTime?")]
    [TestCase(typeof(float), "float")]
    [TestCase(typeof(float?), "float?")]
    [TestCase(typeof(bool), "bool")]
    [TestCase(typeof(bool?), "bool?")]
    [TestCase(typeof(decimal), "decimal")]
    [TestCase(typeof(decimal?), "decimal?")]
    [TestCase(typeof(long), "long")]
    [TestCase(typeof(long?), "long?")]
    [TestCase(typeof(double), "double")]
    [TestCase(typeof(double?), "double?")]
    [TestCase(typeof(Guid), "System.Guid")]
    [TestCase(typeof(Guid?), "System.Guid?")]
    public void ShouldConverTypeToString(Type netType, string netTypeString) =>
        Assert.AreEqual(netTypeString, Types.GetNetTypeString(netType));

    [TestCase(DataTypes.String, typeof(string))]
    [TestCase(DataTypes.Integer, typeof(int?))]
    [TestCase(DataTypes.Date, typeof(DateTime?))]
    [TestCase(DataTypes.Number, typeof(float?))]
    [TestCase(DataTypes.Bool, typeof(bool?))]
    [TestCase(DataTypes.Money, typeof(decimal?))]
    [TestCase(DataTypes.Bigint, typeof(long?))]
    [TestCase(DataTypes.Double, typeof(double?))]
    [TestCase(DataTypes.Real, typeof(float?))]
    [TestCase(DataTypes.Duration, typeof(string))]
    [TestCase(DataTypes.OutlineCode, typeof(string))]
    [TestCase(DataTypes.IntegerList, typeof(string))]
    [TestCase(DataTypes.TextList, typeof(string))]
    [TestCase(DataTypes.Indicator, typeof(string))]
    [TestCase(DataTypes.CurrencyRate, typeof(string))]
    [TestCase(DataTypes.Rtf, typeof(string))]
    [TestCase(DataTypes.Percentage, typeof(double?))]
    [TestCase(DataTypes.Predecessors, typeof(string))]
    [TestCase(DataTypes.Unknown, typeof(Types.Undefined))]
    [TestCase((DataTypes) 1984, typeof(Types.Undefined))]
    public void ShouldConvertDataTypeToType(DataTypes dataType, Type type) =>
        Assert.AreEqual(type, Types.GetNetType(dataType));

    [TestCase(typeof(string), DataTypes.String)]
    [TestCase(typeof(int), DataTypes.Integer)]
    [TestCase(typeof(int?), DataTypes.Integer)]
    [TestCase(typeof(DateTime), DataTypes.Date)]
    [TestCase(typeof(DateTime?), DataTypes.Date)]
    [TestCase(typeof(float), DataTypes.Real)]
    [TestCase(typeof(float?), DataTypes.Real)]
    [TestCase(typeof(bool), DataTypes.Bool)]
    [TestCase(typeof(bool?), DataTypes.Bool)]
    [TestCase(typeof(decimal), DataTypes.Number)]
    [TestCase(typeof(decimal?), DataTypes.Number)]
    [TestCase(typeof(long), DataTypes.Number)]
    [TestCase(typeof(long?), DataTypes.Number)]
    [TestCase(typeof(double), DataTypes.Double)]
    [TestCase(typeof(double?), DataTypes.Double)]
    [TestCase(typeof(Guid), DataTypes.Unknown)]
    [TestCase(typeof(Guid?), DataTypes.Unknown)]
    [TestCase(typeof(Types.Undefined), DataTypes.Unknown)]
    [TestCase(typeof(UnknownBackendType), DataTypes.Unknown)]
    public void ShouldConvertTypeToDataType(Type type, DataTypes dataType) =>
        Assert.AreEqual(dataType, Types.GetDataType(type));
}