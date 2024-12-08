using BepInEx.Logging;
using WebSocketSharp.Server;

namespace LiveDataLogger
{
    public class LdlServer : WebSocketBehavior
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("[LDL-Server]");

        protected override void OnOpen()
        {
            Logger.LogInfo("Client Connected.");
        }
    }
}
