using BepInEx.Configuration;
using LethalCompanyTrollMenuMod.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public Message(string message, MessageType type)
        {
            this.message = message;
            this.type = type;
        }
    }
    internal class TrollConsole : MonoBehaviour
    {
        public static TrollConsole Instance;
        public static bool showConsole = false;
        public static List<Message> messages = new List<Message>();
        private static Vector2 scrollViewVector = Vector2.zero;
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
                        GUI.Label(r,"[ERROR] "+ message.message, TrollMenuStyle.errorStyle);
                        break;
                    case MessageType.SUCCESS:
                        GUI.Label(r, "[SUCCESS] " + message.message, TrollMenuStyle.successStyle);
                        break;
                    case MessageType.INFO:
                        GUI.Label(r, "[INFO] " + message.message);
                        break;
                    
                }
            }
            // End the ScrollView
            GUI.EndScrollView();
        }

        void Update()
        {
            //On f2 press
            if (new KeyboardShortcut(KeyCode.F2).IsUp())
            {
                showConsole = !showConsole;
            }

        }

        public static void DisplayMessage(string message, MessageType type = MessageType.INFO)
        {
            messages.Add(new Message(message, type));
            scrollViewVector.y = messages.Count * 30;
        }
        void Awake()
        {
            TrollMenu.mls.LogInfo("LOADED CONSOLE");
        }
    }
}
