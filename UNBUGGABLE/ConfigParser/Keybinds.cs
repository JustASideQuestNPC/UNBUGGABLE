using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UNBUGGABLE.Resources;

public class Keybinds
{
    [JsonRequired][JsonPropertyName("undo")]
    public List<string> Undo { get; set; } = ["ctrl+z"];
    [JsonRequired][JsonPropertyName("redo")]
    public List<string> Redo { get; set; } = ["ctrl+y"];
    [JsonRequired][JsonPropertyName("saveFile")]
    public List<string> SaveFile { get; set; } = ["ctrl+s"];
    [JsonRequired][JsonPropertyName("openFile")]
    public List<string> OpenFile { get; set; } = ["ctrl+o"];
    [JsonRequired][JsonPropertyName("resetPlaySpeed")]
    public List<string> ResetPlaySpeed { get; set; } = ["r"];
    [JsonRequired][JsonPropertyName("reloadConfig")]
    public List<string> ReloadConfig { get; set; } = [];
    [JsonRequired][JsonPropertyName("moveForward")]
    public List<string> MoveForward { get; set; } = ["scrollDown", "down"];
    [JsonRequired][JsonPropertyName("moveBack")]
    public List<string> MoveBack { get; set; } = ["scrollUp", "up"];
    [JsonRequired][JsonPropertyName("quickScrollModifier")]
    public List<string> QuickScrollModifier { get; set; } = ["f"];
    [JsonRequired][JsonPropertyName("playPause")]
    public List<string> PlayPause { get; set; } = ["space"];
    [JsonRequired][JsonPropertyName("zoomIn")]
    public List<string> ZoomIn { get; set; } = ["ctrl+scrollUp", "ctrl+oemPlus"];
    [JsonRequired][JsonPropertyName("zoomOut")]
    public List<string> ZoomOut { get; set; } = ["ctrl+scrollDown", "ctrl+oemMinus"];
    [JsonRequired][JsonPropertyName("prevLabel")]
    public List<string> PrevLabel { get; set; } = ["pageUp"];
    [JsonRequired][JsonPropertyName("nextLabel")]
    public List<string> NextLabel { get; set; } = ["pageDown"];
    [JsonRequired][JsonPropertyName("prevNoteSnap")]
    public List<string> PrevNoteSnap { get; set; } = ["left"];
    [JsonRequired][JsonPropertyName("nextNoteSnap")]
    public List<string> NextNoteSnap { get; set; } = ["right"];
    [JsonRequired][JsonPropertyName("placeTopLane")]
    public List<string> PlaceTopLane { get; set; } = ["d3"];
    [JsonRequired][JsonPropertyName("placeBottomLane")]
    public List<string> PlaceBottomLane { get; set; } = ["d4"];
    [JsonRequired][JsonPropertyName("placeCameraLane")]
    public List<string> PlaceCameraLane { get; set; } = ["d5"];
    [JsonRequired][JsonPropertyName("placeCenterLane")]
    public List<string> PlaceCenterLane { get; set; } = ["d6"];
    [JsonRequired][JsonPropertyName("selectAll")]
    public List<string> SelectAll { get; set; } = ["ctrl+a"];
    [JsonRequired][JsonPropertyName("selectAllTopLane")]
    public List<string> SelectTopLane { get; set; } = ["alt+d3"];
    [JsonRequired][JsonPropertyName("selectAllBottomLane")]
    public List<string> SelectBottomLane { get; set; } = ["alt+d4"];
    [JsonRequired][JsonPropertyName("selectAllCameraLane")]
    public List<string> SelectCameraLane { get; set; } = ["alt+d5"];
    [JsonRequired][JsonPropertyName("selectAllCenterLane")]
    public List<string> SelectCenterLane { get; set; } = ["alt+d6"];
    [JsonRequired][JsonPropertyName("cut")]
    public List<string> Cut { get; set; } = ["ctrl+x"];
    [JsonRequired][JsonPropertyName("copy")]
    public List<string> Copy { get; set; } = ["ctrl+c"];
    [JsonRequired][JsonPropertyName("paste")]
    public List<string> Paste { get; set; } = ["ctrl+v"];
    [JsonRequired][JsonPropertyName("clearSelection")]
    public List<string> ClearSelection { get; set; } = ["escape"];
    [JsonRequired][JsonPropertyName("deleteSelection")]
    public List<string> DeleteSelection { get; set; } = ["delete", "back"];
    [JsonRequired][JsonPropertyName("mirrorSelection")]
    public List<string> MirrorSelection { get; set; } = ["ctrl+m"];
    [JsonRequired][JsonPropertyName("moveSelectionForward")]
    public List<string>MoveSelectionForward { get; set; } = ["shift+down"];
    [JsonRequired][JsonPropertyName("moveSelectionBack")]
    public List<string> MoveSelectionBack { get; set; } = ["shift+up"];
    [JsonRequired][JsonPropertyName("setFinishFlag")]
    public List<string> SetFinishFlag { get; set; } = ["e", "f"];
    [JsonRequired][JsonPropertyName("setWhistleFlag")]
    public List<string> SetWhistleFlag { get; set; } = ["w"];
    [JsonRequired][JsonPropertyName("setClapFlag")]
    public List<string> SetClapFlag { get; set; } = ["c", "r"];
    [JsonRequired][JsonPropertyName("setNoiszSpawn")]
    public List<string> SetNoiszFlag { get; set; } = ["n"];
    [JsonRequired][JsonPropertyName("copId0")]
    public List<string> CopId0 { get; set; } = ["ctrl+d0", "ctrl+oem3"];
    [JsonRequired][JsonPropertyName("copId1")] 
    public List<string> CopId1 { get; set; } = ["ctrl+d1"];
    [JsonRequired][JsonPropertyName("copId2")] 
    public List<string> CopId2 { get; set; } = ["ctrl+d2"];
    [JsonRequired][JsonPropertyName("copId3")] 
    public List<string> CopId3 { get; set; } = ["ctrl+d3"];
    [JsonRequired][JsonPropertyName("copId4")] 
    public List<string> CopId4 { get; set; } = ["ctrl+d4"];
    [JsonRequired][JsonPropertyName("prevCop")]
    public List<string> PrevCop { get; set; } = ["oemComma", "oemPipe"];
    [JsonRequired][JsonPropertyName("nextCop")]
    public List<string> NextCop { get; set; } = ["oemPeriod", "oemQuestion"];
    [JsonRequired][JsonPropertyName("addBpmChange")] 
    public List<string> AddBpmChange { get; set; } = ["f9"];
    [JsonRequired][JsonPropertyName("removeBpmChange")]
    public List<string> RemoveBpmChange { get; set; } = ["ctrl+f9"];
    [JsonRequired][JsonPropertyName("addLabel")] 
    public List<string> AddLabel { get; set; } = ["l"];
    [JsonRequired][JsonPropertyName("removeLabel")] 
    public List<string> RemoveLabel { get; set; } = ["ctrl+l"];
    [JsonRequired][JsonPropertyName("addMarker1")] 
    public List<string> AddMarker1 { get; set; } = ["q"];
    [JsonRequired][JsonPropertyName("addMarker2")] 
    public List<string> AddMarker2 { get; set; } = ["shift+q"];
    [JsonRequired][JsonPropertyName("addMarker3")] 
    public List<string> AddMarker3 { get; set; } = ["ctrl+q"];
    [JsonRequired][JsonPropertyName("setBreakpoint")] 
    public List<string> SetBreakpoint { get; set; } = ["b"];
    [JsonRequired][JsonPropertyName("removeBreakpoint")]
    public List<string> RemoveBreakpoint { get; set; } = ["ctrl+b"];
    [JsonRequired][JsonPropertyName("emergencyReload")]
    public List<string> EmergencyReload { get; set; } = ["ctrl+alt+r"];
}