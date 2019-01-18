using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper.V2.Assets;
using UnityEngine;

namespace SeamothHabitatBuilder.Prefabs
{
    class SeamothBuilderPrefab : ModPrefab
    {

        public SeamothBuilderPrefab(string classID, string prefabFileName, TechType techType)
        {
            this.ClassID = classID;
            this.PrefabFileName = prefabFileName;
            this.TechType = techType;
        }


        // G
        public override GameObject GetGameObject() {
            var prefab = Resources.Load<GameObject>("WorldEntities/Tools/SeamothElectricalDefense");
            var obj = GameObject.Instantiate<GameObject>(prefab);

            obj.GetComponent<TechTag>().type = this.TechType;
            obj.GetComponent<PrefabIdentifier>().ClassId = this.ClassID;

            return obj;
        }

    }
}
