using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UNBUGGABLE.ConfigParser;

public static class JsonHelper
{
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
            File.WriteAllText(outputPath,
                              JsonSerializer.Serialize(outputData, Utils.JsonSerializerOptions));
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
            // skip anything that isn't in the updated file -- in practice, this skips configs that
            // existed in a previous updated and have since been removed
            if (!obj2.ContainsKey(key))
            {
                continue;
            }
            
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