using BepInEx;
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
        public static ManualLogSource LogSource;

        // EFT:
        public static GameObject game;
        private GameWorld gameWorld;
        private Player myPlayer;

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

                    Logger.LogDebug("[LDL|DEBUG]: Tracking players...");

                    IEnumerable<Player> allPlayers = gameWorld.AllPlayersEverExisted;
                    foreach (Player player in allPlayers)
                    {
                        String playerName = player.name;

                        Logger.LogDebug("[LDL|DEBUG]: Player " + playerName);
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
    }
}
