using Client;
using Server;
using Godot;
using Shared;
using System.Text.Json;
using static Shared.GlobalConstants;
namespace Shared;

/// <summary>
/// The node handling all networking relating to a game session.
/// To avoid one huge file, the class is split into different files depending on the usage e.g. GameSessionNetworkGameState for game state changes
/// </summary>
public partial class GameSessionNetwork : Node
{
    JsonSerializerOptions serializeOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true // needed for public fields, particularly godot structs e.g. Vector2 - these will be 0 otherwise
    };

    public void Setup()
    {
        serializeOptions.Converters.Add(new ObservableDictionaryConverterFactory());
        serializeOptions.Converters.Add(new SubscribableBiDictConverterFactory());
        serializeOptions.Converters.Add(new SubscribableValueConverterFactory());
    }

    #region Server-side RPCs    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ServerInitGameSessionForCallingPeer(string playerName)
    {
        int peerId = Multiplayer.GetRemoteSenderId();

        // Update server model
        ServerGameSession.Singleton.SessionState.PeerIdToPlayerName[peerId] = playerName;
        Rpc(nameof(ClientUpdatePeerIdToPlayerName), peerId, playerName, (int)UpdateType.Add);

        // Replicate server model
        string gameStateJson = JsonSerializer.Serialize(ServerGameSession.Singleton.GameState, serializeOptions);
        string sessionStateJson = JsonSerializer.Serialize(ServerGameSession.Singleton.SessionState, serializeOptions);
        RpcId(peerId, nameof(ClientInitGameSession), gameStateJson, sessionStateJson);
    }
    #endregion

    #region Client-side RPCs
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ClientInitGameSession(string modelJson, string sessionStateJson)
    {
        ClientGameSession gameSession = ClientGameSession.Singleton;

        //TODO: Needs error check? Now null-forgiving
        gameSession.GameState = JsonSerializer.Deserialize<Model>(modelJson, serializeOptions)!;
        gameSession.SessionState = JsonSerializer.Deserialize<GameSessionState>(sessionStateJson, serializeOptions)!;

        //Choose seat
        var playerSeatView = GD.Load<PackedScene>(PLAYER_SEAT_VIEW_PATH).Instantiate<PlayerSeatView>();
        gameSession.AddChild(playerSeatView);
    }
    #endregion
}