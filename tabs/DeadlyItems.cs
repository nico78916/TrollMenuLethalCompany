using GameNetcodeStuff;
using LethalCompanyTrollMenuMod.helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LethalCompanyTrollMenuMod.tabs
{
    internal class DeadlyItems
    {
        public static Select<int> spawnMode = null;
        public static int currentMode = 0;
        public static Select<PlayerControllerB> playerSelect = null;
        public static PlayerControllerB currentPlayer = null;
        public static int range = 10;
        public static string rangeString = range.ToString();


        public static void OnMenuOpened()
        {
            if (!TrollMenu.isInGame) return;
            if (spawnMode == null)
            {
                Dictionary<string, int> spawnType = new Dictionary<string, int>()
                {
                    {"Random Spawn",0 },
                    {"Spawn Near Player",1 },
                    {"Spawn Near Random Player",2 }
                };
                spawnMode = new Select<int>(spawnType);
                spawnMode.SetDefault(0);
            }
            if (playerSelect == null)
            {
                playerSelect = new Select<PlayerControllerB>(TrollMenu.allPlayers);
            }
        }
        public static void Draw(Rect wr)
        {
            int y = 50;
            GUI.Label(new Rect(wr.x + 10f, wr.y + y, 200f, 30f), "Deadly Items", TrollMenuStyle.labelStyle);
            y += 30;
            currentMode = spawnMode.Draw(new Rect(wr.x + 10f, wr.y + y, 200f, 30f));
            y += 90;// 3 * 30 because there are 3 spawn modes
            currentPlayer = playerSelect.Draw(new Rect(wr.x + wr.width + 10, wr.y, 200f, 30f));
            GUI.Label(new Rect(wr.x + 10f, wr.y + y, 200f, 25f), "Near Player Range: ");
            rangeString = GUI.TextField(new Rect(wr.x + 200f, wr.y + y, 100f, 30f), rangeString);
            if(rangeString != "")
                if (int.TryParse(rangeString, out int result))
                {
                    range = result;
                }
                else
                {
                    range = 10;
                }
            rangeString = range.ToString();
            //Spawn Deadly Items
            foreach(KeyValuePair<string, SpawnableMapObject> deadlyItem in TrollMenu.deadlyobjects)
            {
                y += 30;
                if(GUI.Button(new Rect(wr.x + 5, wr.y + y, wr.width - 10, 25), deadlyItem.Key))
                {
                    if(currentMode == 0)
                    {
                        TrollConsole.DisplayMessage("Spawning " + deadlyItem.Key + "at random position");
                        TrollMenu.SpawnHostileObjectAtRandomPos(deadlyItem.Value);
                    }
                    else if(currentMode == 1)
                    {
                        TrollConsole.DisplayMessage("Spawning " + deadlyItem.Key + "at "+currentPlayer.playerUsername+" position");
                        TrollMenu.SpawnHostileObjectNearPlayer(deadlyItem.Value,currentPlayer,range);
                    }
                    else if(currentMode == 2)
                    {
                        PlayerControllerB[] plys = TrollMenu.alivePlayers.Values.ToArray();
                        PlayerControllerB ply = plys[TrollMenu.roundManager.AnomalyRandom.Next(0, plys.Length)];
                        TrollConsole.DisplayMessage("Spawning " + deadlyItem.Key + "at " + ply.playerUsername + " position");
                        TrollMenu.SpawnHostileObjectNearPlayer(deadlyItem.Value, ply, range);
                    }
                }
                
            }
        }
    }
}
