using Il2CppTLD.Interactions;

namespace DisableDoors
{
    public class Main : MelonMod 
    {
        internal static CustomGameSettings options = new();

        public override void OnInitializeMelon()
        {
            options = new CustomGameSettings();
            options.AddToCustomModeMenu(Position.BelowGear);
        }

        internal static readonly List<string> SwingingDoorObjectKeywords = new()
        {
            "FishingCabin_Door", "ForestryLookout_Door", "PorchDoor", "StoneCabinADoor", "Door_Mesh_LOD0", "WoodDoorInt"
        };

        internal static readonly List<string> VehicleDoorObjectKeywords = new()
        {
            "RightFrontDoor", "LeftFrontDoor", "RightRearDoor", "LeftRearDoor", "CrewSpeederDoor", "Helicopter_A_Door", "PlaneBeaver_Door_"
        };

        internal static readonly List<string> InteriorLoadingDoorKeywords = new()
        {
            "InteriorLoadTrigger", "TRIGGER_"
        };

        internal static readonly List<string> TransitionWhitelist = new()
        {
            "Transition", "Region"
        };

        internal static string GetPrefKey(string prefix, string saveName) => $"{prefix}_{saveName}";

        internal static void SaveSettings()
        {
            string saveName = SaveGameSystem.GetCurrentSaveName();
            PlayerPrefs.SetString(GetPrefKey("DisableInteriorLoadTrigger", saveName), options.DisableInteriorLoadTrigger.ToString());
            PlayerPrefs.SetString(GetPrefKey("DisableSwingingDoors", saveName), options.DisableSwingingDoors.ToString());
            PlayerPrefs.SetString(GetPrefKey("DisableVehicleDoors", saveName), options.DisableVehicleDoors.ToString());
            PlayerPrefs.SetString(GetPrefKey("DisableMines", saveName), options.DisableMines.ToString());
            PlayerPrefs.SetString(GetPrefKey("DisableCaves", saveName), options.DisableCaves.ToString());
            PlayerPrefs.Save();
        }

        internal static void DeleteSettings(string saveName)
        {
            if (saveName == null) return;
            if (saveName == SaveGameSlots.AUTOSAVE_SLOT_NAME) return;
            if (saveName == SaveGameSlots.QUICKSAVE_SLOT_PREFIX) return;
            PlayerPrefs.DeleteKey(GetPrefKey("DisableInteriorLoadTrigger", saveName));
            PlayerPrefs.DeleteKey(GetPrefKey("DisableSwingingDoors", saveName));
            PlayerPrefs.DeleteKey(GetPrefKey("DisableVehicleDoors", saveName));
            PlayerPrefs.DeleteKey(GetPrefKey("DisableMines", saveName));
            PlayerPrefs.DeleteKey(GetPrefKey("DisableCaves", saveName));
        }
    }

    internal class CustomGameSettings : ModSettingsBase
    {
        [Name("Disable Interior Loading Doors")]
        [Description("Disable doors that load to an interior location.")]
        public bool DisableInteriorLoadTrigger = false;

        [Name("Disable Swinging Doors")]
        [Description("Disable swinging doors that animate open and closed")]
        public bool DisableSwingingDoors = false;

        [Name("Disable Vehicle Doors")]
        [Description("Disable cab doors for motorized vehicles")]
        public bool DisableVehicleDoors = false;

        [Name("Disable Mines")]
        [Description("Disables mines except for transitions")]
        public bool DisableMines = false;

