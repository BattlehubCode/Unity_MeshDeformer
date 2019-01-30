using System;
using UnityEngine;

namespace Battlehub.RTHandles
{
    [RequireComponent(typeof(Camera))]
    public class Grid : MonoBehaviour
    {
        private Camera m_camera;
        public Camera Camera;
        private void Start()
        {
            m_camera = GetComponent<Camera>();
            if (Camera == null)
            {
                Camera = Camera.main;
            }
            m_camera.clearFlags = CameraClearFlags.Nothing;
            m_camera.renderingPath = RenderingPath.Forward;
            m_camera.cullingMask = 0;
            SetupCamera();
        }

        private void OnPreRender()
        {
            m_camera.farClipPlane = RuntimeHandles.GetGridFarPlane();
        }

        private void OnPostRender()
        {
            RuntimeHandles.DrawGrid();
        }

        private void Update()
        {
            SetupCamera();
        }

        private void SetupCamera()
        {
            m_camera.transform.position = Camera.transform.position;
            m_camera.transform.rotation = Camera.transform.rotation;
            m_camera.transform.localScale = Camera.transform.localScale;

            if (m_camera.fieldOfView != Camera.fieldOfView)
            {
                m_camera.fieldOfView = Camera.fieldOfView;
            }

            if (m_camera.orthographic != Camera.orthographic)
            {
                m_camera.orthographic = Camera.orthographic;
            }

            if (m_camera.orthographicSize != Camera.orthographicSize)
            {
                m_camera.orthographicSize = Camera.orthographicSize;
            }

            if (m_camera.rect != Camera.rect)
            {
                m_camera.rect = Camera.rect;
            }
        }
    }
}

