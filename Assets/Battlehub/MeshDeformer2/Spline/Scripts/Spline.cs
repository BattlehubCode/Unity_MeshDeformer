using UnityEngine;

namespace  Battlehub.MeshDeformer2
{
    [ExecuteInEditMode]
    public class Spline : SplineBase
    {
        private const float Mag = 5.0f;

        protected override void OnCurveChanged()
        {
            TrackVersion();
        }
#if UNITY_EDITOR
        protected override void AwakeOverride()
        {
            TrackVersion();
        }
#endif

        protected override float GetMag()
        {
            return Mag;
        }
        private void AppendCurve(float mag, bool enforceNeighbour)
        {
            Vector3 dir = transform.InverseTransformDirection(GetDirection(1.0f));
            Vector3 point = GetPoint(1.0f);
            point = transform.InverseTransformPoint(point);

            int pointsCount = 3;
            float deltaT = 1.0f / pointsCount;
            float t = deltaT;
            Vector3[] points = new Vector3[pointsCount];
            for (int i = 0; i < pointsCount; ++i)
            {
                points[i] = point + dir * mag * t;
                t += deltaT;
            }

            AppendCurve(points, enforceNeighbour);
        }

        private void PrependCurve(float mag, int curveIndex, bool enforceNeighbour, bool shrinkPreceding)
        {
            const int pointsCount = 3;
            Vector3[] points = new Vector3[pointsCount];
            if (!shrinkPreceding)
            {
                Vector3 dir = GetDirection(0.0f, curveIndex);
                Vector3 point = GetPointLocal(0.0f, curveIndex);

                dir = transform.InverseTransformDirection(dir);

                float deltaT = 1.0f / pointsCount;
                float t = 1.0f;
                
                for (int i = 0; i < pointsCount; ++i)
                {
                    points[i] = point - dir * mag * t;
                    t -= deltaT;
                }
            }

            PrependCurve(points, curveIndex, mag, enforceNeighbour, shrinkPreceding);
        }

        public override void Load(SplineSnapshot snapshot)
        {
            LoadSpline(snapshot);
            TrackVersion();
        }

        public void Insert(int curveIndex)
        {
            PrependCurve(Mag, curveIndex, false, true);
            TrackVersion();
        }

        public void Append()
        {
            AppendCurve(Mag, false);
            TrackVersion();
        }

        public void Prepend()
        {
            if (!Loop)
            {
                const int curveIndex = 0;
                PrependCurve(Mag, curveIndex, false, false);
            }
            else
            {
                AppendCurve(Mag, false);
            }
            TrackVersion();
        }

        public bool Remove(int curveIndex)
        {
            bool result = RemoveCurve(curveIndex);
            TrackVersion();
            return result;
        }

        private void TrackVersion()
        {
#if UNITY_EDITOR
            PersistentVersions[0]++;
            OnVersionChanged();
#endif
        }

    }
}

