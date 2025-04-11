using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Shared;
using static Shared.GlobalConstants;
namespace Client;

public partial class PlayerSeatView : Node
{
    private ClientGameSession _cgs = ClientGameSession.Singleton;
#nullable disable
    [Export]
    VBoxContainer AvailableSeats { get; set; }    
    [Export]
    Button ToGameButton { get; set; }
    [Export]
    ColorPicker ColorPicker { get; set; }
#nullable enable    
    private IDisposable? _unsubscriberSeatIdToPeerId;
    private List<IDisposable> _unsubscribersToPeerIdToPeerColor = new();
    private List<SubscribableDictionary<int, string>.Unsubscriber> _unsubscribersToPeerIdToPlayerName = new();
    Dictionary<int, Button> _seatIdToSeatButton = new();
    public static Color DEFAULT_COLOR = new(0.6f, 0.6f, 0.6f);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ColorPicker.ColorChanged += ColorPicker_ColorChanged;
        ToGameButton.Pressed += ToGameButton_Pressed;
        // init seat buttons
        foreach (var playerSeat in _cgs.GameState.PlayerSeats)
        {
            // Create Button
            Button seatButton = new Button();
            StyleBoxFlat buttonStyleNormal = new StyleBoxFlat();
            StyleBoxFlat buttonStyleHover = new StyleBoxFlat();
            StyleBoxFlat buttonStylePressed = new StyleBoxFlat();
            StyleBoxFlat buttonStyleDisabled = new StyleBoxFlat();
            seatButton.AddThemeStyleboxOverride("normal", buttonStyleNormal);
            seatButton.AddThemeStyleboxOverride("hover", buttonStyleHover);
            seatButton.AddThemeStyleboxOverride("pressed", buttonStylePressed);
            seatButton.AddThemeStyleboxOverride("disabled", buttonStyleDisabled);

            // Register button
            _seatIdToSeatButton[playerSeat.SeatId] = seatButton;

            seatButton.Pressed += () => SeatButtonPressedHandler(playerSeat.SeatId);

            var unsubscriber = _cgs.SessionState.PeerIdToPeerColor.Subscribe((peerId, color, isRemoved) =>
                {
                    if (_cgs.SessionState.SeatIdToPeerId.ContainsValue(peerId))
                    {
                        int seatId = _cgs.SessionState.SeatIdToPeerId.GetByValue(peerId);
                        SetSeatButtonColor(_seatIdToSeatButton[seatId], color);
                    }
                }
            );
            _unsubscribersToPeerIdToPeerColor.Add(unsubscriber);

            // Init seat button
            // Seat taken?
            if (_cgs.SessionState.SeatIdToPeerId.ContainsKey(playerSeat.SeatId))
            {
                int peerId = _cgs.SessionState.SeatIdToPeerId.GetByKey(playerSeat.SeatId);
                SeatIdToPeerIdUpdateHandler(playerSeat.SeatId, peerId, UpdateType.Add); // adding seat same as taking seat
            }
            else // Seat free
            {
                SeatIdToPeerIdUpdateHandler(playerSeat.SeatId, 0, UpdateType.Remove); // superflous peerId
            }

            AvailableSeats.AddChild(seatButton);
        }
        // subscribe to updates
        _unsubscriberSeatIdToPeerId = _cgs.SessionState.SeatIdToPeerId.Subscribe(SeatIdToPeerIdUpdateHandler);
    }

    private void ToGameButton_Pressed()
    {
        // Change scene to start game when seat selected
        WorkerGridViewer workerGridViewer = new();

        int currentSeatId = _cgs.SessionState.SeatIdToPeerId.Single(kv => kv.Value == Multiplayer.GetUniqueId()).Key;
        workerGridViewer.Setup(currentSeatId);

        ClientGameSession.Singleton.AddChild(workerGridViewer);
        QueueFree();
    }

    private void SetSeatButtonColor(Button button, Color? colorMaybe)
    {
        Color color = colorMaybe is not null ? (Color)colorMaybe : DEFAULT_COLOR;

        ((StyleBoxFlat)button.GetThemeStylebox("normal")).BgColor = color;
        ((StyleBoxFlat)button.GetThemeStylebox("hover")).BgColor = color;
        ((StyleBoxFlat)button.GetThemeStylebox("pressed")).BgColor = color;
        ((StyleBoxFlat)button.GetThemeStylebox("disabled")).BgColor = color;
    }

    private void ColorPicker_ColorChanged(Color color)
    {
        _cgs.GameSessionNetwork.RpcId(1, nameof(GameSessionNetwork.ServerRequestNewColorForCallingPeer), color);
    }

    private void SeatButtonPressedHandler(int seatId)
    {
        _cgs.GameSessionNetwork.RpcId(1, nameof(GameSessionNetwork.ServerRequestSeatForCallingPeer), seatId);
    }

    private void SeatIdToPeerIdUpdateHandler(int seatId, int peerId, UpdateType updateType)
    {
        // TODO - cannot select other seat after unselecting own seat
        Button seatButton = _seatIdToSeatButton[seatId];
        if (updateType == UpdateType.Remove) // seat is free
        {
            seatButton.Text = $"Seat {seatId}";
            SetSeatButtonColor(seatButton, DEFAULT_COLOR);

            int selfPeerId = Multiplayer.GetUniqueId();
            if (selfPeerId == peerId) // freeing own seat
            {
                // cannot start game if unselecting own seat
                ToGameButton.Disabled = true;
                // Enable other unselected seats
                for (int id = 0; id < _cgs.GameState.PlayerSeats.Count; id++)
                    if (!_cgs.SessionState.SeatIdToPeerId.ContainsKey(id))
                        _seatIdToSeatButton[id].Disabled = false;
            }
            else // freeing other seat
            {
                // self has no seat 
                if (!_cgs.SessionState.SeatIdToPeerId.ContainsValue(selfPeerId))
                    seatButton.Disabled = false;
                // else self has seat - do nothing
            }
        }
        else
        {
            if (_cgs.SessionState.PeerIdToPlayerName.ContainsKey(peerId))
                seatButton.Text = _cgs.SessionState.PeerIdToPlayerName[peerId];
            if (_cgs.SessionState.PeerIdToPeerColor.ContainsKey(peerId))
                SetSeatButtonColor(seatButton, _cgs.SessionState.PeerIdToPeerColor[peerId]);

            if (Multiplayer.GetUniqueId() == peerId)
            {
                // Can start game when seat selected
                ToGameButton.Disabled = false;
                // disable other seat buttons
                foreach (var button in _seatIdToSeatButton.Values)
                    button.Disabled = true;
                // enable own button for releasing seat
                seatButton.Disabled = false;
            }
            else
            {
                // Cannot free other seats
                seatButton.Disabled = true;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _unsubscriberSeatIdToPeerId?.Dispose();
        foreach (var unsubscriber in _unsubscribersToPeerIdToPeerColor)
            unsubscriber?.Dispose();
    }
}