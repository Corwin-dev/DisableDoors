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
            // MelonLogger.Msg($"[DisableDoors] Scene loaded: {sceneName}");
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
                List<string> chainNames = new List<string>();

                while (current != null)
                {
                    chainNames.Add(current.name);
                    foreach (string keyword in DoorKeywords)
                    {
                        if (current.name.ToLower().Contains(keyword.ToLower()))
                        {
                            // MelonLogger.Msg($"{current.name} matches {keyword}");
                            isDoor = true;
                            break;
                        }
                    }
                    if (isDoor) break;
                    current = current.parent;
                }
                chainNames.Reverse();
                string chain = string.Join(" -> ", chainNames);
                if (isDoor)
                {
                    // Disable only the interaction component
                    interaction.enabled = false;


                    // MelonLogger.Msg($"Disabled: {obj.name} | Chain: {chain}");
                }
                else
                {
                    // MelonLogger.Msg($"Not a door: {obj.name}");
                }
            }
        }
    }
}
