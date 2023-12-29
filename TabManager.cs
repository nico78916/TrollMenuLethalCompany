using BepInEx.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using LethalCompanyTrollMenuMod.tabs;

namespace LethalCompanyTrollMenuMod.Component
{
    
    internal class TabManager : MonoBehaviour
    {
        private Rect wr = new Rect(20, 20, 500, 800);
        private int toolbarInt = 0;
        private Type[] toolBarTypes = null;
        private string[] toolbarStrings = { "No tabs spotted" };
        private bool showMenu = false;
        public static bool showMessage = false;

        private Type[] getTabs()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            List<Type> tabs = new List<Type>();
            foreach (Type type in types)
            {
                if (type.Namespace == "LethalCompanyTrollMenuMod.tabs")
                {
                    if(type.Name.Contains("<")) continue;
                    tabs.Add(type);
                }
            }
            return tabs.ToArray();
        }

        

        void OnGUI()
        {
            if(!showMenu) return;
            wr = new Rect(20, 20, 500, 800);
            // Make a background box
            GUI.Box(wr, "Troll Menu");
            toolbarInt = GUI.Toolbar(new Rect(wr.x, wr.y+25, wr.width, 25), toolbarInt, toolbarStrings);
            //Getting the current tab
            var currentTab = toolBarTypes[toolbarInt];
            //Execute the Draw() method of the current tab
            object[] parameters = new object[] { wr };
            currentTab.GetMethod("Draw").Invoke(null, parameters);
        }

        void Awake()
        {
            TrollMenu.mls.LogInfo("LOADED MENU");
            //Getting all the tabs of the tabs namespace
            var tabs = getTabs();
            toolbarStrings = tabs.Select(x => x.Name).ToArray();
            toolBarTypes = tabs;
        }

        private string CurrentScene()
        {
            return SceneManager.GetActiveScene().name;
        }

        private string scene = "";

        void Update()
        {
            if (new KeyboardShortcut(KeyCode.F1).IsUp() && (TrollMenu.isInGame || showMenu))
            {
                showMenu = !showMenu;
                if(showMenu)
                {
                    TrollMenu.allPlayers.Clear();
                    foreach (var ply in StartOfRound.Instance.allPlayerScripts)
                    {
                        TrollMenu.allPlayers.Add(ply.playerUsername,ply);
                    }
                    foreach (Type type in toolBarTypes)
                    {
                        if(type.GetMethod("OnMenuOpened") != null)
                            type.GetMethod("OnMenuOpened").Invoke(null, null);
                    }
                }
            }
            if (CurrentScene() != scene)
            {
                scene = CurrentScene();
                TrollMenu.isInGame = scene == "SampleSceneRelay";
                SpawnMenu.currentPlayer = null;
                SpawnMenu.playerSelect.SetToDefault();
                DeadlyItems.currentPlayer = null;
                DeadlyItems.playerSelect.SetToDefault();
            }
        }

        void OnDestroy() {
            TrollMenu.mls.LogError("UNLOADED MENU");
        }

    }
}
