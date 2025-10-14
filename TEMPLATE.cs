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

    [HarmonyPatch(typeof(BaseInteraction), "InitializeInteraction")]
    public static class Patch_BaseInteraction_InititalizeInteraction
    {
        static void Prefix(BaseInteraction __instance)
        {
            if (__instance.gameObject.transform.name == "InteriorLoadTrigger")
            {
                __instance.CanInteract = !DisableDoorsSettings.Instance.DisableDoors;
            }
        }
    }
}

