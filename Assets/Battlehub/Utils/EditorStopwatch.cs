#define ENABLE_STOPWATCH

using UnityEngine;

namespace Battlehub.Utils
{
    public class EditorStopwatch
    {
        public static EditorStopwatch Main;

        static EditorStopwatch()
        {
            Main = new EditorStopwatch();
        }

#if UNITY_EDITOR && ENABLE_STOPWATCH
        private System.Diagnostics.Stopwatch m_stopwatch = new System.Diagnostics.Stopwatch();
#endif

        public void Start()
        {
#if UNITY_EDITOR && ENABLE_STOPWATCH
            m_stopwatch.Reset();
            m_stopwatch.Start();
#endif
        }

        public void Stop(string output)
        {
#if UNITY_EDITOR && ENABLE_STOPWATCH
            m_stopwatch.Stop();
            Debug.Log(m_stopwatch.ElapsedMilliseconds + " ms " + output);
            
#endif
        }

    }
}

