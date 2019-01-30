using UnityEngine;
namespace Battlehub.UIControls
{
    [RequireComponent(typeof(RectTransform))]
    public class TreeViewDropMarker : ItemDropMarker
    {
        private TreeView m_treeView;
        private RectTransform m_siblingGraphicsRectTransform;
        public GameObject ChildGraphics;
        public override ItemDropAction Action
        {
            get { return base.Action; }
            set
            {
                base.Action = value;
                ChildGraphics.SetActive(base.Action == ItemDropAction.SetLastChild);
                SiblingGraphics.SetActive(base.Action != ItemDropAction.SetLastChild);
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_treeView = GetComponentInParent<TreeView>();
            m_siblingGraphicsRectTransform = SiblingGraphics.GetComponent<RectTransform>();
        }

        public override void SetTraget(ItemContainer item)
        {
            base.SetTraget(item);
            if(item == null)
            {
                return;
            }

            TreeViewItem tvItem = (TreeViewItem)item;
            if(tvItem != null)
            {
                m_siblingGraphicsRectTransform.offsetMin = new Vector2(tvItem.Indent, m_siblingGraphicsRectTransform.offsetMin.y);
            }
            else
            {
                m_siblingGraphicsRectTransform.offsetMin = new Vector2(0, m_siblingGraphicsRectTransform.offsetMin.y);
            }
        }

        public override void SetPosition(Vector2 position)
        {
            if(Item == null)
            {
                return;
            }

            RectTransform rt = Item.RectTransform;
            TreeViewItem tvItem = (TreeViewItem)Item;
            Vector2 localPoint;

            Camera camera = null;
            if(ParentCanvas.renderMode == RenderMode.WorldSpace)
            {
                camera = m_treeView.Camera;
            }

            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, position, camera, out localPoint))
            {
                if (localPoint.y > -rt.rect.height / 4)
                {
                    Action = ItemDropAction.SetPrevSibling;
                    RectTransform.position = rt.position;
                }
                else if (localPoint.y < rt.rect.height / 4 - rt.rect.height && !tvItem.HasChildren)
                {
                    Action = ItemDropAction.SetNextSibling;
                    RectTransform.position = rt.position - new Vector3(0, rt.rect.height, 0);
                }
                else
                {
                    Action = ItemDropAction.SetLastChild;
                    RectTransform.position = rt.position;
                }
            }

           
        }
       
    }
}

