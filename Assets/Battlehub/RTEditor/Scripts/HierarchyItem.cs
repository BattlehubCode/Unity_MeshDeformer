using System;
using System.Collections.Generic;
using UnityEngine;


namespace Battlehub.RTEditor
{
    public class HierarchyItem : MonoBehaviour
    {
        private ExposeToEditor m_parentExp;
        private ExposeToEditor m_exposeToEditor;
        private Transform m_parentTransform;
        private bool m_isAwaked;

        private void Awake()
        {
            m_exposeToEditor = GetComponent<ExposeToEditor>();
            if (transform.parent != null)
            {
                m_parentExp = CreateChainToParent(transform.parent);
                m_parentTransform = transform.parent;
            }
            m_isAwaked = true;
        }

        private ExposeToEditor CreateChainToParent(Transform parent)
        {
            ExposeToEditor parentExp = null;
            if(parent != null)
            {
                parentExp = parent.GetComponentInParent<ExposeToEditor>();
            }
            if (parentExp == null)
            {
                return null;
            }
            while (parent != null && parent.gameObject != parentExp.gameObject)
            {
                if (!parent.GetComponent<ExposeToEditor>())
                {
                    if(!parent.GetComponent<HierarchyItem>())
                    {
                        parent.gameObject.AddComponent<HierarchyItem>();
                    }
                }
                parent = parent.parent;
            }
            return parentExp;
        }

        private void TryDestroyChainToParent(Transform parent, ExposeToEditor parentExp)
        {
            if (parentExp == null)
            {
                return;
            }

            while (parent != null && parent.gameObject != parentExp.gameObject)
            {
                if (!parent.GetComponent<ExposeToEditor>())
                {
                    HierarchyItem hierarchyItem = parent.GetComponent<HierarchyItem>();
                    if(hierarchyItem)
                    {
                        if(!HasExposeToEditorChildren(parent))
                        {
                            Destroy(hierarchyItem);
                        }
                    }
                }
                parent = parent.parent;
            }
        }

        private bool HasExposeToEditorChildren(Transform parentTransform)
        {
            int childrenCount = parentTransform.childCount;
            if(childrenCount == 0)
            {
                return false;
            }

            for(int i = 0; i < childrenCount; ++i)
            {
                Transform childTransform = parentTransform.GetChild(i);
                ExposeToEditor child = childTransform.GetComponent<ExposeToEditor>();
                if (child != null)
                {
                    return true;
                }
                HierarchyItem hierarchyItem = childTransform.GetComponent<HierarchyItem>();
                if (hierarchyItem != null)
                {
                    if(HasExposeToEditorChildren(childTransform))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        private void UpdateChildren(Transform parentTransform, ExposeToEditor parentExp)
        {
            int childrenCount = parentTransform.childCount;
            if (childrenCount == 0)
            {
                return;
            }

            for (int i = 0; i < childrenCount; ++i)
            {
                Transform childTransform = parentTransform.GetChild(i);
                ExposeToEditor child = childTransform.GetComponent<ExposeToEditor>();
                HierarchyItem childHierarcyItem = childTransform.GetComponent<HierarchyItem>();
                if (child != null)
                {
                    child.Parent = parentExp;
                    childHierarcyItem.m_parentExp = parentExp;
                }
                else
                {
                    if (childHierarcyItem != null)
                    {
                        UpdateChildren(childTransform, parentExp);
                    }
                } 
            }
        }

        private void OnTransformParentChanged()
        {
            if (!m_isAwaked)
            {
                return;
            }

            if (transform.parent != m_parentTransform)
            {
                if(m_parentTransform != null && m_parentExp != null)
                {
                    TryDestroyChainToParent(m_parentTransform, m_parentExp);
                }
                
                ExposeToEditor parentExp = CreateChainToParent(transform.parent);
                if(parentExp != m_parentExp)
                {
                    if(m_exposeToEditor == null) //intermediate hierarchy item
                    {
                        UpdateChildren(transform, parentExp);
                    }
                    else
                    {
                        m_exposeToEditor.Parent = parentExp;
                    }
                    m_parentExp = parentExp;
                }
                m_parentTransform = transform.parent;
            }            
        }
      
    }
}
