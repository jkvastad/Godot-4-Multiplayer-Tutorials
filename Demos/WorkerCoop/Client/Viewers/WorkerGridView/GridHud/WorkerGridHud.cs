using Client;
using Godot;
using Shared;
using static Shared.GlobalConstants;

public partial class WorkerGridHud : Node
{
#nullable disable
    [Export]
    public Label PlayerName { get; set; }
    [Export]
    public Label SeatId { get; set; }
    [Export]
    public Button2DWrapper LeftSeat { get; set; }
    [Export]
    public Button2DWrapper RightSeat { get; set; }

    private SubscribableValue<int>.Unsubscriber _unsubscriberForCurrentlyViewedSeat;
    private SubscribableDictionary<int, Color?>.Unsubscriber _unsubscriberForSeatLabelColor;
    private SubscribableDictionary<int, Color?>.Unsubscriber _unsubscriberForPlayerNameColor;
    private SubscribableBiDict<int, int>.Unsubscriber _unsubscriberForSeatIdToPeerId;

    private SubscribableValue<int> _currentlyViewedSeat;
#nullable enable    
    public void Setup(string playerName, SubscribableValue<int> seatId)
    {
        PlayerName.Text = playerName;
        SeatId.Text = $"{seatId.Value}";

        _currentlyViewedSeat = seatId;
        _unsubscriberForCurrentlyViewedSeat = seatId.Subscribe(CurrentlyViewedSeatChangeHandler);

        _unsubscriberForSeatIdToPeerId = ClientGameSession.Singleton.SessionState.SeatIdToPeerId.Subscribe(SeatTakenOrReleasedHandler);

        SubscribableDictionary<int, Color?> PeerIdToPeerColor = ClientGameSession.Singleton.SessionState.PeerIdToPeerColor;
        _unsubscriberForPlayerNameColor = PeerIdToPeerColor.Subscribe(PlayerNameColorChangeHandler);
        _unsubscriberForSeatLabelColor = PeerIdToPeerColor.Subscribe(SeatColorChangeHandler);

        // init colors
        if (ClientGameSession.Singleton.SessionState.SeatIdToPeerId.ContainsKey(seatId))
        {
            int peerId = ClientGameSession.Singleton.SessionState.SeatIdToPeerId.GetByKey(seatId);
            if (ClientGameSession.Singleton.SessionState.PeerIdToPeerColor.ContainsKey(peerId))
            {
                Color? colorOrNull = ClientGameSession.Singleton.SessionState.PeerIdToPeerColor[peerId];
                Color color = colorOrNull is null ? Colors.White : colorOrNull.Value;
                UpdateSeatColor(color);
                UpdatePlayerNameColor(color);
            }
            else
            {
                UpdateSeatColor(Colors.White);
                UpdatePlayerNameColor(Colors.White);
            }
        }
        else
        {
            UpdateSeatColor(Colors.White);
            UpdatePlayerNameColor(Colors.White);
        }
    }

    public void PlayerNameColorChangeHandler(int peerId, Color? colorOrNull, UpdateType updateType)
    {
        if (Multiplayer.GetUniqueId() == peerId)
        {
            if (updateType == UpdateType.Add && colorOrNull is not null)
                UpdatePlayerNameColor((Color)colorOrNull);
            else
                UpdatePlayerNameColor(Colors.White);
        }
    }

    public void UpdatePlayerNameColor(Color color)
    {
        PlayerName.AddThemeColorOverride("font_color", color);
    }

    public void SeatColorChangeHandler(int peerId, Color? colorOrNull, UpdateType updateType)
    {
        // if peer with active seat changes color, change seat color
        if (ClientGameSession.Singleton.SessionState.SeatIdToPeerId.ContainsValue(peerId))
        {
            var seatId = ClientGameSession.Singleton.SessionState.SeatIdToPeerId.GetByValue(peerId);
            if (_currentlyViewedSeat.Value == seatId) // has our viewed seat changed?
            {
                if (updateType == UpdateType.Add && colorOrNull is not null)
                    UpdateSeatColor((Color)colorOrNull);
                else
                    UpdateSeatColor(Colors.White);
            }
        }
    }

    public void SeatTakenOrReleasedHandler(int seatId, int peerId, UpdateType updateType)
    {
        if (seatId == _currentlyViewedSeat)
        {
            if (updateType == UpdateType.Add)
            {
                if (ClientGameSession.Singleton.SessionState.PeerIdToPeerColor.ContainsKey(peerId))
                    if (ClientGameSession.Singleton.SessionState.PeerIdToPeerColor[peerId] is not null)
                        UpdateSeatColor((Color)ClientGameSession.Singleton.SessionState.PeerIdToPeerColor[peerId]!);
                    else
                        UpdateSeatColor(Colors.White);
            }
            else
                UpdateSeatColor(Colors.White);
        }
    }

    public void UpdateSeatColor(Color color)
    {
        SeatId.AddThemeColorOverride("font_color", color);
    }

    public void CurrentlyViewedSeatChangeHandler(int seatId)
    {
        SeatId.Text = $"{seatId}";
        if (ClientGameSession.Singleton.SessionState.SeatIdToPeerId.ContainsKey(seatId))
        {
            int peerId = ClientGameSession.Singleton.SessionState.SeatIdToPeerId.GetByKey(seatId);
            if (ClientGameSession.Singleton.SessionState.PeerIdToPeerColor.ContainsKey(peerId))
            {
                Color? colorOrNull = ClientGameSession.Singleton.SessionState.PeerIdToPeerColor[peerId];
                Color color = colorOrNull is null ? Colors.White : colorOrNull.Value;
                UpdateSeatColor(color);
            }
            else
                UpdateSeatColor(Colors.White);
        }
        else
            UpdateSeatColor(Colors.White);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _unsubscriberForCurrentlyViewedSeat.Dispose();
        _unsubscriberForPlayerNameColor.Dispose();
        _unsubscriberForSeatLabelColor.Dispose();
        _unsubscriberForSeatIdToPeerId.Dispose();
    }
}
