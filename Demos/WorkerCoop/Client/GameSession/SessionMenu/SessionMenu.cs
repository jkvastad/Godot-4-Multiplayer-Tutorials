using System;
using Client;
using Godot;
using Shared;

/// <summary>
/// Put as child to a viewer (the previous scene) to add a menu overlay, temporarily overriding the previous view's control.
/// </summary>
public partial class SessionMenu : Node
{
#nullable disable
    [Export]
    Button ReturnToGame { get; set; }
    [Export]
    Button ToSeatSelect { get; set; }
    [Export]
    Button DisconnectClient { get; set; }
    [Export]
    Button ExitToDesktop { get; set; }
    Node PreviousScene { get; set; }
    Action<double> PreviousControl { get; set; }
#nullable enable

    public void Setup(Node previousScene, Action<double> previousControl)
    {
        PreviousScene = previousScene;
        PreviousControl = previousControl;
    }

    public override void _Ready()
    {
        ClientGameSession.Singleton.InputPolling -= PreviousControl;
        ClientGameSession.Singleton.InputPolling += SessionMenuController;

        ReturnToGame.Pressed += ReturnToGame_Pressed;
        ToSeatSelect.Pressed += ToSeatSelect_Pressed;
        DisconnectClient.Pressed += DisconnectClient_Pressed;
        ExitToDesktop.Pressed += ExitToDesktop_Pressed;
        // bind buttons
        // escape button triggers return to game button
    }
    private void ReturnToGame_Pressed()
    {
        ClientGameSession.Singleton.InputPolling += PreviousControl;
        QueueFree();
    }
    private void ToSeatSelect_Pressed()
    {
        PreviousScene.QueueFree(); //also removes session menu, which should be child to the previous scene

        PlayerSeatView playerSeatView = GD.Load<PackedScene>(GlobalConstants.PLAYER_SEAT_VIEW_PATH).Instantiate<PlayerSeatView>();
        ClientGameSession.Singleton.AddChild(playerSeatView);
    }
    private void DisconnectClient_Pressed()
    {
        Multiplayer.MultiplayerPeer.Close();
        ClientGameSession.Singleton.QueueFree();
        Main.Singleton.CallDeferred(nameof(Main.StartClient));
    }

    private void ExitToDesktop_Pressed()
    {
        GetTree().Quit();
    }

    public void SessionMenuController(double delta)
    {
        if (Input.IsActionJustPressed(InputBindings.ESCAPE_BUTTON))
        {
            ReturnToGame_Pressed();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ClientGameSession.Singleton.InputPolling -= SessionMenuController;
    }
}