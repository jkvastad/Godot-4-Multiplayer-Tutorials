using Client;
using Server;
using Godot;
using System.Collections.Generic;
using System.Linq;
using static Shared.GlobalConstants;
using System.Text.Json;
namespace Shared;
public partial class GameSessionNetwork : Node
{
    /// <summary>
    /// Add worker from calling peers seat to gridSeatId's grid at gridPosition.
    /// </summary>    
    #region Server-side RPCs
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ServerRequestAddWorkerToWorkerGrid(int gridSeatId, Vector2I gridPosition)
    {
        // Calling peer has seat?
        int callingPeer = Multiplayer.GetRemoteSenderId();
        if (ServerGameSession.Singleton.SessionState.SeatIdToPeerId.ContainsValue(callingPeer))
        {
            int callingSeatId = ServerGameSession.Singleton.SessionState.SeatIdToPeerId.GetByValue(callingPeer);
            // Calling seat has workers?
            List<Worker> seatWorkers = ServerGameSession.Singleton.GameState.PlayerSeats[callingSeatId].Workers;
            if (seatWorkers.Count < 1)
                return;

            // Add worker to grid
            Worker worker = seatWorkers.First();
            seatWorkers.Remove(worker);
            ServerGameSession.Singleton.GameState.PlayerSeats[gridSeatId].WorkerGrid.AddWorker(worker, gridPosition);

            string workerJson = JsonSerializer.Serialize(worker);

            Rpc(nameof(ClientUpdateSeatWorker), callingSeatId, worker.Id, (int)UpdateType.Remove);
            Rpc(nameof(ClientUpdateGridWorker), workerJson, gridSeatId, gridPosition, (int)UpdateType.Add);
        }
    }

    /// <summary>
    /// If calling peer's seat owns workerSeatId, and workerId is on gridSeatId, return worker to seat
    /// </summary>    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ServerRequestRemoveWorkerFromWorkerGrid(int gridSeatId, int workerSeatId, int workerId)
    {
        int callingPeer = Multiplayer.GetRemoteSenderId();
        if (ServerGameSession.Singleton.SessionState.SeatIdToPeerId.ContainsValue(callingPeer)) // calling peer has seat?
        {
            int callingSeatId = ServerGameSession.Singleton.SessionState.SeatIdToPeerId.GetByValue(callingPeer);
            PlayerSeat callingSeat = ServerGameSession.Singleton.GameState.PlayerSeats[workerSeatId];
            WorkerGrid targetWorkerGrid = ServerGameSession.Singleton.GameState.PlayerSeats[gridSeatId].WorkerGrid;
            Worker? workerOrNull = targetWorkerGrid.GetWorker(workerId);

            if (workerOrNull is not null && workerOrNull.SeatOwnerId == callingSeatId) // workerId on target grid? && calling seat owns workerId?
            {
                Worker worker = targetWorkerGrid.RemoveWorker(workerId)!;
                callingSeat.Workers.Add(worker);

                string workerJson = JsonSerializer.Serialize(worker);

                Rpc(nameof(ClientUpdateGridWorker), workerJson, gridSeatId, Vector2I.Zero, (int)UpdateType.Remove); // superflous vector 2I since removing
                Rpc(nameof(ClientUpdateSeatWorker), workerSeatId, workerId, (int)UpdateType.Add);
            }
        }
    }
    #endregion
    #region Client-side RPCs


    /// <summary>
    /// Updates playerSeatId's seat by removing or adding the workerId worker to the seat's pool of workers.
    /// </summary>    

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ClientUpdateSeatWorker(int playerSeatId, int workerId, int updateType)
    {
        if ((UpdateType)updateType == UpdateType.Remove)
            ClientGameSession.Singleton.GameState.PlayerSeats[playerSeatId].Workers.Remove(workerId);

        if ((UpdateType)updateType == UpdateType.Add)
        {
            Worker worker = new(workerId, new(playerSeatId));
            ClientGameSession.Singleton.GameState.PlayerSeats[playerSeatId].Workers.Add(worker);
        }
    }

    /// <summary>
    /// Updates gridSeatId's grid by removing or adding the worker. No gridposition required when removing (pass along e.g zero vector).
    /// </summary>    
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 0)]
    public void ClientUpdateGridWorker(string workerJson, int gridSeatId, Vector2I gridPosition, int updateType)
    {
        Worker worker = JsonSerializer.Deserialize<Worker>(workerJson)!;
        if ((UpdateType)updateType == UpdateType.Remove)
            ClientGameSession.Singleton.GameState.PlayerSeats[gridSeatId].WorkerGrid.RemoveWorker(worker.Id);

        if ((UpdateType)updateType == UpdateType.Add)
            ClientGameSession.Singleton.GameState.PlayerSeats[gridSeatId].WorkerGrid.AddWorker(worker, gridPosition);

    }

    #endregion
}
