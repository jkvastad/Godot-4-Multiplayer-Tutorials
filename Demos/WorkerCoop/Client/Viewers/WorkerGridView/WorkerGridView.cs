using System;
using System.Collections.Generic;
using Godot;
using Shared;
using static Shared.GlobalConstants;

namespace Client;

public partial class WorkerGridView : Node
{
    private WorkerGrid _workerGrid = null!;
    private Dictionary<int, Vector2I> _gridBoxViewIdToGridPosition = [];
    private Dictionary<int, Vector2I> _gridWorkerIdToGridPosition = [];
    private Dictionary<int, GridWorkerView> _gridWorkerIdToWorkerView = [];
    private int _seatId;
    private WorkerGrid.Unsubscriber _workerGridUnSubscriber = null!;
    private static float _gridBoxHeight = 1f;

    public void Setup(WorkerGrid workerGrid, int seatId)
    {
        _workerGrid = workerGrid;
        _seatId = seatId;
    }
    public override void _Ready()
    {
        InitGrid();
        _workerGridUnSubscriber = _workerGrid.Subscribe(WorkerGridUpdateHandler);
    }

    private void WorkerGridUpdateHandler(Worker worker, Vector2I gridPosition, UpdateType updateType)
    {
        if (updateType == UpdateType.Remove)
            RemoveGridWorkerView(worker);
        if (updateType == UpdateType.Add)
        {
            int stackPosition = _workerGrid.GetWorkers(gridPosition).Count - 1;
            AddGridWorkerView(worker, gridPosition, stackPosition);
        }
    }

    // Draw grid from nothing
    public void InitGrid()
    {
        int boxId = 0;
        for (int x = 0; x < _workerGrid.Width; x++)
        {
            for (int z = 0; z < _workerGrid.Height; z++)
            {
                GridBoxView gridBox = GD.Load<PackedScene>(GRID_BOX_VIEW_PATH).Instantiate<GridBoxView>();
                _gridBoxViewIdToGridPosition[boxId] = new(x, z);
                gridBox.Setup(new(x, 0, z), _seatId, boxId, this);
                AddChild(gridBox, true);
                boxId++;

                var workers = _workerGrid.GetWorkers(new(x, z));
                for (int stackPosition = 0; stackPosition < workers.Count; stackPosition++)
                {
                    AddGridWorkerView(workers[stackPosition], new(x, z), stackPosition);
                }
            }
        }
    }

    private void AddGridWorkerView(Worker worker, Vector2I gridPosition, int stackPosition)
    {
        GridWorkerView gridWorkerView = GD.Load<PackedScene>(GRID_WORKER_VIEW_PATH).Instantiate<GridWorkerView>();
        gridWorkerView.Setup(worker);
        _gridWorkerIdToWorkerView[worker.Id] = gridWorkerView;
        _gridWorkerIdToGridPosition[worker.Id] = gridPosition;
        Vector3 workerViewPosition = GetWorker3DStackPosition(gridPosition, stackPosition, gridWorkerView.WorkerHeight);
        gridWorkerView.Position = workerViewPosition;
        AddChild(gridWorkerView, true);
    }

    private static Vector3 GetWorker3DStackPosition(Vector2I gridPosition, int stackPosition, float gridWorkerViewHeight)
    {
        Vector3 workerViewPosition = new(
                    gridPosition.X,
                    gridWorkerViewHeight * stackPosition + _gridBoxHeight / 2.0f + gridWorkerViewHeight / 2.0f, // All meshes centered on mid
                    gridPosition.Y);
        return workerViewPosition;
    }

    public void RemoveGridWorkerView(Worker worker)
    {
        _gridWorkerIdToWorkerView[worker.Id].QueueFree();
        _gridWorkerIdToWorkerView.Remove(worker.Id);

        Vector2I gridPosition = _gridWorkerIdToGridPosition[worker.Id];
        _gridWorkerIdToGridPosition.Remove(worker.Id);
        UpdateGridPosition(gridPosition);
    }

    public void UpdateGridPosition(Vector2I gridPosition)
    {
        List<Worker> workers = _workerGrid.GetWorkers(gridPosition);
        for (int stackPosition = 0; stackPosition < workers.Count; stackPosition++)
        {
            GridWorkerView gridWorkerView = _gridWorkerIdToWorkerView[workers[stackPosition].Id];
            Vector3 workerPosition = GetWorker3DStackPosition(gridPosition, stackPosition, gridWorkerView.WorkerHeight);
            gridWorkerView.SlideTo(workerPosition);
        }
    }

    public Vector2I GetBoxPosition(int boxId)
    {
        if (_gridBoxViewIdToGridPosition.ContainsKey(boxId))
            return _gridBoxViewIdToGridPosition[boxId];
        throw new ArgumentException($"{nameof(GridBoxView)} id {boxId} not in {nameof(WorkerGridView)}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _workerGridUnSubscriber.Dispose();
    }
}