using Newtonsoft.Json;
using System.Collections.Generic;

namespace sf_demo.Salesforce
{
    public class MultiResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("errors")]
        public List<ApiError> Errors { get; set; }
    }
}