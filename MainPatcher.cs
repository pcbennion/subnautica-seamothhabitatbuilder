using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;
using Harmony;

namespace SeamothHabitatBuilder  
{
    public class MainPatcher
    {
        // The new tech type
        public static TechType SeamothBuilderModule;

        // Effects and other stuff to copy from the Builder tool
        public static Transform nozzleLeft;
        public static Transform nozzleRight;
        public static Transform beamLeft;
        public static Transform beamRight;
        public static Animator animator;
        public static FMOD_CustomLoopingEmitter buildSound;
        public static FMODAsset completeSound;

        public static void Patch()
        {
            try
            {
                SMLHelper.V2.Handlers.PrefabHandler.

                // Hook up with harmony
                var harmony = HarmonyInstance.Create("com.standpeter.seamothhabitatbuilder");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                // Create TechType and register its inventory icon
                SeamothBuilderModule = TechTypeHandler.AddTechType("SeamothBuilderModule", "Seamoth habitat builder", "Allows the Seamoth to perform habitat construction tasks.");
                SpriteHandler.RegisterSprite(SeamothBuilderModule, "QMods/SeamothHabitatBuilder/Assets/SeamothBuilderModule.png");

                // Create blueprint
                TechData blueprint = new TechData
                {
                    craftAmount = 1,
                    Ingredients = new List<Ingredient>(2)
                    {
                        new Ingredient(TechType.Builder, 1),
                        new Ingredient(TechType.AdvancedWiringKit, 1)
                    }
                };
                CraftDataHandler.SetTechData(SeamothBuilderModule, blueprint);

                // Make the item craftable
                const string subTree = "Seamoth modules";
                CraftTreeHandler.AddCraftingNode(CraftTree.Type.SeamothUpgrades, SeamothBuilderModule, subTree);
                KnownTechHandler.UnlockOnStart(SeamothBuilderModule);

                // Set how the new item gets used
                CraftDataHandler.AddToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades, SeamothBuilderModule);
                CraftDataHandler.SetQuickSlotType(SeamothBuilderModule, QuickSlotType.Selectable);
                CraftDataHandler.SetEquipmentType(SeamothBuilderModule, EquipmentType.SeamothModule);

                // Register the prefab
                PrefabHandler.RegisterPrefab()

                Console.WriteLine("[SeamothHabitatBuilder] Succesfully patched!");
            }
            catch (Exception e)
            {
                Console.WriteLine("[SeamothHabitatBuilder] Caught exception! " + e.InnerException.Message);
                Console.WriteLine(e.InnerException.StackTrace);
            }

        }
    }
}
