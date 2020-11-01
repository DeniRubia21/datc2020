using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace L05
{
    class Program
    {
        private static CloudTableClient tableClient;
        private static CloudTable studentsTable;
        private static CloudTable metricsTable;
        static void Main(string[] args)
        {
            Task.Run(async () => { await Initialize(); })
                .GetAwaiter()
                .GetResult();
        }

        static async Task Initialize()
        {
            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=datc2020alexl4;AccountKey=50NAdHWefMdk3lb69O73pOEPkTvFRkToztEqvfGjO3E2+pKsOw7lvP6gpFSClGqfZ1ZohmQXtulPYdqdAR1yaA==;EndpointSuffix=core.windows.net";

            var account = CloudStorageAccount.Parse(storageConnectionString);
            tableClient = account.CreateCloudTableClient();

            studentsTable = tableClient.GetTableReference("studenti");
            metricsTable = tableClient.GetTableReference("metrics");

            await studentsTable.CreateIfNotExistsAsync();
            await metricsTable.CreateIfNotExistsAsync();

            await GetAllStudents();
        }

        private static async Task GetAllStudents()
        {
            TableQuery<StudentEntity> query = new TableQuery<StudentEntity>();
            TableContinuationToken token = null;

            int general_counter = 0;
                int upt_counter = 0;
                int uvt_counter = 0;

            do
            {
                TableQuerySegment<StudentEntity> resultSegment = await studentsTable.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;

                foreach(StudentEntity entity in resultSegment.Results)
                {
                    general_counter++;
                    if(entity.PartitionKey == "UPT")
                    {
                        upt_counter++;
                    }
                    if(entity.PartitionKey == "UVT")
                    {
                        uvt_counter++;
                    }
                    Console.WriteLine("Student: " + entity.PartitionKey + " / " + entity.Timestamp);
                }
            }
            while(token != null);
            await AddMetric("UVT", uvt_counter);
            await AddMetric("UPT", upt_counter);
            await AddGeneralMetric(general_counter);
        }
        private static async Task AddGeneralMetric(int counter)
        {
            var generalmetric = new MetricsEntity("general");
            generalmetric.Count = counter;

            var insertOperation = TableOperation.Insert(generalmetric);
            await metricsTable.ExecuteAsync(insertOperation);
        }
        private static async Task AddMetric(string university, int counter)
        {
            var generalmetric = new MetricsEntity(university);
            generalmetric.Count = counter;

            var insertOperation = TableOperation.Insert(generalmetric);
            await metricsTable.ExecuteAsync(insertOperation);
        }
    }
}