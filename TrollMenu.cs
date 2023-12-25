using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;
using GameNetcodeStuff;
using System.Reflection;
using Unity.Netcode;
using LethalCompanyTrollMenuMod.Component;
using LethalCompanyTrollMenuMod.tabs;

namespace LethalCompanyTrollMenuMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class TrollMenu : BaseUnityPlugin
    {
        // Mod Details
        private const string modGUID = "Nico78916.TrollMenu";
        private const string modName = "Lethal Company Troll Menu";
        private const string modVersion = "1.0.0.0";

        public static ManualLogSource mls { get; private set; }

        private readonly Harmony harmony = new Harmony(modGUID);

        public static PlayerControllerB playerRef;

        internal static TabManager Window;

        private static GameObject obj;

        public static Dictionary<string, EnemyType> insideEnemies = new Dictionary<string, EnemyType>();
        public static Dictionary<string, EnemyType> outsideEnemies = new Dictionary<string, EnemyType>();
        public static Dictionary<string, AnomalyType> anomalies = new Dictionary<string, AnomalyType>();
        public static Dictionary<string, SpawnableMapObject> deadlyobjects = new Dictionary<string, SpawnableMapObject>();

        public static bool isInGame = false;

        public static EnemyVent[] enemyVentsCache = new EnemyVent[0];

        public static GameObject[] outsideSpawn = new GameObject[0];


        public static RoundManager roundManager;

        public static Dictionary<string, PlayerControllerB> alivePlayers = new Dictionary<string, PlayerControllerB>();
        public static Dictionary<string, PlayerControllerB> deadPlayers = new Dictionary<string, PlayerControllerB>();
        public static Dictionary<string, PlayerControllerB> allPlayers = new Dictionary<string, PlayerControllerB>();

        private static RandomMapObject[] randomMapObjects = new RandomMapObject[0];

        public static List<EnemyAI> aliveEnemies {
            get {

                if (!isInGame || roundManager == null) return new List<EnemyAI>();
                return roundManager.SpawnedEnemies;
            }
        }

        void Awake()
        {
            mls = BepInEx.Logging.Logger.CreateLogSource("TrollMenu");
            // Plugin startup logic
            mls.LogInfo($"Loaded {modGUID}. Patching.");
            harmony.PatchAll(typeof(TrollMenu));
            mls = Logger;
            mls.LogInfo("Creating Menu");
            obj = new UnityEngine.GameObject("TrollMenu");
            DontDestroyOnLoad(obj);
            obj.hideFlags = HideFlags.HideAndDontSave;
            obj.AddComponent<TabManager>();
            obj.AddComponent<TrollConsole>();
            Window = obj.GetComponent<TabManager>();

            TrollMenuStyle.Awake();
        }

        public EnemyAI[] FindAliveEnemies()
        {
            if (!isInGame) return new EnemyAI[0];
            return FindObjectsOfType<EnemyAI>();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPrefix]
        public static bool LoadNewLevelPatch(ref RoundManager __instance)
        {
            //Update playerList
            alivePlayers.Clear();
            deadPlayers.Clear();
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (!player.isPlayerDead)
                {
                    if(!alivePlayers.ContainsKey(player.playerUsername))
                        alivePlayers.Add(player.playerUsername, player);
                }
                else
                {
                    if(!deadPlayers.ContainsKey(player.playerUsername))
                        deadPlayers.Add(player.playerUsername, player);
                }
                if(!allPlayers.ContainsKey(player.playerUsername))
                    allPlayers.Add(player.playerUsername, player);
            }
            roundManager = __instance;
            roundManager.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            //Get all the spawnable enemies
            EnemyType[] instances = Resources.FindObjectsOfTypeAll<EnemyType>();
            foreach (EnemyType enemy in instances)
            {
                if (enemy.isOutsideEnemy)
                {                  
                    if(!outsideEnemies.ContainsKey(enemy.name))
                    outsideEnemies.Add(enemy.name, enemy);
                }
                else
                {
                    if(!insideEnemies.ContainsKey(enemy.name))
                    insideEnemies.Add(enemy.name, enemy);
                }
                mls.LogInfo("Added " + enemy.name + " (" + (enemy.isOutsideEnemy ? "outside" : "inside") + ") to the list");
            }
            mls.LogInfo("Found " + instances.Length + " EnemyType");
            foreach (EnemyType type in instances)
            {
                mls.LogInfo("\t" + type.enemyName);
            }
            return true;
        }


        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPrefix]
        public static bool SpawnScrapInLevelPatch(ref RoundManager __instance)
        {
            if (ScrapMenu.randomValue)
            {
                TrollMenu.mls.LogError("Random Scrap Value is activated");
                TrollMenu.mls.LogInfo("Min Scrap Value: " + ScrapMenu.minScrapValue);
                TrollMenu.mls.LogInfo("Max Scrap Value: " + ScrapMenu.maxScrapValue);
                __instance.currentLevel.maxTotalScrapValue = ScrapMenu.maxScrapValue;
                __instance.currentLevel.minTotalScrapValue = ScrapMenu.minScrapValue;
            }
            if (ScrapMenu.randomAmount)
            {
                TrollMenu.mls.LogError("Random Scrap Amount is activated");
                TrollMenu.mls.LogInfo("Min Scrap: " + ScrapMenu.minScrap);
                TrollMenu.mls.LogInfo("Max Scrap: " + ScrapMenu.maxScrap);
                __instance.currentLevel.minScrap = ScrapMenu.minScrap;
                __instance.currentLevel.maxScrap = ScrapMenu.maxScrap;
            }
            if (!ScrapMenu.randomAmount && !ScrapMenu.randomValue)
            {
                TrollMenu.mls.LogError("No Scrap changes detected");
            }

            return true;
        }


        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GeneratedFloorPostProcessing))]
        [HarmonyPostfix]
        public static void GeneratedFloorPostProcessingPatch(ref RoundManager __instance)
        {
            randomMapObjects = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();
            mls.LogError("Found " + randomMapObjects.Length + " RandomMapObject");
            SpawnableMapObject[] spawnableMapObjects = __instance.spawnableMapObjects;
            mls.LogInfo("Found " + spawnableMapObjects.Length + " SpawnableMapObject");
            foreach (SpawnableMapObject spawnableMapObject in spawnableMapObjects)
            {
                mls.LogInfo("Found " + spawnableMapObject.prefabToSpawn.name);
                if(!deadlyobjects.ContainsKey(spawnableMapObject.prefabToSpawn.name))
                deadlyobjects.Add(spawnableMapObject.prefabToSpawn.name, spawnableMapObject);
            }
        }

        
        private static void SpawnInsideEnemy(EnemyType enemy, EnemyVent vent)
        {
            vent.enemyType = enemy;
            roundManager.SpawnEnemyFromVent(vent);
        }

        public static void SpawnInsideEnemy(EnemyType enemy)
        {
            //Get all the vents
            enemyVentsCache = FindObjectsOfType<EnemyVent>();
            if (enemy == null)
            {
                mls.LogError("Enemy is null");
                return;
            }
            if (enemy.enemyPrefab == null)
            {
                mls.LogError("Enemy Prefab is null");
                return;
            }
            if (enemy.isOutsideEnemy)
            {
                mls.LogError("Enemy is outside enemy");
                return;
            }
            if (enemyVentsCache.Length == 0)
            {
                mls.LogError("No vents found");
                return;
            }
            EnemyVent vent = enemyVentsCache[Random.Range(0, enemyVentsCache.Length)];
            if (vent == null)
            {
                mls.LogError("Vent is null");
                return;
            }
            SpawnInsideEnemy(enemy, vent);
        }

        public static void SpawnOutsideEnemy(EnemyType enemy)
        {
            //Get all the spawn points
            outsideSpawn = GameObject.FindGameObjectsWithTag("OutsideAINode");
            if (enemy == null)
            {
                mls.LogError("Enemy is null");
                return;
            }
            if (enemy.enemyPrefab == null)
            {
                mls.LogError("Enemy Prefab is null");
                return;
            }
            if (!enemy.isOutsideEnemy)
            {
                mls.LogError("Enemy is inside enemy");
                return;
            }
            if (outsideSpawn.Length == 0)
            {
                mls.LogError("No outside spawn found");
                return;
            }
            GameObject[] spawnPoints = outsideSpawn;
            if (spawnPoints == null)
            {
                mls.LogError("No spawnPoints");
                return;
            }
            Vector3 positionInRadius = roundManager.GetRandomNavMeshPositionInRadius(spawnPoints[roundManager.AnomalyRandom.Next(0, spawnPoints.Length)].transform.position, 4f);
            int index1 = 0;
            bool flag = false;
            for (int index2 = 0; index2 < spawnPoints.Length - 1; ++index2)
            {
                for (int index3 = 0; index3 < roundManager.spawnDenialPoints.Length; ++index3)
                {
                    flag = true;
                    if ((double)Vector3.Distance(positionInRadius, roundManager.spawnDenialPoints[index3].transform.position) < 16.0)
                    {
                        index1 = (index1 + 1) % spawnPoints.Length;
                        positionInRadius = roundManager.GetRandomNavMeshPositionInRadius(spawnPoints[index1].transform.position, 4f);
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    break;
            }
            SpawnOutsideEnemy(enemy, positionInRadius);
        }
        private static void SpawnOutsideEnemy(EnemyType enemy, Vector3 position)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(enemy.enemyPrefab, position, Quaternion.Euler(Vector3.zero));
            gameObject.gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            roundManager.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
            ++gameObject.GetComponent<EnemyAI>().enemyType.numberSpawned;
        }


        public static void SpawnEnemyInsideNearPlayer(EnemyType enemy, PlayerControllerB player)
        {
            //Get all the vents
            enemyVentsCache = FindObjectsOfType<EnemyVent>();
            if (enemy == null)
            {
                mls.LogError("Enemy is null");
                return;
            }
            if (enemy.enemyPrefab == null)
            {
                mls.LogError("Enemy Prefab is null");
                return;
            }
            if (enemy.isOutsideEnemy)
            {
                mls.LogError("Enemy is outside enemy");
                return;
            }
            if (enemyVentsCache.Length == 0)
            {
                mls.LogError("No vents found");
                return;
            }
            //Check for player is inside
            if (!player.isInsideFactory)
            {
                mls.LogError("The player isn't inside");
                return;
            }
            //Get the closest vent
            EnemyVent vent = enemyVentsCache.OrderBy(x => Vector3.Distance(x.transform.position, player.transform.position)).First();
            if (vent == null)
            {
                mls.LogError("Vent is null");
                return;
            }
            SpawnInsideEnemy(enemy, vent);
        }
        public static void SpawnEnemyOutsideNearPlayer(EnemyType enemy, PlayerControllerB player)
        {
            //Get all the spawn points
            outsideSpawn = GameObject.FindGameObjectsWithTag("OutsideAINode");
            if (enemy == null)
            {
                mls.LogError("Enemy is null");
                return;
            }
            if (enemy.enemyPrefab == null)
            {
                mls.LogError("Enemy Prefab is null");
                return;
            }
            if (!enemy.isOutsideEnemy)
            {
                mls.LogError("Enemy is inside enemy");
                return;
            }
            if (outsideSpawn.Length == 0)
            {
                mls.LogError("No outside spawn found");
                return;
            }
            GameObject[] spawnPoints = outsideSpawn;
            if (spawnPoints == null)
            {
                mls.LogError("No spawnPoints");
                return;
            }
            //Get the closest spawn point
            Vector3 position = spawnPoints.OrderBy(x => Vector3.Distance(x.transform.position, player.transform.position)).First().transform.position;
            Vector3 positionInRadius = roundManager.GetRandomNavMeshPositionInRadius(position, 4f);
            int index1 = 0;
            bool flag = false;
            for (int index2 = 0; index2 < spawnPoints.Length - 1; ++index2)
            {
                for (int index3 = 0; index3 < roundManager.spawnDenialPoints.Length; ++index3)
                {
                    flag = true;
                    if ((double)Vector3.Distance(positionInRadius, roundManager.spawnDenialPoints[index3].transform.position) < 16.0)
                    {
                        index1 = (index1 + 1) % spawnPoints.Length;
                        positionInRadius = roundManager.GetRandomNavMeshPositionInRadius(spawnPoints[index1].transform.position, 4f);
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    break;
            }
            SpawnOutsideEnemy(enemy, positionInRadius);
        }

        public static void SpawnHostileObjectAtPosition(SpawnableMapObject obj, Vector3 pos)
        {

            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(obj.prefabToSpawn, pos, Quaternion.identity, roundManager.mapPropsContainer.transform);
            gameObject.transform.eulerAngles = !obj.spawnFacingAwayFromWall ? new Vector3(gameObject.transform.eulerAngles.x, (float)roundManager.AnomalyRandom.Next(0, 360), gameObject.transform.eulerAngles.z) : new Vector3(0.0f, roundManager.YRotationThatFacesTheFarthestFromPosition(pos + Vector3.up * 0.2f), 0.0f);
            gameObject.GetComponent<NetworkObject>().Spawn(true);
            TrollConsole.DisplayMessage("Spawned " + obj.prefabToSpawn.name,MessageType.SUCCESS);
        }



        public static void SpawnHostileObjectAtRandomPos(SpawnableMapObject obj)
        {
            //On récupère tous les insideAINodes
            GameObject[] insideAINodes = roundManager.insideAINodes;
            if(obj == null)
            {
                mls.LogError("SpawnableMapObject is null");
                return;
            }
            GameObject randomMapObject = insideAINodes[roundManager.AnomalyRandom.Next(0, insideAINodes.Length)];
            if (randomMapObject == null)
            {
                mls.LogError("RandomMapObject is null");
                return;
            }
            Vector3 positionInRadius = roundManager.GetRandomNavMeshPositionInRadius(randomMapObject.transform.position, 100f);
            SpawnHostileObjectAtPosition(obj, positionInRadius);
        }
        public static void SpawnHostileObjectNearPlayer(SpawnableMapObject obj,PlayerControllerB ply)
        {
            if(!ply.isInsideFactory)
            {
                mls.LogError("Player isn't inside the factory");
                return;
            }
            SpawnHostileObjectNearPlayer(obj, ply,50);
        }
        public static void SpawnHostileObjectNearPlayer(SpawnableMapObject obj, PlayerControllerB ply,int range)
        {
            if (!ply.isInsideFactory)
            {
                mls.LogError("Player isn't inside the factory");
                TrollConsole.DisplayMessage("Player isn't inside the factory", MessageType.ERROR);
                return;
            }
            Vector3 positionInRadius = roundManager.GetRandomNavMeshPositionInRadius(ply.transform.position, (float)range);
            TrollConsole.DisplayMessage("Position of "+obj.prefabToSpawn.name + " = " +positionInRadius);
            TrollConsole.DisplayMessage("Spawning " + obj.prefabToSpawn.name + " at "+ Vector3.Distance(positionInRadius,ply.transform.position) +"m from "+ ply.playerUsername);
            SpawnHostileObjectAtPosition(obj, positionInRadius);

        }
    }
}