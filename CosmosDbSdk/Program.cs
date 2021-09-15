using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            var client = new CosmosClient(configuration["Cosmos:Endpoint"], configuration["Cosmos:MasterKey"]);


                await InitDatabaseAndContainerIfNotExists(client);
                await QueryForDocumentsWithoutLinqAsync(client);
                await QueryForDocumentsWithLinqAsync(client);
                await ReplacingDocumentsAsync(client);

            Console.WriteLine("Restart the execution? (y/n)");
            if (Console.ReadLine() == "y")
            {
                string strCmdText;
                strCmdText = "/C dotnet CosmosDbSdk.dll";
                Process.Start("CMD.exe", strCmdText);
            }

        }

        private static async Task QueryForDocumentsWithoutLinqAsync(CosmosClient client)
        {
            var container = client.GetContainer("CosmosDb", "ItemsContainer");
            Console.WriteLine("Getting all documents from the container...");
            var sql = "SELECT * FROM c";
            var iterator = container.GetItemQueryIterator<Item>(sql);
            var page = await iterator.ReadNextAsync();
            foreach(var doc in page)
            {
                Console.WriteLine("Document: " + doc.Name + ", id:" + doc.Id + doc.Id + ", count: " + doc.Count);
            }
        }

        private static async Task QueryForDocumentsWithLinqAsync(CosmosClient client)
        {
            var container = client.GetContainer("CosmosDb", "ItemsContainer");
            Console.WriteLine("Getting documents from the container where count > 18 using linq...");

            var iterator = container.GetItemLinqQueryable<Item>()
                .Where(i => i.Count > 18)
                .ToFeedIterator();

            while(iterator.HasMoreResults)
            {
                foreach (var doc in await iterator.ReadNextAsync())
                {
                    Console.WriteLine("Document: " + doc.Name + ", id:" + doc.Id + ", count: " + doc.Count);
                }
            }

        }

        private static async Task InitDatabaseAndContainerIfNotExists(CosmosClient client)
        {
            var databaseResponse = await client.CreateDatabaseIfNotExistsAsync("CosmosDb");

            Console.WriteLine("Database creation status: " + databaseResponse.StatusCode);

            var containerCreation = await databaseResponse.Database.CreateContainerIfNotExistsAsync(new ContainerProperties("ItemsContainer", "/address/containerDemoPartition"));

            Console.WriteLine("Container creation status: " + containerCreation.StatusCode);

            if (containerCreation.StatusCode == HttpStatusCode.Created)
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
                {
                    var request = await containerCreation.Container.CreateItemAsync<Item>(item);
                    Console.WriteLine(request.StatusCode);
                }
            }
        }

        private static async Task ReplacingDocumentsAsync(CosmosClient client)
        {
            var container = client.GetContainer("CosmosDb", "ItemsContainer");
            Console.WriteLine();
            Console.WriteLine("Incrementing documents count by replacing them...");

            var iterator = container.GetItemLinqQueryable<Item>()
                .ToFeedIterator();

            while (iterator.HasMoreResults)
            {
                foreach (var doc in await iterator.ReadNextAsync())
                {
                    doc.Count++;

                    await container.ReplaceItemAsync<Item>(doc, doc.Id.ToString());
                }
            }
        }
    }
}
