using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.UIControls
{
    public class ItemExpandingArgs : EventArgs
    { 
        public object Item
        {
            get;
            private set;
        }

        public IEnumerable Children
        {
            get;
            set;
        }

        public ItemExpandingArgs(object item)
        {
            Item = item;
        }
    }

    public class TreeViewItemDataBindingArgs : ItemDataBindingArgs
    {
        public bool HasChildren
        {
            get;
            set;
        }
    }

    public class TreeView : ItemsControl<TreeViewItemDataBindingArgs>
    {
        public event EventHandler<ItemExpandingArgs> ItemExpanding;
          
        public int Indent = 20;

        protected override void OnEnableOverride()
        {
            base.OnEnableOverride();
            TreeViewItem.ParentChanged += OnTreeViewItemParentChanged;
        }

        protected override void OnDisableOverride()
        {
            base.OnDisableOverride();
            TreeViewItem.ParentChanged -= OnTreeViewItemParentChanged;
        }

        public void AddChild(object parent, object item)
        {
            if (parent == null)
            {
                Add(item);
            }
            else
            {
                TreeViewItem parentContainer = (TreeViewItem)GetItemContainer(parent);
                if(parentContainer == null)
                {
                    return;
                }

                int index = -1;
                if (parentContainer.IsExpanded)
                {
                    if (parentContainer.HasChildren)
                    {
                        TreeViewItem lastChild = parentContainer.LastChild();
                        index = IndexOf(lastChild.Item) + 1;
                    }
                    else
                    {
                        index = IndexOf(parentContainer.Item) + 1;
                    }
                }
                else
                {
                    parentContainer.CanExpand = true;
                }

                if(index > -1)
                {
                    
                    TreeViewItem addedItem = (TreeViewItem)Insert(index, item);
                    addedItem.Parent = parentContainer;
                }
            }
        }

        public void ChangeParent(object parent, object item)
        {
            if(IsDropInProgress)
            {
                return;
            }

            ItemContainer dragItem = GetItemContainer(item);
            if (dragItem == null)
            {
                return;
            }

            ItemContainer dropTarget = GetItemContainer(parent);
            ItemContainer[] dragItems = new[] { dragItem };
            if (CanDrop(dragItems, dropTarget))
            {
                Drop(dragItems, dropTarget, ItemDropAction.SetLastChild);
            }
        }

        private bool m_expandSilently;
        public void Expand(TreeViewItem item)
        {
            if(m_expandSilently)
            {
                return;
            }

            if(ItemExpanding != null)
            {
                ItemExpandingArgs args = new ItemExpandingArgs(item.Item);
                ItemExpanding(this, args);

                IEnumerable children = args.Children;
                int containerIndex = item.transform.GetSiblingIndex();
                int itemIndex = IndexOf(item.Item);

                item.CanExpand = children != null;

                if (item.CanExpand)
                {
                    foreach(object childItem in children)
                    {
                        containerIndex++;
                        itemIndex++;

                        TreeViewItem childContainer = (TreeViewItem)InstantiateItemContainer(containerIndex);
                        childContainer.Parent = item;
                        childContainer.Item = childItem;

                        InsertItem(itemIndex, childItem);
                        DataBindItem(childItem, childContainer);
                    }

                    UpdateSelectedItemIndex();
                }   
            }
        }

        public void Collapse(TreeViewItem item)
        {
            int containerIndex = item.transform.GetSiblingIndex();
            int itemIndex = IndexOf(item.Item);

            if(SelectedItems != null)
            {
                List<object> selectedItems = SelectedItems.OfType<object>().ToList();
                int refContainerIndex = containerIndex + 1;
                int refItemIndex = itemIndex + 1;
                Unselect(selectedItems, item, ref refContainerIndex, ref refItemIndex);
                SelectedItems = selectedItems;
            }
            
            Collapse(item, containerIndex + 1, itemIndex + 1);
        }

        private void Unselect(List<object> selectedItems, TreeViewItem item, ref int containerIndex, ref int itemIndex )
        {
            while (true)
            {
                TreeViewItem child = (TreeViewItem)GetItemContainer(containerIndex);
                if (child == null || child.Parent != item)
                {
                    break;
                }
                containerIndex++;
                itemIndex++;
                selectedItems.Remove(child.Item);
                Unselect(selectedItems, child, ref containerIndex, ref itemIndex);
            }
        }

        private void Collapse(TreeViewItem item, int containerIndex, int itemIndex)
        {
            while (true)
            {
                TreeViewItem child = (TreeViewItem)GetItemContainer(containerIndex);
                if (child == null || child.Parent != item)
                {
                    break;
                }

                Collapse(child, containerIndex + 1, itemIndex + 1);
                RemoveItemAt(itemIndex);
                DestroyItemContainer(containerIndex); 
            }
        }

        protected override ItemContainer InstantiateItemContainerOverride(GameObject container)
        {
            TreeViewItem itemContainer = container.GetComponent<TreeViewItem>();
            if (itemContainer == null)
            {
                itemContainer = container.AddComponent<TreeViewItem>();
                itemContainer.gameObject.name = "TreeViewItem";
            }
            return itemContainer;
        }

        protected override void DestroyItem(object item)
        {
            TreeViewItem itemContainer = (TreeViewItem)GetItemContainer(item);
            if(itemContainer != null)
            {
                Collapse(itemContainer);
                base.DestroyItem(item);
                if(itemContainer.Parent != null && !itemContainer.Parent.HasChildren)
                {
                    itemContainer.Parent.CanExpand = false;
                }
            }
        }

        protected override void DataBindItem(object item, ItemContainer itemContainer)
        {
            TreeViewItemDataBindingArgs args = new TreeViewItemDataBindingArgs();
            args.Item = item;
            args.ItemPresenter = itemContainer.gameObject;
            RaiseItemDataBinding(args);

            TreeViewItem treeViewItem = (TreeViewItem)itemContainer;
            treeViewItem.CanExpand = args.HasChildren;
        }

        protected override bool CanDrop(ItemContainer[] dragItems, ItemContainer dropTarget)
        {
            if(!base.CanDrop(dragItems, dropTarget))
            {
                return false;
            }

            TreeViewItem tvDropTarget = (TreeViewItem)dropTarget;
            if (tvDropTarget == null)
            {
                return true;
            }

            foreach(ItemContainer dragItem in dragItems)
            {
                TreeViewItem tvDragItem = (TreeViewItem)dragItem;
                if (tvDropTarget.IsDescendantOf(tvDragItem))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnTreeViewItemParentChanged(object sender, ParentChangedEventArgs e)
        {
            TreeViewItem tvItem = (TreeViewItem)sender;
            if (!CanHandleEvent(tvItem))
            {
                return;
            }

            TreeViewItem oldParent = e.OldParent;
            if (oldParent != null && !oldParent.HasChildren)
            {
                oldParent.CanExpand = false;
            }

            if (DropMarker.Action != ItemDropAction.SetLastChild && DropMarker.Action != ItemDropAction.None)
            {
                return;
            }
           
            TreeViewItem tvDropTarget = e.NewParent;
            if(tvDropTarget != null)
            {
                if (tvDropTarget.CanExpand)
                {
                    tvDropTarget.IsExpanded = true;
                }
                else
                {
                    tvDropTarget.CanExpand = true;
                    m_expandSilently = true;
                    tvDropTarget.IsExpanded = true;
                    m_expandSilently = false;
                }
            }
           
            TreeViewItem dragItemChild = tvItem.FirstChild();
            TreeViewItem lastChild = null;
            if(tvDropTarget != null)
            {
                lastChild = tvDropTarget.LastChild();
                if (lastChild == null)
                {
                    lastChild = tvDropTarget;
                }
            }
            else
            {
                lastChild = (TreeViewItem)LastItemContainer();
            }
          
            if(lastChild != tvItem)
            {
                DropItemAfter(lastChild, tvItem);
            }
            
            if (dragItemChild != null)
            {
                MoveSubtree(tvItem, dragItemChild);
            }
        }

        private void MoveSubtree(TreeViewItem parent, TreeViewItem child)
        {
            int parentSiblingIndex = parent.transform.GetSiblingIndex();
            int siblingIndex = child.transform.GetSiblingIndex();
            bool incrementSiblingIndex = false;
            if(parentSiblingIndex < siblingIndex)
            {
                incrementSiblingIndex = true;
            }

            TreeViewItem prev = parent;
            while (child != null && child.IsDescendantOf(parent))
            {
                if(prev == child)
                {
                    break;
                }
                DropItemAfter(prev, child);
                prev = child;
                if(incrementSiblingIndex)
                {
                    siblingIndex++;
                }
                child = (TreeViewItem)GetItemContainer(siblingIndex);
            }
        }

        protected override void Drop(ItemContainer[] dragItems, ItemContainer dropTarget, ItemDropAction action)
        {
            TreeViewItem tvDropTarget = (TreeViewItem)dropTarget;
            if (action == ItemDropAction.SetLastChild)
            {
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    TreeViewItem tvDragItem = (TreeViewItem)dragItems[i];
                    tvDragItem.Parent = tvDropTarget;
                }
            }
            else if (action == ItemDropAction.SetPrevSibling)
            {
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    TreeViewItem tvDragItem = (TreeViewItem)dragItems[i];
                    TreeViewItem dragItemChild = tvDragItem.FirstChild();

                    DropItemBefore(tvDropTarget, tvDragItem);
                    if(dragItemChild != null)
                    {
                        MoveSubtree(tvDragItem, dragItemChild);
                    }

                    tvDragItem.Parent = tvDropTarget.Parent;
                }
            }
            else if (action == ItemDropAction.SetNextSibling)
            {
                for (int i = dragItems.Length - 1; i >= 0 ; --i)
                {
                    TreeViewItem tvDragItem = (TreeViewItem)dragItems[i];
                    TreeViewItem dragItemChild = tvDragItem.FirstChild();

                    DropItemAfter(tvDropTarget, tvDragItem);
                    if(dragItemChild != null)
                    {
                        MoveSubtree(tvDragItem, dragItemChild);
                    }

                    tvDragItem.Parent = tvDropTarget.Parent;
                }
            }

            UpdateSelectedItemIndex();
        }
    }
}
