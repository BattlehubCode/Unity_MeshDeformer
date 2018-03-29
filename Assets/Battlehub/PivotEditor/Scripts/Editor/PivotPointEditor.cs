using UnityEngine;
using UnityEditor;

namespace Battlehub.MeshTools
{
    [CustomEditor(typeof(PivotDesignTime))]
    public class PivotPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PivotDesignTime pivot = (PivotDesignTime)target;

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("To Center Of Mass"))
                {
                    ToCenterOfMass(pivot);
                }

                if (GUILayout.Button("To Bounds Center"))
                {
                    ToBoundsCenter(pivot);
                }
            }
            GUILayout.EndHorizontal();
        }

        public static void ToBoundsCenter(PivotDesignTime pivot)
        {
            Undo.RecordObject(pivot.transform, "Battlehub.PivotPointEditor.BoundsCenter");
            pivot.ToBoundsCenter();
            EditorUtility.SetDirty(pivot.transform);
        }

        public static void ToCenterOfMass(PivotDesignTime pivot)
        {
            Undo.RecordObject(pivot.transform, "Battlehub.PivotPointEditor.CenterOfMass");
            pivot.ToCenterOffMass();
            EditorUtility.SetDirty(pivot.transform);
        }


    }
}


