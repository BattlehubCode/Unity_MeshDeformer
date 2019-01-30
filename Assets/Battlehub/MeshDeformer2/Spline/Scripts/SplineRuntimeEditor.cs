using UnityEngine;
using System;
using System.Linq;

using Battlehub.RTHandles;
using Battlehub.UIControls;
using UnityEngine.EventSystems;
using Battlehub.RTEditor;

namespace  Battlehub.MeshDeformer2
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(GLRenderer))]
    public class SplineRuntimeEditor : MonoBehaviour
    {
        public static event EventHandler Created;
        public static event EventHandler Destroyed;

        public Camera Camera;
        public float SelectionMargin = 20;
        public static readonly Color MirroredModeColor = Color.red;
        public static readonly Color AlignedModeColor = Color.blue;
        public static readonly Color FreeModeColor = Color.yellow;
        public static readonly Color ControlPointLineColor = Color.gray;

        private Material m_connectedMaterial;
        private Material m_normalMaterial;
        private Material m_mirroredModeMaterial;
        private Material m_alignedModeMaterial;
        private Material m_freeModeMaterial;
        private Mesh m_controlPointMesh;

        private bool m_isApplicationQuit;

        public Mesh ControlPointMesh
        {
            get { return m_controlPointMesh; }
        }

        public Material ConnectedMaterial
        {
            get { return m_connectedMaterial; }
        }

        public Material MirroredModeMaterial
        {
            get { return m_mirroredModeMaterial; }
        }

        public Material AlignedModeMaterial
        {
            get { return m_alignedModeMaterial; }
        }

        public Material FreeModeMaterial
        {
            get { return m_freeModeMaterial; }
        }

        public Material NormalMaterial
        {
            get { return m_normalMaterial; }
        }

        private static SplineRuntimeEditor m_instance;
        public static SplineRuntimeEditor Instance
        {
            get { return m_instance; }
        }

        private void Awake()
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeObject = null;
#endif

            if (Camera == null)
            {
                Camera = Camera.main;
                if (Camera.main == null)
                {
                    Debug.LogError("Add Camera with MainCamera Tag");
                }
            }

            if (m_instance != null)
            {
                Debug.LogWarning("Another instance of SplineEditorSettings already exist");
            }

            if (m_mirroredModeMaterial == null)
            {
                Shader shader = Shader.Find("Battlehub/SplineEditor/SSBillboard");

                m_mirroredModeMaterial = new Material(shader);
                m_mirroredModeMaterial.name = "MirroredModeMaterial";
                m_mirroredModeMaterial.color = MirroredModeColor;
                m_mirroredModeMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                m_mirroredModeMaterial.SetInt("_ZWrite", 1);
                m_mirroredModeMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            }

            if (m_alignedModeMaterial == null)
            {
                m_alignedModeMaterial = Instantiate(m_mirroredModeMaterial);
                m_alignedModeMaterial.name = "AlignedModeMaterial";
                m_alignedModeMaterial.color = AlignedModeColor;
            }

            if (m_freeModeMaterial == null)
            {
                m_freeModeMaterial = Instantiate(m_mirroredModeMaterial);
                m_freeModeMaterial.name = "FreeModeMaterial";
                m_freeModeMaterial.color = FreeModeColor;
            }

            if (m_normalMaterial == null)
            {
                m_normalMaterial = Instantiate(m_mirroredModeMaterial);
                m_normalMaterial.name = "SplineMaterial";
                m_normalMaterial.color = Color.green;
            }

            if (m_connectedMaterial == null)
            {
                m_connectedMaterial = Instantiate(m_mirroredModeMaterial);
                m_connectedMaterial.name = "BranchMaterial";
                m_connectedMaterial.color = new Color32(0xa5, 0x00, 0xff, 0xff);
            }

            if (m_controlPointMesh == null)
            {
                m_controlPointMesh = new Mesh();
                m_controlPointMesh.name = "control point mesh";
                m_controlPointMesh.vertices = new[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(0, 0, 0),
                    new Vector3(0, 0, 0),
                    new Vector3(0, 0, 0)
                };
                m_controlPointMesh.triangles = new[]
                {
                    0, 1, 2, 0, 2, 3
                };
                m_controlPointMesh.uv = new[]
                {
                    new Vector2(-1, -1),
                    new Vector2(1, -1),
                    new Vector2(1, 1),
                    new Vector2(-1, 1)
                };
                m_controlPointMesh.RecalculateBounds();
            }

            m_instance = this;
            EnableRuntimeEditing();

            RuntimeSelection.SelectionChanged += OnRuntimeSelectionChanged;
        }

        private void Start()
        {
            if (Created != null)
            {
                Created(this, EventArgs.Empty);
            }
        }

        private void OnApplicationQuit()
        {
            m_isApplicationQuit = true;
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                DisableRuntimeEditing();
            }

            bool enteringPlayMode = false;
#if UNITY_EDITOR
            enteringPlayMode = UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !UnityEditor.EditorApplication.isPlaying;
