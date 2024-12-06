using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveDataLogger
{
    [BepInPlugin("info.rusty.spt.livedatalogger", "LiveDataLogger", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        private void Awake()
        {
            LogSource = Logger;
            LogSource.LogInfo("Plugin LiveDataLogger Loaded!");
        }
    }
}
