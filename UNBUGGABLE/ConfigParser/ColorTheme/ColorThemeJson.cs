using System.Text.Json.Serialization;

namespace UNBUGGABLE.Resources;
public class MainWindowThemeJson
{
    public class EventIndicatorThemeJson
    {
        [JsonRequired][JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("textColor")]
        public string TextColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("textSize")]
        public double TextSize { get; set; } = 0;
    }

    [JsonRequired][JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("eventIndicator")]
    public EventIndicatorThemeJson EventIndicator { get; set; } = new();
}

public class ElementThemeJson
{
    [JsonRequired][JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("outlineColor")]
    public string OutlineColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("outlineThickness")]
    public double OutlineThickness { get; set; } = 0;
    [JsonRequired][JsonPropertyName("cornerRadius")]
    public double CornerRadius { get; set; } = 0;
}

public class TextElementThemeJson : ElementThemeJson
{
    [JsonRequired][JsonPropertyName("textColor")]
    public string TextColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("textSize")]
    public double TextSize { get; set; } = 0;
}

public class ButtonThemeJson : ElementThemeJson
{
    public class HoveredThemeJson
    {
        [JsonRequired][JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("outlineColor")]
        public string OutlineColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("iconColor")]
        public string IconColor { get; set; } = "";
    }
    
    [JsonRequired][JsonPropertyName("iconColor")]
    public string IconColor { get; set; } = "";
    
    [JsonRequired][JsonPropertyName("hovered")]
    public HoveredThemeJson Hovered { get; set; } = new();
}

public class TopBarThemeJson
{
    public class SliderThemeJson
    {
        [JsonRequired][JsonPropertyName("topColor")]
        public string TopColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("bottomColor")]
        public string BottomColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("iconColor")]
        public string IconColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("handleColor")]
        public string HandleColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("topThickness")]
        public double TopThickness { get; set; } = 0;
        [JsonRequired][JsonPropertyName("bottomThickness")]
        public double BottomThickness { get; set; } = 0;
        [JsonRequired][JsonPropertyName("handleWidth")]
        public double HandleWidth { get; set; } = 0;
        [JsonRequired][JsonPropertyName("handleHeight")]
        public double HandleHeight { get; set; } = 0;
    }
    
    [JsonRequired][JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = "";
    
    [JsonRequired][JsonPropertyName("sliders")]
    public SliderThemeJson Sliders { get; set; } = new();
    
    [JsonRequired][JsonPropertyName("buttons")]
    public ButtonThemeJson Buttons { get; set; } = new();
    
    [JsonRequired][JsonPropertyName("tooltips")]
    public TextElementThemeJson Tooltips { get; set; } = new();
    
    [JsonRequired][JsonPropertyName("saveFileContextMenu")]
    public TextElementThemeJson SaveFileContextMenu { get; set; } = new();
}

public class DialogThemeJson : TextElementThemeJson
{
    [JsonRequired][JsonPropertyName("inputBoxes")]
    public TextElementThemeJson InputBoxes { get; set; } = new();
    [JsonRequired][JsonPropertyName("buttons")]
    public ButtonThemeJson Buttons { get; set; } = new();
}

public class QuickInfoThemeJson {
    [JsonRequired][JsonPropertyName("titleColor")]
    public string TitleColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("titleSize")]
    public double TitleSize { get; set; } = 0;
    [JsonRequired][JsonPropertyName("infoColor")]
    public string InfoColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("infoSize")]
    public double InfoSize { get; set; } = 0;
}

public class LineThemeJson
{
    [JsonRequired][JsonPropertyName("color")]
    public string Color { get; set; } = "";
    [JsonRequired][JsonPropertyName("thickness")]
    public double Thickness { get; set; } = 0;
}

