﻿using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LiveDataLogger
{
    [BepInPlugin("info.rusty.spt.livedatalogger", "LiveDataLogger", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource = BepInEx.Logging.Logger.CreateLogSource("LiveDataLogger");
        StoreData sd;

        // EFT:
        public static GameObject game;
        private GameWorld gameWorld;
        private Player myPlayer;

        #pragma warning disable IDE0051 // Suppress not used error.
        private void Awake()
        {
            LogSource = Logger;
            LogSource.LogInfo("Plugin LiveDataLogger Loaded!");

            game = new GameObject();

            Logger.LogInfo("[LDL|INFO]: Config Loaded");
            Logger.LogInfo("[LDL|INFO]: Patches Loaded");

            StartCoroutine(Tracker());
        }
        IEnumerator Tracker()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f / 5.0f);

                try
                {
                    if (!MapLoaded())
                        continue; // Skip if we are not loaded into a map.

                    gameWorld = Singleton<GameWorld>.Instance;
                    myPlayer = gameWorld?.MainPlayer;

                    if (gameWorld == null || myPlayer == null || gameWorld.LocationId == "hideout")
                        continue; // Skip if in hideout

                    IEnumerable<Player> allPlayers = gameWorld.AllPlayersEverExisted;
                    try
                    {
                        sd = new StoreData();
                        sd.timestamp = (long)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);
                        sd.raidId = gameWorld.gameObject.GetInstanceID().ToString();
                        sd.map = gameWorld.LocationId;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Failed to create sd");
                    }
                    
                    foreach (Player player in allPlayers)
                    {
                        bool playerAlive = player.HealthController.IsAlive;
                        // Process player only if alive:
                        if (playerAlive)
                        {
                            PlayerData pd = new PlayerData
                            {
                                profileId = player.ProfileId,
                                name = player.Profile.Nickname,
                                level = player.Profile.Info.Level,
                                group = player?.AIData?.BotOwner?.BotsGroup?.Id ?? 0,
                                position = player.Position.normalized,
                                heading = player.LookDirection.normalized,
                                type = player.IsAI ? "BOT" : "HUMAN"
                            };
                            sd.players.Add(pd);
                        }
                    }
                    String data = sd.ToJson();
                    System.IO.File.WriteAllText("LiveDataLogger.log", data);
                }

                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    Logger.LogError($"{ex.Message}");
                }
            }
        }
        public static bool MapLoaded() => Singleton<GameWorld>.Instantiated;
        private class PlayerData
        {
            public String profileId { get; set; }
            public String name { get; set; }
            public int level { get; set; }
            public int group { get; set; }
            public Vector3 position { get; set; }
            public Vector3 heading { get; set; }
            public string type { get; set; }
        }
        private class StoreData
        {
            public StoreData()
            {
                this.players = new List<PlayerData>();
            }
            public long timestamp { get; set; }
            public String raidId { get; set; }
            public String map {  get; set; }
            public List<PlayerData> players { get; set; }
            public String ToJson(bool pretty = false)
            {
                if (pretty)
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
                }
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }
            public String ToPrettyJson()
            {
                return this.ToJson(true);
            }
        }
    }
}
