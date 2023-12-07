using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinServiceDemo.Console
{

    public class UpdateRequest
    {
        [JsonProperty("$type")]
        public string Type { get; set; } = "json";

        [JsonProperty("$value")]
        public RequestValue Value { get; set; } = new RequestValue();
    }

    public class RequestValue
    {
        public string id { get; set; }
        public string documentId { get; set; }
        public string printerId { get; set; }
        public string content { get; set; }
    }

    class Request
    {
        public UpdateRequest request { get; set; } = new UpdateRequest();
    }

}
