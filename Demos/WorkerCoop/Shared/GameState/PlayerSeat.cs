using System.Text.Json.Serialization;
using WorkerCoop.Utilities;
namespace Shared;

public class PlayerSeat
{
    public int SeatId { get; set; } = -1; // unassigned
    public string SeatName { get; set; } = "DEFAULT_SEAT_NAME";
    public ListId<Worker> Workers { get; set; } = new();
    public WorkerGrid WorkerGrid { get; set; } = new(4, 3);
    public PlayerSeat(int seatId)
    {
        SeatId = seatId;
        SeatName = $"Seat {SeatId}";
    }
    [JsonConstructor]
    public PlayerSeat(int seatId, string seatName, ListId<Worker> workers, WorkerGrid workerGrid)
    {
        SeatId = seatId;
        SeatName = seatName;
        Workers = workers;
        WorkerGrid = workerGrid;
    }
}
