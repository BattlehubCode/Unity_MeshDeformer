using UnityEngine;
using System.Collections;
using System;
using  Battlehub.MeshDeformer2;
namespace Battlehub.MeshDeformer2
{
    public class MeshDeformerFollow : MonoBehaviour
    {
        public float Duration = 15.0f;
        public bool Rotation = true;
        public SplineBase Spline;
        public float Offset;

        private float m_t;
        public float CurveBaseLength = 1.0f;
      
        public float T
        {
            get { return m_t; }
            set { m_t = value * Duration; }
        }

        private void Start()
        {
            if(Spline is MeshDeformer)
            {
                MeshDeformer deformer = (MeshDeformer)Spline;
                if (deformer.Original != null)
                {
                    Vector3 from;
                    Vector3 to;
                    deformer.Original.GetBounds(deformer.Axis, out from, out to);
                    CurveBaseLength = (from - to).magnitude;
                }

                float t = Wrap(m_t + Offset * Duration / 250.0f);
                Move(t);
            }
        }

        private float Wrap(float t)
        {
            return (Duration + t % Duration) % Duration;
        }

        private void FixedUpdate()
        {
            float t = Wrap(m_t + Offset * Duration / 250.0f);
            float v = Spline.GetVelocity(t / Duration).magnitude / CurveBaseLength;

            if (m_t >= Duration)
            {
                m_t = (m_t - Duration) + Time.deltaTime / v;

            }
            else
            {
                m_t += Time.deltaTime / v;
            }

            Move(t);
        }

        private void Move(float t)
        {
            
            Vector3 position = Spline.GetPoint(t / Duration);
            Vector3 dir = Spline.GetDirection(t / Duration);
            float twist = Spline.GetTwist(t / Duration);

            transform.position = position;

            if(Rotation)
            {
                transform.LookAt(position + dir);
                transform.RotateAround(position, dir, twist);
            }
            
        }
    }
}
