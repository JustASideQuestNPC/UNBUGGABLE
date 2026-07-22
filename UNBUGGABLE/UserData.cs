using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UNBUGGABLE;

namespace UNBEATABLEChartEditor;

/// <summary>
/// Reads and writes persistent data.
/// </summary>
public static class UserData
{
    /// <summary>
    /// The chart file (if any) that was open when the editor last closed.
    /// </summary>
    public static string LastOpenedChartFile = "";
    
    private const string DataFilePath = "userData.json";

    public static void LoadData()
    {
        var path = Path.Combine(Environment.CurrentDirectory, DataFilePath);
        if (File.Exists(path))
        {
            try
            {
                var jsonNode = JsonSerializer.Deserialize<JsonNode>(File.ReadAllText(path));
                if (jsonNode != null)
                {
                    var json = jsonNode.AsObject();
                    if (json["lastOpenedChartFile"]?.AsValue()
                                                   .GetValueKind() is JsonValueKind.String)
                    {
                        LastOpenedChartFile = json["lastOpenedChartFile"]!.GetValue<string>();
                    }
                    if (json["songVolume"]?.AsValue()
                                                   .GetValueKind() is JsonValueKind.Number)
                    {
                        Chart.SongVolume = (int)Math.Floor(json["songVolume"]!.GetValue<float>());
                    }
                    if (json["sfxVolume"]?.AsValue()
                                          .GetValueKind() is JsonValueKind.Number)
                    {
                        Chart.SfxVolume = (int)Math.Floor(json["sfxVolume"]!.GetValue<float>());
                    }
                }
            }
            catch (JsonException e)
            {
                Trace.WriteLine($"Could not parse user data: {e.Message}");
            }
        }
    }

    public static void SaveData()
    {
        var data = new JsonObject
        {
            ["lastOpenedChartFile"] = LastOpenedChartFile,
            ["songVolume"] = Chart.SongVolume,
            ["sfxVolume"] = Chart.SfxVolume
        };
        
        var path = Path.Combine(Environment.CurrentDirectory, DataFilePath);
        File.WriteAllText(path, data.ToString());
    }
}