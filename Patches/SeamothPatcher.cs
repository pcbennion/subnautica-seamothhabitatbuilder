using SeamothHabitatBuilder.MonoBehaviors;
using Harmony;

namespace SeamothHabitatBuilder.Patches
{
    //=========================================================================
    // Seamoth Patcher
    // 
    // Uses Harmony to patch the Seamoth vehicle so it supports the new Builder
    // module
    //
    // Functions patched:
    // - OnUpgradeModuleToggle (postfix)
    // - Start                 (prefix)
    //=========================================================================

    // Helper clas for translating numerical IDs to the actual module slot
    public class SeamothSlots
    {
        // Copy-paste of SeaMoth._slotIDs.
        public static readonly string[] slotIDs = new string[]
        {
            "SeamothModule1",
            "SeamothModule2",
            "SeamothModule3",
            "SeamothModule4"
        };
    }

    //=========================================================================
    // SeaMoth.OnUpgradeModuleToggle - Postfix
    //
    // Patches the module selection code of the Seamoth so that the builder can
    // be activated
    //
    // Runs whenever a module on the Seamoth hotbar is selected or deselectd
    //=========================================================================
    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("OnUpgradeModuleToggle")]
    public class SeamothOnUpgradeModuleToggle {
        static void PostFix(SeaMoth instance, int slotID, bool active) {
            // Finds the tech type in the toggled slot
            TechType type = instance.modules.GetTechTypeInSlot(SeamothSlots.slotIDs[slotID]);
            // If it was the builder module, access the module and activate/deactivate it
            if (type == MainPatcher.SeamothBuilderModule) {
                SeamothBuilder tool = instance.GetComponent<SeamothBuilder>();
                if (tool) tool.enable = active;
            }
        } 
    }

    //=========================================================================
    // SeaMoth.Start - Prefix
    //
    // Adds the builder component to the Seamoth so its MonoBehaviour will run
    //
    // Runs when the Seamoth Gameobject is created by the engine
    //=========================================================================
    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("Start")]
    public class SeamothStart {
        static void Prefix(SeaMoth instance) {
            instance.gameObject.AddComponent<SeamothBuilder>();
        }
    }
}
