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

        public static bool isInGame = false;

        public static EnemyVent[] enemyVentsCache = new EnemyVent[0];

        public static GameObject[] outsideSpawn = new GameObject[0];

        public static RoundManager roundManager;



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
            Window = obj.GetComponent<TabManager>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            //store the current location of the assembly
            string location = assembly.Location;
            //We split the location to find "Lethal Company"
            string[] split = location.Split('\\');
            //Getting the index of "Lethal Company"
            int index = Array.IndexOf(split, "Lethal Company");
            //We create a new array with the length of the split array minus the index of "Lethal Company"
            List<string> newSplit = new List<string>();
            //We copy the split array into the newSplit array
            for (int i = 0; i <= index; i++)
            {
                newSplit.Add(split[i]);
            }
            //We add the \Lethal Company_Data\Managed\Assembly-CSharp.dll to the newSplit array
            newSplit.Add("Lethal Company_Data");
            newSplit.Add("Managed");
            newSplit.Add("Assembly-CSharp.dll");
            //We join the newSplit array to get the path to the Assembly-CSharp.dll
            string path = string.Join("\\", newSplit.ToArray());
            mls.LogInfo(path);
            //We load the Assembly-CSharp.dll
            Assembly cSharp = Assembly.LoadFile(path);
            //We get all the ...AI classes
            Type[] types = cSharp.GetTypes().Where(x => x.Name.EndsWith("AI")).ToArray();
            //We print it in log
            mls.LogInfo("Found " + types.Length + " AI");
            foreach (Type type in types)
            {
                mls.LogInfo("\t" + type.Name);
            }

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
            roundManager = __instance;
            roundManager.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            //Get all the spawnable enemies
            EnemyType[] instances = Resources.FindObjectsOfTypeAll<EnemyType>();
            foreach (EnemyType enemy in instances)
            {
                if (enemy.isOutsideEnemy)
                {
                    outsideEnemies.Add(enemy.name, enemy);
                }
                else
                {
                    insideEnemies.Add(enemy.name, enemy);
                }
                mls.LogInfo("Added " + enemy.name + " (" + (enemy.isOutsideEnemy ? "outside" : "inside") + ") to the list");
            }
            mls.LogInfo("Found " + instances.Length + " EnemyType");
            foreach (EnemyType type in instances)
            {
                mls.LogInfo("\t" + type.enemyName);
            }
            //Get all the anomalies
            AnomalyType[] anomalyTypes = Resources.FindObjectsOfTypeAll<AnomalyType>();
            foreach (AnomalyType anomaly in anomalyTypes)
            {
                anomalies.Add(anomaly.name, anomaly);
                mls.LogInfo("Added " + anomaly.name + " to the list");
            }
            SpawnableMapObject[] spawnableMapObjects = __instance.spawnableMapObjects;
            foreach (SpawnableMapObject spawnableMapObject in spawnableMapObjects)
            {
                mls.LogInfo("Found " + spawnableMapObject.prefabToSpawn.name);
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
        }

        public static void SpawnHostileObjectAtRandomPos(SpawnableMapObject obj)
        {
            RandomMapObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();
            List<RandomMapObject> randomMapObjectList = new List<RandomMapObject>();

            for (int index2 = 0; index2 < objectsOfType.Length; ++index2)
            {
                if (objectsOfType[index2].spawnablePrefabs.Contains(obj.prefabToSpawn))
                    randomMapObjectList.Add(objectsOfType[index2]);
            }
            RandomMapObject randomMapObject = randomMapObjectList[roundManager.AnomalyRandom.Next(0, randomMapObjectList.Count)];
            Vector3 positionInRadius = roundManager.GetRandomNavMeshPositionInRadius(randomMapObject.transform.position, randomMapObject.spawnRange);
            SpawnHostileObjectAtPosition(obj, positionInRadius);
        }
        public static void SpawnHostileObjectNearPlayer(SpawnableMapObject obj,PlayerControllerB ply)
        {
            RandomMapObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();
            List<RandomMapObject> randomMapObjectList = new List<RandomMapObject>();
            for (int index2 = 0; index2 < objectsOfType.Length; ++index2)
            {
                if (objectsOfType[index2].spawnablePrefabs.Contains(obj.prefabToSpawn))
                    randomMapObjectList.Add(objectsOfType[index2]);
            }
            //Sort by distance
            randomMapObjectList = randomMapObjectList.OrderBy(x => Vector3.Distance(x.transform.position, ply.transform.position)).ToList();
            RandomMapObject randomMapObject = randomMapObjectList.First();
            Vector3 positionInRadius = roundManager.GetRandomNavMeshPositionInRadius(randomMapObject.transform.position, randomMapObject.spawnRange);
            SpawnHostileObjectAtPosition(obj, positionInRadius);
        }
    }
}