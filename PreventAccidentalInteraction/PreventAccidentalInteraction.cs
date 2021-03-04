using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace PreventAccidentalInteraction
{
    [BepInPlugin("com.github.phantomgamers.ValheimPreventAccidentalInteraction", "PreventAccidentalInteraction", "1.0.3")]
    public class PreventAccidentalInteraction : BaseUnityPlugin
    {
        static readonly string defaultInteractionBlackList = "ItemStand,Sign,TeleportWorld";
        static public ConfigEntry<string> configInteractionBlacklist { get; private set; }

        void Awake()
        {
            configInteractionBlacklist = Config.Bind("General",
                "InteractionBlacklist",
                defaultInteractionBlackList,
                "Classes whose interactions should be limited");
            Harmony.CreateAndPatchAll(typeof(Patches));
        }
    }

    [HarmonyPatch]
    class Patches
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string s in PreventAccidentalInteraction.configInteractionBlacklist.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                yield return AccessTools.Method(AccessTools.TypeByName(s), "Interact");
            }
        }

        static void Prefix(Humanoid __0, ref bool __result, ref bool __runOriginal, ref ItemStand __instance)
        {
            if (!__0.IsCrouching() && __instance.m_guardianPower == null)
            {
                __result = false;
                __runOriginal = false;
            }
        }
    }
}
