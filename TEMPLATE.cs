using MelonLoader;
using UnityEngine;
using Il2CppTLD;
using Il2CppTLD.Interactions;

namespace DisableDoors
{
    public class Main : MelonMod
    {
        private readonly string[] DoorKeywords = { "InteriorLoadTrigger" };

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            DisableDoorInteractions();
        }

        private void DisableDoorInteractions()
        {
            BaseInteraction[] interactions = UnityEngine.Object.FindObjectsOfType<BaseInteraction>();

            foreach (BaseInteraction interaction in interactions)
            {
                GameObject obj = interaction.gameObject;

                foreach (string keyword in DoorKeywords)
                {
                    if (current.name.ToLower().Contains(keyword.ToLower()))
                    {
                        interaction.enabled = false;
                        break;
                    }
                }
            }
        }
    }
}


