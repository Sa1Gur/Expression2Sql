using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QueryingCore;

public static class BuildingExtensions
{
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
        return items;
    }

    public static void Append(this StringBuilder stringBuilder, Action<IndentedTextWriter> action, int indent = 0)
    {
        using var writer = new StringWriter(stringBuilder);
        using var indentedWriter = new IndentedTextWriter(writer);
        indentedWriter.Indent += indent;
        action(indentedWriter);
        indentedWriter.Indent -= indent;
    }
}
