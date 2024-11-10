using System.Collections.Generic;
using UnityEngine;
using UtageExtensions;

namespace Ryneus
{
    public class MapAssign : MonoBehaviour
    {
        [SerializeField] private GameObject mapRoot = null;
        [SerializeField] private GameObject defaultMap = null;
        [SerializeField] private GameObject battleMap = null;

        public GameObject CreateMap(MapType map)
        {
            var prefab = Instantiate(GetMapObject(map));
            prefab.transform.SetParent(mapRoot.transform, false);
            return prefab;
        }

        public void CreateMapObject(GameObject gameObject)
        {
            gameObject.transform.SetParent(mapRoot.transform, false);
        }

        private GameObject GetMapObject(MapType scene)
        {
            return scene switch
            {
                MapType.Default => defaultMap,
                MapType.Battle => battleMap,
                _ => null,
            };
        }
        
        public void ClearMap()
        {
            transform.DestroyChildren();
        }
    }

    public enum MapType
    {
        Default = 0,
        Battle,
    }
}
