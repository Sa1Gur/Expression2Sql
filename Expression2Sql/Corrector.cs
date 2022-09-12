using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CSharp;

namespace QueryingCore.CodeGeneration;

public sealed class Corrector
{
    const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

    readonly Random _rnd = new Random();

    readonly IDictionary<string, string> _memoized = new Dictionary<string, string>();

    public string Correct(string name)
    {
        if (_memoized.TryGetValue(name, out var corrected))
            return corrected;

        corrected = CorrectCore(name);

        if (name != corrected)
        {
            var inc = 0;
            while (_memoized.Any(m => m.Key != name && m.Value == corrected))
            {
                corrected = CorrectCore(name);
                if (inc++ > 10)
                {
                    // слишком много однообразых конструкций с недопустимыми символами
                    corrected = Correct(Guid.NewGuid().ToString());
                    break;
                }
            }
        }

        _memoized.Add(name, corrected);

        return corrected;
    }

    string CorrectCore(string name)
    {
        var noSpecSymbols = Regex.Replace(name, @"(?i)[^a-zА-Я0-9_]", _ => Alphabet[_rnd.Next(0, Alphabet.Length - 1)].ToString());

        //замена: 0слово => _0слово
        var regex = new Regex("^[0-9]", RegexOptions.IgnoreCase);
        var match = regex.Match(noSpecSymbols);
        string numberReplace = regex.Replace(noSpecSymbols, "_" + match);

        //замена для зарезервированных слов C#: this => _this 
        using var cs = new CSharpCodeProvider();
        var identifier = cs.CreateValidIdentifier(numberReplace);

        return identifier;
    }
}
