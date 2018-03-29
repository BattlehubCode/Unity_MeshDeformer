using UnityEngine;
using UnityEditor;
using Battlehub.MeshTools;
using Battlehub.MeshDeformer2;

namespace Battlehub.Integration
{
    public partial class CommandsPanel : EditorWindow
    {
        [MenuItem("Tools/Mesh Deformer/Show Panel")]
        public static void Launch1()
        {
            EditorWindow window = GetWindow<CommandsPanel>();
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(IconPath);
            window.titleContent = new GUIContent("Tools", icon);
            window.Show();
        }

        private void OnSelectionChange()
        {
            Repaint();
        }

        partial void DoMeshDeformerSection()
        {
            GUILayoutOption height = GUILayout.Height(30);
    
            bool canModifyDeformer = MeshDeformerMenu.CanModifyDeformer();
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUI.enabled = MeshDeformerMenu.CanDeformXAxis();
                    if (GUILayout.Button("Deform X", height))
                    {
                        MeshDeformerMenu.DeformXAxis();
                    }

                    GUI.enabled = MeshDeformerMenu.CanAppend();
                    if (GUILayout.Button("Insert", height))
                    {
                        MeshDeformerMenu.Insert();
                    }

                    GUILayout.BeginHorizontal();
                    {
                        GUI.enabled = canModifyDeformer;
                        if (GUILayout.Button("Rig", height))
                        {
                            MeshDeformerMenu.SetRigidMode();
                        }

                        GUI.enabled = canModifyDeformer;
                        if (GUILayout.Button("Free", height))
                        {
                            MeshDeformerMenu.SetFreeMode();
                        }

                    }
                    GUILayout.EndHorizontal();

                    GUI.enabled = MeshDeformerMenu.CanStraighten();
                    if (GUILayout.Button("Straghten", height))
                    {
                        MeshDeformerMenu.Straighten();
                    }

                    GUI.enabled = MeshDeformerMenu.CanExtractSpline();
                    if (GUILayout.Button("Extract", height))
                    {
                        MeshDeformerMenu.ExtractSpline();
                    }

                    GUI.enabled = MeshDeformerMenu.CanRollback();
                    if (GUILayout.Button("Rollback", height))
                    {
                        MeshDeformerMenu.Rollback();
                    }

                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    GUI.enabled = MeshDeformerMenu.CanDeformYAxis();
                    if (GUILayout.Button("Deform Y", height))
                    {
                        MeshDeformerMenu.DeformYAxis();
                    }

                    GUILayout.BeginHorizontal();
                    {
                        GUI.enabled = canModifyDeformer;
                        MeshDeformer deformer = null;
                        if (GUI.enabled)
                        {
                            deformer = Selection.activeTransform.GetComponentInParent<MeshDeformer>();
                        }

                        if (GUILayout.Button("X", height))
                        {
                            MeshDeformerEditor.ChangeAxis(deformer, Axis.X);
                        }

                        if (GUILayout.Button("Y", height))
                        {
                            MeshDeformerEditor.ChangeAxis(deformer, Axis.Y);
                        }

                        if (GUILayout.Button("Z", height))
                        {
                            MeshDeformerEditor.ChangeAxis(deformer, Axis.Z);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUI.enabled = canModifyDeformer;
                    if (GUILayout.Button("Aligned", height))
                    {
                        MeshDeformerMenu.SetAlignedMode();
                    }

                    GUI.enabled = MeshDeformerMenu.CanFit();
                    if (GUILayout.Button("Smooth", height))
                    {
                        MeshDeformerMenu.Fit();
                    }

                    GUI.enabled = MeshDeformerMenu.CanDuplicate();
                    if (GUILayout.Button("Duplicate", height))
                    {
                        MeshDeformerMenu.Duplicate();
                    }

                    GUI.enabled = MeshDeformerMenu.CanRemoveDeformer();
                    if (GUILayout.Button("Remove", height))
                    {
                        MeshDeformerMenu.RemoveDeformer();
                    }


                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    GUI.enabled = MeshDeformerMenu.CanDeformZAxis();
                    if (GUILayout.Button("Deform Z", height))
                    {
                        MeshDeformerMenu.DeformZAxis();
                    }

                    GUILayout.BeginHorizontal();
                    {
                        GUI.enabled = MeshDeformerMenu.CanAppend();
                        if (GUILayout.Button("<-+", height))
                        {
                            MeshDeformerMenu.Prepend();
                        }

                        if (GUILayout.Button("+->", height))
                        {
                            MeshDeformerMenu.Append();
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUI.enabled = canModifyDeformer;
                    if (GUILayout.Button("Mirrored", height))
                    {
                        MeshDeformerMenu.SetMirroredMode();
                    }

                    GUI.enabled = MeshDeformerMenu.CanRemove();
                    if (GUILayout.Button("Remove", height))
                    {
                        MeshDeformerMenu.Remove();
                    }

                    GUI.enabled = MeshDeformerMenu.CanSubdivide();
                    if (GUILayout.Button("Subdivide", height))
                    {
                        MeshDeformerMenu.Subdivide();
                    }

                    GUI.enabled = MeshDeformerMenu.CanCombineAndSave();
                    if (GUILayout.Button("Save", height))
                    {
                        MeshDeformerMenu.CombineAndSave();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    }
}

