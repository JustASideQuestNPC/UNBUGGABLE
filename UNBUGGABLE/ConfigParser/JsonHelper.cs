using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UNBUGGABLE.Resources;

public static class JsonHelper
{
    /// <summary>
    /// Verifies that a JSON object has the correct keys and types.
    /// </summary>
    /// <param name="json">The JSON value to check.</param>
    /// <param name="types">A dictionary with the correct keys and types. Allowed types are</param>
    /// <param name="allowMissing">Whether the object can be missing one or more of the required
    ///                            keys. Useful for JSON objects that have default values.</param>
    public static bool VerifyJsonObject(JsonObject json, Dictionary<string, JsonValueKind> types, 
        bool allowMissing = false)
    {
        // why is there no JSON.tryParse?
        try
        {
            foreach (var (themeName, _) in json)
            {
                if (!types.ContainsKey(themeName))
                {
                    return false;
                }
            }

            foreach (var (themeName, type) in types)
            {
                if (!json.ContainsKey(themeName) && !allowMissing)
                {
                    return false;
                }
                
                if (json[themeName]!.GetValueKind() != type)
                {
                    return false;
                }
            }
        }
        catch (JsonException e)
        {
            Trace.WriteLine($"Could not parse JSON: {e.Message}");
            return false;
        }

        return true;
    }

    public static bool TryMergeFiles(string file1Path, string file2Path, string outputPath)
    {
        Trace.WriteLine($"Merging JSON files: {file1Path} and {file2Path} into {outputPath}");
        if (!File.Exists(file1Path) || !File.Exists(file2Path))
        {
            Trace.WriteLine("Could not merge JSON files: File(s) not found.");
            return false;
        }
        
        try
        {
            var file1Data = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(file1Path));
            var file2Data = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(file2Path));

            if (file1Data == null || file2Data == null)
            {
                Trace.WriteLine("Could not merge JSON files: Invalid JSON data.");
                return false;
            }
            
            var outputData = RecursiveMergeObjects(file1Data, file2Data);
            File.WriteAllText(outputPath, outputData.ToString());
            return true;
        }
        catch (JsonException e)
        {
            Trace.WriteLine($"Could not merge JSON files: {e.Message}");
            return false;
        }
    }

    private static JsonObject RecursiveMergeObjects(JsonObject obj1, JsonObject obj2)
    {
        var result = (JsonObject)obj2.DeepClone();

        foreach (var (key, value) in obj1)
        {
            if (value is JsonObject overrideObj &&
                result[key] is JsonObject defaultObj)
            {
                result[key] = RecursiveMergeObjects(overrideObj, defaultObj);
            }
            else
            {
                result[key] = value?.DeepClone();
            }
        }

        return result;
    }
}