using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CosmosDbSdk
{
    public class Item
    {
        [JsonPropertyName("id")]
        public Guid id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