public class NoteLaneThemesJson
{
    [JsonRequired][JsonPropertyName("topColor")]
    public string TopColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("topWidth")]
    public double TopWidth { get; set; } = 0;
    [JsonRequired][JsonPropertyName("bottomColor")]
    public string BottomColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("bottomWidth")]
    public double BottomWidth { get; set; } = 0;
    [JsonRequired][JsonPropertyName("centerColor")]
    public string CenterColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("centerWidth")]
    public double CenterWidth { get; set; } = 0;
    [JsonRequired][JsonPropertyName("cameraColor")]
    public string CameraColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("cameraWidth")]
    public double CameraWidth { get; set; } = 0;
}

public class NoteViewerThemeJson : ElementThemeJson
{
    public class LaneNumberThemeJson
    {
        [JsonRequired][JsonPropertyName("color")]
        public string Color { get; set; } = "";
        [JsonRequired][JsonPropertyName("outlineColor")]
        public string OutlineColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("outlineThickness")]
        public double OutlineThickness { get; set; } = 0;
        [JsonRequired][JsonPropertyName("textSize")]
        public double TextSize { get; set; } = 0;   
    }
    
    public class LabeledLineThemeJson
    {
        [JsonRequired][JsonPropertyName("color")]
        public string Color { get; set; } = "";
        [JsonRequired][JsonPropertyName("lineThickness")]
        public double LineThickness { get; set; } = 0;
        [JsonRequired][JsonPropertyName("textSize")]
        public double TextSize { get; set; } = 0;   
    }
    
    public class FullBeatSnapLineThemeJson : LineThemeJson
    {
        [JsonRequired][JsonPropertyName("textSize")]
        public double TextSize { get; set; } = 0;  
    }
    
    public class MarkersThemeJson
    {
        [JsonRequired][JsonPropertyName("color1")]
        public string Color1 { get; set; } = "";
        [JsonRequired][JsonPropertyName("color2")]
        public string Color2 { get; set; } = "";
        [JsonRequired][JsonPropertyName("color3")]
        public string Color3 { get; set; } = "";
        [JsonRequired][JsonPropertyName("arrowScale")]
        public double ArrowScale { get; set; } = 0;
    }
    
    public class BreakpointThemeJson : LineThemeJson
    {
        [JsonRequired][JsonPropertyName("arrowScale")]
        public double ArrowScale { get; set; } = 0;
    }
    
    [JsonRequired][JsonPropertyName("selectDragColor")]
    public string SelectDragColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("deleteDragColor")]
    public string DeleteDragColor { get; set; } = "";
    
    [JsonRequired][JsonPropertyName("noteLanes")]
    public NoteLaneThemesJson NoteLanes { get; set; } = new();
    [JsonRequired][JsonPropertyName("laneNumbers")]
    public LaneNumberThemeJson LaneNumbers { get; set; } = new();
    [JsonRequired][JsonPropertyName("bpmChange")]
    public LabeledLineThemeJson BpmChanges { get; set; } = new();   
    [JsonRequired][JsonPropertyName("label")]
    public LabeledLineThemeJson Labels { get; set; } = new();
    [JsonRequired][JsonPropertyName("fullBeatSnapLine")]
    public FullBeatSnapLineThemeJson FullBeatSnapLine { get; set; } = new();
    [JsonRequired][JsonPropertyName("subBeatSnapLine")]
    public LineThemeJson SubBeatSnapLine { get; set; } = new();
    [JsonRequired][JsonPropertyName("currentTimeLine")]
    public LineThemeJson CurrentTimeLine { get; set; } = new();
    [JsonRequired][JsonPropertyName("markers")]
    public MarkersThemeJson Markers { get; set; } = new();   
    [JsonRequired][JsonPropertyName("breakpoint")]
    public BreakpointThemeJson Breakpoint { get; set; } = new();  
}

