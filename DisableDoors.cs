using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Il2CppTLD.Interactions;
using ModSettings;

namespace DisableDoors
{
    internal class DisableDoorsSettings : JsonModSettings
    {
        internal static DisableDoorsSettings? Instance;
        [Name("Disable Doors")]
        [Description("Enables the mod")]
        public bool DisableDoors = true;
    }
    public class Main : MelonMod 
    {
        public override void OnInitializeMelon()
        {
            DisableDoorsSettings.Instance = new DisableDoorsSettings(); 
            DisableDoorsSettings.Instance.AddToModSettings("Disable Doors");
        }
    }

    [HarmonyPatch(typeof(TimedHoldInteraction), "PerformHold")]
    public static class Patch_TimedHoldInteraction_PerformHold
    {
        static bool Prefix(TimedHoldInteraction __instance)
        {
            if (DisableDoorsSettings.Instance?.DisableDoors != true) return true;

            if (__instance?.gameObject?.name == "InteriorLoadTrigger")
                return false;

            return true;
        }
    }
}

