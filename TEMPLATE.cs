using MelonLoader;
using UnityEngine;
using Il2CppTLD;
using Il2CppTLD.Interactions;
using System.Collections.Generic;

[assembly: MelonGame("Hinterland", "TheLongDark")]

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

                // Walk parent chain to see if it’s a door
                Transform current = obj.transform;
                bool isDoor = false;

                while (current != null)
                {
                    foreach (string keyword in DoorKeywords)
                    {
                        if (current.name.ToLower().Contains(keyword.ToLower()))
                        {
                            isDoor = true;
                            break;
                        }
                    }
                    if (isDoor) break;
                    current = current.parent;
                }
                if (isDoor)
                {
                    // Disable only the interaction component
                    interaction.enabled = false;
                }
            }
        }
    }
}