public class GamePreviewThemeJson : ElementThemeJson
{
    public class ViewableAreaThemeJson
    {
        [JsonRequired][JsonPropertyName("outlineColor")]
        public string OutlineColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("outlineThickness")]
        public double OutlineThickness { get; set; } = 0;
    }

    public class NoteTargetsThemeJson
    {
        public class TargetCirclesThemeJson
        {
            [JsonRequired][JsonPropertyName("radius")]
            public double Radius { get; set; } = 0;
            [JsonRequired][JsonPropertyName("fillColor")]
            public string FillColor { get; set; } = "";
            [JsonRequired][JsonPropertyName("outlineColor")]
            public string OutlineColor { get; set; } = "";
            [JsonRequired][JsonPropertyName("outlineThickness")]
            public double OutlineThickness { get; set; } = 0;
        }
        
        [JsonRequired][JsonPropertyName("lineColor")]
        public string LineColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("lineThickness")]
        public double LineThickness { get; set; } = 0;
        [JsonRequired][JsonPropertyName("circles")]
        public TargetCirclesThemeJson TargetCircles { get; set; } = new();
    }
    
    [JsonRequired][JsonPropertyName("copColor")]
    public string CopColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("viewableArea")]
    public ViewableAreaThemeJson ViewableArea { get; set; } = new();
    [JsonRequired][JsonPropertyName("cameraArrowColor")]
    public string CameraArrowColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("cameraArrowScale")]
    public double CameraArrowScale { get; set; } = 0;
    [JsonRequired][JsonPropertyName("noteTargets")]
    public NoteTargetsThemeJson NoteTargets { get; set; } = new();
}

public class PlacementPriorityListThemeJson : ElementThemeJson
{
    public class ListEntryThemeJson : TextElementThemeJson
    {
        [JsonRequired][JsonPropertyName("reorderIconColor")]
        public string ReorderIconColor { get; set; } = "";   
    }
    
    [JsonRequired][JsonPropertyName("titleColor")]
    public string TitleColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("titleSize")]
    public double TitleSize { get; set; } = 0;
    [JsonRequired][JsonPropertyName("listEntries")]
    public ListEntryThemeJson ListEntries { get; set; } = new();
}

public class InstantNoteThemeJson
{
    public class SelectedThemeJson
    {
        [JsonRequired][JsonPropertyName("fillColor")]
        public string FillColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("outlineColor")]
        public string OutlineColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("outlineThickness")]
        public double OutlineThickness { get; set; } = 0;
        
    }
    
    [JsonRequired][JsonPropertyName("fillColor")]
    public string FillColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("outlineColor")]
    public string OutlineColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("outlineThickness")]
    public double OutlineThickness { get; set; } = 0;
    [JsonRequired][JsonPropertyName("selected")]
    public SelectedThemeJson Selected { get; set; } = new();   
}

public class NonInstantNoteThemeJson
{
    public class SelectedThemeJson
    {
        [JsonRequired][JsonPropertyName("fillColor")]
        public string FillColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("outlineColor")]
        public string OutlineColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("outlineThickness")]
        public double OutlineThickness { get; set; } = 0;
        [JsonRequired][JsonPropertyName("tailColor")]
        public string TailColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("tailOutlineColor")]
        public string TailOutlineColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("tailOutlineThickness")]
        public double TailOutlineThickness { get; set; } = 0;
    }
    
    [JsonRequired][JsonPropertyName("fillColor")]
    public string FillColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("outlineColor")]
    public string OutlineColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("outlineThickness")]
    public double OutlineThickness { get; set; } = 0;
    [JsonRequired][JsonPropertyName("tailColor")]
    public string TailColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("tailOutlineColor")]
    public string TailOutlineColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("tailOutlineThickness")]
    public double TailOutlineThickness { get; set; } = 0;
    [JsonRequired][JsonPropertyName("selected")]
    public SelectedThemeJson Selected { get; set; } = new();
}

