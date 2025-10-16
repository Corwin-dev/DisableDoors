using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Il2CppTLD.Interactions;
using ModSettings;
using UnityEngine.SceneManagement;
using System.Linq;

namespace DisableDoors
{
    internal class Settings : JsonModSettings
    {
        internal static Settings? Instance;

        [Name("Disable Doors")]
        [Description("Enables the mod")]
        public bool DisableDoors = true;

        [Name("Disable Interior Loading Doors")]
        [Description("This disables doors that load to an interior")]
        public bool DisableInteriorLoadTrigger = true;

        [Name("Disable Swinging Doors")]
        [Description("This disables doors that animate open or closed. ie Fishing Hut, Lookout Tower")]
        public bool DisablSwingingDoors = false;

        [Name("Disable Vehicle Doors")]
        [Description("This disables doors for cars and trucks")]
        public bool DisableVehicleDoors = false;

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

            string name = __instance?.gameObject?.name;
            if (string.IsNullOrEmpty(name)) return true;

            // Exploration: print names containing "door", "load", or "trigger" (case-insensitive)
            MelonLogger.Msg($"[Interaction] GameObject name: {name}");

            // Define keyword arrays for each setting
            string[] interiorKeywords = { "InteriorLoadTrigger", "TRIGGER_" };
            string[] swingingKeywords = { "FishingCabin_Door", "ForestryLookout_Door", "PorchDoor", "StoneCabinADoor" };
            string[] vehicleKeywords = { "RightFrontDoor", "LeftFrontDoor", "RightRearDoor", "LeftRearDoor", "CrewSpeederDoor" };

            if (Settings.Instance.DisableInteriorLoadTrigger &&
                interiorKeywords.Any(k => name.Contains(k, System.StringComparison.Ordinal)))
                return false;

            if (Settings.Instance.DisablSwingingDoors &&
                swingingKeywords.Any(k => name.Contains(k, System.StringComparison.Ordinal)))
                return false;

            if (Settings.Instance.DisableVehicleDoors &&
                vehicleKeywords.Any(k => name.Contains(k, System.StringComparison.Ordinal)))
                return false;

            return true;
        }
    }
}