using Godot;

public partial class Lobby : Control
{
    int DEFAULT_PORT = 8910;
    [Export]
    Button _hostButton;
    [Export]
    Button _joinButton;
    [Export]
    LineEdit _IPAddressInput;

    ENetMultiplayerPeer peer = new();

    [Signal]
    public delegate void LobbyDoneEventHandler(Lobby lobby);

    public override void _Ready()
    {
        _hostButton.Pressed += HostGame;
        _joinButton.Pressed += JoinGame;
    }

    private void HostGame()
    {
        peer.CreateServer(DEFAULT_PORT);
        Multiplayer.MultiplayerPeer = peer;
        DisableButtons();
    }

    private void JoinGame()
    {
        string IPAddress = _IPAddressInput.Text;
        if (IPAddress.IsValidIPAddress())
            peer.CreateClient(IPAddress, DEFAULT_PORT);
        else
            return;
        Multiplayer.MultiplayerPeer = peer;
        DisableButtons();
    }

    private void DisableButtons()
    {
        _hostButton.Disabled = true;
        _joinButton.Disabled = true;
        EmitSignal(SignalName.LobbyDone, this);
    }
}
