using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Shared;
namespace Client;

public partial class WorkerGridViewer : Node
{
    private ClientGameSession _cgs = ClientGameSession.Singleton;

    private WorkerGridView? _currentWorkerGridView;
    private WorkerGridHud? _gridHud;

    public SubscribableValue<int> CurrentlyViewedSeat = new(-1); // set in setup
    public static Color DEFAULT_COLOR = Colors.White;

    public void Setup(int seatId)
    {
        CurrentlyViewedSeat = new(seatId);
    }
    public override void _Ready()
    {
        AddNewWorkerGridView();
        AddGridHud();
    }

    private void AddGridHud()
    {
        string playerName = _cgs.SessionState.PeerIdToPlayerName[Multiplayer.GetUniqueId()];
        _gridHud = GD.Load<PackedScene>(GlobalConstants.WORKER_GRID_HUD_PATH).Instantiate<WorkerGridHud>();
        _gridHud.Setup(playerName, CurrentlyViewedSeat);

        _gridHud.LeftSeat.Button2D.Pressed += LeftSeatButton_Pressed;
        _gridHud.RightSeat.Button2D.Pressed += RightSeatButton_Pressed;
        _cgs.InputPolling += GridViewerController;

        CallDeferred(MethodName.AddChild, _gridHud);
    }

    private void AddNewWorkerGridView()
    {
        _currentWorkerGridView = GD.Load<PackedScene>(GlobalConstants.WORKER_GRID_VIEW_PATH).Instantiate<WorkerGridView>();
        _currentWorkerGridView.Setup(_cgs.GameState.PlayerSeats[CurrentlyViewedSeat].WorkerGrid, CurrentlyViewedSeat);

        CallDeferred(MethodName.AddChild, _currentWorkerGridView);
    }
    public void ChangeViewToSeatId(int seatId)
    {
        CurrentlyViewedSeat.Value = seatId;
        _currentWorkerGridView?.QueueFree();
        AddNewWorkerGridView();
    }

    private void LeftSeatButton_Pressed()
    {
        int totalSeats = ClientGameSession.Singleton.GameState.PlayerSeats.Count;
        int newSeatId = (CurrentlyViewedSeat - 1 + totalSeats) % totalSeats; //negative values are still negative
        ChangeViewToSeatId(newSeatId);
    }
    private void RightSeatButton_Pressed()
    {
        int totalSeats = ClientGameSession.Singleton.GameState.PlayerSeats.Count;
        int newSeatId = (CurrentlyViewedSeat + 1) % totalSeats;
        ChangeViewToSeatId(newSeatId);
    }

    public void GridViewerController(double delta)
    {
        if (Input.IsActionJustPressed(InputBindings.ESCAPE_BUTTON))
        {
            SessionMenu sessionMenu = GD.Load<PackedScene>(GlobalConstants.SESSION_MENU_PATH).Instantiate<SessionMenu>();
            sessionMenu.Setup(this, new Action<double>(GridViewerController));
            AddChild(sessionMenu, true);
            return;
        }
        if(Input.IsActionJustPressed(InputBindings.LEFT_MOUSE_BUTTON))
        {
            // 2D Raycast
            Vector2 clickedPoint = _cgs.GetViewport().GetMousePosition();
            var result2D = RaycastUtilities.PerformIntersectPoint2D(clickedPoint).SortedIntersectPoints();

            if (result2D.Count > 0)
            {
                //Try for click on button
                if (result2D[0]["collider"].Obj is Button2D button2D)
                {
                    button2D.Press();
                    return;
                }
            }

            // 3D Raycast            

            //Look for worker or grid box

            var result3D = RaycastUtilities.PerformIntersectRay3D(clickedPoint);

            if (result3D.Count > 1 && result3D["collider"].Obj is GridWorkerView workerView)
            {
                //unassign if owned by client
                Worker worker = workerView.Worker;
                int? workerSeatIdOrNull = worker.SeatOwnerId;
                if (workerSeatIdOrNull is not null)
                {
                    int workerSeatId = (int)workerSeatIdOrNull;
                    if (_cgs.SessionState.SeatIdToPeerId.ContainsKey(workerSeatId)
                        && Multiplayer.GetUniqueId() == _cgs.SessionState.SeatIdToPeerId.GetByKey(workerSeatId))
                    {
                        _cgs.GameSessionNetwork.RpcId(1, nameof(GameSessionNetwork.ServerRequestRemoveWorkerFromWorkerGrid),
                            CurrentlyViewedSeat.Value,
                            workerSeatId,
                            worker.Id);
                    }
                }
                return;
            }

            if (result3D.Count > 1 && result3D["collider"].Obj is GridBoxView gridBox)
            {
                //assign to worker grid owning the box
                WorkerGridView workerGridView = gridBox.WorkerGridView;
                Vector2I gridPosition = workerGridView.GetBoxPosition(gridBox.Id);
                _cgs.GameSessionNetwork.RpcId(1, nameof(GameSessionNetwork.ServerRequestAddWorkerToWorkerGrid), CurrentlyViewedSeat.Value, gridPosition);
                return;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _cgs.InputPolling -= GridViewerController;
    }
}