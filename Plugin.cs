using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WatsonWebsocket;

namespace LiveDataLogger
{
    [BepInPlugin("info.rusty.spt.livedatalogger", "LiveDataLogger", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource = BepInEx.Logging.Logger.CreateLogSource("LiveDataLogger");
        private WatsonWsServer WsServer;

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

            // Start a Websocket Server:
            WsServer = new WatsonWsServer("localhost", 8001, false);
            if (WsServer.IsListening)
            {
                Logger.LogInfo("WsServer Started");
                StartCoroutine(Tracker());
            } else
            {
                Logger.LogInfo("WsServer failed to start.");
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
                            int count = WsServer.ListClients().Count();
                            if (count > 0)
                            {
                                Logger.LogInfo($"Sending Data to {count} clients.");
                                Logger.LogInfo($"{data}");
                                foreach (ClientMetadata client in WsServer.ListClients())
                                {
                                    WsServer.SendAsync(client.Guid, data).Wait();
                                }
                            } else
                            {
                                Logger.LogInfo("No Clients connected, not sending anything.");
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
