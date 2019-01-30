using UnityEngine;
using System.Linq;

namespace  Battlehub.MeshDeformer2
{
    public class SplineFollowOffsetEval : MonoBehaviour
    {
        public SplineFollow[] SplineFollow;
        public float[] Distances;
        public int Precision = 10000;
        public float InitialOffset = 0;

        private void Start()
        {
            SplineFollow[] splineFollow = SplineFollow.Where(sf => sf != null).ToArray();
            if(splineFollow.Length == 0)
            {
                return;
            }
            if(Distances.Length == 0)
            {
                Debug.LogError("At least one distance required");
                return;
            }
            int initialDistancesCount = Distances.Length;
            System.Array.Resize(ref Distances, SplineFollow.Length - 1);
            for(int i = initialDistancesCount; i < Distances.Length; ++i)
            {
                Distances[i] = Distances[i % initialDistancesCount];
            }

            float offset = InitialOffset;
            SplineBase spline = splineFollow[0].Spline;
            
            for(int i = 0;; ++i)
            {
                SplineFollow sf = splineFollow[i];
                if(sf.Spline != spline)
                {
                    Debug.LogError("SplineFollow.Spline != " + spline);
                    return;
                }
                sf.Offset = offset;
                if(i == splineFollow.Length - 1)
                {
                    break;
                }
                float distance = Distances[i];
                Vector3 pt0 = spline.GetPoint(offset);
                for(int j = 1; j <= Precision; ++j)
                {
                    float t = offset - ((float)j) / Precision;
                    if(t < 0)
                    {
                        t = (1.0f + t % 1.0f);
                    }
                    Vector3 pt = spline.GetPoint(t);
                    if((pt - pt0).magnitude >= distance)
                    {
                        offset = t;
                        break;
                    }
                }
            }
        }
    }

}
