using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UNBUGGABLE.Resources;

public class Keybinds
{
    [JsonRequired][JsonPropertyName("undo")]
    public List<string> Undo = ["ctrl+z"];
    [JsonRequired][JsonPropertyName("redo")]
    public List<string> Redo = ["ctrl+y"];
    [JsonRequired][JsonPropertyName("saveFile")]
    public List<string> SaveFile = ["ctrl+s"];
    [JsonRequired][JsonPropertyName("openFile")]
    public List<string> OpenFile = ["ctrl+o"];
    [JsonRequired][JsonPropertyName("resetPlaySpeed")]
    public List<string> ResetPlaySpeed = ["r"];
    [JsonRequired][JsonPropertyName("reloadConfig")]
    public List<string> ReloadConfig = [];
    [JsonRequired][JsonPropertyName("moveForward")]
    public List<string> MoveForward = ["scrollDown", "down"];
    [JsonRequired][JsonPropertyName("moveBack")]
    public List<string> MoveBack = ["scrollUp", "up"];
    [JsonRequired][JsonPropertyName("quickScrollModifier")]
    public List<string> QuickScrollModifier = ["f"];
    [JsonRequired][JsonPropertyName("playPause")]
    public List<string> PlayPause = ["space"];
    [JsonRequired][JsonPropertyName("zoomIn")]
    public List<string> ZoomIn = ["ctrl+scrollUp", "ctrl+oemPlus"];
    [JsonRequired][JsonPropertyName("zoomOut")]
    public List<string> ZoomOut = ["ctrl+scrollDown", "ctrl+oemMinus"];
    [JsonRequired][JsonPropertyName("prevLabel")]
    public List<string> PrevLabel = ["pageUp"];
    [JsonRequired][JsonPropertyName("nextLabel")]
    public List<string> NextLabel = ["pageDown"];
    [JsonRequired][JsonPropertyName("prevNoteSnap")]
    public List<string> PrevNoteSnap = ["left"];
    [JsonRequired][JsonPropertyName("nextNoteSnap")]
    public List<string> NextNoteSnap = ["right"];
    [JsonRequired][JsonPropertyName("placeTopLane")]
    public List<string> PlaceTopLane = ["d3"];
    [JsonRequired][JsonPropertyName("placeBottomLane")]
    public List<string> PlaceBottomLane = ["d4"];
    [JsonRequired][JsonPropertyName("placeCameraLane")]
    public List<string> PlaceCameraLane = ["d5"];
    [JsonRequired][JsonPropertyName("placeCenterLane")]
    public List<string> PlaceCenterLane = ["d6"];
    [JsonRequired][JsonPropertyName("selectAll")]
    public List<string> SelectAll = ["ctrl+a"];
    [JsonRequired][JsonPropertyName("selectAllTopLane")]
    public List<string> SelectTopLane = ["alt+d3"];
    [JsonRequired][JsonPropertyName("selectAllBottomLane")]
    public List<string> SelectBottomLane = ["alt+d4"];
    [JsonRequired][JsonPropertyName("selectAllCameraLane")]
    public List<string> SelectCameraLane = ["alt+d5"];
    [JsonRequired][JsonPropertyName("selectAllCenterLane")]
    public List<string> SelectCenterLane = ["alt+d6"];
    [JsonRequired][JsonPropertyName("cut")]
    public List<string> Cut = ["ctrl+x"];
    [JsonRequired][JsonPropertyName("copy")]
    public List<string> Copy = ["ctrl+c"];
    [JsonRequired][JsonPropertyName("paste")]
    public List<string> Paste = ["ctrl+v"];
    [JsonRequired][JsonPropertyName("clearSelection")]
    public List<string> ClearSelection = ["escape"];
    [JsonRequired][JsonPropertyName("deleteSelection")]
    public List<string> DeleteSelection = ["delete", "back"];
    [JsonRequired][JsonPropertyName("mirrorSelection")]
    public List<string> MirrorSelection = ["ctrl+m"];
    [JsonRequired][JsonPropertyName("moveSelectionForward")]
    public List<string>MoveSelectionForward = ["shift+down"];
    [JsonRequired][JsonPropertyName("moveSelectionBack")]
    public List<string> MoveSelectionBack = ["shift+up"];
    [JsonRequired][JsonPropertyName("setFinishFlag")]
    public List<string> SetFinishFlag = ["e", "f"];
    [JsonRequired][JsonPropertyName("setWhistleFlag")]
    public List<string> SetWhistleFlag = ["w"];
    [JsonRequired][JsonPropertyName("setClapFlag")]
    public List<string> SetClapFlag = ["c", "r"];
    [JsonRequired][JsonPropertyName("setNoiszSpawn")]
    public List<string> SetNoiszFlag = ["n"];
    [JsonRequired][JsonPropertyName("copId0")]
    public List<string> CopId0 = ["ctrl+d0", "ctrl+oem3"];
    [JsonRequired][JsonPropertyName("copId1")] 
    public List<string> CopId1 = ["ctrl+d1"];
    [JsonRequired][JsonPropertyName("copId2")] 
    public List<string> CopId2 = ["ctrl+d2"];
    [JsonRequired][JsonPropertyName("copId3")] 
    public List<string> CopId3 = ["ctrl+d3"];
    [JsonRequired][JsonPropertyName("copId4")] 
    public List<string> CopId4 = ["ctrl+d4"];
    [JsonRequired][JsonPropertyName("prevCop")]
    public List<string> PrevCop = ["oemComma", "oemPipe"];
    [JsonRequired][JsonPropertyName("nextCop")]
    public List<string> NextCop = ["oemPeriod", "oemQuestion"];
    [JsonRequired][JsonPropertyName("addBpmChange")] 
    public List<string> AddBpmChange = ["f9"];
    [JsonRequired][JsonPropertyName("removeBpmChange")]
    public List<string> RemoveBpmChange = ["ctrl+f9"];
    [JsonRequired][JsonPropertyName("addLabel")] 
    public List<string> AddLabel = ["l"];
    [JsonRequired][JsonPropertyName("removeLabel")] 
    public List<string> RemoveLabel = ["ctrl+l"];
    [JsonRequired][JsonPropertyName("addMarker1")] 
    public List<string> AddMarker1 = ["q"];
    [JsonRequired][JsonPropertyName("addMarker2")] 
    public List<string> AddMarker2 = ["shift+q"];
    [JsonRequired][JsonPropertyName("addMarker3")] 
    public List<string> AddMarker3 = ["ctrl+q"];
    [JsonRequired][JsonPropertyName("setBreakpoint")] 
    public List<string> SetBreakpoint = ["b"];
    [JsonRequired][JsonPropertyName("removeBreakpoint")]
    public List<string> RemoveBreakpoint = ["ctrl+b"];
    [JsonRequired][JsonPropertyName("emergencyReload")]
    public List<string> EmergencyReload = ["ctrl+alt+r"];
}