#endif


            if (!m_isApplicationQuit && !enteringPlayMode)
            {
                SplineControlPoint[] controlPoints = Resources.FindObjectsOfTypeAll<SplineControlPoint>();
                for (int i = 0; i < controlPoints.Length; ++i)
                {
                    SplineControlPoint controlPoint = controlPoints[i];
                    if (controlPoint != null)
                    {
                        controlPoint.DestroyRuntimeComponents();
                    }
                }
            }

            if (Destroyed != null)
            {
                Destroyed(this, EventArgs.Empty);
            }

            RuntimeSelection.SelectionChanged -= OnRuntimeSelectionChanged;

            m_instance = null;
        }

        private void DisableRuntimeEditing()
        {
            if (Camera != null)
            {
                GLCamera glCamera = Camera.GetComponent<GLCamera>();
                if (glCamera != null)
                {
                    DestroyImmediate(glCamera);
                }
            }
        }

        private void EnableRuntimeEditing()
        {
            if (Camera == null)
            {
                return;
            }
            if (!Camera.GetComponent<GLCamera>())
            {
                Camera.gameObject.AddComponent<GLCamera>();
            }
        }

        private void LateUpdate()
        {
            if (m_instance == null)
            {
                m_instance = this;
                SplineBase[] splines = FindObjectsOfType<SplineBase>();
                for (int i = 0; i < splines.Length; ++i)
                {
                    SplineBase spline = splines[i];
                    if (spline.IsSelected)
                    {
                        spline.Select();
                    }
                }
            }
        }

        private void OnRuntimeSelectionChanged(UnityEngine.Object[] unselected)
        {
            SplineBase minSpline = null;
            int minIndex = -1;
            float minDistance = float.PositiveInfinity;
            if (unselected != null)
            {
                GameObject[] gameObjects = unselected.OfType<GameObject>().ToArray();

                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    GameObject go = gameObjects[i];
                    if (go == null)
                    {
                        continue;
                    }

                    SplineBase spline = go.GetComponentInParent<SplineBase>();
                    if (spline == null)
                    {
                        continue;
                    }

                    spline.Select();
                    float distance = minDistance;
                    SplineBase hitSpline;
                    int selectedIndex = HitTest(spline, minDistance, out hitSpline, out distance);
                    if (distance < minDistance && selectedIndex != -1)
                    {
                        minDistance = distance;
                        minIndex = selectedIndex;
                        minSpline = hitSpline;
                    }
                    spline.Unselect();
                }

                if (minSpline != null)
                {
                    SplineControlPoint ctrlPoint = minSpline.GetSplineControlPoints().Where(p => p.Index == minIndex).FirstOrDefault();
                    if (ctrlPoint != null)
                    {
                        RuntimeSelection.activeObject = ctrlPoint.gameObject;
                    }
                    minSpline.Select();

                    return;
                }
            }

            if (RuntimeSelection.gameObjects != null)
            {
                GameObject[] gameObjects = RuntimeSelection.gameObjects;
                if (gameObjects != null)
                {
                    for (int i = 0; i < gameObjects.Length; ++i)
                    {
                        SplineBase spline = gameObjects[i].GetComponentInParent<SplineBase>();
                        if (spline != null)
                        {
                            spline.Select();
                        }
                    }
                }
            }
        }

        private int HitTest(SplineBase spline, float distance, out SplineBase resultSpline, out float resultDistance)
        {
            resultSpline = null;
            resultDistance = float.MaxValue;
            int minIndex = -1;

            float minDistance;
            int index = HitTest(spline, out minDistance);
            if (index > -1 && minDistance < distance)
            {
                resultSpline = spline;
                resultDistance = minDistance;
                distance = minDistance;
                minIndex = index;
            }

            //if (spline.Children != null)
            //{
            //    for (int i = 0; i < spline.Children.Length; ++i)
            //    {
            //        SplineBase child = spline.Children[i];
            //        SplineBase childResult;
            //        float childDistance;
            //        int childIndex = HitTestRecursive(child, distance, out childResult, out childDistance);
            //        if (childIndex > -1)
            //        {
            //            resultSpline = childResult;
            //            resultDistance = childDistance;
            //            distance = minDistance;

            //            minIndex = childIndex;
            //        }
            //    }
            //}

            return minIndex;
        }

        private int HitTest(SplineBase spline, out float minDistance)
        {
            minDistance = float.PositiveInfinity;
            if (Camera == null)
            {
                Debug.LogError("Camera is null");
                return -1;
            }

            if (RuntimeSelection.gameObjects == null)
            {
                return -1;
            }

            Vector3[] controlPoints = new Vector3[spline.ControlPointCount];
            for (int j = 0; j < controlPoints.Length; j++)
            {
                controlPoints[j] = spline.GetControlPoint(j);
            }

            minDistance = SelectionMargin * SelectionMargin;
            int selectedIndex = -1;
            Vector2 mousePositon = Input.mousePosition;
            for (int i = 0; i < controlPoints.Length; ++i)
            {
                Vector3 ctrlPoint = controlPoints[i];
                if (spline.IsControlPointLocked(i))
                {
                    continue;
                }
                Vector2 pt = Camera.WorldToScreenPoint(ctrlPoint);
                float mag = (pt - mousePositon).sqrMagnitude;
                if (mag < minDistance)
                {
                    minDistance = mag;
                    selectedIndex = i;
                }
            }

            return selectedIndex;
        }

        public void OnClosed()
        {
            if (RuntimeSelection.gameObjects == null)
            {
                return;
            }

            GameObject[] gameObjects = RuntimeSelection.gameObjects.OfType<GameObject>().ToArray();
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];
                if (go == null)
                {
                    continue;
                }

                SplineBase spline = go.GetComponentInParent<SplineBase>();
                if (spline == null)
                {
                    continue;
                }

                spline.Unselect();
            }
        }

        public void OnOpened()
        {

        }
    }

}
