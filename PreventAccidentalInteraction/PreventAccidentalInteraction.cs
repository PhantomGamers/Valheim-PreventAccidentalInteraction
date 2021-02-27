using BepInEx;

using HarmonyLib;

namespace PreventAccidentalInteraction
{
    [BepInPlugin("com.github.phantomgamers.ValheimPreventAccidentalInteraction", "PreventAccidentalInteraction", "1.0.1")]
    public class PreventAccidentalInteraction : BaseUnityPlugin
    {
        void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Patches));
        }
    }

    class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemStand), "Interact")]
        [HarmonyPatch(typeof(Sign), "Interact")]
        [HarmonyPatch(typeof(TeleportWorld), "Interact")]
        static void InteractPrefix(Humanoid __0, ref bool __result, ref bool __runOriginal)
        {
            if (!__0.IsCrouching())
            {
                __result = false;
                __runOriginal = false;
            }
        }
    }
}
