#if USE_BINARY_FORMATTER
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.IO;
using System.Linq;
#endif

using UnityEngine;
using Battlehub.RTEditor;
using System;

namespace Battlehub.MeshDeformer2
{
#if USE_BINARY_FORMATTER
    public sealed class VersionDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
            {
                Type typeToDeserialize = null;

                assemblyName = Assembly.GetExecutingAssembly().FullName;

                // The following line of code returns the type. 
                typeToDeserialize = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));

                return typeToDeserialize;
            }

            return null;
        }
    }
#endif


    public class SplineRuntimeCmd : MonoBehaviour
    {
        public Spline m_spline;
        public SplineControlPoint m_controlPoint;

        private Spline GetSelectedSpline()
        {
            if (RuntimeSelection.activeGameObject == null)
            {
                return null;
            }

            return RuntimeSelection.activeGameObject.GetComponentInParent<Spline>();
        }

        private SplineControlPoint GetSelectedControlPoint()
        {
            if (RuntimeSelection.activeGameObject == null)
            {
                return null;
            }

            return RuntimeSelection.activeGameObject.GetComponentInParent<SplineControlPoint>();
        }

        public void Awake()
        {
            m_spline = GetSelectedSpline();
            RuntimeSelection.SelectionChanged += OnRuntimeSelectionChanged;
        }

        public void OnDestroy()
        {
            RuntimeSelection.SelectionChanged -= OnRuntimeSelectionChanged;
        }

        private void OnRuntimeSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            m_controlPoint = GetSelectedControlPoint();
            m_spline = GetSelectedSpline();
        }

        public void RunAction<T>(Action<T, GameObject> action)
        {
            GameObject[] selectedObjects = RuntimeSelection.gameObjects;
            if (selectedObjects == null)
            {
                return;
            }

            for (int i = 0; i < selectedObjects.Length; ++i)
            {
                GameObject selectedObject = selectedObjects[i];
                if (selectedObject == null)
                {
                    continue;
                }

                T spline = selectedObject.GetComponentInParent<T>();
                if (spline == null)
                {
                    continue;
                }

                if (action != null)
                {
                    action(spline, selectedObject);
                }
            }
        }

        public virtual void Append()
        {
            RunAction<Spline>((spline, go) =>
            {
                spline.Append();   
            });
        }

        public virtual void Insert()
        {
            RunAction<Spline>((spline, go) =>
            {
                if (go != null)
                {
                    SplineControlPoint ctrlPoint = go.GetComponent<SplineControlPoint>();
                    if (ctrlPoint != null)
                    {
                        spline.Insert((ctrlPoint.Index + 2) / 3);
                    }
                }
            });
        }

        public virtual void Prepend()
        {
            RunAction<Spline>((spline, go) =>
            {
                spline.Prepend();   
            });
        }

        public virtual void Remove()
        {
            RunAction<Spline>((spline, go) =>
            {
                if (go != null)
                {
                    SplineControlPoint ctrlPoint = go.GetComponent<SplineControlPoint>();
                    if (ctrlPoint != null)
                    {
                        int curveIndex = Mathf.Min((ctrlPoint.Index + 1) / 3, spline.CurveCount - 1);
                        spline.Remove(curveIndex);
                    }
                    RuntimeSelection.activeObject = spline.gameObject;
                }
            });
        }

        public virtual void Smooth()
        {
            RunAction<SplineBase>((spline, go) => spline.Smooth());
        }

        public virtual void SetMirroredMode()
        {
            RunAction<SplineBase>((spline, go) => spline.SetControlPointMode(ControlPointMode.Mirrored));
        }

        public virtual void SetAlignedMode()
        {
            RunAction<SplineBase>((spline, go) => spline.SetControlPointMode(ControlPointMode.Aligned));
        }

        public virtual void SetFreeMode()
        {
            RunAction<SplineBase>((spline, go) => spline.SetControlPointMode(ControlPointMode.Free));
        }

 
   
    

