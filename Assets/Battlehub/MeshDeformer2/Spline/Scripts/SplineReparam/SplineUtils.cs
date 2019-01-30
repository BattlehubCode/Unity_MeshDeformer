
using UnityEngine;

namespace  Battlehub.MeshDeformer2
{
    public static class SplineUtils
    {
        #region Extra
        // Legendre-Gauss abscissae with n=24 (x_i values, defined at i=n as the roots of the nth order Legendre polynomial Pn(x))
        private static readonly float[] Tvalues = {
            -0.0640568928626056260850430826247450385909f,
            0.0640568928626056260850430826247450385909f,
            -0.1911188674736163091586398207570696318404f,
            0.1911188674736163091586398207570696318404f,
            -0.3150426796961633743867932913198102407864f,
            0.3150426796961633743867932913198102407864f,
            -0.4337935076260451384870842319133497124524f,
            0.4337935076260451384870842319133497124524f,
            -0.5454214713888395356583756172183723700107f,
            0.5454214713888395356583756172183723700107f,
            -0.6480936519369755692524957869107476266696f,
            0.6480936519369755692524957869107476266696f,
            -0.7401241915785543642438281030999784255232f,
            0.7401241915785543642438281030999784255232f,
            -0.8200019859739029219539498726697452080761f,
            0.8200019859739029219539498726697452080761f,
            -0.8864155270044010342131543419821967550873f,
            0.8864155270044010342131543419821967550873f,
            -0.9382745520027327585236490017087214496548f,
            0.9382745520027327585236490017087214496548f,
            -0.9747285559713094981983919930081690617411f,
            0.9747285559713094981983919930081690617411f,
            -0.9951872199970213601799974097007368118745f,
            0.9951872199970213601799974097007368118745f
        };
        // Legendre-Gauss weights with n=24 (w_i values, defined by a function linked to in the Bezier primer article)
        private static readonly float[] Cvalues = {
            0.1279381953467521569740561652246953718517f,
            0.1279381953467521569740561652246953718517f,
            0.1258374563468282961213753825111836887264f,
            0.1258374563468282961213753825111836887264f,
            0.1216704729278033912044631534762624256070f,
            0.1216704729278033912044631534762624256070f,
            0.1155056680537256013533444839067835598622f,
            0.1155056680537256013533444839067835598622f,
            0.1074442701159656347825773424466062227946f,
            0.1074442701159656347825773424466062227946f,
            0.0976186521041138882698806644642471544279f,
            0.0976186521041138882698806644642471544279f,
            0.0861901615319532759171852029837426671850f,
            0.0861901615319532759171852029837426671850f,
            0.0733464814110803057340336152531165181193f,
            0.0733464814110803057340336152531165181193f,
            0.0592985849154367807463677585001085845412f,
            0.0592985849154367807463677585001085845412f,
            0.0442774388174198061686027482113382288593f,
            0.0442774388174198061686027482113382288593f,
            0.0285313886289336631813078159518782864491f,
            0.0285313886289336631813078159518782864491f,
            0.0123412297999871995468056670700372915759f,
            0.0123412297999871995468056670700372915759f
        };

        public static float GetLengthLG(this SplineBase spline)
        {
            float z = 0.5f, sum = 0.0f;
            int len = Tvalues.Length;
            for (int i = 0; i < len; i++)
            {
                float t = z * Tvalues[i] + z;
                sum += Cvalues[i] * spline.GetVelocity(t).magnitude;
            }

            return z * sum;
        }

        public static float GetLengthBF(this SplineBase spline, float deltaT = 0.01f, float from = 0.0f, float to = 1.0f)
        {
            from = Mathf.Min(Mathf.Max(0, from), 1.0f);
            to = Mathf.Max(Mathf.Min(1.0f, to), from);

            int steps = Mathf.CeilToInt(Mathf.Max(Mathf.Min((to - from) / deltaT, 1000000), 1));
            float length = 0;
            Vector3 prev = spline.GetPoint(from);
            for (int i = 0; i < steps; ++i)
            {
                float t = from + (((float)(i + 1)) / steps) * (to - from);
                Vector3 next = spline.GetPoint(t);
                length += (next - prev).magnitude;
                prev = next;
            }

            return length;

        }

