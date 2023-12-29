using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;
using LethalCompanyTrollMenuMod.helpers;
using System;
using System.Linq;

namespace LethalCompanyTrollMenuMod.tabs
{
    internal class SpawnMenu
    {
        private static Vector2 scrollViewVector = Vector2.zero;
        private static Rect scrollRect = new Rect(0, 0, 100, 100);
        public static PlayerControllerB currentPlayer = null;
        public static List<bool> plyToggle = new List<bool>();
        public static List<bool> lastToggleState = new List<bool>();
        public static Select<PlayerControllerB> playerSelect = null;
        private static Dictionary<string,int> spawnType = new Dictionary<string, int>()
        {
            {"Random Spawn",0 },
            {"Spawn Near Player",1 },
            {"Spawn Near Random Player",2 },
            {"Spawn Behind Player", 3 },
            {"Spawn Behind Random Player", 4 }
        };
        private static Select<int> spawnSelect = null;
        private static int selected = 0;

        public static void OnMenuOpened()
        {
            if (!TrollMenu.isInGame) return;
            if (playerSelect == null)
            {
                playerSelect = new Select<PlayerControllerB>(TrollMenu.allPlayers);
            }
            if(spawnSelect == null)
            {
                spawnSelect = new Select<int>(spawnType);
                spawnSelect.SetDefault(0);
            }

        }
        public static void Draw(Rect wr)
        {
            int y = 75;
            GUI.Label(new Rect(wr.x, y, wr.width, 25), "Spawn Menu");
            y += 25;
            selected = spawnSelect.Draw(new Rect(wr.x, y, 200, 25));
            y += 130;
            scrollRect = new Rect(0, 0, wr.width, 100 + 30 * TrollMenu.outsideEnemies.Count + 30 * TrollMenu.insideEnemies.Count);
            scrollViewVector = GUI.BeginScrollView(new Rect(wr.x, y, wr.width, wr.height-y), scrollViewVector, scrollRect);
            GUI.Label(new Rect(0, 0, wr.width, 25), "Outside Enemies");
            int ly = 25;
            foreach(KeyValuePair<string,EnemyType> enemy in TrollMenu.outsideEnemies)
            {
                string name = enemy.Key;
                EnemyType type = enemy.Value;
                if (GUI.Button(new Rect(0, ly, wr.width, 25), name))
                {
                    PlayerControllerB ply = GetRandomPlayer();
                    switch (selected)
                    {
                        case 1:
                            TrollMenu.SpawnEnemyOutsideNearPlayer(type, currentPlayer);
                            break;
                        case 2:
                            TrollMenu.SpawnEnemyOutsideNearPlayer(type, ply);
                            break;
                        case 3:
                            SpawnEnemyBehindPlayer(type, currentPlayer);
                            break;
                        case 4:
                            SpawnEnemyBehindPlayer(type, ply);
                            break;
                        default:
                            TrollMenu.SpawnOutsideEnemy(type);
                            break;
                    }

                }
                ly += 30;
            }
            ly += 25;
            GUI.Label(new Rect(0,ly, wr.width, 25), "Inside Enemies");
            ly += 25;
            foreach (KeyValuePair<string, EnemyType> enemy in TrollMenu.insideEnemies)
            {
                string name = enemy.Key;
                EnemyType type = enemy.Value;
                if (GUI.Button(new Rect(0, ly, wr.width, 25), name))
                {
                    PlayerControllerB ply = GetRandomPlayer();
                    switch (selected)
                    {
                        case 1 :
                            TrollMenu.SpawnEnemyInsideNearPlayer(type, currentPlayer);
                            break;
                        case 2:
                            TrollMenu.SpawnEnemyInsideNearPlayer(type, ply);
                            break;
                        case 3:
                            SpawnEnemyBehindPlayer(type, currentPlayer);
                            break;
                        case 4:
                            SpawnEnemyBehindPlayer(type, ply);
                            break;
                        default:
                            TrollMenu.SpawnInsideEnemy(type);
                            break;
                    }
                }
                ly += 30;
            }
            GUI.EndScrollView();
            GUI.Box(new Rect(wr.x + wr.width, wr.y, wr.width, 30 * GetPlayer().Length), "Select a player");
            if(TrollMenu.allPlayers.Count == 0)
            {
                GUI.Label(new Rect(wr.x + wr.width, wr.y + 25, wr.width, 25), "No players found");
                return;
            }
            currentPlayer = playerSelect.Draw(new Rect(wr.x + wr.width, wr.y+25, wr.width, 25));
        }

        private static PlayerControllerB GetRandomPlayer()
        {
            PlayerControllerB[] players = GetPlayer();
            var ply = players[TrollMenu.roundManager.AnomalyRandom.Next(0, players.Length)];
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
            return ply;
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
        private static void SpawnEnemyBehindPlayer(EnemyType enemy,PlayerControllerB ply)
        {
            if (enemy == null)
            {
                TrollConsole.DisplayMessage("No enemy selected", MessageType.ERROR);
                return;
            }
            if (ply == null)
            {
                TrollConsole.DisplayMessage("No player selected", MessageType.ERROR);
                return;
            }
            TrollConsole.DisplayMessage("Spawning " + enemy.name + " behind " + ply.playerUsername + "("+ (ply.isInsideFactory ? "Inside" : "Outside") + ")");
            Vector3 pos = TrollMenu.GetPositionBehind(ply.transform.position, ply.transform.forward, 10f);
            EnemyAI enemyAI = TrollMenu.SpawnEnemy(enemy, pos, !ply.isInsideFactory);
            enemyAI.destination = ply.transform.position;
            enemyAI.moveTowardsDestination = true;

        }
    }
}
