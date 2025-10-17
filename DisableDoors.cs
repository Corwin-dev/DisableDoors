using HarmonyLib;
using Il2CppTLD.Interactions;
using MelonLoader;
using ModSettings;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace DisableDoors
{
    internal class Settings : JsonModSettings
    {
        internal static Settings? Instance;

        [Name("Disable Doors")]
        [Description("Enables the mod")]
        public bool DisableDoors = true;

        [Name("Disable Interior Loading Doors")]
        public bool DisableInteriorLoadTrigger = true;

        [Name("Disable Swinging Doors")]
        public bool DisableSwingingDoors = false;

        [Name("Disable Vehicle Doors")]
        public bool DisableVehicleDoors = false;

        [Name("Disable Riken Entrance")]
        public bool DisableRiken = true;
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
            var s = Settings.Instance;
            if (s?.DisableDoors != true) return true;

            var n = __instance.gameObject.name;
            if (string.IsNullOrEmpty(n)) return true;

            MelonLogger.Msg($"TimedHoldInteraction on {n}");

            if (s.DisableInteriorLoadTrigger && (n.StartsWith("InteriorLoadTrigger") || n.StartsWith("TRIGGER_")))
                return Block(__instance);

            if (s.DisableSwingingDoors && (n.Contains("FishingCabin_Door") || n.Contains("ForestryLookout_Door") || n.Contains("PorchDoor") || n.Contains("StoneCabinADoor") || n.Equals("Door_Mesh_LOD0") || n.Contains("WoodDoorInt")))
                return Block(__instance);

            if (s.DisableVehicleDoors && (n.Contains("RightFrontDoor") || n.Contains("LeftFrontDoor") || n.Contains("RightRearDoor") || n.Contains("LeftRearDoor") || n.Contains("CrewSpeederDoor") || n.Contains("Helicopter_A_Door") || n.Contains("PlaneBeaver_Door_")))
                return Block(__instance);

            if (s.DisableRiken)
            {
                var zone = __instance.gameObject.GetComponent<LoadingZone>();
                var scene = zone?.m_PartnerLoadScene;
                if (scene != null && scene.m_SceneToLoad == "WhalingShipA")
                    return Block(__instance);
            }

            return true;
        }

        static bool Block(TimedHoldInteraction i)
        {
            i._HoverText_k__BackingField = "Disabled";
            return false;
        }
    }

    [HarmonyPatch(typeof(LoadScene), "Activate", new[] { typeof(bool) })]
    public static class Patch_LoadScene_Activate
    {
        static bool Prefix(LoadScene __instance)
        {
            var s = Settings.Instance;
            if (s?.DisableDoors != true) return true;
            if (s.DisableRiken && __instance.m_SceneToLoad?.Equals("WhalingShipA", System.StringComparison.OrdinalIgnoreCase) == true)
                return false;
            return true;
        }
    }
}