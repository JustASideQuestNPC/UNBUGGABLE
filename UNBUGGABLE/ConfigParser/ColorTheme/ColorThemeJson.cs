using System.Text.Json.Serialization;

namespace UNBUGGABLE.Resources;
public class MainWindowThemeJson
{
    public class EventIndicatorThemeJson
    {
        [JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; } = "#FF4B7E";
        [JsonPropertyName("textColor")]
        public string TextColor { get; set; } = "#161616";
        [JsonPropertyName("textSize")]
        public double TextSize { get; set; } = 16;
    }

    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = "#1B1F21";
    [JsonPropertyName("eventIndicator")]
    public EventIndicatorThemeJson EventIndicator { get; set; } = new();
}

public class ElementThemeJson
{
    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = "";
    [JsonPropertyName("outlineColor")]
    public string OutlineColor { get; set; } = "";
    [JsonPropertyName("outlineThickness")]
    public double OutlineThickness { get; set; } = 0;
    [JsonPropertyName("cornerRadius")]
    public double CornerRadius { get; set; } = 0;
}

public class TextElementThemeJson : ElementThemeJson
{
    [JsonPropertyName("textColor")]
    public string TextColor { get; set; } = "";
    [JsonPropertyName("textSize")]
    public double TextSize { get; set; } = 0;
}

public class ButtonThemeJson : ElementThemeJson
{
    public class HoveredThemeJson
    {
        [JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; } = "";
        [JsonPropertyName("outlineColor")]
        public string OutlineColor { get; set; } = "";
        [JsonPropertyName("iconColor")]
        public string IconColor { get; set; } = "";
    }
    
    [JsonPropertyName("iconColor")]
    public string IconColor { get; set; } = "";
    
    [JsonPropertyName("hovered")]
    public HoveredThemeJson Hovered { get; set; } = new();
}

public class TopBarThemeJson
{
    public class SliderThemeJson
    {
        [JsonPropertyName("topColor")]
        public string TopColor { get; set; } = "#FF4B7E";
        [JsonPropertyName("bottomColor")]
        public string BottomColor { get; set; } = "#C9C9A5";
        [JsonPropertyName("iconColor")]
        public string IconColor { get; set; } = "#FF4B7E";
        [JsonPropertyName("handleColor")]
        public string HandleColor { get; set; } = "#FF4B7E";
        [JsonPropertyName("topThickness")]
        public double TopThickness { get; set; } = 4;
        [JsonPropertyName("bottomThickness")]
        public double BottomThickness { get; set; } = 4;
        [JsonPropertyName("handleWidth")]
        public double HandleWidth { get; set; } = 18;
        [JsonPropertyName("handleHeight")]
        public double HandleHeight { get; set; } = 18;
    }
    
    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = "#FAF7D6";
    
    [JsonPropertyName("sliders")]
    public SliderThemeJson Sliders { get; set; } = new();
    
    [JsonPropertyName("buttons")]
    public ButtonThemeJson Buttons { get; set; } = new();
    
    [JsonPropertyName("tooltips")]
    public TextElementThemeJson Tooltips { get; set; } = new();
    
    [JsonPropertyName("saveFileContextMenu")]
    public TextElementThemeJson SaveFileContextMenu { get; set; } = new();
}

public class DialogThemeJson : TextElementThemeJson
{
    [JsonPropertyName("inputBoxes")]
    public TextElementThemeJson InputBoxes { get; set; } = new();
    [JsonPropertyName("buttons")]
    public ButtonThemeJson Buttons { get; set; } = new();
}

public class QuickInfoThemeJson {
    [JsonPropertyName("titleColor")]
    public string TitleColor { get; set; } = "";
    [JsonPropertyName("titleSize")]
    public double TitleSize { get; set; } = 0;
    [JsonPropertyName("infoColor")]
    public string InfoColor { get; set; } = "";
    [JsonPropertyName("infoSize")]
    public double InfoSize { get; set; } = 0;
}

public class LineThemeJson
{
    [JsonPropertyName("color")]
    public string Color { get; set; } = "";
    [JsonPropertyName("thickness")]
    public double Thickness { get; set; } = 0;
}

