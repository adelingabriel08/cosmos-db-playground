using Newtonsoft.Json;
using System;

namespace CosmosDbSdk
{
    public class Item
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
