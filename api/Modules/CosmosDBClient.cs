using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace NaftaScheduler
{
    class CosmosDBClient
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("schedulercosmosdb_DOCUMENTDB");
        private string database_name = "Events";
        private Container container;
        public CosmosDBClient(string containerId)
        {
            CosmosClient client = new CosmosClient(ConnectionString);
            this.container = client.GetDatabase(database_name).GetContainer(containerId);
        }

        public async Task<List<T>> QueryItemsAsync<T>(string sqlQueryText = "SELECT * FROM c")
        {
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<T> queryResultSetIterator = container.GetItemQueryIterator<T>(queryDefinition);
            List<T> documents = new List<T>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<T> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (T document in currentResultSet)
                {
                    documents.Add(document);
                }
            }
            return documents;
        }
        public async Task AddItemAsync<T>(T document)
        {
            ItemResponse<T> response = await this.container.CreateItemAsync<T>(document);
        }
        public async Task UpdateItemAsync<T>(Guid documentToUpdateId, T document)
        {
            await this.container.ReplaceItemAsync<T>(document, documentToUpdateId.ToString());
        }
        public async Task DeleteItemAsync<T>(Guid documentToUpdateId, string partitionKey)
        {
            await this.container.DeleteItemAsync<T>(documentToUpdateId.ToString(), new PartitionKey(partitionKey));
        }
        public async Task<T> ExecProcAsync<T>(string procedureId, string partionkeyValue, dynamic[] parameters)
        {
            var response = await this.container.Scripts.ExecuteStoredProcedureAsync<T>(procedureId, new PartitionKey(partionkeyValue), parameters);
            return response;
        }
    }
}