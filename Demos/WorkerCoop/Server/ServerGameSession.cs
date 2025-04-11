using System.Linq;
using Godot;
using Shared;
namespace Server;

/// <summary>
/// The server's version of the game session. 
/// This is the authorative game session which the clients replicate
/// All game state changes are handled via requests to the server
/// </summary>
public partial class ServerGameSession : Node
{
#nullable disable
    [Export]
    GameSessionNetwork GameSessionNetwork { get; set; }
    public static ServerGameSession Singleton { get; private set; }
#nullable enable
    public Model GameState { get; set; } = new();
    public GameSessionState SessionState { get; set; } = new();

    public int TimeoutMS { get; set; }
    public void Setup(int playerSeats, int timeoutMS)
    {
        Singleton = this;

        TimeoutMS = timeoutMS;

        for (var i = 0; i < playerSeats; i++)
        {
            PlayerSeat playerSeat = new PlayerSeat(i);
            for (var j = 0; j < 3; j++) // 3 workers each            
                playerSeat.Workers.Add(new Worker(Worker.GenerateNextWorkerId(), new(i)));

            GameState.PlayerSeats.Add(playerSeat);
        }

        GameSessionNetwork.Setup();
    }

    public override void _Ready()
    {
        Multiplayer.PeerDisconnected += Multiplayer_PeerDisconnected;
        Multiplayer.PeerConnected += Multiplayer_PeerConnected;
    }

    private void Multiplayer_PeerConnected(long id)
    {
        int peerId = (int)id;
        //Default values in enet.h are 32, 5000, 30000, see e.g. ENET_PEER_TIMEOUT_LIMIT in the enum there
        //https://github.com/lsalzman/enet/blob/master/include/enet/enet.h
        if (TimeoutMS > 0)
        {
            ENetPacketPeer newPeer = ((ENetMultiplayerPeer)Multiplayer.MultiplayerPeer).GetPeer(peerId);
            newPeer.SetTimeout(32, TimeoutMS, TimeoutMS);
        }
    }

    private void Multiplayer_PeerDisconnected(long id)
    {        
        int peerId = (int)id;
        if (SessionState.SeatIdToPeerId.Values.Contains(peerId))
        {
            int seatId = SessionState.SeatIdToPeerId.Single(kv => kv.Value == peerId).Key;


            SessionState.PeerIdToPeerColor.Remove(peerId);
            GameSessionNetwork.Rpc(nameof(GameSessionNetwork.ClientUpdatePeerIdToColorMapping),
                peerId, Colors.White, (int)GlobalConstants.UpdateType.Remove); // Dummy color for signature match

            SessionState.PeerIdToPlayerName.Remove(peerId);
            GameSessionNetwork.Rpc(nameof(GameSessionNetwork.ClientUpdatePeerIdToPlayerName),
                peerId, Colors.White, (int)GlobalConstants.UpdateType.Remove); // Dummy color for signature match

            // Remove seat last - else seat to peer mapping becomes unbound causing problems with peer mappings
            SessionState.SeatIdToPeerId.Remove(seatId);
            GameSessionNetwork.Rpc(nameof(GameSessionNetwork.ClientUpdateSeatIdPeerIdMapping),
                seatId, peerId, true);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Multiplayer.PeerDisconnected -= Multiplayer_PeerDisconnected;
    }
}
