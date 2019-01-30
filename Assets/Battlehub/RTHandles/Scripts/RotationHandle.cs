using System;
using UnityEngine;

namespace Battlehub.RTHandles
{
    //NOTE: Does not work with Global pivot rotation (always local)
    public class RotationHandle : BaseHandle
    {
        public float GridSize = 15.0f;
        public float XSpeed = 10.0f;
        public float YSpeed = 10.0f;

        private Matrix4x4 m_targetInverse;
        private Matrix4x4 m_matrix;
        private Matrix4x4 m_inverse;
   
        private const float innerRadius = 1.0f;
        private const float outerRadius = 1.2f;
        private const float hitDot = 0.2f;

        private float m_deltaX;
        private float m_deltaY;
        
        public static RotationHandle Current
        {
            get;
            private set;
        }

        protected override RuntimeTool Tool
        {
            get { return RuntimeTool.Rotate; }
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

        protected override void OnEnableOverride()
        {
            base.OnEnableOverride();
            
        }

        private bool Intersect(Ray r, Vector3 sphereCenter, float sphereRadius, out float hit1Distance, out float hit2Distance)
        {
            hit1Distance = 0.0f;
            hit2Distance = 0.0f;
           
            Vector3 L = sphereCenter - r.origin;
            float tc = Vector3.Dot(L, r.direction);
            if (tc < 0.0)
            {
                return false;
            }

            float d2 = Vector3.Dot(L, L) - (tc * tc);
            float radius2 = sphereRadius * sphereRadius;
            if (d2 > radius2)
            {
                return false;
            }

            float t1c = Mathf.Sqrt(radius2 - d2);
            hit1Distance = tc - t1c;
            hit2Distance = tc + t1c;

            return true;
        }

        private RuntimeHandleAxis Hit()
        {
            float hit1Distance;
            float hit2Distance;
            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
            float scale = RuntimeHandles.GetScreenScale(Target.position, Camera);
            if (Intersect(ray, Target.position, outerRadius * scale, out hit1Distance, out hit2Distance))
            {
                Vector3 dpHitPoint;
                GetPointOnDragPlane(GetDragPlane(), Input.mousePosition, out dpHitPoint);
                bool isInside = (dpHitPoint - Target.position).magnitude <= innerRadius * scale;

                if(isInside)
                {
                    Intersect(ray, Target.position, innerRadius * scale, out hit1Distance, out hit2Distance);
                    
                    Vector3 hitPoint = m_targetInverse.MultiplyPoint(ray.GetPoint(hit1Distance));
                    Vector3 radiusVector = hitPoint.normalized;
               
                    float dotX = Mathf.Abs(Vector3.Dot(radiusVector, Vector3.right));
                    float dotY = Mathf.Abs(Vector3.Dot(radiusVector, Vector3.up));
                    float dotZ = Mathf.Abs(Vector3.Dot(radiusVector, Vector3.forward));

                    if (dotX < hitDot)
                    {
                        return RuntimeHandleAxis.X;
                    }
                    else if (dotY < hitDot)
                    {
                        return RuntimeHandleAxis.Y;
                    }
                    else if (dotZ < hitDot)
                    {
                        return RuntimeHandleAxis.Z;
                    }
                    else
                    {
                        return RuntimeHandleAxis.Free;
                    }
                }
                else
                {
                    return RuntimeHandleAxis.Screen;
                }
            }

            return RuntimeHandleAxis.None;
        }

        protected override bool OnBeginDrag()
        {
         
            m_targetInverse = Matrix4x4.TRS(Target.position, Target.rotation, Vector3.one).inverse;
            SelectedAxis = Hit();
            m_deltaX = 0.0f;
            m_deltaY = 0.0f;

            if (SelectedAxis == RuntimeHandleAxis.Screen)
            {
                Vector2 center = Camera.WorldToScreenPoint(Target.position);
                Vector2 point = Input.mousePosition;

                float angle = Mathf.Atan2(point.y - center.y, point.x - center.x);
                m_matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(Mathf.Rad2Deg * angle, Vector3.forward), Vector3.one);
            }
            else
            {
                m_matrix = Matrix4x4.TRS(Vector3.zero, Target.rotation, Vector3.one);
            }
            m_inverse = m_matrix.inverse;

            return SelectedAxis != RuntimeHandleAxis.None;
        }

