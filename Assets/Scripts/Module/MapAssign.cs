using UnityEngine;

namespace Ryneus
{
    public class MapAssign : MonoBehaviour
    {
        [SerializeField] private GameObject mapRoot = null;
        [SerializeField] private GameObject defaultScene = null;

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
                MapType.Default => defaultScene,
                _ => null,
            };
        }
    }

    public enum MapType
    {
        Default = 0,
    }
}
