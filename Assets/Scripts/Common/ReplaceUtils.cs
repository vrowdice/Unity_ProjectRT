using System.Collections.Generic;

public static class ReplaceUtils
{
    public static string FormatNumber(long number)
    {
        if (number >= 1_000_000_000)
            return (number / 1_000_000_000f).ToString("0.###") + "B";
        else if (number >= 1_000_000)
            return (number / 1_000_000f).ToString("0.###") + "M";
        else if (number >= 1_000)
            return (number / 1_000f).ToString("0.###") + "K";
        else
            return number.ToString();
    }

    /// <summary>
    /// Replaces placeholders in the format {key} with the corresponding values from the dictionary.
    /// </summary>
    public static string ReplacePlaceholders(string template, Dictionary<string, string> values)
    {
        foreach (var pair in values)
        {
            template = template.Replace("{" + pair.Key + "}", pair.Value);
        }
        return template;
    }

    /// <summary>
    /// Replaces placeholders in a list of templates and returns a new list of filled strings.
    /// </summary>
    public static List<string> ReplacePlaceholders(List<string> templates, Dictionary<string, string> values)
    {
        List<string> result = new List<string>();
        foreach (var template in templates)
        {
            result.Add(ReplacePlaceholders(template, values));
        }
        return result;
    }
}
