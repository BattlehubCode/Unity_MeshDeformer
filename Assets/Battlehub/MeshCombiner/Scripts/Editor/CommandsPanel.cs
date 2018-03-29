using UnityEngine;
using UnityEditor;
using Battlehub.MeshTools;

namespace Battlehub.Integration
{
    public partial class CommandsPanel : EditorWindow
    {
        [MenuItem("Tools/Mesh/Show Panel")]
        private static void Launch2()
        {
            EditorWindow window = GetWindow<CommandsPanel>();
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(IconPath);
            window.titleContent = new GUIContent("Tools", icon);
            window.Show();
        }

        partial void DoMeshToolsSection()
        {
            GUILayoutOption height = GUILayout.Height(30);
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUI.enabled = MeshToolsMenu.CanEditPivot();
                    if (GUILayout.Button("Edit Pivot", height))
                    {
                        MeshToolsMenu.EditPivot();
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    Transform selection = Selection.activeTransform;
                    PivotDesignTime pivotDesignTime = null;
                    if (selection != null)
                    {
                        pivotDesignTime = selection.GetComponent<PivotDesignTime>();
                    }

                    GUI.enabled = pivotDesignTime != null;
                    if (GUILayout.Button("BoundsCenter", height))
                    {
                        PivotPointEditor.ToBoundsCenter(pivotDesignTime);
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    Transform selection = Selection.activeTransform;
                    PivotDesignTime pivotDesignTime = null;
                    if (selection != null)
                    {
                        pivotDesignTime = selection.GetComponent<PivotDesignTime>();
                    }

                    GUI.enabled = pivotDesignTime != null;
                    if (GUILayout.Button("MassCenter", height))
                    {
                        PivotPointEditor.ToCenterOfMass(pivotDesignTime);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                GUI.enabled = MeshToolsMenu.CanCombine();
                if (GUILayout.Button("Combine", height))
                {
                    MeshToolsMenu.Combine();
                }

                GUI.enabled = MeshToolsMenu.CanSaveMesh();
                if (GUILayout.Button("Save", height))
                {
                    MeshToolsMenu.SaveMesh();
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}

