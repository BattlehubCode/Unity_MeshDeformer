using UnityEditor;

namespace Battlehub.RTEditor
{
    [CustomEditor(typeof(RuntimeEditor))]
    public class RuntimeEditorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RuntimeEditor rtEditor = (RuntimeEditor)target;

            EditorGUI.BeginChangeCheck();
            //bool isOn = EditorGUILayout.Toggle("IsOn", rtEditor.IsOn);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    Undo.RecordObject(rtEditor, "Battlehub.RTEditor.IsOn");
            //    EditorUtility.SetDirty(rtEditor);
            //    rtEditor.IsOn = isOn;
            //}

            EditorGUI.BeginChangeCheck();
            int layer = EditorGUILayout.LayerField("RaycastLayer", rtEditor.RaycastLayer);
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(rtEditor, "Battlehub.RTEditor.RaycastLayer");
                EditorUtility.SetDirty(rtEditor);
                rtEditor.RaycastLayer = layer;
            }
        }

    }
}

