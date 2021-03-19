using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace PreventAccidentalInteraction
{
    [BepInPlugin("com.github.phantomgamers.ValheimPreventAccidentalInteraction", "PreventAccidentalInteraction", "1.0.5")]
    public class PreventAccidentalInteraction : BaseUnityPlugin
    {
        static readonly string defaultInteractionBlocklist = "ItemStand,Sign,TeleportWorld";
        static readonly string defaultHoverTextAllowlist = "TeleportWorld";
        static public ConfigEntry<string> ConfigInteractionBlocklist { get; private set; }
        static public ConfigEntry<string> ConfigHoverTextAllowlist { get; private set; }
        static public ConfigEntry<bool> BlockGuardianStoneInteractions { get; private set; }
        static public ConfigEntry<bool> BlockHoverText { get; private set; }
        static public ConfigEntry<bool> BlockPortalHoverText { get; private set; }
        static public ConfigEntry<string> BlockInteractionMethod { get; private set; }
        static public ConfigEntry<string> BlockInteractionKey { get; private set; }
        static public string[] BlockInteractionKeys = default;
        static public string[] InteractionBlocklist = default;
        static public string[] HoverTextAllowlist = default;

        void Awake()
        {
            ConfigInteractionBlocklist = Config.Bind("General", "InteractionBlocklist", defaultInteractionBlocklist,
                                                     "Classes whose interactions should be limited");
            InteractionBlocklist = ConfigInteractionBlocklist.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            ConfigHoverTextAllowlist = Config.Bind("General", "HoverTextAllowlist", defaultHoverTextAllowlist,
                                                     "Classes whose hover text should not be limited");
            HoverTextAllowlist = ConfigHoverTextAllowlist.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            BlockGuardianStoneInteractions = Config.Bind("General", "BlockGuardianStoneInteractions", true, "Should Guardian Stone interactions be blocked?");
            BlockHoverText = Config.Bind("General", "BlockHoverText", true, "Should hover text be blocked?");
            BlockInteractionMethod = Config.Bind("General", "BlockInteractionMethod", "crouch",
                                                 "Which method should be used to detect whether an interaction should be allowed?\nValid values: crouch, keyheld");
            BlockInteractionKey = Config.Bind("General", "BlockInteractionKey", "left shift",
                                              "Key that should be held down when BlockInteractionMethod is set to keyheld\nuse https://docs.unity3d.com/Manual/ConventionalGameInput.html\nmultiple keys can be included, separated by commas (e.g. \"left shift, left ctrl\" will require left shift + left control be held down)");
            _ = Config.Bind<int>("General", "NexusID", 161, "Nexus mod ID for updates");
            BlockInteractionKeys = BlockInteractionKey.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public static bool ShouldBlockInteraction(ItemStand instance = default)
        {
            bool result = false;

         //   UnityEngine.Debug.Log("GuardianStone: " + (BlockGuardianStoneInteractions.Value == false
         //       && !instance.m_guardianPower.m_name.IsNullOrWhiteSpace()).ToString());
            if (BlockGuardianStoneInteractions.Value == false
                && !instance.m_guardianPower.m_name.IsNullOrWhiteSpace())
            {
                return false;
            }

           // UnityEngine.Debug.Log("keymethod: " + BlockInteractionMethod.Value);
            if(BlockInteractionMethod.Value == "keyheld")
            {
                foreach(string s in BlockInteractionKeys)
                {
                   // UnityEngine.Debug.Log("keyheld " + s + ": " + Input.GetKey(s).ToString());
                    result = !Input.GetKey(s) || result;
                }
            } else if(BlockInteractionMethod.Value == "crouch")
            {
               // UnityEngine.Debug.Log("crouching: " + Player.m_localPlayer.IsCrouching().ToString());
                result = !Player.m_localPlayer.IsCrouching();
            }

           // UnityEngine.Debug.Log("result: " + result.ToString());
            return result;
        }
    }

    [HarmonyPatch]
    class InteractPatch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string s in PreventAccidentalInteraction.InteractionBlocklist)
            {
                yield return AccessTools.Method(AccessTools.TypeByName(s), "Interact");
            }
        }

        static void Prefix(Humanoid __0, ItemStand __instance, ref bool __result, ref bool __runOriginal)
        {
            if (PreventAccidentalInteraction.ShouldBlockInteraction(__instance))
            {
                __result = false;
                __runOriginal = false;
            }
        }
    }

    [HarmonyPatch]
    class GetHoverTextPatch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            if (!PreventAccidentalInteraction.BlockHoverText.Value)
                yield break;

            var sList = PreventAccidentalInteraction.InteractionBlocklist.ToList();

            foreach(string s in PreventAccidentalInteraction.HoverTextAllowlist)
            {
                    sList.Remove(s);
            }

            foreach (string s in sList)
            {
                yield return AccessTools.Method(AccessTools.TypeByName(s), "GetHoverText");
            }
        }

        static void Prefix(ItemStand __instance, ref string __result, ref bool __runOriginal)
        {
           // UnityEngine.Debug.Log("ShouldBlock: " + PreventAccidentalInteraction.ShouldBlockInteraction().ToString());
            if (PreventAccidentalInteraction.ShouldBlockInteraction(__instance))
            {
                __result = String.Empty;
                __runOriginal = false;
            }
        }
    }
}
