using SMLHelper.V2.Assets;
using UnityEngine;

namespace SeamothHabitatBuilder.Prefabs
{
    //=========================================================================
    // SeamothBuilderPrefab
    // 
    // Uses SMLHelper V2 to implement a prefab for the new Builder module on
    // the Seamoth. 
    //
    // Implements ModPrefab.
    //=========================================================================

    class SeamothBuilderPrefab : ModPrefab
    {
        //=====================================================================
        // Constructor
        //=====================================================================
        public SeamothBuilderPrefab(string classId, string prefabFileName, TechType techType = TechType.None) : base(classId, prefabFileName, techType)
        {
            this.ClassID = classId;
            this.PrefabFileName = prefabFileName;
            this.TechType = techType;
        }

        //=====================================================================
        // GetGameObject
        //
        // Provides a fully-featured GameObject for the SeamothBuilderModule
        // inventory item by copy-pasting most functionality from a stock
        // Seamoth module
        //=====================================================================
        public override GameObject GetGameObject() {
            // Find the Seamoth Electrical Defense module and create a fresh copy
            var prefab = Resources.Load<GameObject>("WorldEntities/Tools/SeamothElectricalDefense");
            var obj = GameObject.Instantiate<GameObject>(prefab);

            // Change the only two properties that matter for module inventory objects
            obj.GetComponent<TechTag>().type = this.TechType;
            obj.GetComponent<PrefabIdentifier>().ClassId = this.ClassID;

            return obj;
        }
    }
}
