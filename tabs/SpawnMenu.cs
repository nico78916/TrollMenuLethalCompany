using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LethalCompanyTrollMenuMod.helpers;
namespace LethalCompanyTrollMenuMod.tabs
{
    internal class SpawnMenu
    {
        private static Vector2 scrollViewVector = Vector2.zero;
        private static Rect scrollRect = new Rect(0, 0, 100, 100);
        public static PlayerControllerB currentPlayer = null;
        public static List<bool> plyToggle = new List<bool>();
        public static List<bool> lastToggleState = new List<bool>();
        public static bool randomSpawn = true;
        public static bool spawnNearPlayer = false;
        public static bool spawnNearRandomPlayer = false;
        private static int lastSelected = 0;
        private static Select<PlayerControllerB> playerSelect = null;
        private static Dictionary<string, PlayerControllerB> players = new Dictionary<string, PlayerControllerB>();
        private static Dictionary<string,int> spawnType = new Dictionary<string, int>()
        {
            {"Random Spawn",0 },
            {"Spawn Near Player",1 },
            {"Spawn Near Random Player",2 }
        };
        private static Select<int> spawnSelect = null;
        private static int selected = 0;

        public static void OnMenuOpened()
        {
            if (!TrollMenu.isInGame) return;
            if (playerSelect == null)
            {
                PlayerControllerB[] plys = GetPlayer();
                foreach (PlayerControllerB ply in plys)
                {
                    if(!players.ContainsKey(ply.playerUsername))
                    players.Add(ply.playerUsername, ply);
                }
            }
            if(playerSelect == null)
            playerSelect = new Select<PlayerControllerB>(players);
            if(spawnSelect == null)
            {
                spawnSelect = new Select<int>(spawnType);
                spawnSelect.SetDefault(0);
            }

        }
        public static void Draw(Rect wr)
        {
            scrollRect = new Rect(0, 0, wr.width, 100 + 30 * TrollMenu.outsideEnemies.Count + 30 * TrollMenu.insideEnemies.Count);
            scrollViewVector = GUI.BeginScrollView(new Rect(0, 25, wr.width, wr.height-25), scrollViewVector, scrollRect);
            int y = 50;
            GUI.Label(new Rect(wr.x, y, wr.width, 25), "Spawn Menu");
            y += 25;
            selected = spawnSelect.Draw(new Rect(wr.x, y, wr.width, 25));
            y += 100;
            if (selected != lastSelected)
            {
                randomSpawn = false;
                spawnNearPlayer = false;
                spawnNearRandomPlayer = false;
                lastSelected = selected;
                switch (selected)
                {
                    case 1:
                        randomSpawn = true;
                        break;
                    case 2:
                        spawnNearPlayer = true;
                        break;
                    case 3:
                        spawnNearRandomPlayer = true;
                        break;
                }
            }
            
            GUI.Label(new Rect(wr.x, y, wr.width, 25), "Outside Enemies");
            foreach(KeyValuePair<string,EnemyType> enemy in TrollMenu.outsideEnemies)
            {
                string name = enemy.Key;
                EnemyType type = enemy.Value;
                if (GUI.Button(new Rect(wr.x, wr.y + y, wr.width, 25), name))
                {
                    if(randomSpawn)
                    {
                        TrollConsole.DisplayMessage("Spawning " + name + " at random position");
                        TrollMenu.SpawnOutsideEnemy(type);
                    }else if(spawnNearPlayer)
                    {
                        
                        TrollMenu.SpawnEnemyOutsideNearPlayer(type, currentPlayer);
                    }
                    else if (spawnNearRandomPlayer)
                    {
                        PlayerControllerB ply = GetRandomPlayer();
                        if (ply == null)
                        {
                            if (currentPlayer == null)
                            {
                                ply = GetPlayer()[0];//There is minimum one player because you can't be in game without a player
                            }
                            else
                            {
                                ply = currentPlayer;
                            }
                        }
                        TrollMenu.SpawnEnemyOutsideNearPlayer(type, ply);
                    }

                }
                y += 30;
            }
            y += 25;
            GUI.Label(new Rect(wr.x, wr.y + y, wr.width, 25), "Inside Enemies");
            y +=25;
            foreach (KeyValuePair<string, EnemyType> enemy in TrollMenu.insideEnemies)
            {
                string name = enemy.Key;
                EnemyType type = enemy.Value;
                if (GUI.Button(new Rect(wr.x, wr.y + y, wr.width, 25), name))
                {
                    if(randomSpawn)
                    {
                        TrollMenu.SpawnInsideEnemy(type);
                    }
                    else if (spawnNearPlayer)
                    {
                        TrollMenu.SpawnEnemyInsideNearPlayer(type, currentPlayer);
                    }else if(spawnNearRandomPlayer)
                    {
                        PlayerControllerB ply = GetRandomPlayer();
                        if(ply == null)
                        {
                            if(currentPlayer == null)
                            {
                                ply = GetPlayer()[0];//There is minimum one player because you can't be in game without a player
                            }
                            else
                            {
                                ply = currentPlayer;
                            }
                        }
                        TrollMenu.SpawnEnemyInsideNearPlayer(type, ply);
                    }
                }
                y += 30;
            }
            GUI.EndScrollView();
            GUI.Box(new Rect(wr.x + wr.width, wr.y, wr.width, 30 * GetPlayer().Length), "Select a player");
            currentPlayer = playerSelect.Draw(new Rect(wr.x + wr.width, wr.y+25, wr.width, 25));
        }

        private static PlayerControllerB GetRandomPlayer()
        {
            PlayerControllerB[] players = GetPlayer();
            if(players == null || players.Length == 0)
            {
                return null;
            }
            return players[UnityEngine.Random.Range(0, players.Length)];
        }

        private static PlayerControllerB[] GetPlayer()
        {
            return StartOfRound.Instance.allPlayerScripts;
        }

        public static void CreatePlayerMenu(Rect wr)
        {
            int y = 0;
            int i = 0;
            GUI.Box(wr, "Player Menu");
            y += 25;
            PlayerControllerB[] players = GetPlayer();
            if(plyToggle.Count != players.Length)
            {
                plyToggle.Clear();
                lastToggleState.Clear();
                for (i = 0; i < players.Length; i++)
                {
                    plyToggle.Add(false);
                    lastToggleState.Add(false);
                }
            }
            if(players == null || players.Length == 0)
            {
                GUI.Label(new Rect(wr.x, wr.y + y, wr.width, 25), "No players found");
                return;
            }
            i = 0;
            foreach(PlayerControllerB player in players)
            {
                plyToggle[i] = GUI.Toggle(new Rect(wr.x, wr.y + y, wr.width, 25), plyToggle[i], player.playerUsername);
                y += 30;
                i++;
            }
            //We compare the last toggle state with the current one, if one changed to true we set all other to false
            for (i = 0; i < plyToggle.Count; i++)
            {
                if (plyToggle[i] != lastToggleState[i])
                {
                    if (plyToggle[i])
                    {
                        for (int j = 0; j < plyToggle.Count; j++)
                        {
                            if (j != i)
                            {
                                plyToggle[j] = false;
                            }
                        }
                        currentPlayer = players[i];
                    }
                    lastToggleState[i] = plyToggle[i];
                }
            }
            //If no player is selected we set the current player to null
            if (!plyToggle.Contains(true))
            {
                currentPlayer = null;
            }
        }

        private static void CreateAnomalyMenu(Rect wr)
        {

        }
    }
}
