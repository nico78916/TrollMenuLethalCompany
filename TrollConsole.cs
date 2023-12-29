using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalCompanyTrollMenuMod
{
    internal enum MessageType
    {
        ERROR,
        SUCCESS,
        INFO
    }
    internal class Message
    {
        public MessageType type = MessageType.INFO;
        public string message;
        public string copybuffer;
        public Message(string message, MessageType type)
        {
            this.message = message;
            this.type = type;
            this.copybuffer = message;
        }
        public Message(string message, MessageType type, string copybuffer)
        {
            this.message = message;
            this.type = type;
            this.copybuffer = copybuffer;
        }
    }
    internal class TrollConsole : MonoBehaviour
    {
        public static TrollConsole Instance;
        public static bool showConsole = false;
        public static List<Message> messages = new List<Message>();
        private static Vector2 scrollViewVector = Vector2.zero;
        private static KeyCode key = KeyCode.F2;
        void OnGUI()
        {
            if (!showConsole) return;
            //Scroll view
            // Begin the ScrollView
            Rect rect = new Rect(0, 0, 400,(messages.Count * 30));
            //scroll to bottom
            scrollViewVector = GUI.BeginScrollView(new Rect(Screen.width - Screen.width / 2, Screen.height - Screen.height / 2, Screen.width / 2, Screen.height / 2), scrollViewVector, rect);
            
            int y = 0;
            foreach (Message message in messages)
            {
                Rect r = new Rect(0, y, Screen.width, 25);
                y += 25;
                switch (message.type)
                {
                    case MessageType.ERROR:
                        GUI.Label(r, "[ERROR] " + message.message, TrollMenuStyle.errorStyle);
                        break;
                    case MessageType.SUCCESS:
                        GUI.Label(r, "[SUCCESS] " + message.message, TrollMenuStyle.successStyle);
                        break;
                    case MessageType.INFO:
                        GUI.Label(r, "[INFO] " + message.message, TrollMenuStyle.infoStyle);
                        break;
                }
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
                {
                    GUIUtility.systemCopyBuffer = message.copybuffer;
                    if(message.copybuffer.StartsWith("http"))
                        System.Diagnostics.Process.Start(message.copybuffer);
                }
            }
            // End the ScrollView
            GUI.EndScrollView();
            
        }

        void Update()
        {
            //On f2 press
            if (new KeyboardShortcut(key).IsUp())
            {
                showConsole = !showConsole;
            }

        }

        public static void DisplayMessage(string message, MessageType type = MessageType.INFO)
        {
            DisplayMessage(message, message, type);
        }
        public static void DisplayMessage(string message, string copybuffer, MessageType type = MessageType.INFO)
        {
            if(type == MessageType.ERROR)
                showConsole = true;
            if(messages.Count > 1000)
                messages.RemoveAt(0);
            messages.Add(new Message(message, type, copybuffer));
            scrollViewVector.y = messages.Count * 30;
        }

        void Awake()
        {
            TrollMenu.mls.LogInfo("LOADED CONSOLE");
            //check if the version is the latest
            GetRequest("https://thunderstore.io/api/experimental/package/TrollNation/Troll_mod/", checkVersion);
        }

        private void checkVersion(string response)
        {
            response = response.Replace(" ", "");
            //Get the regex for "version_number" : "[0-9.]+"
            string version = Regex.Match(response, "\"version_number\"[ ]*:[ ]*\"[0-9\\.]+\"").Value;
            TrollMenu.mls.LogInfo("Version : " + version);
            version = version.Replace("\"", "");
            version = version.Split(':')[1];
            if (version != TrollMenu.modVersion)
            {
                showConsole = true;
                DisplayMessage("You have version (" + TrollMenu.modVersion + ")", MessageType.INFO);
                DisplayMessage("There is a new version of the mod available (" + version + ")", MessageType.SUCCESS);
                DisplayMessage("You can download it at https://thunderstore.io/c/lethal-company/p/TrollNation/Troll_mod/", "https://thunderstore.io/c/lethal-company/p/TrollNation/Troll_mod/", MessageType.SUCCESS);
                DisplayMessage("You can click the link to copy it", MessageType.INFO);
                DisplayMessage("You can close this by pressing " + key.ToString(), MessageType.INFO);

            }
        }


        async Task GetRequest(string uri,Action<string> success = null, Action<int> failed = null)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        if (success != null)
                            success(responseBody);
                    }
                    else
                    {
                        if (failed != null)
                            failed((int)response.StatusCode);
                    }
                }
                catch (HttpRequestException e)
                {
                    if (failed != null)
                        failed(-1);
                    TrollMenu.mls.LogError($"Exception: {e.Message}");
                }
            }
        }
    }
}
