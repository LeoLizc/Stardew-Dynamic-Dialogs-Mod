using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Stardew_Dynamic_Dialogs
{
    internal class GPTResponse
    {
        public List<GPTCompletion> choices { get; set; }
        public GPTUsage usage { get; set; }
        public long created { get; set; }
        public string id { get; set; }
        public string model { get; set; }

        [JsonPropertyName("object")]
        public string obj { get; set; }

        public class GPTCompletion
        {
            public string finish_reasons { get; set; }
            public int index { get; set; }
            public GPTMessage message { get; set; }
            public dynamic logprobs { get; set; }

        }

        public class GPTMessage
        {
            public string content {  get; set; }
            public string role {  get; set; }
        }

        public class GPTUsage
        {
            public int completion_tokens { get; set; }
            public int prompt_tokens { get; set; }
            public int total_tokens { get; set; }

        }
    }
}
