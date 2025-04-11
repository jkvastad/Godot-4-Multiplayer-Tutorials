#if TOOLS
using Godot;

[Tool]
public partial class Button2DNode: EditorPlugin
{
    public override void _EnterTree()
    {
        // Initialization of the plugin goes here.
        // Add the new type with a name, a parent type, a script and an icon.
        var script = GD.Load<Script>("res://Utilities/Button2D/Button2DWrapper.cs");
        
        Texture2D texture = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("PanelContainer", "EditorIcons");
        //var texture = GD.Load<Texture2D>("res://addons/MyCustomNode/Icon.png");
        AddCustomType("Button2DWrapper", "Control", script, texture);
    }

    public override void _ExitTree()
    {
        // Clean-up of the plugin goes here.
        // Always remember to remove it from the engine when deactivated.
        RemoveCustomType("Button2DWrapper");
    }
}
#endif