public class NoteLaneThemesJson
{
    [JsonPropertyName("topColor")]
    public string TopColor { get; set; } = "";
    [JsonPropertyName("topWidth")]
    public double TopWidth { get; set; } = 0;
    [JsonPropertyName("bottomColor")]
    public string BottomColor { get; set; } = "";
    [JsonPropertyName("bottomWidth")]
    public double BottomWidth { get; set; } = 0;
    [JsonPropertyName("centerColor")]
    public string CenterColor { get; set; } = "";
    [JsonPropertyName("centerWidth")]
    public double CenterWidth { get; set; } = 0;
    [JsonPropertyName("cameraColor")]
    public string CameraColor { get; set; } = "";
    [JsonPropertyName("cameraWidth")]
    public double CameraWidth { get; set; } = 0;
}

public class NoteViewerThemeJson : ElementThemeJson
{
    public class LaneNumberThemeJson
    {
        [JsonPropertyName("color")]
        public string Color { get; set; } = "";
        [JsonPropertyName("outlineColor")]
        public string OutlineColor { get; set; } = "";
        [JsonPropertyName("outlineThickness")]
        public double OutlineThickness { get; set; } = 0;
        [JsonPropertyName("textSize")]
        public double TextSize { get; set; } = 0;   
    }
    
    public class LabeledLineThemeJson
    {
        [JsonPropertyName("color")]
        public string Color { get; set; } = "";
        [JsonPropertyName("lineThickness")]
        public double LineThickness { get; set; } = 0;
        [JsonPropertyName("textSize")]
        public double TextSize { get; set; } = 0;   
    }
    
    public class FullBeatSnapLineThemeJson : LineThemeJson
    {
        [JsonPropertyName("textSize")]
        public double TextSize { get; set; } = 0;  
    }
    
    public class MarkersThemeJson
    {
        [JsonPropertyName("color1")]
        public string Color1 { get; set; } = "";
        [JsonPropertyName("color2")]
        public string Color2 { get; set; } = "";
        [JsonPropertyName("color3")]
        public string Color3 { get; set; } = "";
        [JsonPropertyName("arrowScale")]
        public double ArrowScale { get; set; } = 0;
    }
    
    public class BreakpointThemeJson : LineThemeJson
    {
        [JsonPropertyName("arrowScale")]
        public double ArrowScale { get; set; } = 0;
    }
    
    [JsonPropertyName("selectDragColor")]
    public string SelectDragColor { get; set; } = "";
    [JsonPropertyName("deleteDragColor")]
    public string DeleteDragColor { get; set; } = "";
    
    [JsonPropertyName("noteLanes")]
    public NoteLaneThemesJson NoteLanes { get; set; } = new();
    [JsonPropertyName("laneNumbers")]
    public LaneNumberThemeJson LaneNumbers { get; set; } = new();
    [JsonPropertyName("bpmChange")]
    public LabeledLineThemeJson BpmChanges { get; set; } = new();   
    [JsonPropertyName("label")]
    public LabeledLineThemeJson Labels { get; set; } = new();
    [JsonPropertyName("fullBeatSnapLine")]
    public FullBeatSnapLineThemeJson FullBeatSnapLine { get; set; } = new();
    [JsonPropertyName("subBeatSnapLine")]
    public LineThemeJson SubBeatSnapLine { get; set; } = new();
    [JsonPropertyName("currentTimeLine")]
    public LineThemeJson CurrentTimeLine { get; set; } = new();
    [JsonPropertyName("markers")]
    public MarkersThemeJson Markers { get; set; } = new();   
    [JsonPropertyName("breakpoint")]
    public BreakpointThemeJson Breakpoint { get; set; } = new();
    
    [JsonPropertyName("noteDirectionArrowColor")]
    public string NoteDirectionArrowColor { get; set; } = "";
    [JsonPropertyName("noteDirectionArrowScale")]
    public double NoteDirectionArrowScale { get; set; } = 0;
}

public class GamePreviewThemeJson : ElementThemeJson
{
    public class ViewableAreaThemeJson
    {
        [JsonPropertyName("outlineColor")]
        public string OutlineColor { get; set; } = "";
        [JsonPropertyName("outlineThickness")]
        public double OutlineThickness { get; set; } = 0;
    }

    public class NoteTargetsThemeJson
    {
        public class TargetCirclesThemeJson
        {
            [JsonPropertyName("radius")]
            public double Radius { get; set; } = 0;
            [JsonPropertyName("fillColor")]
            public string FillColor { get; set; } = "";
            [JsonPropertyName("outlineColor")]
            public string OutlineColor { get; set; } = "";
            [JsonPropertyName("outlineThickness")]
            public double OutlineThickness { get; set; } = 0;
        }
        
