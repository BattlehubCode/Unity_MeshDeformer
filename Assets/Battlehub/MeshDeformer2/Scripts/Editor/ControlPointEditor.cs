using UnityEngine;
using UnityEditor;
using System.Linq;
using  Battlehub.MeshDeformer2;

namespace Battlehub.MeshDeformer2
{
    [CustomEditor(typeof(ControlPoint))]
    [CanEditMultipleObjects]
    public class ControlPointEditor : MeshDeformerEditor
    {
        private MeshDeformer m_meshDeformer;
        

        protected override void OnDisableOverride()
        {
            ControlPoint controlPoint = (ControlPoint)target;
            if(controlPoint != null)
            {
                controlPoint.enabled = true;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        protected override void OnInspectorGUIOverride()
        {
            if (m_meshDeformer == null)
            {
                m_meshDeformer = GetTarget() as MeshDeformer;
            }

            if (m_meshDeformer == null)
            {
                return;
            }

            if (targets.Length == 1)
            {
                int curveIndex = (SelectedIndex - 1) / 3;
                GUILayout.BeginHorizontal();
                {
                    if (curveIndex == 0)
                    {
                        if (GUILayout.Button("Prepend"))
                        {
                            Undo.RecordObject(m_meshDeformer, "Battlehub.MeshDeformer.Prepend");
                            RecordScaffolds(m_meshDeformer, "Battlehub.MeshDeformer.Prepend");
                            Undo.RegisterCreatedObjectUndo(m_meshDeformer.Prepend(), "Battlehub.MeshDeformer.Prepend");
                            EditorUtility.SetDirty(m_meshDeformer);
                            Selection.activeGameObject = m_meshDeformer.GetComponentsInChildren<ControlPoint>(true).First().gameObject;
                        }
                    }

                    if (GUILayout.Button("Insert"))
                    {
                        Undo.RecordObject(m_meshDeformer, "Battlehub.MeshDeformer.Insert");
                        RecordScaffolds(m_meshDeformer, "Battlehub.MeshDeformer.Insert");
                        Undo.RegisterCreatedObjectUndo(m_meshDeformer.Insert((SelectedIndex + 2) / 3), "Battlehub.MeshDeformer.Insert");
                        EditorUtility.SetDirty(m_meshDeformer);
                        Selection.activeGameObject = m_meshDeformer.GetComponentsInChildren<ControlPoint>(true).ElementAt(SelectedIndex + 3).gameObject;
                    }

                    if (curveIndex == m_meshDeformer.CurveCount - 1)
                    {
                        if (GUILayout.Button("Append"))
                        {
                            Undo.RecordObject(m_meshDeformer, "Battlehub.MeshDeformer.Append");
                            Undo.RegisterCreatedObjectUndo(m_meshDeformer.Append(), "Battlehub.MeshDeformer.Append");
                            EditorUtility.SetDirty(m_meshDeformer);
                            Selection.activeGameObject = m_meshDeformer.GetComponentsInChildren<ControlPoint>(true).Last().gameObject;
                        }
                    }


                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Straighten"))
                {
                    Undo.RecordObject(m_meshDeformer, "Battlehub.MeshDeformer.Straighten");
                    RecordScaffolds(m_meshDeformer, "Battlehub.MeshDeformer.Straighten");
                    EditorUtility.SetDirty(m_meshDeformer);
                    m_meshDeformer.Straighten(SelectedIndex);
                }

                if (SelectedIndex >= 0 && curveIndex < m_meshDeformer.CurveCount)
                {
                    if (GUILayout.Button("Remove"))
                    {
                        Remove();
                    }
                }
            }
        }

        private void Remove()
        {
            int curveIndex = (SelectedIndex - 1) / 3;
            MeshDeformer deformer = m_meshDeformer;
            Undo.RecordObject(deformer, "Battlehub.MeshDeformer.Remove");
            RecordScaffolds(deformer, "Battlehub.MeshDeformer.Remove");
            GameObject removeObject;
            deformer.Remove(curveIndex, out removeObject);
            if (removeObject != null)
            {
                Undo.DestroyObjectImmediate(removeObject);
            }
            EditorUtility.SetDirty(deformer);
        }

        protected override void SceneGUIOverride()
        {
            ControlPoint controlPoint = (ControlPoint)target;
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == (KeyCode.V))
                        {
                            controlPoint.enabled = false;
                        }
                        break;
                    }
                case EventType.KeyUp:
                    {
                        if (Event.current.keyCode == (KeyCode.V))
                        {
                            controlPoint.enabled = true;
                        }
                        break;
                    }
            }

            base.SceneGUIOverride();
        }

        protected override SplineBase GetTarget()
        {
            ControlPoint controlPoint = (ControlPoint)target;
            if(controlPoint != null)
            {
                SplineBase spline = controlPoint.GetComponentInParent<SplineBase>();
                return spline;
            }
            return null;
        }

        private void OnSceneGUI()
        {
            ControlPoint controlPoint = (ControlPoint)target;
            if (controlPoint == m_firstTarget)
            {
                SelectedIndex = controlPoint.Index;
                SceneGUIOverride();
            }
        }

    

    }
}

