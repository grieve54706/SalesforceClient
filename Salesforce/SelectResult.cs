using System.Collections.Generic;
using Newtonsoft.Json;

namespace sf_demo.Salesforce
{
    class SelectResult<T>
    {
        [JsonProperty("totalSize")]
        public int TotalSize { get; set; }
        [JsonProperty("done")]
        public bool Done { get; set; }
        [JsonProperty("nextRecordsUrl")]
        public string NextRecordsUrl { get; set; }
        [JsonProperty("records")]
        public List<T> Records { get; set; }
    }
}
