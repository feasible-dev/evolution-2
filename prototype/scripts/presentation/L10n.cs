using System.Globalization;
using System.Text.Json;
using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>Startup-selected catalogs. Message IDs and formatting arguments stay outside translations.</summary>
public static class L10n
{
    public static string Language { get; }
    public static CultureInfo Culture { get; }
    private static readonly Dictionary<string, string> English;
    private static readonly Dictionary<string, string> Messages;

    static L10n()
    {
        var requested = OS.GetCmdlineUserArgs().FirstOrDefault(a => a.StartsWith("--lang="))?[7..] ?? "en";
        Language = requested == "fr" ? "fr" : "en";
        Culture = CultureInfo.GetCultureInfo(Language == "fr" ? "fr-FR" : "en-US");
        CultureInfo.CurrentCulture = Culture;
        CultureInfo.CurrentUICulture = Culture;
        CultureInfo.DefaultThreadCurrentCulture = Culture;
        English = ReadCatalog("en");
        Messages = Language == "en" ? English : ReadCatalog(Language);
    }

    public static Dictionary<string, string> ReadCatalog(string language) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(
            Godot.FileAccess.GetFileAsString($"res://localization/{language}.json"))
        ?? throw new InvalidOperationException($"Empty language catalog: {language}");

    public static string Text(string id, params object[] arguments)
    {
        var template = Messages.GetValueOrDefault(id) ?? English.GetValueOrDefault(id)
            ?? throw new KeyNotFoundException($"Missing localization message: {id}");
        return arguments.Length == 0 ? template : string.Format(Culture, template, arguments);
    }
}
