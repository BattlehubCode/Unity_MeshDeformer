using UnityEngine;
using UnityEditor;

namespace Battlehub.Integration
{
    public partial class CommandsPanel : EditorWindow
    {
        partial void DoMeshDeformerSection();
        partial void DoMeshToolsSection();
        partial void OnGUI();
        public Vector2 scrollPosition;
        public const string IconPath = "Assets/Battlehub/Integration/Editor/icon.png";

        partial void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            
            GUILayout.Label("Mesh", EditorStyles.boldLabel);
            DoMeshToolsSection();

            GUI.enabled = true;
            GUILayout.Label("Mesh Deformer", EditorStyles.boldLabel);
            DoMeshDeformerSection();
            GUILayout.EndScrollView();
        }

        private void OnSelectionChanged()
        {
            Repaint();
        }

    }

}