        [JsonPropertyName("lineColor")]
        public string LineColor { get; set; } = "";
        [JsonPropertyName("lineThickness")]
        public double LineThickness { get; set; } = 0;
        [JsonPropertyName("circles")]
        public TargetCirclesThemeJson TargetCircles { get; set; } = new();
    }
    
    [JsonPropertyName("copColor")]
    public string CopColor { get; set; } = "";
    [JsonPropertyName("viewableArea")]
    public ViewableAreaThemeJson ViewableArea { get; set; } = new();
    [JsonPropertyName("cameraArrowColor")]
    public string CameraArrowColor { get; set; } = "";
    [JsonPropertyName("cameraArrowScale")]
    public double CameraArrowScale { get; set; } = 0;
    [JsonPropertyName("noteTargets")]
    public NoteTargetsThemeJson NoteTargets { get; set; } = new();
}

public class PlacementPriorityListThemeJson : ElementThemeJson
{
    public class ListEntryThemeJson : TextElementThemeJson
    {
        [JsonPropertyName("reorderIconColor")]
        public string ReorderIconColor { get; set; } = "";   
    }
    
    [JsonPropertyName("titleColor")]
    public string TitleColor { get; set; } = "";
    [JsonPropertyName("titleSize")]
    public double TitleSize { get; set; } = 0;
    [JsonPropertyName("listEntries")]
    public ListEntryThemeJson ListEntries { get; set; } = new();
}

public class InstantNoteThemeJson
{
    public class SelectedThemeJson
    {
        [JsonPropertyName("fillColor")]
        public string FillColor { get; set; } = "";
        [JsonPropertyName("outlineColor")]
        public string OutlineColor { get; set; } = "";
        [JsonPropertyName("outlineThickness")]
        public double OutlineThickness { get; set; } = 0;
        
    }
    
    [JsonPropertyName("fillColor")]
    public string FillColor { get; set; } = "";
    [JsonPropertyName("outlineColor")]
    public string OutlineColor { get; set; } = "";
    [JsonPropertyName("outlineThickness")]
    public double OutlineThickness { get; set; } = 0;
    [JsonPropertyName("selected")]
    public SelectedThemeJson Selected { get; set; } = new();   
}

public class NonInstantNoteThemeJson
{
    public class SelectedThemeJson
    {
        [JsonPropertyName("fillColor")]
        public string FillColor { get; set; } = "";
        [JsonPropertyName("outlineColor")]
        public string OutlineColor { get; set; } = "";
        [JsonPropertyName("outlineThickness")]
        public double OutlineThickness { get; set; } = 0;
        [JsonPropertyName("tailColor")]
        public string TailColor { get; set; } = "";
        [JsonPropertyName("tailOutlineColor")]
        public string TailOutlineColor { get; set; } = "";
        [JsonPropertyName("tailOutlineThickness")]
        public double TailOutlineThickness { get; set; } = 0;
    }
    
    [JsonPropertyName("fillColor")]
    public string FillColor { get; set; } = "";
    [JsonPropertyName("outlineColor")]
    public string OutlineColor { get; set; } = "";
    [JsonPropertyName("outlineThickness")]
    public double OutlineThickness { get; set; } = 0;
    [JsonPropertyName("tailColor")]
    public string TailColor { get; set; } = "";
    [JsonPropertyName("tailOutlineColor")]
    public string TailOutlineColor { get; set; } = "";
    [JsonPropertyName("tailOutlineThickness")]
    public double TailOutlineThickness { get; set; } = 0;
    [JsonPropertyName("selected")]
    public SelectedThemeJson Selected { get; set; } = new();
}

public class NoteThemesJson
{
    public class CommonThemeJson
    {
        [JsonPropertyName("flagTextColor")]
        public string FlagTextColor { get; set; } = "";
        [JsonPropertyName("flagTextOutlineColor")]
        public string FlagTextOutlineColor { get; set; } = "";
        [JsonPropertyName("flagTextOutlineThickness")]
        public double FlagTextOutlineThickness { get; set; } = 0;
        [JsonPropertyName("flagTextSize")]
        public double FlagTextSize { get; set; } = 0;   
    }
    
