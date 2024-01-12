using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_Dynamic_Dialogs
{
    internal class NPCPromptData
    {
        public string description { get; set; }
        public bool moved { get; set; } = true;
        public int maxTalkPerDay { get; set; } = 1;
        public int talkToday { get; set; } = 0;
    }
}
