using Client;
using Godot;
using Shared;
using WorkerCoop.Utilities;
using static Shared.GlobalConstants;

public partial class GridBoxView : Area3D, IHasId
{
    GameSessionState _cgs = ClientGameSession.Singleton.SessionState;

    public static Color DEFAULT_COLOR = Colors.White;
#nullable disable
    [Export]
    private MeshInstance3D MeshInstance3D { get; set; }
    public WorkerGridView WorkerGridView { get; internal set; }

    public int Id { get; private set; }

    private SubscribableDictionary<int, Color?>.Unsubscriber _colorUnsubscriber;
    private SubscribableBiDict<int, int>.Unsubscriber _seatToPeerUnsubscriber;
#nullable enable
    private int _boxSeatId;
    private Color? _boxColor = DEFAULT_COLOR;


    public void Setup(Vector3 position, int seatId, int boxId, WorkerGridView workerGridView)
    {
        _boxSeatId = seatId;
        Position = position;
        Id = boxId;
        WorkerGridView = workerGridView;

        _colorUnsubscriber = _cgs.PeerIdToPeerColor.Subscribe(PeerIdToPeerColorHandler);
        _seatToPeerUnsubscriber = _cgs.SeatIdToPeerId.Subscribe(SeatIdToPeerIdHandler);

        // default or peerId color?
        if (_cgs.SeatIdToPeerId.ContainsKey(seatId)) //the seat is taken
        {
            int peerId = _cgs.SeatIdToPeerId.GetByKey(seatId)!;
            if (_cgs.PeerIdToPeerColor.ContainsKey(peerId)) // a color is chosen
                _boxColor = _cgs.PeerIdToPeerColor[peerId];
        }
        UpdateBoxColor();
    }

    private void SeatIdToPeerIdHandler(int seatId, int peerId, UpdateType updateType)
    {
        // if our seat...
        if (seatId == _boxSeatId)
        {
            if (updateType == UpdateType.Remove) // ... is released - do default color
            {
                _boxColor = DEFAULT_COLOR;
                UpdateBoxColor();
            }
            else // ... is taken - try peer color
            {
                if (_cgs.PeerIdToPeerColor.ContainsKey(peerId))
                    PeerIdToPeerColorHandler(peerId, _cgs.PeerIdToPeerColor[peerId], UpdateType.Add);
                else
                    PeerIdToPeerColorHandler(peerId, null, UpdateType.Add);
            }
        }
    }

    private void PeerIdToPeerColorHandler(int peerId, Color? newColor, UpdateType updateType)
    {
        // Check if our seat color is updated        
        if (_cgs.SeatIdToPeerId.ContainsValue(peerId)
            && _cgs.SeatIdToPeerId.GetByValue(peerId) == _boxSeatId)
        {
            if (updateType == UpdateType.Remove || newColor is null)
                _boxColor = DEFAULT_COLOR;
            else
                _boxColor = newColor;

            UpdateBoxColor();
        }
    }

    private void UpdateBoxColor()
    {
        ShaderMaterial shaderMaterial = (ShaderMaterial)MeshInstance3D.Mesh.SurfaceGetMaterial(0);
        if (_boxColor is not null)
            shaderMaterial.SetShaderParameter("cube_color", (Color)_boxColor);
        else
            shaderMaterial.SetShaderParameter("cube_color", DEFAULT_COLOR);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _colorUnsubscriber?.Dispose();
        _seatToPeerUnsubscriber?.Dispose();
    }
}