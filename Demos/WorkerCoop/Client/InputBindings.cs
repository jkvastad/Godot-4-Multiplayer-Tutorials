using Godot;
/// <summary>
/// Input bindings for the game. See e.g. GridViewerController in WorkerGridViewer for usage
/// </summary>
public class InputBindings
{
    public const string LEFT_MOUSE_BUTTON = "left_mouse_pressed";
    public const string ESCAPE_BUTTON = "escape_button_pressed";

    static InputBindings()
    {
        InputMap.AddAction(LEFT_MOUSE_BUTTON);
        InputMap.ActionAddEvent(LEFT_MOUSE_BUTTON, new InputEventMouseButton() { ButtonIndex = MouseButton.Left });

        InputMap.AddAction(ESCAPE_BUTTON);
        InputMap.ActionAddEvent(ESCAPE_BUTTON, new InputEventKey() { Keycode = Key.Escape });
    }
}