    [JsonPropertyName("common")]
    public CommonThemeJson Common { get; set; } = new();
    [JsonPropertyName("single")]
    public InstantNoteThemeJson Single { get; set; } = new();
    [JsonPropertyName("spike")]
    public InstantNoteThemeJson Spike { get; set; } = new();
    [JsonPropertyName("hold")]
    public NonInstantNoteThemeJson Hold { get; set; } = new();
    [JsonPropertyName("double")]
    public NonInstantNoteThemeJson Double { get; set; } = new();
    [JsonPropertyName("freestyle")]
    public InstantNoteThemeJson Freestyle { get; set; } = new();
    [JsonPropertyName("mash")]
    public NonInstantNoteThemeJson Mash { get; set; } = new();
    [JsonPropertyName("camera")]
    public InstantNoteThemeJson Camera { get; set; } = new();
    [JsonPropertyName("cop1")]
    public NonInstantNoteThemeJson Cop1 { get; set; } = new();
    [JsonPropertyName("cop2")]
    public NonInstantNoteThemeJson Cop2 { get; set; } = new();
    [JsonPropertyName("cop3")]
    public NonInstantNoteThemeJson Cop3 { get; set; } = new();
    [JsonPropertyName("cop4")]
    public NonInstantNoteThemeJson Cop4 { get; set; } = new();
}

public class DebugInfoThemeJson
{
    [JsonPropertyName("overlayBackgroundColor")]
    public string OverlayBackgroundColor { get; set; } = "";
    [JsonPropertyName("overlayTextColor")]
    public string OverlayTextColor { get; set; } = "";
    [JsonPropertyName("overlayTextSize")]
    public double OverlayTextSize { get; set; } = 0;
    [JsonPropertyName("noteTimestampTextColor")]
    public string NoteTimestampTextColor { get; set; } = "";
    [JsonPropertyName("noteTimestampTextOutlineColor")]
    public string NoteTimestampTextOutlineColor { get; set; } = "";
    [JsonPropertyName("noteTimestampTextOutlineThickness")]
    public double NoteTimestampTextOutlineThickness { get; set; } = 0;
    [JsonPropertyName("noteTimestampTextSize")]
    public double NoteTimestampTextSize { get; set; } = 0;
}

