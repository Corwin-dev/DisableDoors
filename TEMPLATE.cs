using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Il2CppTLD.Interactions;

namespace DisableDoors
{
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
            if (__instance == null || __instance.gameObject == null || DisableDoorsSettings.Instance == null)
                return true;

            if (DisableDoorsSettings.Instance.DisableDoors && __instance.gameObject.transform.name == "InteriorLoadTrigger")
            {
                return false;
            }
            return true;
        }
    }
}

