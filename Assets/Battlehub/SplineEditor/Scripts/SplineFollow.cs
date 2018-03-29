using UnityEngine;
using UnityEngine.Events;

namespace Battlehub.SplineEditor
{
    public class SplineFollow : MonoBehaviour
    {
        public float Speed = 5.0f;
        public SplineBase Spline;
        public float Offset;
        public bool IsRunning = true;
        public bool IsLoop = false;
        //public UnityEvent Completed;

        private SplineBase m_spline;
        private bool m_isRunning;
        private bool m_isCompleted;

        private float m_t;

        private void Start()
        {
            if(!Spline)
            {
                Debug.LogError("Set Spline Field!");
                enabled = false;
                return;
            }
            m_isCompleted = true;
        }

        private void FixedUpdate()
        {
            if(IsRunning != m_isRunning)
            {
                if(m_isCompleted)
                {
                    Restart();
                }
                m_isRunning = IsRunning;
            }

            if(IsRunning)
            {
                Move();
            }
            
        }

        private void Restart()
        {
            m_spline = Spline;
            m_t = Offset % 1;
            m_isCompleted = false;
            IsRunning = true;
        }

        private void Move()
        {
            float t = m_t;
            UpdatePosition(t);

            float v = m_spline.GetVelocity(t).magnitude;
            v *= m_spline.CurveCount;
            if (m_t >= 1.0f)
            {
                m_t = (m_t - 1.0f) + (Time.deltaTime * Speed) / v;
                if (!m_spline.Loop && !IsLoop)
                {
                    m_t = 1.0f;
                    m_isCompleted = true;
                    IsRunning = false;
                    m_isRunning = false;
                    //Completed.Invoke();
                }

                if (IsLoop)
                {
                    if (m_spline != Spline)
                    {
                        Restart();
                    }
                }
            }
            else
            {
                m_t += (Time.deltaTime * Speed) / v;
            }
        }

        private void UpdatePosition(float t)
        {
            Vector3 position = m_spline.GetPoint(t);
            Vector3 dir = m_spline.GetDirection(t);
            float twist = m_spline.GetTwist(t);

            transform.position = position;
            transform.LookAt(position + dir);
            transform.RotateAround(position, dir, twist);
        }
    }
}