public class ColorThemeJson
{
    [JsonPropertyName("mainWindow")]
    public MainWindowThemeJson MainWindow { get; set; } = new()
    {
        BackgroundColor = "#1B1F21",
        EventIndicator = new()
        {
            BackgroundColor = "#FF4B7E",
            TextColor = "#161616",
            TextSize = 16
        }
    };
    [JsonPropertyName("topBar")]
    public TopBarThemeJson TopBar { get; set; } = new()
    {
        BackgroundColor = "#FAF7D6",
        Sliders = new()
        {
            TopColor = "#FF4B7E",
            BottomColor = "#C9C9A5",
            IconColor = "#FF4B7E",
            HandleColor = "#FF4B7E",
            TopThickness = 4,
            BottomThickness = 4,
            HandleWidth = 18,
        },
        Buttons = new()
        {
            BackgroundColor = "#00000000",
            OutlineColor = "#FF4B7E",
            OutlineThickness = 2,
            CornerRadius = 4,
            IconColor = "#FF4B7E",
            Hovered = new()
            {
                BackgroundColor = "#C9C9A5",
                OutlineColor = "",
                IconColor = ""
            }
        },
        Tooltips = new()
        {
            BackgroundColor = "#FAF7D6",
            OutlineColor = "#FF4B7E",
            OutlineThickness = 2,
            CornerRadius = 0,
            TextColor = "#161616",
            TextSize = 14
        },
        SaveFileContextMenu = new()
        {
            BackgroundColor = "#1B1F21",
            OutlineColor = "#FF4B7E",
            OutlineThickness = 0,
            CornerRadius = 4,
            TextColor = "#FFFFFF",
            TextSize = 14
        }
    };
    [JsonPropertyName("dialogs")]
    public DialogThemeJson Dialogs { get; set; } = new()
    {
        BackgroundColor = "#FAF7D6",
        OutlineColor = "#FF4B7E",
        OutlineThickness = 6,
        CornerRadius = 6,
        TextColor = "#161616",
        TextSize = 22,
        InputBoxes = new()
        {
            BackgroundColor = "#C9C9A5",
            OutlineColor = "#FF4B7E",
            OutlineThickness = 2,
            CornerRadius = 0,
            TextColor = "#161616",
            TextSize = 18
        },
        Buttons = new()
        {
            BackgroundColor = "#00000000",
            OutlineColor = "#FF4B7E",
            OutlineThickness = 2,
            CornerRadius = 4,
            IconColor = "#FF4B7E",
            Hovered = new()
            {
                BackgroundColor = "#C9C9A5",
                OutlineColor = "",
                IconColor = ""
            }
        },
    };
    [JsonPropertyName("quickInfo")]
    public QuickInfoThemeJson QuickInfo { get; set; } = new()
    {
        TitleColor = "#D0D0D0",
        TitleSize = 15,
        InfoColor = "#FFFFFF",
        InfoSize = 22
    };
    [JsonPropertyName("noteViewer")]
    public NoteViewerThemeJson NoteViewer { get; set; } = new()
    {
        BackgroundColor = "#1B1F21",
        OutlineColor = "#FF4B7E",
        OutlineThickness = 4,
        CornerRadius = 0,
        SelectDragColor = "#FFFFFF64",
        DeleteDragColor = "#FF000064",
        NoteDirectionArrowColor = "#FBB7DE96",
        NoteDirectionArrowScale = 1.0,
        NoteLanes = new()
        {
            TopColor = "#9C999C19",
            TopWidth = 32,
            BottomColor = "#9C999C19",
            BottomWidth = 32,
            CenterColor = "#FF9A9A19",
            CenterWidth = 32,
            CameraColor = "#FBB7DE19",
            CameraWidth = 32
        },
        LaneNumbers = new()
        {
            Color = "#FFFFFF",
            OutlineColor = "#161616",
            OutlineThickness = 2,
            TextSize = 40
        },
        FullBeatSnapLine = new()
        {
            Color = "#E0E0E096",
            Thickness = 3,
            TextSize = 16
        },
        SubBeatSnapLine = new()
        {
            Color = "#80808096",
            Thickness = 3
        },
        CurrentTimeLine = new()
        {
            Color = "#FF000064",
            Thickness = 8
        },
        Breakpoint = new()
        {
            Color = "#FF0000",
            Thickness = 3,
            ArrowScale = 1.0
        },
        BpmChanges = new()
        {
            Color = "#0981EA",
            LineThickness = 5,
            TextSize = 22
        },
        Labels = new()
        {
            Color = "#EADF09",
            LineThickness = 5,
            TextSize = 16
        },
        Markers = new()
        {
            Color1 = "#40DB11",
            Color2 = "#0979EA",
            Color3 = "#B609EA",
            ArrowScale = 1.0
        }
    };
    [JsonPropertyName("gamePreview")]
    public GamePreviewThemeJson GamePreview { get; set; } = new()
    {
        BackgroundColor = "#1B1F21",
        OutlineColor = "#FF4B7E",
        OutlineThickness = 4,
        CornerRadius = 0,
        CopColor = "#FF4B7E",
        ViewableArea = new()
        {
            OutlineColor = "#FBB7DE",
            OutlineThickness = 5
        },
        CameraArrowColor = "#FBB7DE80",
        CameraArrowScale = 1.0,
        NoteTargets = new()
        {
            LineColor = "#808080",
            LineThickness = 2,
            TargetCircles = {
                Radius = 30,
                FillColor = "#00000000",
                OutlineColor = "#FF4B7E",
                OutlineThickness = 5
            }
        }
    };
    [JsonPropertyName("placementPriorityList")]
    public PlacementPriorityListThemeJson PlacementPriorityList { get; set; } =
        new()
    {
        BackgroundColor = "#1B1F21",
        OutlineColor = "#FF4B7E",
        OutlineThickness = 4,
        CornerRadius = 0,
        TitleColor = "#D0D0D0",
        TitleSize = 15,
        ListEntries = new()
        {
            BackgroundColor = "#1B1F21",
            OutlineColor = "#FF4B7E",
            OutlineThickness = 2,
            CornerRadius = 4,
            TextColor = "#FFFFFF",
            TextSize = 22,
            ReorderIconColor = "#FF4B7E"
        }
    };
    [JsonPropertyName("notes")]
    public NoteThemesJson NoteThemes { get; set; } = new()
    {
        Common = new()
        {
            FlagTextColor = "#FFFFFF",
            FlagTextSize = 40,
            FlagTextOutlineColor = "#161616",
            FlagTextOutlineThickness = 2
        },
        Single = new()
        {
            FillColor = "#9C999C",
            OutlineColor = "#000000",
            OutlineThickness = 4,
            Selected = new()
            {
                FillColor = "",
                OutlineColor = "#FFFFFF",
                OutlineThickness = -1
            }
        },
        Spike = new()
        {
            FillColor = "#FFCC00",
            OutlineColor = "#000000",
            OutlineThickness = 4,
            Selected = new()
            {
                FillColor = "",
                OutlineColor = "#FFFFFF",
                OutlineThickness = -1
            }
        },
        Hold = new()
        {
            FillColor = "#9C999C",
            OutlineColor = "#000000",
            OutlineThickness = 4,
            TailColor = "#9C999C96",
            TailOutlineColor = "#00000000",
            TailOutlineThickness = 0,
            Selected = new()
            {
                FillColor = "",
                OutlineColor = "#FFFFFF",
                OutlineThickness = -1,
                TailColor = "",
                TailOutlineColor = "#FFFFFF",
                TailOutlineThickness = 2
            }
        },
        Double = new()
        {
            FillColor = "#65CCFF",
            OutlineColor = "#000000",
            OutlineThickness = 4,
            TailColor = "#65CCFF96",
            TailOutlineColor = "#00000000",
            TailOutlineThickness = 0,
            Selected = new()
            {
                FillColor = "",
                OutlineColor = "#FFFFFF",
                OutlineThickness = -1,
                TailColor = "",
                TailOutlineColor = "#FFFFFF",
                TailOutlineThickness = 2
            }
        },
        Freestyle = new()
        {
            FillColor = "#FF9A9A",
            OutlineColor = "#000000",
            OutlineThickness = 4,
            Selected = new()
            {
                FillColor = "",
                OutlineColor = "#FFFFFF",
                OutlineThickness = -1
            }
        },
        Mash = new()
        {
            FillColor = "#FF9A9A",
            OutlineColor = "#000000",
            OutlineThickness = 4,
            TailColor = "#FF9A9A96",
            TailOutlineColor = "#00000000",
            TailOutlineThickness = 0,
            Selected = new()
            {
                FillColor = "",
                OutlineColor = "#FFFFFF",
                OutlineThickness = -1,
                TailColor = "",
                TailOutlineColor = "#FFFFFF",
                TailOutlineThickness = 2
            }
        },
        Camera = new()
        {
            FillColor = "#FBB7DE",
            OutlineColor = "#000000",
            OutlineThickness = 4,
            Selected = new()
            {
                FillColor = "",
                OutlineColor = "#FFFFFF",
                OutlineThickness = -1
            }
        },
        Cop1 = new()
        {
            FillColor = "#3259E5",
            OutlineColor = "#000000",
            OutlineThickness = 4,
            TailColor = "#3259E596",
            TailOutlineColor = "#00000000",
            TailOutlineThickness = 0,
            Selected = new()
            {
                FillColor = "",
                OutlineColor = "#FFFFFF",
                OutlineThickness = -1,
                TailColor = "",
                TailOutlineColor = "#FFFFFF",
                TailOutlineThickness = 2
            }
        },
        Cop2 = new()
        {
            FillColor = "#ED4964",
            OutlineColor = "#000000",
            OutlineThickness = 4,
            TailColor = "#ED496496",
            TailOutlineColor = "#00000000",
            TailOutlineThickness = 0,
            Selected = new()
            {
                FillColor = "",
                OutlineColor = "#FFFFFF",
                OutlineThickness = -1,
                TailColor = "",
                TailOutlineColor = "#FFFFFF",
                TailOutlineThickness = 2
            }
        },
        Cop3 = new()
        {
            FillColor = "#44F430",
            OutlineColor = "#000000",
            OutlineThickness = 4,
            TailColor = "#44F43096",
            TailOutlineColor = "#00000000",
            TailOutlineThickness = 0,
            Selected = new()
            {
                FillColor = "",
                OutlineColor = "#FFFFFF",
                OutlineThickness = -1,
                TailColor = "",
                TailOutlineColor = "#FFFFFF",
                TailOutlineThickness = 2
            }
        },
        Cop4 = new()
        {
            FillColor = "#F4E430",
            OutlineColor = "#000000",
            OutlineThickness = 4,
            TailColor = "#F4E43096",
            TailOutlineColor = "#00000000",
            TailOutlineThickness = 0,
            Selected = new()
            {
                FillColor = "",
                OutlineColor = "#FFFFFF",
                OutlineThickness = -1,
                TailColor = "",
                TailOutlineColor = "#FFFFFF",
                TailOutlineThickness = 2
            }
        }
    };
    [JsonPropertyName("debugInfo")]
    public DebugInfoThemeJson DebugInfo { get; set; } = new()
    {
        OverlayBackgroundColor = "#00000080",
        OverlayTextColor = "#FFFFFF",
        OverlayTextSize = 14,
        NoteTimestampTextColor = "#FFFFFF80",
        NoteTimestampTextOutlineColor = "#16161680",
        NoteTimestampTextOutlineThickness = 1,
        NoteTimestampTextSize = 20
    };
}