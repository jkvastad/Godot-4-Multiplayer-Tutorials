using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;
using WorkerCoop.Utilities;
using static Shared.GlobalConstants;
namespace Shared;

public class WorkerGrid
{
    public int Height { get; private set; }
    public int Width { get; private set; }
    [JsonInclude]
    private List<List<ListId<Worker>>> _workerGrid = [];
    [JsonInclude]
    private Dictionary<int, Vector2I> _workerIdToGridPosition = [];

    /// <summary>
    /// worker, position, Add/Remove
    /// </summary>
    private event Action<Worker, Vector2I, UpdateType>? _workerGridUpdated;    
    
    public WorkerGrid(int width, int height)
    {
        Height = height;
        Width = width;
        //init empty worker grid
        for (int i = 0; i < width; i++)
        {
            _workerGrid.Add([]);
            for (int j = 0; j < height; j++)
                _workerGrid[i].Add([]);
        }
    }
    [JsonConstructor]
    public WorkerGrid(int width, int height, List<List<ListId<Worker>>> _WorkerGrid, Dictionary<int, Vector2I> _WorkerIdToGridPosition)
    {
        Height = height;
        Width = width;
        _workerGrid = _WorkerGrid;
        _workerIdToGridPosition = _WorkerIdToGridPosition;
    }

    public Unsubscriber Subscribe(Action<Worker, Vector2I, UpdateType> action)
    {
        _workerGridUpdated += action;
        return new Unsubscriber(action, this);
    }

    public class Unsubscriber : IDisposable
    {
        Action<Worker, Vector2I, UpdateType> _action;
        WorkerGrid _workerGrid;
        public Unsubscriber(Action<Worker, Vector2I, UpdateType> action, WorkerGrid workerGrid)
        {
            _action = action;
            _workerGrid = workerGrid;
        }
        public void Dispose()
        {
            _workerGrid._workerGridUpdated -= _action;
        }
    }

    public void AddWorker(Worker worker, Vector2I gridPosition)
    {
        _workerGrid[gridPosition.X][gridPosition.Y].Add(worker);
        _workerIdToGridPosition[worker.Id] = gridPosition;
        _workerGridUpdated?.Invoke(worker, gridPosition, UpdateType.Add);
    }

    // Returns removed worker or null if workerId not present
    public Worker? RemoveWorker(int workerId)
    {
        Worker? worker = GetWorker(workerId);

        if (worker is not null)
        {
            Vector2I workerGridPosition = _workerIdToGridPosition[workerId];
            _workerGrid[workerGridPosition.X][workerGridPosition.Y].Remove(workerId);
            _workerIdToGridPosition.Remove(workerId);
            _workerGridUpdated?.Invoke(worker, new(), UpdateType.Remove); // superflous position
        }
        return worker;
    }

    public List<Worker> GetWorkers(Vector2I gridPosition)
    {
        return _workerGrid[gridPosition.X][gridPosition.Y];
    }

    public Worker? GetWorker(int workerId)
    {
        if (_workerIdToGridPosition.ContainsKey(workerId))
        {
            Vector2I workerGridPosition = _workerIdToGridPosition[workerId];            
            Worker worker = _workerGrid[workerGridPosition.X][workerGridPosition.Y].GetById(workerId)!;
            return worker;
        }
        return null;
    }
}