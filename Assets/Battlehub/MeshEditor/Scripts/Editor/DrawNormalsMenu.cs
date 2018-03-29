using UnityEngine;
using UnityEditor;

namespace Battlehub.ShowNormals
{
    public class DrawNormalsMenu
    {
        [MenuItem("Tools/Mesh/Normals/Show", validate =true)]
        public static bool CanShowNormals()
        {
            return Selection.gameObjects.Length > 0;
        }

        [MenuItem("Tools/Mesh/Normals/Show")]
        public static void ShowNormals()
        {
            foreach(GameObject go in Selection.gameObjects)
            {
                DrawNormals show = go.GetComponent<DrawNormals>();
                if(show == null)
                {
                    Undo.RegisterCreatedObjectUndo(go.AddComponent<DrawNormals>(), "Battlehub.ShowNormals");
                }
            }
        }

        [MenuItem("Tools/Mesh/Normals/Hide", validate = true)]
        public static bool CanHideNormals()
        {
            return Selection.gameObjects.Length > 0;
        }

        [MenuItem("Tools/Mesh/Normals/Hide")]
        public static void HideNormals()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                DrawNormals show = go.GetComponent<DrawNormals>();
                if (show != null)
                {
                    Undo.DestroyObjectImmediate(show);
                }
            }
        }
    }
}
