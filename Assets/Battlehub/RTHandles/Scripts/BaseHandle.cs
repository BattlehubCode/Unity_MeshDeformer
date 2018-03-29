using UnityEngine;
using System.Collections;
using System;

namespace Battlehub.RTHandles
{
    public abstract class BaseHandle : MonoBehaviour, IGL
    {
        protected float EffectiveGridSize
        {
            get;
            private set;
        }

        public KeyCode SnapToGridKey = KeyCode.LeftControl;
        public Camera Camera;
        public float SelectionMargin = 10;
        public Transform Target;
        private static BaseHandle m_draggingTool;

        private RuntimeHandleAxis m_selectedAxis;
        private bool m_isDragging;
        private Plane m_dragPlane;

        public bool IsDragging
        {
            get { return m_isDragging; }
        }

        protected abstract RuntimeTool Tool
        {
            get;
        }

        protected Quaternion Rotation
        {
            get { return RuntimeTools.PivotRotation == RuntimePivotRotation.Local ? Target.transform.rotation : Quaternion.identity; }
        }

        protected RuntimeHandleAxis SelectedAxis
        {
            get { return m_selectedAxis; }
            set { m_selectedAxis = value; }
        }

        protected Plane DragPlane
        {
            get { return m_dragPlane; }
            set { m_dragPlane = value; }
        }

        protected abstract float CurrentGridSize
        {
            get;
        }

        private void Start()
        {
            if (GLRenderer.Instance == null)
            {
                GameObject glRenderer = new GameObject();
                glRenderer.name = "GLRenderer";
                glRenderer.AddComponent<GLRenderer>();

                Camera[] cameras = Camera.allCameras;
                for (int i = 0; i < cameras.Length; ++i)
                {
                    Camera cam = cameras[i];
                    if (!cam.GetComponent<GLCamera>())
                    {
                        cam.gameObject.AddComponent<GLCamera>();
                    }
                }
            }

            if(Target == null)
            {
                Target = transform;
            }

            if (Camera == null)
            {
                Camera = Camera.main;
            }

            if (GLRenderer.Instance != null)
            {
                GLRenderer.Instance.Add(this);
            }

            if (Target != null && Target.position != transform.position)
            {
                transform.position = Target.position;
            }

            StartOverride();
        }

        protected virtual void StartOverride()
        {

        }

        private void OnEnable()
        {
            if (GLRenderer.Instance != null)
            {
                GLRenderer.Instance.Add(this);
            }
        }

        private void OnDisable()
        {
            if (GLRenderer.Instance != null)
            {
                GLRenderer.Instance.Remove(this);
            }
        }

        private void OnDestroy()
        {
            if (GLRenderer.Instance != null)
            {
                GLRenderer.Instance.Remove(this);
            }

            OnDestroyOverride();
        }

        protected virtual void OnDestroyOverride()
        {

        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (RuntimeTools.Current != Tool && RuntimeTools.Current != RuntimeTool.None)
                {
                    return;
                }

                if (Camera == null)
                {
                    Debug.LogError("Camera is null");
                    return;
                }

                if(m_draggingTool != null)
                {
                    return;
                }

                m_isDragging = OnBeginDrag();
                if(m_isDragging)
                {
                    m_draggingTool = this; 
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnDrop();
                m_isDragging = false;
                m_draggingTool = null;
            }
            else
            {
                if (m_isDragging)
                {
                    if(Input.GetKey(SnapToGridKey))
                    {
                        EffectiveGridSize = CurrentGridSize;
                    }
                    else
                    {
                        EffectiveGridSize = 0;
                    }

                    OnDrag();
                }
            }

            UpdateOverride();
        }

        protected virtual bool OnBeginDrag()
        {
            return false;
        }

        protected virtual void OnDrag()
        {

        }

        protected virtual void OnDrop()
        {

        }

        protected virtual void UpdateOverride()
        {
            if (Target != null && Target.position != transform.position)
            {
                if (IsDragging)
                {
                    Target.position = transform.position;
                    Target.rotation = transform.rotation;
                }
                else
                {
                    transform.position = Target.position;
                    transform.rotation = Target.rotation;
                }
            }
        }

        protected bool HitCenter()
        {
            Vector2 screenCenter = Camera.WorldToScreenPoint(transform.position);
            Vector2 mouse = Input.mousePosition;

            return (mouse - screenCenter).magnitude <= SelectionMargin;
        }

        protected bool HitAxis(Vector3 axis, Matrix4x4 matrix, out float distanceToAxis)
        {
            axis = matrix.MultiplyVector(axis);
            Vector2 screenVectorBegin = Camera.WorldToScreenPoint(transform.position);
            Vector2 screenVectorEnd = Camera.WorldToScreenPoint(axis + transform.position);

            Vector3 screenVector = screenVectorEnd - screenVectorBegin;
            float screenVectorMag = screenVector.magnitude;
            screenVector.Normalize();
            if (screenVector != Vector3.zero)
            {
                Vector2 perp = PerpendicularClockwise(screenVector).normalized;
                Vector2 mousePosition = Input.mousePosition;
                Vector2 relMousePositon = mousePosition - screenVectorBegin;

                distanceToAxis = Mathf.Abs(Vector2.Dot(perp, relMousePositon));
                Vector2 hitPoint = (relMousePositon - perp * distanceToAxis);
                float vectorSpaceCoord = Vector2.Dot(screenVector, hitPoint);

                bool result = vectorSpaceCoord <= screenVectorMag + SelectionMargin && vectorSpaceCoord >= -SelectionMargin && distanceToAxis <= SelectionMargin;
                if (!result)
                {
                    distanceToAxis = float.PositiveInfinity;
                }
                else
                {
                    if (screenVectorMag < SelectionMargin)
                    {
                        distanceToAxis = 0.0f;
                    }
                }
                return result;
            }
            else
            {
                Vector2 mousePosition = Input.mousePosition;

                distanceToAxis = (screenVectorBegin - mousePosition).magnitude;
                bool result = distanceToAxis <= SelectionMargin;
                if (!result)
                {
                    distanceToAxis = float.PositiveInfinity;
                }
                else
                {
                    distanceToAxis = 0.0f;
                }
                return result;
            }
        }

        protected Plane GetDragPlane(Matrix4x4 matrix, Vector3 axis)
        {
            Plane plane = new Plane(matrix.MultiplyVector(axis).normalized, matrix.MultiplyPoint(Vector3.zero));

            //Plane camPlane = GetDragPlane();

            //if(Math.Abs(Vector3.Dot(camPlane.normal, plane.normal)) < 0.1f)
            //{
            //    return camPlane;
            //}

            return plane;

        }

        protected Plane GetDragPlane()
        {
            Vector3 toCam = Camera.cameraToWorldMatrix.MultiplyVector(Vector3.forward); //Camera.transform.position - transform.position;
            Plane dragPlane = new Plane(toCam.normalized, transform.position);
            return dragPlane;
        }

        protected bool GetPointOnDragPlane(Vector3 screenPos, out Vector3 point)
        {
            return GetPointOnDragPlane(m_dragPlane, screenPos, out point);
        }

        protected bool GetPointOnDragPlane(Plane dragPlane, Vector3 screenPos, out Vector3 point)
        {
            Ray ray = Camera.ScreenPointToRay(screenPos);
            float distance;
            if (dragPlane.Raycast(ray, out distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        private static Vector2 PerpendicularClockwise(Vector2 vector2)
        {
            return new Vector2(-vector2.y, vector2.x);
        }



        void IGL.Draw()
        {
            DrawOverride();
        }

        protected virtual void DrawOverride()
        {

        }
    }
}
