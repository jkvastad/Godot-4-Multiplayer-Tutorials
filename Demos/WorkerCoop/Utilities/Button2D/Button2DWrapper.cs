using Godot;

public partial class Button2DWrapper : PanelContainer
{
#nullable disable
    [Export]
    public Button2D Button2D { get; set; }
    [Export]
    public Label ButtonText { get; set; }
#nullable enable    
}
