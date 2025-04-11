using System.Text.Json.Serialization;
using WorkerCoop.Utilities;
namespace Shared;

    public class Worker : IHasId
    {
        public SubscribableValue<int?> SeatOwnerId { get; set; } = new(null);
        public int Id { get; private set; }

        private static int nextWorkerId = 0;
        
        [JsonConstructor]
        public Worker(int id, SubscribableValue<int?> seatOwnerId)
        {
            Id = id;
            SeatOwnerId = seatOwnerId;
        }

        public static int GenerateNextWorkerId()
        {
            return nextWorkerId++;
        }
    }
