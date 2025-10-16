using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Il2CppTLD.Interactions;
using ModSettings;

namespace DisableDoors
{
    internal class Settings : JsonModSettings
    {
        internal static Settings? Instance;
        [Name("Disable Doors")]
        [Description("Enables the mod")]
        public bool DisableDoors = true;
    }
    public class Main : MelonMod 
    {
        public override void OnInitializeMelon()
        {
            Settings.Instance = new Settings();
            Settings.Instance.AddToModSettings("Disable Doors");
        }
    }

    [HarmonyPatch(typeof(TimedHoldInteraction), "PerformHold")]
    public static class Patch_TimedHoldInteraction_PerformHold
    {
        static bool Prefix(TimedHoldInteraction __instance)
        {
            if (Settings.Instance?.DisableDoors != true) return true;

            if (__instance?.gameObject?.name == "InteriorLoadTrigger")
                return false;

            return true;
        }
    }
}

