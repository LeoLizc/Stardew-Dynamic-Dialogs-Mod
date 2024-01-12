using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Stardew_Dynamic_Dialogs
{
    internal class ModEntry : Mod
    {

        private static HttpClient httpClient = new()
        {
            //BaseAddress = new Uri("https://jsonplaceholder.typicode.com"),
        };

        private Dictionary<string, NPCPromptData> npcsData;
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            //helper.Events.Content.AssetRequested += OnSpeak;

            var model = helper.Data.ReadJsonFile<Dictionary<string, NPCPromptData>>("assets/NPCs.json");
            if (model == null)
            {
                this.Monitor.Log($"No pudo cargar el json", LogLevel.Debug);
                return;
            }

            npcsData = model;
            helper.Events.World.NpcListChanged += OnNPCEnterMap;
            helper.Events.Player.Warped += PlayerWarped;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnNPCEnterMap(object sender, NpcListChangedEventArgs e)
        {
            if (!e.Added.Any()) return;

            var npc = e.Added.First();
            if (npcsData.ContainsKey(npc.Name))
            {
                npcsData[npc.Name].moved = true;

                if (e.IsCurrentLocation)
                {
                    this.Monitor.Log($"{npc.Name} entered", LogLevel.Debug);

                    LoadDialogAsync(npc);
                }
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            this.Monitor.Log($"Day Started", LogLevel.Debug);
            foreach (var npc in npcsData.Values)
            {
                npc.talkToday = 0;
            }
        }

        private void PlayerWarped(object sender, WarpedEventArgs e)
        {
            this.Monitor.Log($"Player Warped -> {e.NewLocation.Name}", LogLevel.Debug);

            foreach (var npc in e.NewLocation.characters)
            {
                if (npcsData.ContainsKey(npc.Name))
                {
                    LoadDialogAsync(npc);
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button} {Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)}.", LogLevel.Debug);
        }

        private async Task LoadDialogAsync(NPC npc) //TODO. No cambiar el dialogo, si el npc no ha cambiado de ubicación
        {
            var npcData = npcsData[npc.Name];

            if (!npc.CurrentDialogue.Any() && npcData.talkToday<npcData.maxTalkPerDay)//IS EMPTY
            {
                npcData.talkToday++;
            }

            if (npcData.talkToday >= npcData.maxTalkPerDay) return; //Si llegó al límite de conversaciones
            if (npc.CurrentDialogue.Any() && !npcData.moved) return; //Si ya cargó la conversación actual y no ha terminado

            npcData.moved = false;
            //TODO: ADD a random possibility to just leave the default dialogs
            this.Monitor.Log($"Empezamos", LogLevel.Debug);

            // Consultar el nuevo dialogo a chatgpt
            //Cargar el prompt de system de npcData
            var system = npcData.description;
            //Cargar el prompt de usar de Utils.loadPrompt(npc)
            var user = Utils.loadPrompt(npc, npcData);
            //Enviar prompts y recibir respuesta
            string response = await askGPT(system, user);
            this.Monitor.Log($"Response: {response}", LogLevel.Debug);

            if (response == null)
            {
                this.Monitor.Log($"Weee resposne es null", LogLevel.Debug);
                return;
            }

            //Asignar ese nuevo dialogo
            //Separar diálogos
            var dialogs = response.Split("|");

            // LOG DE DIALOGOS
            this.Monitor.Log($"{npc.Name}:", LogLevel.Debug);
            this.Monitor.Log($"Viejos diálogos", LogLevel.Debug);

            foreach (var item in npc.CurrentDialogue)
            {

                this.Monitor.Log($"{item.getCurrentDialogue()}", LogLevel.Debug);
                this.Monitor.Log($"{item.CurrentEmotion}", LogLevel.Debug);
            }

            //Para cada diálogo
            bool first = true;
            foreach (var dialog in dialogs.Reverse())
            {
                //Separar el dialogo en partes
                var dialogParts = dialog.Split("$");

                //Asignar el nuevo dialogo
                npc.setNewDialogue(dialogParts[1], !first);
                npc.CurrentDialogue.First().CurrentEmotion = $"${dialogParts[0]}";

                first = false;
            }

        }

        private async Task<string> askGPT2(string system, string user)
        {
            return "k$Tenemos mucha suerte de contar con una biblioteca. Cuando te pierdes en un libro, es fácil olvidar las realidades de tu vida.|h$...Quizá por eso me gusta tanto leer.|u$...Perdón. Me he dejado llevar";
        }

        private async Task<string> askGPT(string system, string user)
        {

            using (var requestMessage =
            new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions"))
            {
                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", "YOUR-TOKEN-HERE");

                var jsonContent = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = system
                        },
                        new 
                        {
                            role = "user",
                            content = "Es un lunes de primavera.\nTe encuentras en el pueblo pasadas las 4:00pm.\nEl nuevo granjero del pueblo se acerca a ti y parece quiere hablarte.\nEs su primer día en el pueblo y no han hablado antes"
                        },
                        new
                        {
                            role = "assistant",
                            content = "k$Oh... hola!|k$Soy Penny..."
                        },
                        new
                        {
                            role = "user",
                            content = user
                        }
                    },
                    temperature = 1,
                    max_tokens = 250,
                    top_p = 1,
                    frequency_penalty = 0,
                    presence_penalty = 0
                };

                requestMessage.Content
                    = new StringContent(JsonSerializer.Serialize(jsonContent), Encoding.UTF8, "application/Json");
                requestMessage.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json");

                this.Monitor.Log($"Wamo a pregunta", LogLevel.Debug);

                var response = await httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    this.Monitor.Log($"Error en la consulta ::c", LogLevel.Debug);
                    return null;
                }
                this.Monitor.Log($"Supone bien", LogLevel.Debug);

                try
                {
                    var content = await response.Content.ReadFromJsonAsync<GPTResponse>();
                    this.Monitor.Log($"Supone bien2", LogLevel.Debug);

                    try
                    {
                        return content.choices[0].message.content;
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"Error con la respuesta:\n{content}", LogLevel.Debug);
                    }
                }catch (Exception ex)
                {
                    this.Monitor.Log($"Error conviertiendo:\n{ex}", LogLevel.Debug);
                }
                finally
                {
                    response.Dispose();
                }
            }

            return null;
        }
    }
}
