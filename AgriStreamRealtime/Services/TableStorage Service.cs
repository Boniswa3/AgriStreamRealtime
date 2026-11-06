using Azure.Data.Tables;
using AgriStreamRealtime.Models;

namespace AgriStreamRealtime.Services
{
    public class TableStorageService
    {
        private readonly TableClient _tableClient;

        public TableStorageService(IConfiguration configuration)
        {
            string? connectionString =
                configuration["AzureStorage:ConnectionString"];

            // Validate that the Azure Storage connection string is provided in the configuration. If it is missing or empty, throw an InvalidOperationException to indicate that the connection string is required for the service to function properly.
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
            }

            // Create a TableServiceClient using the provided connection string and get a reference to the "InventoryLogs" table. This client will be used to interact with the Azure Table Storage service for logging inventory activities.
            var serviceClient =
                new TableServiceClient(connectionString);

            _tableClient = serviceClient.GetTableClient(
                "MonitoringLogs");
        }

        // Initialize the Table Storage service by creating the "MonitoringLogs" table if it does not already exist. This method ensures that the table is available for logging monitoring activities.
        public async Task InitializeAsync()
        {
            await _tableClient.CreateIfNotExistsAsync();
        }

        public async Task LogActivityAsync(MonitoringLog log)
        {
            await InitializeAsync();

            await _tableClient.AddEntityAsync(log);
        }

        // Retrieve all monitoring log entries from the "MonitoringLogs" table in Azure Table Storage. This method initializes the table if necessary and queries all log entries, returning them as a list of MonitoringLog objects.
        public async Task<List<MonitoringLog>> GetLogsAsync()
        {
            await InitializeAsync();

            var logs = new List<MonitoringLog>();

            await foreach (var log in _tableClient.QueryAsync<MonitoringLog>())
            {
                logs.Add(log);
            }

            return logs;
        }
    }
}

