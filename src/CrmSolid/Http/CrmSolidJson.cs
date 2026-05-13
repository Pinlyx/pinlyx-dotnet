using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrmSolid.Http;

/// <summary>
/// Centralized <see cref="JsonSerializerOptions"/> used by all SDK serialization.
/// camelCase property naming, ignores nulls on write, accepts string enums.
/// </summary>
internal static class CrmSolidJson
{
    public static readonly JsonSerializerOptions Default = BuildOptions();

    private static JsonSerializerOptions BuildOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        return options;
    }
}
