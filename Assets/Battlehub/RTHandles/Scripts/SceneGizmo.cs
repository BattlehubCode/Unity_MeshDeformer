using Battlehub.Utils;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Battlehub.RTHandles
{
    [RequireComponent(typeof(Camera))]
    public class SceneGizmo : MonoBehaviour
    {
        public Camera SceneCamera;
        public Transform Pivot;
        public Vector2 Size = new Vector2(96, 96);

        public UnityEvent OrientationChanging;
        public UnityEvent OrientationChanged;
        public UnityEvent ProjectionChanged;
     
        private float m_aspect;
        private Camera m_camera;
        

        private float m_xAlpha = 1.0f;
        private float m_yAlpha = 1.0f;
        private float m_zAlpha = 1.0f;
        private float m_animationDuration = 0.2f;

        private GUIStyle m_buttonStyle;
        private GUIStyle m_oldButtonStyle;
        private Rect m_buttonRect;

        private bool m_mouseOver;
        private Vector3 m_selectedAxis;
        private GameObject m_collidersGO;
        private BoxCollider m_colliderProj;
        private BoxCollider m_colliderUp;
        private BoxCollider m_colliderDown;
        private BoxCollider m_colliderForward;
        private BoxCollider m_colliderBackward;
        private BoxCollider m_colliderLeft;
        private BoxCollider m_colliderRight;
        private Collider[] m_colliders;

        private Vector3 m_position;
        private Quaternion m_rotation;
        private Vector3 m_gizmoPosition;
        private IAnimationInfo m_rotateAnimation;
        private IAnimationInfo m_positionAnimation;

        private float m_screenHeight;
        private float m_screenWidth;

        private bool IsOrthographic
        {
            get { return m_camera.orthographic; }
            set
            {
                m_camera.orthographic = value;
                SceneCamera.orthographic = value;
                if (ProjectionChanged != null)
                {
                    ProjectionChanged.Invoke();
                    InitColliders();
                }
            }
        }

        private void Awake()
        {
            if (SceneCamera == null)
            {
                SceneCamera = Camera.main;
            }

            if(Pivot == null)
            {
                Pivot = transform;
            }

            m_collidersGO = new GameObject();
            m_collidersGO.transform.SetParent(transform, false);
            m_collidersGO.transform.position = GetGizmoPosition();
            m_collidersGO.transform.rotation = Quaternion.identity;
            m_collidersGO.name = "Colliders";

            m_colliderProj = m_collidersGO.AddComponent<BoxCollider>();
            m_colliderUp = m_collidersGO.AddComponent<BoxCollider>();
            m_colliderDown = m_collidersGO.AddComponent<BoxCollider>();
            m_colliderLeft = m_collidersGO.AddComponent<BoxCollider>();
            m_colliderRight = m_collidersGO.AddComponent<BoxCollider>();
            m_colliderForward = m_collidersGO.AddComponent<BoxCollider>();
            m_colliderBackward = m_collidersGO.AddComponent<BoxCollider>();

            m_colliders = new[] { m_colliderProj, m_colliderUp, m_colliderDown, m_colliderRight, m_colliderLeft, m_colliderForward, m_colliderBackward };
            DisableColliders();

            m_camera = GetComponent<Camera>();
            m_camera.clearFlags = CameraClearFlags.Depth;
            m_camera.renderingPath = RenderingPath.Forward;
            m_camera.cullingMask = 0;
            SceneCamera.orthographic = m_camera.orthographic;

            m_screenHeight = Screen.height;
            m_screenWidth = Screen.width;

            UpdateLayout();
            InitColliders();
            UpdateAlpha(ref m_xAlpha, Vector3.right, 1);
            UpdateAlpha(ref m_yAlpha, Vector3.up, 1);
            UpdateAlpha(ref m_zAlpha, Vector3.forward, 1);

        }

        private void Start()
        {
            if (Run.Instance == null)
            {
                GameObject runGO = new GameObject();
                runGO.name = "Run";
                runGO.AddComponent<Run>();
            }
        }

        public void UpdateLayout()
        {
            if(m_camera == null)
            {
                return;
            }

            m_aspect = m_camera.aspect;

            m_camera.pixelRect = new Rect(SceneCamera.pixelRect.min.x + SceneCamera.pixelWidth - Size.x, SceneCamera.pixelRect.min.y + SceneCamera.pixelHeight - Size.y, Size.x, Size.y);
            m_camera.depth = SceneCamera.depth + 1;
            m_aspect = m_camera.aspect;

            m_buttonRect = new Rect(SceneCamera.pixelRect.min.x + SceneCamera.pixelWidth - Size.x / 2 - 20, (Screen.height - SceneCamera.pixelRect.yMax) + Size.y - 5.0f, 40, 30);
            m_buttonStyle = new GUIStyle();
            m_buttonStyle.alignment = TextAnchor.MiddleCenter;
            m_buttonStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);
            m_buttonStyle.fontSize = 12;
        }

        private Vector3 GetGizmoPosition()
        {
            return transform.TransformPoint(Vector3.forward * 5);
        }

        private void OnPostRender()
        {
            RuntimeHandles.DoSceneGizmo(GetGizmoPosition(), Quaternion.identity, m_selectedAxis, Size.y / 96, m_xAlpha, m_yAlpha, m_zAlpha);
        }

        private void OnGUI()
        {
            if (SceneCamera.orthographic)
            {
                if (GUI.Button(m_buttonRect, "Iso", m_buttonStyle))
                {
                    IsOrthographic = false;
                }
            }
            else
            {
                if (GUI.Button(m_buttonRect, "Persp", m_buttonStyle))
                {
                    IsOrthographic = true;
                }
            }
        }

        private void Update()
        {
            if (m_position != transform.position || m_rotation != transform.rotation)
            {
                InitColliders();
                m_position = transform.position;
                m_rotation = transform.rotation;
            }

            if (m_screenHeight != Screen.height || m_screenWidth != Screen.width)
            {
                m_screenHeight = Screen.height;
                m_screenWidth = Screen.width;
                UpdateLayout();
            }

            if (m_aspect != m_camera.aspect)
            {
                m_camera.pixelRect = new Rect(SceneCamera.pixelWidth - Size.x, SceneCamera.pixelHeight - Size.y, Size.x, Size.y);
                m_aspect = m_camera.aspect;
            }

            float delta = Time.deltaTime / m_animationDuration;
            bool updateAlpha = UpdateAlpha(ref m_xAlpha, Vector3.right, delta);
            updateAlpha |= UpdateAlpha(ref m_yAlpha, Vector3.up, delta);
            updateAlpha |= UpdateAlpha(ref m_zAlpha, Vector3.forward, delta);

            m_camera.transform.rotation = SceneCamera.transform.rotation;
            Vector2 guiMousePositon = Input.mousePosition;
            guiMousePositon.y = Screen.height - guiMousePositon.y;
            bool isMouseOverButton = m_buttonRect.Contains(guiMousePositon, true);
            RuntimeTools.IsSceneGizmoSelected = isMouseOverButton;

            if (m_camera.pixelRect.Contains(Input.mousePosition))
            {
                if(!m_mouseOver || updateAlpha)
                {
                    EnableColliders();
                }

                Collider collider = HitTest();
                if(collider == null || m_rotateAnimation != null && m_rotateAnimation.InProgress)
                {
                    m_selectedAxis = Vector3.zero;
                }
                else if(collider == m_colliderProj)
                {
                    m_selectedAxis = Vector3.one;
                }
                else if(collider == m_colliderUp)
                {
                    m_selectedAxis = Vector3.up;
                }
                else if(collider == m_colliderDown)
                {
                    m_selectedAxis = Vector3.down;
                }
                else if(collider == m_colliderForward)
                {
                    m_selectedAxis = Vector3.forward;
                }
                else if(collider == m_colliderBackward)
                {
                    m_selectedAxis = Vector3.back;
                }
                else if(collider == m_colliderRight)
                {
                    m_selectedAxis = Vector3.right;
                }
                else if(collider == m_colliderLeft)
                {
                    m_selectedAxis = Vector3.left;
                }

          
                if (m_selectedAxis != Vector3.zero || isMouseOverButton)
                {
                    RuntimeTools.IsSceneGizmoSelected = true;
                }
                else
                {
                    RuntimeTools.IsSceneGizmoSelected = false;
                }
               
                if (Input.GetMouseButtonUp(0))
                {
                    
                    if(m_selectedAxis != Vector3.zero)
                    {
                        if (m_selectedAxis == Vector3.one)
                        {
                            IsOrthographic = !IsOrthographic;
                        }
                        else
                        {
                            if(m_rotateAnimation == null || !m_rotateAnimation.InProgress)
                            {
                                if(OrientationChanging != null)
                                {
                                    OrientationChanging.Invoke();
                                }
                            }

                            if(m_rotateAnimation != null)
                            {     
                                m_rotateAnimation.Abort();
                            }

                            Vector3 pivot =  Pivot.transform.position;
                            Vector3 radiusVector = Vector3.back * (SceneCamera.transform.position - pivot).magnitude;
                            Quaternion targetRotation = Quaternion.LookRotation(-m_selectedAxis, Vector3.up);
                            m_rotateAnimation = new QuaternionAnimationInfo(SceneCamera.transform.rotation, targetRotation, 0.4f, QuaternionAnimationInfo.EaseOutCubic,
                                (target, value, t, completed) =>
                                {
                                    SceneCamera.transform.position = pivot + value * radiusVector;
                                    SceneCamera.transform.rotation = value;

                                    if (completed)
                                    {
                                        DisableColliders();
                                        EnableColliders();

                                        if (OrientationChanged != null)
                                        {
                                            OrientationChanged.Invoke();
                                        }
                                    }

                                });

                            Run.Instance.Animation(m_rotateAnimation);
                        }
                    }
                }

                m_mouseOver = true;
            }
            else
            {
                if(m_mouseOver)
                {
                    DisableColliders();
                    RuntimeTools.IsSceneGizmoSelected = false;
                }

                m_mouseOver = false;
            }
          
        }

        private void EnableColliders()
        {
            m_colliderProj.enabled = true;
            if (m_zAlpha == 1)
            {
                m_colliderForward.enabled = true;
                m_colliderBackward.enabled = true;
            }
            if (m_yAlpha == 1)
            {
                m_colliderUp.enabled = true;
                m_colliderDown.enabled = true;
            }
            if (m_xAlpha == 1)
            {
                m_colliderRight.enabled = true;
                m_colliderLeft.enabled = true;
            }
        }


        private void DisableColliders()
        {
            for (int i = 0; i < m_colliders.Length; ++i)
            {
                m_colliders[i].enabled = false;
            }
        }

        private Collider HitTest()
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
            float minDistance = float.MaxValue;
            Collider result = null;
            for(int i = 0; i < m_colliders.Length; ++i)
            {
                Collider collider = m_colliders[i];
                RaycastHit hitInfo;
                if (collider.Raycast(ray, out hitInfo, m_gizmoPosition.magnitude * 5))
                {
                    if(hitInfo.distance < minDistance)
                    {
                        minDistance = hitInfo.distance;
                        result = hitInfo.collider;
                    }
                }
            }

            return result;
        }

        private void InitColliders()
        {
            m_gizmoPosition = GetGizmoPosition();
            float sScale = RuntimeHandles.GetScreenScale(m_gizmoPosition, m_camera) * Size.y / 96;

            m_collidersGO.transform.rotation = Quaternion.identity;
            m_collidersGO.transform.position = GetGizmoPosition();

            const float size = 0.15f;
            m_colliderProj.size = new Vector3(size, size, size) * sScale;

            m_colliderUp.size = new Vector3(size, size * 2, size) * sScale;
            m_colliderUp.center = new Vector3(0.0f, size + size / 2, 0.0f) * sScale;

            m_colliderDown.size = new Vector3(size, size * 2, size) * sScale;
            m_colliderDown.center = new Vector3(0.0f, -(size + size / 2), 0.0f) * sScale;

            m_colliderForward.size = new Vector3(size, size, size * 2) * sScale;
            m_colliderForward.center = new Vector3(0.0f,  0.0f, size + size / 2) * sScale;

            m_colliderBackward.size = new Vector3(size, size, size * 2) * sScale;
            m_colliderBackward.center = new Vector3(0.0f, 0.0f, -(size + size / 2)) * sScale;

            m_colliderRight.size = new Vector3(size * 2, size, size) * sScale;
            m_colliderRight.center = new Vector3(size + size / 2, 0.0f, 0.0f) * sScale;

            m_colliderLeft.size = new Vector3(size * 2, size, size) * sScale;
            m_colliderLeft.center = new Vector3(-(size + size / 2), 0.0f, 0.0f) * sScale;
        }

        private bool UpdateAlpha(ref float alpha, Vector3 axis, float delta)
        {
            bool hide = Math.Abs(Vector3.Dot(SceneCamera.transform.forward, axis)) > 0.9;
            if (hide)
            {
                if (alpha > 0.0f)
                {
                    
                    alpha -= delta;
                    if (alpha < 0.0f)
                    {
                        alpha = 0.0f;
                    }
                    return true;
                }
            }
            else
            {
                if (alpha < 1.0f)
                {
                    alpha += delta;
                    if (alpha > 1.0f)
                    {
                        alpha = 1.0f;
                    }
                    return true;
                }
            }

            return false;
        }
    }
}

