using Godot;
namespace Client;

/// <summary>
/// The client lobby, used to start a client connecting to a server at a port and ip 
/// </summary>
public partial class ClientLobby : Control
{    
#nullable disable
    [Export]
    public LineEdit IpInput { get; set; }
    [Export]
    public LineEdit PortInput { get; set; }
    [Export]
    public Button JoinGame { get; set; }
    [Export]
    public LineEdit PlayerName { get; set; }
    [Export]
    public LobbyNetwork LobbyNetwork { get; set; }
#nullable enable    

    public int TimeoutMS { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        JoinGame.Pressed += JoinServer;
        Multiplayer.ConnectedToServer += ClientStartGameSession;
    }

    private void JoinServer()
    {
        var peer = new ENetMultiplayerPeer();
        Multiplayer.ConnectedToServer += Multiplayer_ConnectedToServer;
        Error error = peer.CreateClient(IpInput.Text, PortInput.Text.ToInt());

        Multiplayer.MultiplayerPeer = peer;
    }

    private void Multiplayer_ConnectedToServer()
    {
        var serverPeer = ((ENetMultiplayerPeer)Multiplayer.MultiplayerPeer).GetPeer(1);
        //Default values in enet.h are 32, 5000, 30000
        if (TimeoutMS > 0)
            serverPeer.SetTimeout(32, TimeoutMS, TimeoutMS);
    }

    public void ClientStartGameSession()
    {
        ClientGameSession clientGameSession = GD.Load<PackedScene>("res://Client/GameSession/client_game_session.tscn").Instantiate<ClientGameSession>();
        clientGameSession.Setup(TimeoutMS);
        clientGameSession.SessionState.PeerIdToPlayerName[Multiplayer.GetUniqueId()] = PlayerName.Text;
        Main.Singleton.AddChild(clientGameSession);

        // Client lobby can be removed once connected
        QueueFree();
    }
}