        [Name("Disable Caves")]
        [Description("Disables caves except for transitions")]
        public bool DisableCaves = false;
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LaunchSandbox))]
    internal class GameManager_LaunchSandbox_Patch
    {
        private static void Prefix()
        {
            Main.SaveSettings();
        }
    }

    [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.DeleteSaveFiles), new Type[] { typeof(string) })]
    internal class ModData_SaveGameSystem_DeleteSaveFiles
    {
        private static void Postfix(string name)
        {
            Main.DeleteSettings(name);
        }
    }

    [HarmonyPatch(typeof(LoadScene), "Awake")]
    public static class Patch_LoadScene_Awake
    {
        static void Prefix(LoadScene __instance)
        {
            if (ExperienceModeManager.GetCurrentExperienceModeType() != ExperienceModeType.Custom)
                return;

            if (__instance.gameObject == null)
            {
                MelonLogger.Msg("[DisableDoors] Skipped: gameObject is null.");
                return;
            }

            string sceneToLoad = __instance.m_SceneToLoad ?? string.Empty;
            string objectName = __instance.gameObject.name ?? string.Empty;

            // Early exit for whitelisted scenes
            if (Main.TransitionWhitelist.Any(wl => sceneToLoad.Contains(wl)))
            {
                MelonLogger.Msg($"Whitelisted: {objectName}, Scene: {sceneToLoad} (No disable/lock applied)");
                return;
            }

            string saveName = SaveGameSystem.GetCurrentSaveName();

            // Settings
            bool interiorEnabled = PlayerPrefs.HasKey("DisableInteriorLoadTrigger_" + saveName) && Convert.ToBoolean(PlayerPrefs.GetString("DisableInteriorLoadTrigger_" + saveName));
            bool minesEnabled = PlayerPrefs.HasKey("DisableMines_" + saveName) && Convert.ToBoolean(PlayerPrefs.GetString("DisableMines_" + saveName));
            bool cavesEnabled = PlayerPrefs.HasKey("DisableCaves_" + saveName) && Convert.ToBoolean(PlayerPrefs.GetString("DisableCaves_" + saveName));

            MelonLogger.Msg($"[DisableDoors] Settings: DisableInteriorLoadTrigger={interiorEnabled}, DisableMines={minesEnabled}, DisableCaves={cavesEnabled}");

            // Keywords
            bool matchesObjectKeyword = Main.InteriorLoadingDoorKeywords.Any(keyword => objectName.Contains(keyword));
            bool isMine = sceneToLoad.Contains("Mine");
            bool isCave = sceneToLoad.Contains("Cave");
            bool isWhalingShip = sceneToLoad.Contains("WhalingShip");

            // Lock logic for InteriorLoadingDoor matches only
            bool shouldDisable = interiorEnabled && matchesObjectKeyword;
            if (shouldDisable)
            {
                __instance.gameObject.SetActive(false);
                MelonLogger.Msg($"SetInactive: {objectName}, Scene: {sceneToLoad} (InteriorLoadingDoor match, not whitelisted)");
                return;
            }

            // Disable logic for mines, caves, and whaling ship (all gated by their settings)
            if ((minesEnabled && isMine) || (cavesEnabled && isCave) || (interiorEnabled && isWhalingShip))
            {
                __instance.gameObject.SetActive(false);
                MelonLogger.Msg($"SetInactive: {objectName}, Scene: {sceneToLoad} (Mine/Cave/WhalingShip match, not whitelisted)");
                return;
            }

            if (!shouldDisable)
            {
                MelonLogger.Msg($"Skipped {objectName}, Scene: {sceneToLoad}");
            }
        }
    }

    [HarmonyPatch(typeof(OpenClose), "Awake")]
    public static class Patch_SwingingDoor_Awake
    {
        static void Prefix(OpenClose __instance)
        {
            if (ExperienceModeManager.GetCurrentExperienceModeType() != ExperienceModeType.Custom) return;
            if (__instance.gameObject == null) return;

            string objectName = __instance.gameObject.name ?? string.Empty;
            string saveName = SaveGameSystem.GetCurrentSaveName();
            bool swingingDoorsEnabled = PlayerPrefs.HasKey(Main.GetPrefKey("DisableSwingingDoors", saveName)) &&
                Convert.ToBoolean(PlayerPrefs.GetString(Main.GetPrefKey("DisableSwingingDoors", saveName)));

            PatchHelper.DisableIfMatch(__instance, objectName, swingingDoorsEnabled, Main.SwingingDoorObjectKeywords, "SwingingDoor");
        }
    }

    [HarmonyPatch(typeof(VehicleDoor), "Awake")]
    public static class Patch_VehicleDoor_Awake
    {
        static void Prefix(VehicleDoor __instance)
        {
            if (ExperienceModeManager.GetCurrentExperienceModeType() != ExperienceModeType.Custom) return;
            if (__instance.gameObject == null) return;

            string objectName = __instance.gameObject.name ?? string.Empty;
            string saveName = SaveGameSystem.GetCurrentSaveName();
            bool vehicleDoorsEnabled = PlayerPrefs.HasKey(Main.GetPrefKey("DisableVehicleDoors", saveName)) &&
                Convert.ToBoolean(PlayerPrefs.GetString(Main.GetPrefKey("DisableVehicleDoors", saveName)));

            PatchHelper.DisableIfMatch(__instance, objectName, vehicleDoorsEnabled, Main.VehicleDoorObjectKeywords, "VehicleDoor");
        }
    }

    // Fix for the LoadingZone patch: use the correct property for null-coalescing
    [HarmonyPatch(typeof(LoadingZone), "Awake")]
    public static class Patch_LoadingZone_Awake
    {
        static void Prefix(LoadingZone __instance)
        {
            if (ExperienceModeManager.GetCurrentExperienceModeType() != ExperienceModeType.Custom)
                return;

            if (__instance.gameObject == null)
            {
                MelonLogger.Msg("[DisableDoors] Skipped LoadingZone: gameObject is null.");
                return;
            }

            LoadScene partnerScene = __instance.m_PartnerLoadScene;
            string objectName = __instance.gameObject.name ?? string.Empty;
            string saveName = SaveGameSystem.GetCurrentSaveName();

            // Check for InteriorLoading preference
            bool interiorEnabled = PlayerPrefs.HasKey(Main.GetPrefKey("DisableInteriorLoadTrigger", saveName)) &&
                Convert.ToBoolean(PlayerPrefs.GetString(Main.GetPrefKey("DisableInteriorLoadTrigger", saveName)));

            if (interiorEnabled && partnerScene != null && partnerScene.m_SceneToLoad.Contains("WhalingShip"))
            {
                __instance.gameObject.SetActive(false);
                MelonLogger.Msg($"SetInactive: {objectName}, PartnerScene: {partnerScene.m_SceneToLoad} (WhalingShip match)");
            }
        }
    }

    internal class PatchHelper
    {
        internal static void DisableIfMatch(TimedHoldInteraction interaction, string objectName, bool settingEnabled, IEnumerable<string> keywords, string logContext)
        {
            if (settingEnabled && keywords.Any(keyword => objectName.Contains(keyword)))
            {
                interaction.CanInteract = false;
                MelonLogger.Msg($"SetInactive: {objectName} ({logContext} match)");
            }
        }
    }
}