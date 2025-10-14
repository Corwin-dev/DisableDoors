using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Il2CppTLD.Interactions;

namespace DisableDoors
{
    public class Main : MelonMod {}

    [HarmonyPatch(typeof(BaseInteraction), "InitializeInteraction")]
    public static class Patch_BaseInteraction_InititalizeInteraction
    {
        static void Prefix(BaseInteraction __instance)
        {
            if (__instance.gameObject.transform.name == "InteriorLoadTrigger")
            {
                __instance.CanInteract = false;
            }
        }
    }
}
