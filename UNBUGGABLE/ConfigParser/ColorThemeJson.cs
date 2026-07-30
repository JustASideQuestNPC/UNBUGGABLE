using System.Text.Json.Serialization;

namespace UNBUGGABLE.Resources;

public class MainWindowThemeJson
{
    [JsonRequired][JsonPropertyName("backgroundColor")]
    public string Background { get; set; } = "";
}

public class ElementThemeJson
{
    [JsonRequired][JsonPropertyName("backgroundColor")]
    public string Background { get; set; } = "";
    [JsonRequired][JsonPropertyName("outlineColor")]
    public string Outline { get; set; } = "";
    [JsonRequired][JsonPropertyName("outlineThickness")]
    public double OutlineThickness { get; set; } = 0;
    [JsonRequired][JsonPropertyName("cornerRadius")]
    public double CornerRadius { get; set; } = 0;
}

public class ButtonThemeJson : ElementThemeJson
{
    public class HoveredThemeJson
    {
        [JsonRequired][JsonPropertyName("backgroundColor")]
        public string Background { get; set; } = "";
        [JsonRequired][JsonPropertyName("outlineColor")]
        public string Outline { get; set; } = "";
        [JsonRequired][JsonPropertyName("iconColor")]
        public string IconColor { get; set; } = "";
    }
    
    [JsonRequired][JsonPropertyName("iconColor")]
    public string IconColor { get; set; } = "";
    
    [JsonRequired][JsonPropertyName("hovered")]
    public HoveredThemeJson Hovered { get; set; } = new HoveredThemeJson();
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
    }

    public class TooltipThemeJson : ElementThemeJson
    {
        [JsonRequired][JsonPropertyName("textColor")]
        public string TextColor { get; set; } = "";
        [JsonRequired][JsonPropertyName("textSize")]
        public double TextSize { get; set; } = 0;
    }
    
    [JsonRequired][JsonPropertyName("backgroundColor")]
    public string Background { get; set; } = "";
    
    [JsonRequired][JsonPropertyName("sliders")]
    public SliderThemeJson Sliders { get; set; } = new SliderThemeJson();
    
    [JsonRequired][JsonPropertyName("buttons")]
    public ButtonThemeJson Buttons { get; set; } = new ButtonThemeJson();
    
    [JsonRequired][JsonPropertyName("tooltips")]
    public TooltipThemeJson Tooltips { get; set; } = new TooltipThemeJson();
}

public class ColorThemeJson
{
    
}