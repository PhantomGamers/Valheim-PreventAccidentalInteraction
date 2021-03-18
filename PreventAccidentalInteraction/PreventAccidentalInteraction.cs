using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace PreventAccidentalInteraction
{
    [BepInPlugin("com.github.phantomgamers.ValheimPreventAccidentalInteraction", "PreventAccidentalInteraction", "1.0.4")]
    public class PreventAccidentalInteraction : BaseUnityPlugin
    {
        static readonly string defaultInteractionBlackList = "ItemStand,Sign,TeleportWorld";
        static public ConfigEntry<string> ConfigInteractionBlacklist { get; private set; }
        static public ConfigEntry<bool> BlockGuardianStoneInteractions { get; private set; }

        void Awake()
        {
            ConfigInteractionBlacklist = Config.Bind("General",
                "InteractionBlacklist",
                defaultInteractionBlackList,
                "Classes whose interactions should be limited");
            BlockGuardianStoneInteractions = Config.Bind("General", "BlockGuardianStoneInteractions", true, "Should Guardian Stone interactions be blocked?");
            _ = Config.Bind<int>("General", "NexusID", 161, "Nexus mod ID for updates");
            Harmony.CreateAndPatchAll(typeof(Patches));
        }
    }

    [HarmonyPatch]
    class Patches
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string s in PreventAccidentalInteraction.ConfigInteractionBlacklist.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                yield return AccessTools.Method(AccessTools.TypeByName(s), "Interact");
            }
        }

        static void Prefix(Humanoid __0, ref bool __result, ref bool __runOriginal, ref ItemStand __instance)
        {
            if (!__0.IsCrouching() && (!PreventAccidentalInteraction.BlockGuardianStoneInteractions.Value || __instance.m_guardianPower.m_name.IsNullOrWhiteSpace()))
            {
                __result = false;
                __runOriginal = false;
            }
        }
    }
}
