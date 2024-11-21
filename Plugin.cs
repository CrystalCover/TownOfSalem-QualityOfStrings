using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static System.Reflection.BindingFlags;
using static System.String;
using static XMLStringTable;

namespace Eca.QualityOfStrings
{
    [BepInPlugin(GUID, Name, Version)]
    internal class Plugin : BaseUnityPlugin
    {
        private const string GUID = "Eca.QualityOfStrings";

        private const string Name = "QualityOfStrings";

        private const string Version = "1.0";

        private static Harmony Harmony { get; } = new Harmony(GUID);

        private void Awake() => Harmony.PatchAll();

        [HarmonyPatch(typeof(LocalizationService), nameof(LocalizationService.Load))]
        private class LocalizationServiceExtra
        {
            private static void Postfix(LocalizationService __instance)
            {
                string qosStringTableFile = Format("{0}.{1}.xml", Assembly.GetExecutingAssembly().Location.TrimEnd(".dll".ToCharArray()), GlobalServiceLocator.LocalizationService.GetCurrentUILanguageCode());
                if (!File.Exists(qosStringTableFile))
                    return;
                StringTable qosStringTable;
                try { qosStringTable = Load(File.ReadAllBytes(qosStringTableFile)); }
                catch { return; }
                Dictionary<string, string> tosStringTable = (Dictionary<string, string>)typeof(LocalizationService).GetField("stringTable_", Instance | Public | NonPublic).GetValue(__instance);
                Dictionary<string, string> tosStyleTable = (Dictionary<string, string>)typeof(LocalizationService).GetField("styleTable_", Instance | Public | NonPublic).GetValue(__instance);
                foreach (TextEntry entry in qosStringTable.entries)
                {
                    if (tosStringTable.ContainsKey(entry.key))
                        tosStringTable[entry.key] = entry.value;
                    else
                        tosStringTable.Add(entry.key, entry.value);
                    if (!IsNullOrEmpty(entry.style))
                        if (tosStyleTable.ContainsKey(entry.key))
                            tosStyleTable[entry.key] = entry.style;
                        else
                            tosStyleTable.Add(entry.key, entry.style);
                }
            }
        }

        [HarmonyPatch(typeof(LobbySceneChatListener), "HandleServerOnHowManyPlayersAndGames")]
        private class Reporter
        {
            private static void Postfix(LobbySceneChatListener __instance) => ((ChatController)typeof(LobbySceneChatListener).GetField("chatController", Instance | Public | NonPublic).GetValue(__instance)).AddMessage("<color=#631631>Quality of Strings</color>");
        }
    }
}