using UnityEngine;

namespace  Battlehub.MeshDeformer2
{
    public class SplineReparam : MonoBehaviour
    {
        [SerializeField]
        private SplineBase m_spline;

        public SplineBase Spline
        {
            get { return m_spline; }
        }

        [SerializeField]
        private float m_error = 0.00001f;

        [SerializeField]
        private int m_maxIterations = 10000;

        private int m_p;
        private float m_l = 0.0f;
        private float[] m_lseg;

        private float Time(int i)
        {
            return ((float)i) / m_p;
        }

        private float ArcLen(float t)
        {
            int curve;
            if (t >= 1f)
            {
                t = 1f;
                curve = (m_spline.ControlPointCount - 1) / 3 - 1;
            }
            else
            {
                t = Mathf.Clamp01(t) * m_spline.CurveCount;
                curve = (int)t;
                t -= curve;
            }

            return m_spline.GetLengthAS(curve, t, m_error);
        }

        private float GetSpeed(int curve, float t)
        {
            return m_spline.GetVelocity(t, curve).magnitude;
        }

        private void Start()
        {
            if(m_spline == null)
            {
                m_spline = GetComponent<SplineBase>();
            }

            Initialize();
        }

        public void Initialize()
        {
            m_l = 0;
            m_p = m_spline.CurveCount;
            m_lseg = new float[m_p + 1];
            m_lseg[0] = 0;
            for (int i = 1; i < m_lseg.Length; i++)
            {
                m_lseg[i] = m_lseg[i - 1] + m_spline.GetLengthAS(i - 1, m_error);
                
            }

            m_l = m_lseg[m_p];
        }

        public float GetS(float t)
        {
            int curve;
            if (t >= 1f)
            {
                t = 1f;
                curve = (m_spline.ControlPointCount - 1) / 3 - 1;
            }
            else
            {
                t = Mathf.Clamp01(t) * m_spline.CurveCount;
                curve = (int)t;
                t -= curve;
            }

            return m_lseg[curve] + m_spline.GetLengthAS(curve, t, m_error);
        }

        public float GetT(float s)
        {
            if (s <= 0)
            {
                return 0.0f;
            }
            if (s >= m_l)
            {
                return 1.0f;
            }

            int i;
            for (i = 1; i < m_p; i++)
            {
                if (s < m_lseg[i])
                {
                    break;
                }
            }

            float length0 = s - m_lseg[i - 1];
            float length1 = m_lseg[i] - m_lseg[i - 1];
            float dt0 = Time(i - 1) + (Time(i) - Time(i - 1)) * length0 / length1;

            float lower = Time(i - 1), upper = Time(i);

            for (int j = 0; j < m_maxIterations; j++) // ‘m_maxIterations’ is application−specified 
            {
                float f = ArcLen(dt0) - length0;
                if (Mathf.Abs(f) < m_error) // ‘ epsilon ’ is application−specified 
                { 
                    // |ArcLength( i, dt0 ) − length0| is close enough to zero, report 
                    // time(i) + dt0 as the time at which ’length’ is attained. 
                    return  dt0;
                } 
                // Generate a candidate for Newton’s method . 
                float df = GetSpeed(i - 1, dt0);
                float dt0Candidate = dt0 - f / df;


                // Update the root−bounding interval and test for containment of the candidate. 
                if (f > 0)
                {
                    upper = dt0;
                    if ( dt0Candidate <= lower)
                    {
                        // Candidate is outside the root−bounding interval. Use bisection instead. 
                        dt0 = 0.5f * ( upper + lower );
                    }
                    else
                    {
                        // There is no need to compare to ’upper’ because the tangent line has positive slope, guaranteeing that the t−axis
                        // intercept is smaller than ’upper’. 
                        dt0 = dt0Candidate;
                    }
                }
                else
                {
                    lower = dt0;
                    if(dt0Candidate >= upper)
                    {
                        // Candidate is outside the root−bounding interval. Use bisection instead. 
                        dt0 = 0.5f * ( upper + lower );
                    }
                    else
                    {
                        // There is no need to compare to ’lower’ because the tangent 
                        // line has positive slope, guaranteeing that the t−axis 
                        // intercept is larger than ’lower’. 
                        dt0 = dt0Candidate;
                    }
                }
            }
            // A root was not found according to the specified number of iterations 
            // and tolerance. You might want to increase iterations or tolerance or 
            // integration accuracy. However, in this application it is likely that 
            // the time values are oscillating, due to the limited numerical 
            // precision of 32−bit floats . It is safe to use the last computed time . 

            return dt0;
        }


        /// <summary>
        /// Get T param of spline 
        /// </summary>
        /// <param name="t">initial t on spline</param>
        /// <param name="offset">desired offset in meters</param>
        /// <returns>t which corresponds to offset</returns>
        public float GetT(float t, float offset)
        {
            float s = GetS(t);
            float targetS = s + offset;
            return GetT(targetS);
        }
    }


}
