using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_Dynamic_Dialogs
{
    internal sealed class ModConfig
    {
        public string apiKey {  get; set; }
        public string gptModel { get; set; } = "gpt-3.5-turbo";
    }
}
