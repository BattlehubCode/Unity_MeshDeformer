using System;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class PositionHandle : BaseHandle
    {
        public float GridSize = 1.0f;
        private Vector3 m_cursorPosition;
        private Vector3 m_currentPosition;

        private Vector3 m_prevPoint;
        private Matrix4x4 m_matrix;
        private Matrix4x4 m_inverse;

        public static PositionHandle Current
        {
            get;
            private set;
        }

        protected override RuntimeTool Tool
        {
            get { return RuntimeTool.Move; }
        }

        protected override float CurrentGridSize
        {
            get { return GridSize; }
        }

        protected override void StartOverride()
        { 
            Current = this;
        }

        protected override void OnDestroyOverride()
        {  
            if (Current == this)
            {
                Current = null;
            }
        }

        private bool HitQuad(Vector3 axis, Matrix4x4 matrix, float size)
        {
            
            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(matrix.MultiplyVector(axis).normalized, matrix.MultiplyPoint(Vector3.zero));

            float distance;
            if(!plane.Raycast(ray, out distance))
            {
                return false;
            }

            Vector3 point = ray.GetPoint(distance);
            point = matrix.inverse.MultiplyPoint(point);

            Vector3 toCam = Camera.transform.position - transform.position;

            float fx = Mathf.Sign(Vector3.Dot(toCam, Vector3.right));
            float fy = Mathf.Sign(Vector3.Dot(toCam, Vector3.up));
            float fz = Mathf.Sign(Vector3.Dot(toCam, Vector3.forward));

            point.x *= fx;
            point.y *= fy;
            point.z *= fz;

            float lowBound = -0.01f;

            bool result = point.x >= lowBound && point.x <= size && point.y >= lowBound && point.y <= size && point.z >= lowBound && point.z <= size;

            if(result)
            {
                DragPlane = GetDragPlane(matrix, axis);
            }
           
            return result;
        }

        protected override bool OnBeginDrag()
        {
            m_cursorPosition = transform.position;
            m_currentPosition = m_cursorPosition;

            float scale = RuntimeHandles.GetScreenScale(transform.position, Camera);
            m_matrix = Matrix4x4.TRS(transform.position, Rotation, Vector3.one);// transform.localScale);
            m_inverse = m_matrix.inverse;

            Matrix4x4 matrix = Matrix4x4.TRS(transform.position, Rotation, new Vector3(scale, scale, scale));
            float s = 0.3f * scale;
            if(HitQuad(Vector3.up, m_matrix, s))
            {
                SelectedAxis = RuntimeHandleAxis.XZ;
                return GetPointOnDragPlane(Input.mousePosition, out m_prevPoint);
            }

            if (HitQuad(Vector3.right, m_matrix, s))
            {
                SelectedAxis = RuntimeHandleAxis.YZ;
                return GetPointOnDragPlane(Input.mousePosition, out m_prevPoint);
            }

            if (HitQuad(Vector3.forward, m_matrix, s))
            {
                SelectedAxis = RuntimeHandleAxis.XY;
                return GetPointOnDragPlane(Input.mousePosition, out m_prevPoint);
            }

            float distToYAxis;
            float distToZAxis;
            float distToXAxis;
            bool hit = HitAxis(Vector3.up, matrix, out distToYAxis);
            hit |= HitAxis(Vector3.forward, matrix, out distToZAxis);
            hit |= HitAxis(Vector3.right, matrix, out distToXAxis);

            if (hit)
            {
                if (distToYAxis <= distToZAxis && distToYAxis <= distToXAxis)
                {
                    SelectedAxis = RuntimeHandleAxis.Y;
                }
                else if (distToXAxis <= distToYAxis && distToXAxis <= distToZAxis)
                {
                    SelectedAxis = RuntimeHandleAxis.X;
                }
                else
                {
                    SelectedAxis = RuntimeHandleAxis.Z;
                }

                DragPlane = GetDragPlane();
                return GetPointOnDragPlane(Input.mousePosition, out m_prevPoint);
            }

            SelectedAxis = RuntimeHandleAxis.None;
            return false;
        }

        protected override void OnDrag()
        {
            Vector3 point;
            if (GetPointOnDragPlane(Input.mousePosition, out point))
            {
                Vector3 offset = m_inverse.MultiplyVector(point - m_prevPoint);
                float mag = offset.magnitude;
                if (SelectedAxis == RuntimeHandleAxis.X)
                {
                    offset.y = offset.z = 0.0f;
                }
                else if (SelectedAxis == RuntimeHandleAxis.Y)
                {
                    offset.x = offset.z = 0.0f;
                }
                else if (SelectedAxis == RuntimeHandleAxis.Z)
                {
                    offset.x = offset.y = 0.0f;
                }

                if(EffectiveGridSize == 0.0)
                {
                    offset = m_matrix.MultiplyVector(offset).normalized * mag;
                    transform.position += offset;
                    m_prevPoint = point;
                }
                else
                {
                    offset = m_matrix.MultiplyVector(offset).normalized * mag;
                    m_cursorPosition += offset;
                    Vector3 toCurrentPosition = m_cursorPosition - m_currentPosition;
                    if (toCurrentPosition.magnitude * 1.5f >= EffectiveGridSize)
                    {
                        m_currentPosition += toCurrentPosition.normalized * EffectiveGridSize;
                        transform.position = m_currentPosition;                       
                    }   
                    m_prevPoint = point;
                }  
            }
        }

   
        protected override void DrawOverride()
        {
            RuntimeHandles.DoPositionHandle(transform.position, Rotation, SelectedAxis);
        }
    }

}
