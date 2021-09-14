using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbSdk
{
    public class Item
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
