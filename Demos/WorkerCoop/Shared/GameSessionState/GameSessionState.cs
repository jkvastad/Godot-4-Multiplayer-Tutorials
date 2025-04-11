using Godot;
namespace Shared;

public class GameSessionState
{
    public SubscribableDictionary<int, string> PeerIdToPlayerName { get; set; } = []; //peerId key, player name value        
    public SubscribableDictionary<int, Color?> PeerIdToPeerColor { get; set; } = []; //peerId key, player color value        
    public SubscribableBiDict<int, int> SeatIdToPeerId { get; set; } = []; //player seat id key, peer id value - null for unassigned    
}

