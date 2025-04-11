using System;
using System.Threading;
using Godot;
using Shared;
namespace Client;

/// <summary>
/// The client's version of the game session
/// Game state changes are sent as requests to the server via the game sessions network 
/// (see e.g. usage of the InputPolling event and the chain from input -> ray trace -> rpc request)
/// Client listens to incoming updates from the server. Updates trigger changes in views (see e.g. usage og Subscribable classes)
/// </summary>
public partial class ClientGameSession : Node
{
#nullable disable
    [Export]
    public GameSessionNetwork GameSessionNetwork { get; set; }
    [Export]
    public Camera3D Camera3D { get; set; }
    public Model GameState { get; set; }
    public static ClientGameSession Singleton { get; private set; }
#nullable enable

    public int TimeoutMS { get; set; }
    public GameSessionState SessionState { get; set; } = new();

    public event Action<double>? InputPolling;

    public void Setup(int timeoutMS)
    {
        TimeoutMS = timeoutMS;
    }

    public override void _Ready()
    {
        Singleton = this;        

        GameSessionNetwork.Setup();

        GameSessionNetwork.RpcId(1, nameof(GameSessionNetwork.ServerInitGameSessionForCallingPeer), SessionState.PeerIdToPlayerName[Multiplayer.GetUniqueId()]);
    }
   
    public override void _PhysicsProcess(double delta)
    {
        InputPolling?.Invoke(delta);
    }
}