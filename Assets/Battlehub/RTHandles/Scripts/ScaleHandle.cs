using UnityEngine;

namespace Battlehub.RTHandles
{
    public class ScaleHandle : BaseHandle
    {
        public float GridSize = 0.1f;
        private Vector3 m_prevPoint;
        private Matrix4x4 m_matrix;
        private Matrix4x4 m_inverse;

        private Vector3 m_roundedScale;
        private Vector3 m_scale;
        private Vector3[] m_refScales;
        private float m_screenScale;    

        public static ScaleHandle Current
        {
            get;
            private set;
        }

        protected override RuntimeTool Tool
        {
            get { return RuntimeTool.Scale; }
        }

        protected override float CurrentGridSize
        {
            get { return GridSize; }
        }

        protected override void StartOverride()
        {
            Current = this;
            m_scale = Vector3.one;
            m_roundedScale = m_scale;
        }

        protected override void OnDestroyOverride()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        protected override bool OnBeginDrag()
        {
            m_screenScale = RuntimeHandles.GetScreenScale(transform.position, Camera);
            m_matrix = Matrix4x4.TRS(transform.position, Rotation, Vector3.one);
            m_inverse = m_matrix.inverse;

            Matrix4x4 matrix = Matrix4x4.TRS(transform.position, Rotation, new Vector3(m_screenScale, m_screenScale, m_screenScale));

            if(HitCenter())
            {
                SelectedAxis = RuntimeHandleAxis.Free;
                DragPlane = GetDragPlane();
            }
            else
            {
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
                }
                else
                {
                    SelectedAxis = RuntimeHandleAxis.None;
                    return false;
                }
            }

            m_refScales = new Vector3[Targets.Length];
            for(int i = 0; i < m_refScales.Length; ++i)
            {
                Quaternion rotation = RuntimeTools.PivotRotation == RuntimePivotRotation.Global ? Targets[i].rotation : Quaternion.identity;
                m_refScales[i] = rotation * Target.localScale;
            }
            DragPlane = GetDragPlane();
            bool result = GetPointOnDragPlane(Input.mousePosition, out m_prevPoint);
            return result;
        }

        protected override void OnDrag()
        {
            Vector3 point;
            if (GetPointOnDragPlane(Input.mousePosition, out point))
            {
                Vector3 offset = m_inverse.MultiplyVector((point - m_prevPoint) / m_screenScale);
                float mag = offset.magnitude;
                if (SelectedAxis == RuntimeHandleAxis.X)
                {
                    offset.y = offset.z = 0.0f;
                    m_scale.x += Mathf.Sign(offset.x) * mag;
                }
                else if (SelectedAxis == RuntimeHandleAxis.Y)
                {
                    offset.x = offset.z = 0.0f;
                    m_scale.y += Mathf.Sign(offset.y) * mag;
                }
                else if(SelectedAxis == RuntimeHandleAxis.Z)
                {
                    offset.x = offset.y = 0.0f;
                    m_scale.z += Mathf.Sign(offset.z) * mag;
                }
                if(SelectedAxis == RuntimeHandleAxis.Free)
                {
                    float sign = Mathf.Sign(offset.x + offset.y);
                    m_scale.x += sign * mag;
                    m_scale.y += sign * mag;
                    m_scale.z += sign * mag;
                }

                m_roundedScale = m_scale;

                if(EffectiveGridSize > 0.01)
                {
                    m_roundedScale.x = Mathf.RoundToInt(m_roundedScale.x / EffectiveGridSize) * EffectiveGridSize;
                    m_roundedScale.y = Mathf.RoundToInt(m_roundedScale.y / EffectiveGridSize) * EffectiveGridSize;
                    m_roundedScale.z = Mathf.RoundToInt(m_roundedScale.z / EffectiveGridSize) * EffectiveGridSize;
                }

                for (int i = 0; i < m_refScales.Length; ++i)
                {
                    Quaternion rotation =  RuntimeTools.PivotRotation == RuntimePivotRotation.Global ? Targets[i].rotation : Quaternion.identity;
                    
                    Targets[i].localScale = Quaternion.Inverse(rotation) * Vector3.Scale(m_refScales[i], m_roundedScale);
                }
                
                m_prevPoint = point;
            }
        }

        protected override void OnDrop()
        {
            m_scale = Vector3.one;
            m_roundedScale = m_scale;
        }

        protected override void DrawOverride()
        {
            RuntimeHandles.DoScaleHandle(m_roundedScale, transform.position, Rotation,  SelectedAxis);
        }
    }
}