public class NoteThemesJson
{
    public class CommonThemeJson
    {
        [JsonRequired][JsonPropertyName("flagTextColor")]
        public string FlagTextColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("flagTextOutlineColor")]
        public string FlagTextOutlineColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("flagTextOutlineThickness")]
        public double FlagTextOutlineThickness { get; set; } = 0;
        [JsonRequired][JsonPropertyName("flagTextSize")]
        public double FlagTextSize { get; set; } = 0;   
    }
    
    [JsonRequired][JsonPropertyName("common")]
    public CommonThemeJson Common { get; set; } = new();
    [JsonRequired][JsonPropertyName("single")]
    public InstantNoteThemeJson Single { get; set; } = new();
    [JsonRequired][JsonPropertyName("spike")]
    public InstantNoteThemeJson Spike { get; set; } = new();
    [JsonRequired][JsonPropertyName("hold")]
    public NonInstantNoteThemeJson Hold { get; set; } = new();
    [JsonRequired][JsonPropertyName("double")]
    public NonInstantNoteThemeJson Double { get; set; } = new();
    [JsonRequired][JsonPropertyName("freestyle")]
    public InstantNoteThemeJson Freestyle { get; set; } = new();
    [JsonRequired][JsonPropertyName("mash")]
    public NonInstantNoteThemeJson Mash { get; set; } = new();
    [JsonRequired][JsonPropertyName("camera")]
    public InstantNoteThemeJson Camera { get; set; } = new();
    [JsonRequired][JsonPropertyName("cop1")]
    public NonInstantNoteThemeJson Cop1 { get; set; } = new();
    [JsonRequired][JsonPropertyName("cop2")]
    public NonInstantNoteThemeJson Cop2 { get; set; } = new();
    [JsonRequired][JsonPropertyName("cop3")]
    public NonInstantNoteThemeJson Cop3 { get; set; } = new();
    [JsonRequired][JsonPropertyName("cop4")]
    public NonInstantNoteThemeJson Cop4 { get; set; } = new();
}

public class DebugInfoThemeJson
{
    [JsonRequired][JsonPropertyName("overlayBackgroundColor")]
    public string OverlayBackgroundColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("overlayTextColor")]
    public string OverlayTextColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("overlayTextSize")]
    public double OverlayTextSize { get; set; } = 0;
    [JsonRequired][JsonPropertyName("noteTimestampTextColor")]
    public string NoteTimestampTextColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("noteTimestampTextOutlineColor")]
    public string NoteTimestampTextOutlineColor { get; set; } = "";
    [JsonRequired][JsonPropertyName("noteTimestampTextOutlineThickness")]
    public double NoteTimestampTextOutlineThickness { get; set; } = 0;
    [JsonRequired][JsonPropertyName("noteTimestampTextSize")]
    public double NoteTimestampTextSize { get; set; } = 0;
}

public class ColorThemeJson
{
    [JsonRequired][JsonPropertyName("mainWindow")]
    public MainWindowThemeJson MainWindow { get; set; } = new();
    [JsonRequired][JsonPropertyName("topBar")]
    public TopBarThemeJson TopBar { get; set; } = new();
    [JsonRequired][JsonPropertyName("dialogs")]
    public DialogThemeJson Dialogs { get; set; } = new();
    [JsonRequired][JsonPropertyName("quickInfo")]
    public QuickInfoThemeJson QuickInfo { get; set; } = new();
    [JsonRequired][JsonPropertyName("noteViewer")]
    public NoteViewerThemeJson NoteViewer { get; set; } = new();
    [JsonRequired][JsonPropertyName("gamePreview")]
    public GamePreviewThemeJson GamePreview { get; set; } = new();
    [JsonRequired][JsonPropertyName("placementPriorityList")]
    public PlacementPriorityListThemeJson PlacementPriorityList { get; set; } = new();
    [JsonRequired][JsonPropertyName("notes")]
    public NoteThemesJson NoteThemes { get; set; } = new();
    [JsonRequired][JsonPropertyName("debugInfo")]
    public DebugInfoThemeJson DebugInfo { get; set; } = new();
}