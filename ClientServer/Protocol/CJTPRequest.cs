using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClientServer.Protocol
{
    /// <summary>
    /// Represents a request in the Custom JSON Transfer Protocol (CJTP).
    /// </summary>

    [JsonSerializable(typeof (CJTPRequest))]
    public class CJTPRequest
    {
        [JsonSerializable(typeof(BodyData))]
        public class BodyData
        {
            public BodyData() { }
            [JsonProperty("cid", NullValueHandling = NullValueHandling.Ignore)]
            
            public string Cid {  get; set; }
            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }
        }

        public CJTPRequest() { }
        
        /// <summary>
        /// Gets or sets the method used in the request.
        /// </summary>
        [JsonProperty("method")]
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the path associated with the request.
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the date associated with the request.
        /// </summary>
        [JsonProperty("date")]
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the body of the request.
        /// </summary>
        [JsonProperty("body")]
        public BodyData Body { get; set; }
    }
}