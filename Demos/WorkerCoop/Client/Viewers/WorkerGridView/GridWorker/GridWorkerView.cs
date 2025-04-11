
using System;
using Client;
using Godot;
using Shared;
using static Shared.GlobalConstants;

public partial class GridWorkerView : Area3D
{
#nullable disable
    [Export]
    private MeshInstance3D MeshInstance3D { get; set; }
    public float WorkerHeight { get; private set; }
    public Worker Worker { get; set; }
    private SubscribableValue<int?>.Unsubscriber _workerSeatIdUnsubscriber;
    SubscribableDictionary<int, Color?>.Unsubscriber _unsubscriberForPeerIdToPeerColor;
    private SubscribableBiDict<int, int>.Unsubscriber _seatToPeerUnsubscriber;
#nullable enable
    private Tween? Tween { get; set; }
    private double tweenSpeed = 0.25;
    public static Color DEFAULT_COLOR = Colors.White;
    public void Setup(Worker worker)
    {
        Worker = worker;

        _workerSeatIdUnsubscriber = Worker.SeatOwnerId.Subscribe(SeatOwnerIdHandler);
        _unsubscriberForPeerIdToPeerColor = ClientGameSession.Singleton.SessionState.PeerIdToPeerColor.Subscribe(PeerIdToPeerColorHandler);
        _seatToPeerUnsubscriber = ClientGameSession.Singleton.SessionState.SeatIdToPeerId.Subscribe(SeatIdToPeerIdHandler);

        WorkerHeight = ((CylinderMesh)MeshInstance3D.Mesh).Height;
    }



    public override void _Ready()
    {
        SeatOwnerIdHandler(Worker.SeatOwnerId);
    }
    private void SeatIdToPeerIdHandler(int seatId, int peerId, UpdateType updateType)
    {
        // seat is our seat?
        if (Worker.SeatOwnerId == seatId)
        {
            // add/remove?
            if (updateType == UpdateType.Add)
            {
                // peer has color?
                if (ClientGameSession.Singleton.SessionState.PeerIdToPeerColor.ContainsKey(peerId))
                    UpdateWorkerColor(ClientGameSession.Singleton.SessionState.PeerIdToPeerColor[peerId]);
            }
            else if (updateType == UpdateType.Remove) // no seat no peer no color
                UpdateWorkerColor(DEFAULT_COLOR);
        }
    }

    public void PeerIdToPeerColorHandler(int peerId, Color? newColor, UpdateType updateType)
    {
        // Worker has seat?
        int? workerSeatIdOrNull = Worker.SeatOwnerId;
        if (workerSeatIdOrNull is not null)
        {
            // Seat has peer?
            int workerSeatId = (int)workerSeatIdOrNull;
            if (ClientGameSession.Singleton.SessionState.SeatIdToPeerId.ContainsKey(workerSeatId))
            {
                // Worker peer is updated peer?
                int seatPeerId = ClientGameSession.Singleton.SessionState.SeatIdToPeerId.GetByKey(workerSeatId);
                if (seatPeerId == peerId)
                {
                    // adding/updating or removing peer to color mapping?
                    if (updateType == UpdateType.Add)
                        UpdateWorkerColor(newColor);
                    else if (updateType == UpdateType.Remove)
                        UpdateWorkerColor(DEFAULT_COLOR);
                }
            }
        }
    }
    public void SeatOwnerIdHandler(int? workerSeatId)
    {
        // Worker view has seat and seat has peer?
        if (workerSeatId is not null && ClientGameSession.Singleton.SessionState.SeatIdToPeerId.ContainsKey((int)workerSeatId))
        {
            int peerId = ClientGameSession.Singleton.SessionState.SeatIdToPeerId.GetByKey((int)workerSeatId);
            UpdateWorkerColor(ClientGameSession.Singleton.SessionState.PeerIdToPeerColor[peerId]);
        }
        else
            UpdateWorkerColor(WorkerGridViewer.DEFAULT_COLOR);
    }

    public void UpdateWorkerColor(Color? colorOrNull)
    {
        Color color = colorOrNull is null ? DEFAULT_COLOR : (Color)colorOrNull;
        var materialCopy = (StandardMaterial3D)MeshInstance3D.Mesh.SurfaceGetMaterial(0).Duplicate();
        materialCopy.AlbedoColor = color;
        // Override material to set unique value per instance instead of all grid workers sharing one color
        MeshInstance3D.MaterialOverride = materialCopy;
    }

    public void SlideTo(Vector3 workerPosition)
    {
        Tween?.Kill(); // Stop current tweening
        Tween = GetTree().CreateTween();
        Tween.TweenProperty(this, new NodePath(PropertyName.Position), workerPosition, tweenSpeed);
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _workerSeatIdUnsubscriber?.Dispose();
        _unsubscriberForPeerIdToPeerColor?.Dispose();
        _seatToPeerUnsubscriber?.Dispose();
    }
}