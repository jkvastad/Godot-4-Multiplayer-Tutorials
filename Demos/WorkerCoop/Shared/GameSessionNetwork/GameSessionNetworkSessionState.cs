using Client;
using Godot;
using static Shared.GlobalConstants;
namespace Shared;

public partial class GameSessionNetwork : Node
{
    #region Client-side RPCs    
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ClientUpdateSeatIdPeerIdMapping(int seatId, int peerId, bool isRemoved)
    {
        if (isRemoved)
            ClientGameSession.Singleton.SessionState.SeatIdToPeerId.Remove(seatId);
        else
            ClientGameSession.Singleton.SessionState.SeatIdToPeerId.Add(seatId, peerId);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ClientUpdatePeerIdToPlayerName(int peerId, string playerName, int updateTypeAsInt)
    {
        UpdateType updateType = (UpdateType)updateTypeAsInt;
        if (updateType == UpdateType.Remove)
            ClientGameSession.Singleton.SessionState.PeerIdToPlayerName.Remove(peerId);
        else if (updateType == UpdateType.Add)
            ClientGameSession.Singleton.SessionState.PeerIdToPlayerName[peerId] = playerName;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ClientUpdatePeerIdToColorMapping(int peerId, Color color, int updateTypeAsInt)
    {
        UpdateType updateType = (UpdateType)updateTypeAsInt;
        if (updateType == UpdateType.Remove)
        {
            ClientGameSession.Singleton.SessionState.PeerIdToPeerColor.Remove(peerId);
        }
        else if (updateType == UpdateType.Add)
            ClientGameSession.Singleton.SessionState.PeerIdToPeerColor[peerId] = color;
    }

    #endregion
}