        protected override void OnDrag()
        {
            float deltaX = Input.GetAxis("Mouse X");
            float deltaY = Input.GetAxis("Mouse Y");

            deltaX = deltaX * XSpeed;
            deltaY = deltaY * YSpeed;

            m_deltaX += deltaX;
            m_deltaY += deltaY;

            Vector3 delta = m_inverse.MultiplyVector(Camera.cameraToWorldMatrix.MultiplyVector(new Vector3(m_deltaY, -m_deltaX, 0)));
            Quaternion rotation;
        
            if (SelectedAxis == RuntimeHandleAxis.X)
            {
                if (EffectiveGridSize != 0.0f)
                {
                    if(Mathf.Abs(delta.x) >= EffectiveGridSize)
                    {
                        delta.x = Mathf.Sign(delta.x) * EffectiveGridSize;
                        m_deltaX = 0.0f;
                        m_deltaY = 0.0f;
                    }
                    else
                    {
                        delta.x = 0.0f;
                    }
                }
                
                rotation = Quaternion.Euler(delta.x, 0, 0);
                
            }
            else if (SelectedAxis == RuntimeHandleAxis.Y)
            {
                if (EffectiveGridSize != 0.0f)
                {
                    if (Mathf.Abs(delta.y) >= EffectiveGridSize)
                    {
                        delta.y = Mathf.Sign(delta.y) * EffectiveGridSize;
                        m_deltaX = 0.0f;
                        m_deltaY = 0.0f;
                    }
                    else
                    {
                        delta.y = 0.0f;
                    }
                }

                rotation = Quaternion.Euler(0, delta.y, 0);
                
            }
            else if (SelectedAxis == RuntimeHandleAxis.Z)
            {
                if (EffectiveGridSize != 0.0f)
                {
                    if (Mathf.Abs(delta.z) >= EffectiveGridSize)
                    {
                        delta.z = Mathf.Sign(delta.z) * EffectiveGridSize;
                        m_deltaX = 0.0f;
                        m_deltaY = 0.0f;
                    }
                    else
                    {
                        delta.z = 0.0f;
                    }
                }
                rotation = Quaternion.Euler(0, 0, delta.z);
            }
            else if(SelectedAxis == RuntimeHandleAxis.Free)
            {
                rotation = Quaternion.Euler(delta.x, delta.y, delta.z);
                m_deltaX = 0.0f;
                m_deltaY = 0.0f;
            }
            else
            {
                delta = m_inverse.MultiplyVector(new Vector3(m_deltaY, -m_deltaX, 0));
                if (EffectiveGridSize != 0.0f)
                {
                    if (Mathf.Abs(delta.x) >= EffectiveGridSize)
                    {
                        delta.x = Mathf.Sign(delta.x) * EffectiveGridSize;
                        m_deltaX = 0.0f;
                        m_deltaY = 0.0f;
                    }
                    else
                    {
                        delta.x = 0.0f;
                    }
                }
                Vector3 axis = m_targetInverse.MultiplyVector(Camera.cameraToWorldMatrix.MultiplyVector(-Vector3.forward));
                rotation = Quaternion.AngleAxis(delta.x, axis);
            }

            if (EffectiveGridSize == 0.0f)
            {
                m_deltaX = 0.0f;
                m_deltaY = 0.0f;
            }

            
            for (int i = 0; i < Targets.Length; ++i)
            {
                Targets[i].rotation *= rotation;
            }
        }

        protected override void DrawOverride()
        {
            RuntimeHandles.DoRotationHandle(Target.rotation, Target.position, SelectedAxis);
        }
    }
}