using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using Battlehub.Integration;


namespace Battlehub.MeshTools
{
    public partial class MeshToolsMenu
    {
        
        [MenuItem("Tools/Mesh/Remove Doubles", validate = true)]
        [MenuItem("Tools/Mesh/Combine", validate = true)]
        public static bool CanCombine()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                return false;
            }

            return selectedObjects.Any(so => so.GetComponent<MeshFilter>());
        }

        [MenuItem("Tools/Mesh/Remove Doubles")]
        public static void RemoveDoubles()
        {
            GameObject[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.Unfiltered | SelectionMode.ExcludePrefab).OfType<GameObject>().ToArray();

            for(int i = 0; i < selection.Length; ++i)
            {
                MeshFilter filter = selection[i].GetComponent<MeshFilter>();
                if(filter == null)
                {
                    continue;
                }

                GameObject duplicate = Object.Instantiate(filter.gameObject);
                Undo.RegisterCreatedObjectUndo(duplicate, "Battlehub.MeshTools.RemoveDoubles");
                duplicate.transform.SetParent(filter.transform.parent);
                duplicate.transform.position = filter.transform.position;
                duplicate.transform.rotation = filter.transform.rotation;
                duplicate.transform.localScale = filter.transform.localScale;
                duplicate.name = filter.gameObject.name;
                Undo.RecordObject(filter.gameObject, "Battlehub.MeshTools.RemoveDoubles");
                filter.gameObject.SetActive(false);

                MeshFilter duplicateFilter = duplicate.GetComponent<MeshFilter>();
                duplicateFilter.sharedMesh = MeshUtils.RemoveDoubles(duplicateFilter.sharedMesh);
                Selection.activeGameObject = duplicateFilter.gameObject;
            }
        }

        [MenuItem("Tools/Mesh/Combine")]
        public static void Combine()
        {
            GameObject activeSelected = Selection.activeTransform.gameObject;

            GameObject[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.Unfiltered | SelectionMode.ExcludePrefab).OfType<GameObject>().ToArray();
     
            CombineResult result = MeshUtils.Combine(selection, activeSelected);
            if(result != null)
            {
                MeshCombinerIntegration.RaiseCombined(result.GameObject, result.Mesh);
            }
        }
    }
}
