using  Battlehub.MeshDeformer2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Test2 : MonoBehaviour {

    [SerializeField]
    private SplineReparam m_reparam;

    [SerializeField]
    private SplineFollow m_splineFollow;

	// Use this for initialization
	void Start () {

        m_reparam.Initialize();

        float seconds = 3.4f;
        float meters = m_splineFollow.Speed * seconds;
        float t = m_reparam.GetT(meters);

        m_splineFollow.transform.position = m_reparam.Spline.GetPoint(t);
        m_splineFollow.Offset = t;
	}
}
