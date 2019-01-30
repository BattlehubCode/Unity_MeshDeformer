using UnityEngine;
using System.Linq;


namespace Battlehub.MeshDeformer2
{
    public class MeshDeformerDemo2 : MonoBehaviour
    {
        public MeshDeformer MeshDeformer;

        public GameObject ObjPrefab;

        private void Update()
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if(Physics.Raycast(ray, out hitInfo))
                {
                    Scaffold scaffold = hitInfo.collider.gameObject.GetComponent<Scaffold>();
                    if (scaffold)
                    {
                        ScaffoldWrapper scaffoldWrapper = MeshDeformer.Scaffolds.Where(s => s.Obj == scaffold).FirstOrDefault();
                        int curveIndex = scaffoldWrapper.CurveIndices[0];
                        if(scaffoldWrapper.CurveIndices.Length > 1)
                        {
                            Debug.LogError("Not Supported");
                            return;
                        }

                        Vector3 hitPoint = hitInfo.point;

                        float t = CurveUtils.GetT(MeshDeformer, curveIndex, hitPoint);
                        Debug.Log(t);

                        GameObject obj = Instantiate(ObjPrefab);
                        MeshDeformerFollow follow = obj.GetComponent<MeshDeformerFollow>();
                        follow.Spline = MeshDeformer;
                        follow.T = t;
                    }
                }
            }
        }
    }
}

