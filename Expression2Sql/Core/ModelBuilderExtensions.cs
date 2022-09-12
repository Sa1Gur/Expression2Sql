using System;
using System.Reflection;

namespace QueryingCore.Core;

public static class ModelBuilderExtensions
{
    public static readonly MethodInfo? DateDiffDaysMethodInfo 
        = typeof(ModelBuilderExtensions)
            .GetMethod(nameof(DateDiffDays), 
                new[] { typeof(DateTime?), typeof(DateTime?) });


    public static int? DateDiffDays(DateTime? firstDay, DateTime? secondDate)
        => throw new NotSupportedException();
}