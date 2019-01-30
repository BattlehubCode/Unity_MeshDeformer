using UnityEngine;
using UnityEditor;

namespace Battlehub.RTEditor
{
    public static class RTEditorMenu
    {
        const string root = "Battlehub/RTEditor/";

        public static GameObject InstantiateRuntimeEditor()
        {
            return InstantiatePrefab("RuntimeEditor.prefab");
        }

       
        public static GameObject InstantiatePrefab(string name)
        {
            Object prefab = AssetDatabase.LoadAssetAtPath("Assets/" + root + "Prefabs/" + name, typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }
    }

}
