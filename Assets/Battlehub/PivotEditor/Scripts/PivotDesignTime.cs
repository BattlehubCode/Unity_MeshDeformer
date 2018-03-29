using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Battlehub.MeshTools
{
	[ExecuteInEditMode]
	public class PivotDesignTime : MonoBehaviour 
	{
		private Vector3 m_prevPosition;
        private Vector3 m_prevTargetPosition;
        private Pivot m_origin;
        private Collider[] m_colliders;
		private void Start () 
		{
            m_origin = GetComponent<Pivot>();
            m_prevTargetPosition = m_origin.Target.transform.position;
            m_prevPosition = transform.position;

            if (m_origin.Target != null)
            {
                m_colliders = m_origin.Target.GetComponents<Collider>();
            }
		}

        public void ToBoundsCenter()
        {
            transform.position = MeshUtils.BoundsCenter(m_origin.Target.gameObject);
        }

        public void ToCenterOffMass()
        {
            transform.position = MeshUtils.CenterOfMass(m_origin.Target.gameObject);
        }

		private void Update () 
		{
            if(m_origin.Target == null)
            {
#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(gameObject);
#else
                DestroyImmediate(gameObject);
#endif
                return;
            }

            if (m_prevPosition != transform.position)
            {
                Vector3 offset = m_origin.Target.position - transform.position;

                MeshUtils.EditPivot(m_origin.Target, offset, m_colliders);
                
                m_origin.Target.position -= offset;
                m_prevPosition = transform.position;
                m_prevTargetPosition = m_origin.Target.transform.position;
            }

            else if (m_origin.Target.transform.position != m_prevTargetPosition)
            {
                transform.position += (m_origin.Target.transform.position - m_prevTargetPosition);

                m_prevTargetPosition = m_origin.Target.transform.position;
                m_prevPosition = transform.position;
            }


        }
	}
}

