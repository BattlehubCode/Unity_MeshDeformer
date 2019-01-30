using UnityEngine;
using  Battlehub.MeshDeformer2;

namespace Battlehub.MeshDeformer2
{
    public class TrainSpeed : MonoBehaviour
    {
        private float m_prevSpeed;
        public float Speed = 1.0f;

        public SplineFollow Cam;
        private SplineFollow[] m_follow;
        
        // Use this for initialization
        private void Start()
        {
            m_follow = GetComponentsInChildren<SplineFollow>();
            SetSpeed();
            
        }

        // Update is called once per frame
        private void Update()
        {
            if(m_prevSpeed != Speed)
            {
                SetSpeed();
            }
        }

        private void SetSpeed()
        {
            for (int i = 0; i < m_follow.Length; ++i)
            {
                m_follow[i].Speed = Speed;
            }
            if (Cam != null)
            {
                Cam.Speed = Speed;
            }
            m_prevSpeed = Speed;
        }
    }
}

