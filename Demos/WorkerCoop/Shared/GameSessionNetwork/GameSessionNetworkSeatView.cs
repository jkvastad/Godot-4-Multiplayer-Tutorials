using Server;
using Godot;
using static Shared.GlobalConstants;
namespace Shared;

public partial class GameSessionNetwork : Node
{
    // PlayerSeatView related networking
    #region Server-side RPCs        
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ServerRequestSeatForCallingPeer(int seatId)
    {        
        // peer can only have one seat

        int remotePeerId = Multiplayer.GetRemoteSenderId();
        SubscribableBiDict<int, int> seatIdToPeerId = ServerGameSession.Singleton.SessionState.SeatIdToPeerId;

        // Requester has seat? 
        if (seatIdToPeerId.ContainsValue(remotePeerId))
        {
            //Seat taken by self? - release seat
            if (seatIdToPeerId.ContainsKey(seatId)
                && seatIdToPeerId.GetByKey(seatId) == remotePeerId)
            {
                seatIdToPeerId.Remove(seatId);
                Rpc(nameof(ClientUpdateSeatIdPeerIdMapping), seatId, remotePeerId, true);
            }
            // Requester has other seat - illegal request, do nothing
            return;
        }

        // Requester has no seat

        // Seat taken by other? - illegal request, do nothing
        if (seatIdToPeerId.ContainsKey(seatId))
            return;

        // Seat free - take it
        seatIdToPeerId.Add(seatId, remotePeerId);
        Rpc(nameof(ClientUpdateSeatIdPeerIdMapping), seatId, remotePeerId, false);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ServerRequestNewColorForCallingPeer(Color color)
    {
        int peerId = Multiplayer.GetRemoteSenderId();
        ServerGameSession.Singleton.SessionState.PeerIdToPeerColor[peerId] = color;
        Rpc(nameof(ClientUpdatePeerIdToColorMapping), peerId, color, (int)UpdateType.Add);
    }

    #endregion

    #region Client-side RPCs
    #endregion
}