#if USE_BINARY_FORMATTER
        public virtual void Load()
        {
            string dataAsString = PlayerPrefs.GetString("SplineEditorSave");
            if (string.IsNullOrEmpty(dataAsString))
            {
                return;
            }
            SplineBase[] splines = FindObjectsOfType<SplineBase>();
            SplineSnapshot[] snapshots = DeserializeFromString<SplineSnapshot[]>(dataAsString);
            
            //Should be replaced with more sophisticated load & save & validation logic
            if (splines.Length != snapshots.Length)
            {
                Debug.LogError("Wrong data in save file");
                return;
                //throw new NotImplementedException("Wrong data in save file.");
            }

            for (int i = 0; i < snapshots.Length; ++i)
            {
                splines[i].Load(snapshots[i]);
            }


        }

        /// <summary>
        /// NOTE: THIS FUNCTION IS PROVIDED AS AN EXAMPLE AND DOES NOT SAVE ANY UNITY GAMEOBJECTS (ONLY SPLINE DATA).
        /// </summary>
        public virtual void Save()
        {
            SplineBase[] splines = FindObjectsOfType<SplineBase>();
            SplineSnapshot[] snapshots = new SplineSnapshot[splines.Length];
            for (int i = 0; i < snapshots.Length; ++i)
            {
                snapshots[i] = splines[i].Save();
            }
            string dataAsString = SerializeToString(snapshots);
            PlayerPrefs.SetString("SplineEditorSave", dataAsString);
        }
#else
        [Serializable]
        public class SplineSnapshots
        {
            public SplineSnapshot[] Data;
            public SplineSnapshots()
            {
                Data = new SplineSnapshot[0];
            }
        }

        public virtual void Load()
        {
            string dataAsString = PlayerPrefs.GetString("SplineEditorSave");
            if (string.IsNullOrEmpty(dataAsString))
            {
                return;
            }
            SplineBase[] splines = FindObjectsOfType<SplineBase>();
            SplineSnapshots snapshots = DeserializeFromString<SplineSnapshots>(dataAsString);

            //Should be replaced with more sophisticated load & save & validation logic
            if (splines.Length != snapshots.Data.Length)
            {
                Debug.LogError("Wrong data in save file");
                return;
                //throw new NotImplementedException("Wrong data in save file.");
            }

            for (int i = 0; i < snapshots.Data.Length; ++i)
            {
                splines[i].Load(snapshots.Data[i]);
            }
        }

        public virtual void Save()
        {
            SplineBase[] splines = FindObjectsOfType<SplineBase>();
            SplineSnapshots snapshots = new SplineSnapshots { Data = new SplineSnapshot[splines.Length] };
            for (int i = 0; i < snapshots.Data.Length; ++i)
            {
                snapshots.Data[i] = splines[i].Save();
            }
            string dataAsString = SerializeToString(snapshots);
            PlayerPrefs.SetString("SplineEditorSave", dataAsString);
        }
#endif

        private static TData DeserializeFromString<TData>(string settings)
        {
#if USE_BINARY_FORMATTER
            byte[] b = Convert.FromBase64String(settings);
            using (var stream = new MemoryStream(b))
            {
                SurrogateSelector ss = new SurrogateSelector();
                Vector3SerializationSurrogate v3ss = new Vector3SerializationSurrogate();
                ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3ss);

                var formatter = new BinaryFormatter();
                formatter.SurrogateSelector = ss;
                stream.Seek(0, SeekOrigin.Begin);
                return (TData)formatter.Deserialize(stream);
            }
#else
            return (TData)JsonUtility.FromJson(settings, typeof(TData));
#endif
        }

        private static string SerializeToString<TData>(TData settings)
        {
#if USE_BINARY_FORMATTER
            using (var stream = new MemoryStream())
            {
                SurrogateSelector ss = new SurrogateSelector();
                Vector3SerializationSurrogate v3ss = new Vector3SerializationSurrogate();
                ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3ss);

                var formatter = new BinaryFormatter();
                formatter.SurrogateSelector = ss;
                formatter.Serialize(stream, settings);
                stream.Flush();
                stream.Position = 0;
                return Convert.ToBase64String(stream.ToArray());
            }
#else
            return JsonUtility.ToJson(settings, false);
#endif
        }
    }

}
