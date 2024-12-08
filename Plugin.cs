﻿using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp.Server;

namespace LiveDataLogger
{
    [BepInPlugin("info.rusty.spt.livedatalogger", "LiveDataLogger", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        private WebSocketServer _server;

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
            var _server = new WebSocketServer(8001);
            _server.AddWebSocketService<LdlServer>("/");
            _server.Start();
            if (_server.IsListening)
            {
                Logger.LogInfo("[LDL|INFO]: WSSV STarted.");
            }
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

                    Logger.LogInfo("[LDL|DEBUG]: Tracking players...");

                    IEnumerable<Player> allPlayers = gameWorld.AllPlayersEverExisted;
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

                            String data = pd.ToJson();
                            if (_server.WebSocketServices["/"].Sessions.Count > 0)
                            {
                                _server.WebSocketServices["/"].Sessions.Broadcast(data);
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    Logger.LogError($"{ex.Message}");
                }
            }
        }
        public static bool MapLoaded() => Singleton<GameWorld>.Instantiated;
        public class LdlServer : WebSocketBehavior
        {
            private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("[LDL-Server]");

            protected override void OnOpen()
            {
                Logger.LogInfo("Client Connected.");
            }
        }
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
    }
}
