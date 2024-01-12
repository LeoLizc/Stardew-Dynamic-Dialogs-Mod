using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace Stardew_Dynamic_Dialogs
{
    internal class Utils
    {
        public static string GetTimeLivingInTwon()
        {
            int year = Game1.year;
            int days = GetSeason() * 28 + Game1.dayOfMonth - 1;

            if (year == 1 && days == 0)
            {
                return "Es su primer día en el pueblo.";
            }

            string response = "Han sido ";

            if (year != 1)
            {
                response += year + " años";
            }

            if (days > 0)
            {
                response += " y " + days;
            }

            response += " desde que llegó al pueblo.";

            return response;
        }

        public static string GetTimeLocation(NPC npc)
        {

            string time = Game1.timeOfDay.ToString();
            string response = $"Estás en {npc.currentLocation.Name} pasadas las {time.Substring(0, time.Length - 2)}:{time.Substring(time.Length - 2)}.";
            return response;
        }

        public static string GetDay()
        {
            return $"Es un {DayOfSeason()} de {Game1.CurrentSeasonDisplayName}.";
        }

        private static int GetSeason()
        {
            return Game1.IsSpring ? 0 : Game1.IsSummer ? 1 : Game1.IsFall ? 2 : 3;
        }

        private static string DayOfSeason()
        {
            return (Game1.dayOfMonth % 7) switch
            {
                0 => "Domingo",
                1 => "Lunes",
                2 => "Martes",
                3 => "Miércoles",
                4 => "jueves",
                5 => "Viernes",
                6 => "Sábado",
                _ => "",
            };
        }

        public static string GetTalkingTime(NPC npc, NPCPromptData npcData)
        {
            if (GetSeason() == 0 && Game1.dayOfMonth == 1 && Game1.year==1)
            {
                return "No han hablado antes.";
            }

            if (Game1.player.hasPlayerTalkedToNPC(npc.Name))
            {
                return "No han hablado antes.";
            }

            int hearthLevel = Game1.player.getFriendshipHeartLevelForNPC(npc.Name);
            if (npc.datable.Value)
            {
                if (hearthLevel <=8)
                    return "No es la primera vez que hablan.\n" +
                        $"Sería la {npcData.talkToday}a vez que hablan hoy" +
                        $"Su nivel de amistad es {hearthLevel}/8, siendo 0 algo neutro, 2 conocidos, 5 una muy buena amistad y 8 que es tu interés amoroso.";
            }
            return "";
        }

        public static string loadPrompt(NPC npc, NPCPromptData npcData)
        {

            return $"{GetDay()}\n{GetTimeLocation(npc)}\nEl nuevo granjero del pueblo se acerca a ti y parece quiere hablarte.\n{GetTimeLivingInTwon()}\n{GetTalkingTime(npc, npcData)}";
        }
    }
}
