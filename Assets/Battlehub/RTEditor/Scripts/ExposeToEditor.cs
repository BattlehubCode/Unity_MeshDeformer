using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battlehub.Utils;

namespace Battlehub.RTEditor
{
    public delegate void ExposeToEditorChangeEvent<T>(ExposeToEditor obj, T oldValue, T newValue);
    public delegate void ExposeToEditorEvent(ExposeToEditor obj);

    [DisallowMultipleComponent]
    public class ExposeToEditor : MonoBehaviour
    {
        public static event ExposeToEditorEvent NameChanged;
        public static event ExposeToEditorEvent TransformChanged;
        public static event ExposeToEditorEvent Awaked;
        public static event ExposeToEditorEvent Started;
        public static event ExposeToEditorEvent Enabled;
        public static event ExposeToEditorEvent Disabled;
        public static event ExposeToEditorEvent Destroyed;
        public static event ExposeToEditorChangeEvent<ExposeToEditor> ParentChanged;

        public bool AddColliders = false;

        public bool DisableOnAwake = false;
        private bool m_applicationQuit;
        //#if UNITY_EDITOR
        //private SaveInPlayMode m_saveInPlayMode;
        //#endif

        private Collider[] m_colliders;
        private HierarchyItem m_hierarchyItem;
        private List<ExposeToEditor> m_children = new List<ExposeToEditor>();
        public int ChildCount
        {
            get { return m_children.Count; }
        }
        public ExposeToEditor GetChild(int index)
        {
            return m_children[index];
        }
        public ExposeToEditor[] GetChildren()
        {
            return m_children.OrderBy(c => c.transform.GetSiblingIndex()).ToArray();
        }

        private ExposeToEditor m_parent;
        public ExposeToEditor Parent
        {
            get { return m_parent; }
            set
            {
                if (m_parent != value)
                {
                    ExposeToEditor oldParent = m_parent;
                    m_parent = value;

                    if (oldParent != null)
                    {
                        oldParent.m_children.Remove(this);
                    }

                    if (m_parent != null)
                    {
                        m_parent.m_children.Add(this);
                    }

                    if (ParentChanged != null)
                    {
                        ParentChanged(this, oldParent, m_parent);
                    }
                }
            }
        }

        private void Awake()
        {
           
            if (DisableOnAwake)
            {
                gameObject.SetActive(false);
            }

            List<Collider> colliders = new List<Collider>();
            MeshFilter filter = GetComponent<MeshFilter>();
            Rigidbody rigidBody = GetComponent<Rigidbody>();

            bool isRigidBody = rigidBody != null;
            if (filter != null)
            {
                if (!isRigidBody && AddColliders)
                {

                    MeshCollider collider = gameObject.AddComponent<MeshCollider>();
                    collider.convex = isRigidBody;
                    collider.sharedMesh = filter.mesh;
                    colliders.Add(collider);
                }

            }

            SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                if (!isRigidBody && AddColliders)
                {
                    MeshCollider collider = gameObject.AddComponent<MeshCollider>();
                    collider.convex = isRigidBody;
                    collider.sharedMesh = skinnedMeshRenderer.sharedMesh;
                    colliders.Add(collider);
                }
            }

            m_colliders = colliders.ToArray();

            if (transform.parent != null)
            {
                ExposeToEditor parent = transform.parent.GetComponentInParent<ExposeToEditor>();
                if (m_parent != parent)
                {
                    m_parent = parent;
                    if (m_parent != null)
                    {
                        m_parent.m_children.Add(this);
                    }
                }
            }

            m_hierarchyItem = gameObject.GetComponent<HierarchyItem>();
            if (m_hierarchyItem == null)
            {
                m_hierarchyItem = gameObject.AddComponent<HierarchyItem>();
            }

            if (Awaked != null)
            {
                Awaked(this);
            }
        }

        private void Start()
        {
            //#if UNITY_EDITOR
            //m_saveInPlayMode = GetComponentInParent<SaveInPlayMode>();

            //if (m_saveInPlayMode != null)
            //{
            //    m_saveInPlayMode.ScheduleDestroy(m_hierarchyItem);
            //    for (int i = 0; i < m_colliders.Length; ++i)
            //    {
            //        m_saveInPlayMode.ScheduleDestroy(m_colliders[i]);
            //    }
            //}
            //#endif


            if (Started != null)
            {
                Started(this);
            }
        }

        private void OnEnable()
        {
            if (Enabled != null)
            {
                Enabled(this);
            }
        }

        private void OnDisable()
        {
            if (Disabled != null)
            {
                Disabled(this);
            }
        }

        private void OnApplicationQuit()
        {
            m_applicationQuit = true;
        }

        private void OnDestroy()
        {
            
            if (!m_applicationQuit)
            {
                Parent = null;
                
                //#if UNITY_EDITOR
                //if (m_saveInPlayMode == null)
                //#endif
                {
                    for (int i = 0; i < m_colliders.Length; ++i)
                    {
                        Collider collider = m_colliders[i];
                        if (collider != null)
                        {
                            Destroy(collider);
                        }
                    }

                    if (m_hierarchyItem != null)
                    {
                        Destroy(m_hierarchyItem);
                    }
                }
                
                if (Destroyed != null)
                {
                    Destroyed(this);
                }
            }
        }

        private void Update()
        {
            if (TransformChanged != null)
            {
                if (transform.hasChanged)
                {
                    transform.hasChanged = false;
                    if (TransformChanged != null)
                    {
                        TransformChanged(this);
                    }
                }
            }
        }

        public void SetName(string name)
        {
            gameObject.name = name;
            if (NameChanged != null)
            {
                NameChanged(this);
            }
        }
    }
}

