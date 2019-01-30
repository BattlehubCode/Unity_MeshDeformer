using UnityEditor;
using UnityEngine;
using System.Linq;

namespace  Battlehub.MeshDeformer2
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SplineControlPoint))]
    public class SplineControlPointEditor : SplineBaseEditor
    {
        private Spline m_spline;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        protected override void DrawSelectedPointInspectorsOverride()
        {
            DrawSelectedPointInspector();
        }

        protected override void OnInspectorGUIOverride()
        {
            if (m_spline == null)
            {
                m_spline = GetTarget() as Spline;
            }

            if (m_spline == null)
            {
                return;
            }

            if(targets.Length == 1)
            {
                int curveIndex = (SelectedIndex - 1) / 3;
                GUILayout.BeginHorizontal();
                {
                    if (curveIndex == 0)
                    {
                        if (GUILayout.Button("Prepend"))
                        {
                            Undo.RecordObject(m_spline, "Battlehub.Spline.Prepend");
                            m_spline.Prepend();
                            EditorUtility.SetDirty(m_spline);
                            Selection.activeGameObject = m_spline.GetComponentsInChildren<SplineControlPoint>(true).First().gameObject;
                        }
                    }

                    if (GUILayout.Button("Insert"))
                    {
                        Undo.RecordObject(m_spline, "Battlehub.Spline.Insert");
                        m_spline.Insert((SelectedIndex + 2) / 3);
                        EditorUtility.SetDirty(m_spline);
                        Selection.activeGameObject = m_spline.GetComponentsInChildren<SplineControlPoint>(true).ElementAt(SelectedIndex + 3).gameObject;
                    }


                    if (curveIndex == m_spline.CurveCount - 1)
                    {
                        if (GUILayout.Button("Append"))
                        {
                            Undo.RecordObject(m_spline, "Battlehub.Spline.Append");
                            m_spline.Append();
                            EditorUtility.SetDirty(m_spline);
                            Selection.activeGameObject = m_spline.GetComponentsInChildren<SplineControlPoint>(true).Last().gameObject;
                        }

                    }
                }
                GUILayout.EndHorizontal();

                if (SelectedIndex >= 0 && curveIndex < m_spline.CurveCount)
                {
                    if (GUILayout.Button("Remove"))
                    {
                        Remove();
                    }
                }
            }
            
            base.OnInspectorGUIOverride();
        }

        private void Remove()
        {
            int curveIndex = (SelectedIndex - 1) / 3;
            Spline spline = m_spline;
            Undo.RecordObject(spline, "Battlehub.Spline.Remove");
            if(!spline.Remove(curveIndex))
            {
                EditorUtility.DisplayDialog("Action cancelled", "Unable to remove last curve", "OK");   
            }
            else
            {
                EditorUtility.SetDirty(spline);
            }
            
        }

        protected override void SceneGUIOverride()
        {
            base.SceneGUIOverride();
        }

        protected override SplineBase GetTarget()
        {
            SplineControlPoint controlPoint = (SplineControlPoint)target;
            if(controlPoint)
            {
                
                SplineBase spline = controlPoint.GetComponentInParent<SplineBase>();
                return spline;
            }
            return null;
        }

        private void OnSceneGUI()
        {
            SplineControlPoint controlPoint = (SplineControlPoint)target;
            SelectedIndex = controlPoint.Index;
            SceneGUIOverride();
        }

    }
}