        public static float GetLengthBF(this SplineBase spline, int curve, float deltaT = 0.01f, float from = 0.0f, float to = 1.0f)
        {
            from = Mathf.Min(Mathf.Max(0, from), 1.0f);
            to = Mathf.Max(Mathf.Min(1.0f, to), from);

            int steps = Mathf.CeilToInt(Mathf.Max(Mathf.Min((to - from) / deltaT, 1000000), 1));
            float length = 0;
            Vector3 prev = spline.GetPoint(from, curve);
            for (int i = 0; i < steps; ++i)
            {
                float t = from + (((float)(i + 1)) / steps) * (to - from);
                Vector3 next = spline.GetPoint(t, curve);
                length += (next - prev).magnitude;
                prev = next;
            }

            return length;
        }
        #endregion Extra

        private static void SplitBez(Vector3[] v, Vector3[] left, Vector3[] right)
        {
            Vector3[,] vtemp = new Vector3[4, 4];
            for (int j = 0; j <= 3; j++)
            {
                vtemp[0, j] = v[j];
            }
            for (int i = 1; i <= 3; i++)
            {
                for (int j = 0; j <= 3 - i; j++)
                {
                    vtemp[i, j] = 0.5f * vtemp[i - 1, j] + 0.5f * vtemp[i - 1, j + 1];
                }
            }
            for (int j = 0; j <= 3; j++)
            {
                left[j] = vtemp[j, 0];
            }
            for (int j = 0; j <= 3; j++)
            {
                right[j] = vtemp[3 - j, j];
            }
        }

        private static void AddIfClose(Vector3[] v, ref float length, float error)
        {
            //bez poly splits
            Vector3[] left = new Vector3[4];
            Vector3[] right = new Vector3[4];

            //arc lenght
            float len = 0.0f;
            //chord lenght
            float chord;

            int index;

            for (index = 0; index <= 2; index++)
            {
                len = len +  Vector3.Distance(v[index], v[index + 1]);
            }

            chord = Vector3.Distance(v[0], v[3]);
            if((len - chord) > error)
            {
                SplitBez(v, left, right);
                AddIfClose(left, ref length, error);
                AddIfClose(right, ref length, error);
            }
            else
            {
                length = length + len;
            }
        }

        public static float GetLengthAS(this SplineBase spline, int curve, float tmax, float error)
        {
            Vector3[] v = spline.Slice(curve, tmax);

            float length = 0.0f;
            AddIfClose(v, ref length, error);                    

            return length;
        }

        public static float GetLengthAS(this SplineBase spline, int curve, float error)
        {   
            Vector3[] v = new Vector3[4];

            v[0] = spline.GetControlPoint(curve * 3);
            v[1] = spline.GetControlPoint(curve * 3 + 1);
            v[2] = spline.GetControlPoint(curve * 3 + 2);
            v[3] = spline.GetControlPoint(curve * 3 + 3);

            float length = 0.0f;
            AddIfClose(v, ref length, error);                    /* kick off recursion */

            return length;
        }

        public static float GetLengthAS(this SplineBase spline, float error)
        {
            float totalLength = 0.0f;
            Vector3[] v = new Vector3[4];

            int curveCount = spline.CurveCount;
            for(int curve = 0; curve < curveCount; ++curve)
            {
                v[0] = spline.GetControlPoint(curve * 3);
                v[1] = spline.GetControlPoint(curve * 3 + 1);
                v[2] = spline.GetControlPoint(curve * 3 + 2);
                v[3] = spline.GetControlPoint(curve * 3 + 3);

                float length = 0.0f;
                AddIfClose(v, ref length, error);
                totalLength += length;
            }
            return totalLength;

            
        }


        public static Vector3[] Slice(this SplineBase spline, int curve, float t)
        {
            Vector3 p1 = spline.GetControlPoint(curve * 3);
            Vector3 p2 = spline.GetControlPoint(curve * 3 + 1);
            Vector3 p3 = spline.GetControlPoint(curve * 3 + 2);
            Vector3 p4 = spline.GetControlPoint(curve * 3 + 3);

            Vector3 p12 = (p2 - p1) * t + p1;
            Vector3 p23 = (p3 - p2) * t + p2;
            Vector3 p34 = (p4 - p3) * t + p3;
            Vector3 p123 = (p23 - p12) * t + p12;
            Vector3 p234 = (p34 - p23) * t + p23;
            Vector3 p1234 = (p234 - p123) * t + p123;

            return new[] { p1, p12, p123, p1234, p234, p34, p4 };
        }

        

    }
}

