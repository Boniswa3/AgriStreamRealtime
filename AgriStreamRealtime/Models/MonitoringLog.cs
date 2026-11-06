using Azure;
using Azure.Data.Tables;

namespace AgriStreamRealtime.Models
{
    public class MonitoringLog : ITableEntity
    {
        public string PartitionKey { get; set; } = "Monitor";

        public string RowKey { get; set; } =
            Guid.NewGuid().ToString();

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public int ServiceId { get; set; }

        public string ServiceTitle { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;

        public DateTimeOffset DateRecorded { get; set; } =
              DateTimeOffset.UtcNow;
    }

}
