using HarmonyLib;
using MelonLoader;
using UnityEngine;
using ModSettings;
using Il2CppTLD.Interactions;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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
        public bool DisableSwingingDoors = false; 

        [Name("Disable Vehicle Doors")]
        [Description("This disables doors for cars and trucks")]
        public bool DisableVehicleDoors = false;

        [Name("Disable Riken Entrance")]
        [Description("This disables the main entrance to the Riken")]
        public bool DisableRiken = true;
    }

    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Settings.Instance = new Settings();
            Settings.Instance.AddToModSettings("Disable Doors");

            // Hook the scene loaded event
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>)OnSceneLoad;
        }

        public void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            //MelonLogger.Msg($"Scene Loaded: {scene.name}, Root objects: {scene.rootCount}");
        }
    }

    [HarmonyPatch(typeof(TimedHoldInteraction), "PerformHold")]
    public static class Patch_TimedHoldInteraction_PerformHold
    {
        static bool Prefix(TimedHoldInteraction __instance)
        {
            if (Settings.Instance?.DisableDoors != true) return true;

            GameObject obj = __instance.gameObject;
            string name = obj.name;
            if (string.IsNullOrEmpty(name)) return true;

            MelonLogger.Msg($"[Interaction] GameObject name: {name}");

            // Define keyword arrays for each setting
            string[] interiorKeywords = { "InteriorLoadTrigger", "TRIGGER_" };
            string[] swingingKeywords = { "FishingCabin_Door", "ForestryLookout_Door", "PorchDoor", "StoneCabinADoor" };
            string[] vehicleKeywords = { "RightFrontDoor", "LeftFrontDoor", "RightRearDoor", "LeftRearDoor", "CrewSpeederDoor" };

            if (Settings.Instance.DisableInteriorLoadTrigger &&
                interiorKeywords.Any(k => name.Contains(k, System.StringComparison.OrdinalIgnoreCase)))
                return false;

            if (Settings.Instance.DisableSwingingDoors &&
                swingingKeywords.Any(k => name.Contains(k, System.StringComparison.OrdinalIgnoreCase)))
                return false;

            if (Settings.Instance.DisableVehicleDoors &&
                vehicleKeywords.Any(k => name.Contains(k, System.StringComparison.OrdinalIgnoreCase)))
                return false;

            if (Settings.Instance.DisableRiken)
            {
                //MelonLogger.Msg("[Debug] DisableRiken is enabled.");
                LoadingZone zone = obj.GetComponent<LoadingZone>();
                if (zone == null) return true;
                LoadScene scene = zone.m_PartnerLoadScene;
                if (scene == null) return true;
                //MelonLogger.Msg($"[Debug] LoadScene.m_SceneToLoad: {scene.m_SceneToLoad}");
                if (string.Equals(scene.m_SceneToLoad, "WhalingShipA", System.StringComparison.OrdinalIgnoreCase))
                {
                    //MelonLogger.Msg("[Interaction] Blocking access to Riken.");
                    return false;
                }
            }

            return true;
        }
    }
    [HarmonyPatch(typeof(LoadScene), "Activate")]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    public static class Patch_LoadScene_Activate
    {
        static bool Prefix(LoadScene __instance)
        {
            if (Settings.Instance?.DisableDoors != true) return true;
            GameObject obj = __instance.gameObject;
            string name = obj.name;
            if (string.IsNullOrEmpty(name)) return true;
            //MelonLogger.Msg($"[LoadScene] GameObject name: {name}");

            if (Settings.Instance.DisableRiken)
            {
                //MelonLogger.Msg("[Debug] DisableRiken is enabled.");
                if (string.Equals(__instance.m_SceneToLoad, "WhalingShipA", System.StringComparison.OrdinalIgnoreCase))
                {
                    //MelonLogger.Msg("[LoadScene] Blocking access to Riken.");
                    return false;
                }
            }
            return true;
        }
    }
}