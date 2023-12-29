using BepInEx.Logging;
using GameNetcodeStuff;
using LethalCompanyTrollMenuMod.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalCompanyTrollMenuMod.tabs
{
    internal class AliveEnemies
    {
        private static Dictionary<string, int> modes = new Dictionary<string, int>()
        {
            {"Kill", 0 },
            {"Teleport to", 1 },
            {"Freeze", 2 },
            {"1 HP", 3 },
            {"Heal", 4 },
        };
        private static Select<int> modeSelect = null;
        private static int selectedMode = 0;
        private static PlayerControllerB selectedPlayer = null;
        private static Select<PlayerControllerB> playerSelect = null;
        private static Rect enemyListView = new Rect(0, 0, 100, 100);
        private static Rect enemyListViewRect = new Rect(0, 0, 100, 200);
        private static Vector2 enemyListViewVector = Vector2.zero;
        private static ManualLogSource log = TrollMenu.mls;

        private static Dictionary<EnemyAI, int> enemiesDefaultHP = new Dictionary<EnemyAI, int>();
        private static Dictionary<EnemyAI, float> enemiesDefaultSpeed = new Dictionary<EnemyAI, float>();
        public static Dictionary<EnemyAI, bool> frozenEnemies = new Dictionary<EnemyAI, bool>();
        public static Dictionary<EnemyAI, bool> teleportEnemy = new Dictionary<EnemyAI, bool>();

        public static void OnMenuOpened()
        {
            log.LogInfo("Menu opened");
            if(modeSelect == null)
            {
                log.LogWarning("modeSelect is null");
                modeSelect = new Select<int>(modes);
                modeSelect.SetDefault(0);
            }
            if (StartOfRound.Instance != null)
            {
                log.LogInfo("StartOfRound found");
            }
            else
            {
                log.LogInfo("StartOfRound not found");
                return;
            }
            
        }

        private static void trigger(EnemyAI enemy)
        {
            switch (selectedMode)
            {
                case 0:
                    Kill(enemy);
                    break;
                case 1:
                    TeleportToEnemy(enemy);
                    break;
                case 2:
                    Freeze(enemy);
                    break;
                case 3:
                    MakeOneHit(enemy);
                    break;
                case 4:
                    Heal(enemy);
                    break;
                case 5:
                    TeleportEnemyToPlayer(enemy, selectedPlayer);
                    break;
            }
        }

        public static void Draw(Rect wr)
        {
            if(wr == null)
            {
                TrollConsole.DisplayMessage("Error while drawing AliveEnemies tab", MessageType.ERROR);
                return;
            }
            int y = 50;
            GUI.Label(new Rect(wr.x, wr.y + y, wr.width, 25), "Alive Enemies");
            y += 25;
            #region Mode selection
                selectedMode = modeSelect.Draw(new Rect(wr.x, wr.y + y, 200, 25));
                y += modes.Count * 25;
            #endregion
            if (TrollMenu.roundManager == null) return;
            enemyListView = new Rect(wr.x, wr.y + y, wr.width, wr.height - y);
            enemyListViewRect = new Rect(0, 0, enemyListView.width, 25 * TrollMenu.roundManager.SpawnedEnemies.Count);
            enemyListViewVector = GUI.BeginScrollView(enemyListView, enemyListViewVector, enemyListViewRect);
            if (TrollMenu.roundManager.SpawnedEnemies.Count == 0)
            {
                GUI.Label(new Rect(0, 0, 200f, 30f), "No enemies alive", TrollMenuStyle.errorLabel);
                GUI.EndScrollView();
                return;
            }
            int ly = 0;
            foreach (EnemyAI enemy in TrollMenu.roundManager.SpawnedEnemies)
            {
                if (enemy.isEnemyDead) continue;
                if(!enemiesDefaultHP.ContainsKey(enemy))
                    enemiesDefaultHP.Add(enemy, enemy.enemyHP);
                if (!enemiesDefaultSpeed.ContainsKey(enemy))
                    enemiesDefaultSpeed.Add(enemy, enemy.agent.speed);
                if (GUI.Button(new Rect(0, ly, enemyListViewRect.width, 25), enemy.name.Replace("(Clone)"," "+Math.Round(Vector3.Distance(StartOfRound.Instance.localPlayerController.transform.position,enemy.serverPosition))+"m")))
                {
                    trigger(enemy);
                }
                ly += 25;
            }
            GUI.EndScrollView();
        }
        private static void Kill(EnemyAI enemy)
        {
            if(enemy.enemyType.canDie == false)
            {
                RoundManager.Instance.DespawnEnemyOnServer(enemy.NetworkObject);
                TrollConsole.DisplayMessage("Made " + enemy.name + " despawn", MessageType.SUCCESS);
                return;
            }
            enemy.KillEnemy(true);
            TrollConsole.DisplayMessage("Killed " + enemy.name, MessageType.SUCCESS);
        }

        private static void TeleportEnemyToPlayer(EnemyAI enemy, PlayerControllerB ply)
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
            Vector3 pos = TrollMenu.GetPositionBehind(ply.transform.position, ply.transform.forward, 10f);
            enemy.transform.position = pos;
            enemy.moveTowardsDestination = false;
            TrollConsole.DisplayMessage("Teleported " + enemy.name + " behind " + ply.playerUsername + " at "+Vector3.Distance(enemy.transform.position,ply.transform.position), MessageType.SUCCESS);
        }


        private static void TeleportToEnemy(EnemyAI enemy)
        {
            //Get the local player
            PlayerControllerB localPlayer = StartOfRound.Instance.localPlayerController;
            localPlayer.isInsideFactory = !enemy.isOutside;
            localPlayer.transform.position = TrollMenu.roundManager.GetRandomNavMeshPositionInRadius(enemy.transform.position, (float)10f);
            TrollConsole.DisplayMessage("Teleported " + localPlayer.playerUsername + " to " + enemy.name, MessageType.SUCCESS);
        }

        private static void Freeze(EnemyAI enemy)
        {
            
            if(frozenEnemies[enemy])
            {
                frozenEnemies[enemy] = false;
                TrollConsole.DisplayMessage("Unfroze " + enemy.name, MessageType.SUCCESS);
            }
            else
            {
                frozenEnemies[enemy] = true;
                TrollConsole.DisplayMessage("Froze " + enemy.name, MessageType.SUCCESS);

            }
                
        }
        private static void MakeOneHit(EnemyAI enemy)
        {
            enemy.enemyHP = 1;
            //enemy.enemyType.canDie = true;
            TrollConsole.DisplayMessage("Made " + enemy.name + " one hit to kill", MessageType.SUCCESS);
        }

        private static void Heal(EnemyAI enemy)
        {
            enemy.enemyHP = enemiesDefaultHP[enemy];
            TrollConsole.DisplayMessage("Healed " + enemy.name, MessageType.SUCCESS);
        }

    }

}
