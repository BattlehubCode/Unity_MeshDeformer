using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    public class ParentChangedEventArgs : EventArgs
    {
        public TreeViewItem OldParent
        {
            get;
            private set;
        }

        public TreeViewItem NewParent
        {
            get;
            private set;
        }

        public ParentChangedEventArgs(TreeViewItem oldParent, TreeViewItem newParent)
        {
            OldParent = oldParent;
            NewParent = newParent;
        }
    }

    public class TreeViewItem : ItemContainer
    {
        public static event EventHandler<ParentChangedEventArgs> ParentChanged;

        private TreeViewExpander m_expander;
        [SerializeField]
        private HorizontalLayoutGroup m_itemLayout;

        private Toggle m_toggle;
        private TreeView m_treeView;
        private int m_indent;
        public int Indent
        {
            get { return m_indent; }
        }

        private TreeViewItem m_parent;
        public TreeViewItem Parent
        {
            get { return m_parent; }
            set
            {
                if(m_parent == value)
                {
                    return;
                }

                TreeViewItem oldParent = m_parent;
                m_parent = value;
                if(m_parent != null && m_treeView != null && m_itemLayout != null)
                {
                    m_indent = m_parent.m_indent + m_treeView.Indent;
                    m_itemLayout.padding = new RectOffset(
                        m_indent,
                        m_itemLayout.padding.right,
                        m_itemLayout.padding.top,
                        m_itemLayout.padding.bottom);
                     
                    

                    int siblingIndex = transform.GetSiblingIndex();
                    SetIndent(this, ref siblingIndex);
                }
                else
                {
                    m_indent = 0;
                    if(m_itemLayout != null)
                    {
                        m_itemLayout.padding = new RectOffset(
                            m_indent,
                            m_itemLayout.padding.right,
                            m_itemLayout.padding.top,
                            m_itemLayout.padding.bottom); 
                    }
                }

                if(m_treeView != null)
                {
                    if (ParentChanged != null)
                    {
                        ParentChanged(this, new ParentChangedEventArgs(oldParent, m_parent));
                    }
                } 
            }
        }

        private void SetIndent(TreeViewItem parent, ref int siblingIndex)
        {
            while(true)
            {   
                TreeViewItem child = (TreeViewItem)m_treeView.GetItemContainer(siblingIndex + 1);
                if(child == null)
                {
                    return;
                }

                if(child.Parent != parent)
                {
                    return;
                }

                child.m_indent = parent.m_indent + m_treeView.Indent;
                child.m_itemLayout.padding.left = child.m_indent;

                siblingIndex++;
                SetIndent(child, ref siblingIndex);
            }
        }

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                if (base.IsSelected != value)
                {
                    m_toggle.isOn = value;
                    base.IsSelected = value;
                }
            }
        }

        private bool m_canExpand;
        public bool CanExpand
        {
            get { return m_canExpand; }
            set
            {
                if(m_canExpand != value)
                {
                    m_canExpand = value;
                    if(m_expander != null)
                    {
                        m_expander.CanExpand = m_canExpand;
                    }
                    if (!m_canExpand)
                    {
                        IsExpanded = false;
                    }
                }
            }
        }

        private bool m_isExpanded;
        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set
            {
                if(m_isExpanded != value)
                {
                    m_isExpanded = value && m_canExpand;
                    if(m_expander != null)
                    {
                        m_expander.IsOn = value && m_canExpand;
                    }
                    if(m_treeView != null)
                    {
                        if (m_isExpanded)
                        {
                            m_treeView.Expand(this);
                        }
                        else
                        {
                            m_treeView.Collapse(this);
                        }
                    }
                }
            }
        }

        public bool HasChildren
        {
            get
            {
                int index = transform.GetSiblingIndex();
                TreeViewItem nextItem = (TreeViewItem)m_treeView.GetItemContainer(index + 1);
                return nextItem != null && nextItem.Parent == this;
            }
        }

        public bool IsDescendantOf(TreeViewItem parent)
        {
            if (parent == null)
            {
                return true;
            }

            TreeViewItem testItem = this;
            while (testItem != null)
            {
                if (parent == testItem)
                {
                    return true;
                }

                testItem = testItem.Parent;
            }

            return false;
        }

        public TreeViewItem FirstChild()
        {
            if (!HasChildren)
            {
                return null;
            }

            int siblingIndex = transform.GetSiblingIndex();
            siblingIndex++;
            TreeViewItem child = (TreeViewItem)m_treeView.GetItemContainer(siblingIndex);

            Debug.Assert(child != null && child.Parent == this);

            return child;
        }

        public TreeViewItem NextChild(TreeViewItem currentChild)
        {
            if(currentChild == null)
            {
                throw new ArgumentNullException("currentChild");
            }

            int siblingIndex = currentChild.transform.GetSiblingIndex();
            siblingIndex++;
            TreeViewItem nextChild = (TreeViewItem)m_treeView.GetItemContainer(siblingIndex);
            while(nextChild != null && nextChild.IsDescendantOf(this))
            {
                if(nextChild.Parent == this)
                {
                    return nextChild;
                }

                siblingIndex++;
                nextChild = (TreeViewItem)m_treeView.GetItemContainer(siblingIndex);
            }

            return null;
        }

        public TreeViewItem LastChild()
        {
            if (!HasChildren)
            {
                return null;
            }

            int siblingIndex = transform.GetSiblingIndex();

            TreeViewItem lastChild = null;
            while (true)
            {
                siblingIndex++;
                TreeViewItem child = (TreeViewItem)m_treeView.GetItemContainer(siblingIndex);
                if (child == null || child.Parent != this)
                {
                    return lastChild;
                }

                lastChild = child;
            }
        }

        protected override void AwakeOverride()
        {   
            m_toggle = GetComponent<Toggle>();
            m_toggle.interactable = false;
            m_toggle.isOn = IsSelected;

            m_expander = GetComponentInChildren<TreeViewExpander>();
            if (m_expander != null)
            {
                m_expander.CanExpand = m_canExpand;
            }
        }

        protected override void StartOverride()
        {
            m_treeView = GetComponentInParent<TreeView>();
            if (IsExpanded)
            {
                m_treeView.Expand(this);
            }

            if (Parent != null)
            {
                m_indent = Parent.m_indent + m_treeView.Indent;
                m_itemLayout.padding.left = m_indent;
            }
        }

      
    }
}
