using System.Text;

namespace Ndump.Core;

/// <summary>
/// Shared helpers for CLR type name manipulation.
/// </summary>
public static class TypeNameHelper
{
    /// <summary>
    /// Convert a CLR generic type name from angle-bracket form to backtick-arity form.
    /// e.g. "Dictionary&lt;String, Int32&gt;+Entry" → "Dictionary`2+Entry"
    /// </summary>
    public static string ConvertToBacktickForm(string name)
    {
        if (!name.Contains('<')) return name;

        var sb = new StringBuilder();
        int i = 0;
        while (i < name.Length)
        {
            if (name[i] == '<')
            {
                int depth = 1, argCount = 1;
                int j = i + 1;
                while (j < name.Length && depth > 0)
                {
                    if (name[j] == '<') depth++;
                    else if (name[j] == '>') depth--;
                    else if (name[j] == ',' && depth == 1) argCount++;
                    j++;
                }
                sb.Append('`');
                sb.Append(argCount);
                i = j;
            }
            else
            {
                sb.Append(name[i]);
                i++;
            }
        }
        return sb.ToString();
    }
}
