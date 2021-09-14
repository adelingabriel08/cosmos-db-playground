using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CosmosDbSdk
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            using (var client = new CosmosClient(configuration["Cosmos:Endpoint"], configuration["Cosmos:MasterKey"]))
            {
                await InitDatabaseAndContainerIfNotExists(client);
            }
            
        }

        private static void QueryForDocuments(CosmosClient client)
        {
            var container = client.GetContainer("CosmosDb", "ContainerDemo");
            var sql = "SELECT * FROM c";
            var iterator = container.GetItemQueryIterator<dynamic>(sql);
        }

        private static async Task InitDatabaseAndContainerIfNotExists(CosmosClient client)
        {
            var databaseResponse = await client.CreateDatabaseIfNotExistsAsync("CosmosDb");

            Console.WriteLine("Database creation status: " + databaseResponse.StatusCode);

            var containerCreation = await databaseResponse.Database.CreateContainerIfNotExistsAsync(new ContainerProperties("ItemsContainer", "/address/containerDemoPartition"));

            Console.WriteLine("Container creation status: " + containerCreation.StatusCode);

            if (containerCreation.StatusCode == HttpStatusCode.OK)
            {
                var documents = new List<Item>()
            {
                new Item(){Id = Guid.NewGuid(), Name = "Document item 1", Count = 5},
                new Item(){Id = Guid.NewGuid(), Name = "Document item 2", Count = 9},
                new Item(){Id = Guid.NewGuid(), Name = "Document item 3", Count = 19},
                new Item(){Id = Guid.NewGuid(), Name = "Document item 4", Count = 51},
                new Item(){Id = Guid.NewGuid(), Name = "Document item 5", Count = -100},
            };
                Console.WriteLine("Uploading documents...");

                foreach (var item in documents)
                    await containerCreation.Container.CreateItemAsync<Item>(item);
            }
        }
    }
}
