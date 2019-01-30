using  Battlehub.MeshDeformer2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    public Spline Spline;

	// Use this for initialization
	void Start () {

        //int curve = 0;

        //Vector3[] points =  SplineUtils.Slice(Spline, curve, 0.5f);

        //Spline.Insert(curve);

        //for(int i = 0; i < points.Length; ++i)
        //{
        //    Spline.SetControlPoint(curve * 3 + i, points[i]);
        //}

        int curve = 0;

        Spline duplicate = Instantiate(Spline);

        Vector3[] points = SplineUtils.Slice(Spline, curve, 0.5f);

        for (int i = points.Length / 2; i >= 0 ; --i)
        {
            Spline.SetControlPoint(curve * 3 + i, points[i]);
        }

        for (int i = points.Length / 2; i < points.Length; ++i)
        {
            duplicate.SetControlPoint(curve * 3 + i - points.Length / 2, points[i]);
        }

    }

    // Update is called once per frame
    void Update () {
		
	}
}
