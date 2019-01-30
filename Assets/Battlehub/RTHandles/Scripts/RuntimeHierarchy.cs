using UnityEngine;
using Battlehub.UIControls;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public class RuntimeHierarchy : MonoBehaviour
    {
        public GameObject TreeViewPrefab;
        private TreeView m_treeView;
        public System.Type TypeCriteria = typeof(GameObject);
        public Color DisabledItemColor = new Color(0.5f, 0.5f, 0.5f);
        public Color EnabledItemColor = new Color(0.2f, 0.2f, 0.2f);

        private void Start()
        {
            if (!TreeViewPrefab)
            {
                Debug.LogError("Set TreeViewPrefab field");
                return;
            }

            m_treeView = Instantiate(TreeViewPrefab).GetComponent<TreeView>();
            m_treeView.transform.SetParent(transform, false);

            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.SelectionChanged += OnSelectionChanged;
            m_treeView.ItemsRemoved += OnItemsRemoved;
            m_treeView.ItemExpanding += OnItemExpanding;
            m_treeView.ItemBeginDrag += OnItemBeginDrag;
            m_treeView.ItemDrop += OnItemDrop;
            m_treeView.ItemEndDrag += OnItemEndDrag;

            RuntimeSelection.SelectionChanged += OnRuntimeSelectionChanged;
#if UNITY_EDITOR
            UnityEditor.Selection.selectionChanged += OnEditorSelectionChanged;
#endif

            HashSet<GameObject> filtered = new HashSet<GameObject>();
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < objects.Length; ++i)
            {
                GameObject obj = objects[i] as GameObject;
                if (obj == null)
                {
                    continue;
                }

                if (!RuntimePrefabs.IsPrefab(obj.transform))
                {
                    if (TypeCriteria == typeof(GameObject))
                    {
                        filtered.Add(obj);
                    }
                    else
                    {
                        Component component = obj.GetComponent(TypeCriteria);
                        if (component)
                        {
                            if (!filtered.Contains(component.gameObject))
                            {
                                filtered.Add(component.gameObject);
                            }
                        }
                    }
                }
            }

            m_treeView.Items = filtered.Where(f => f.transform.parent == null && CanExposeToEditor(f)).OrderBy(t => t.transform.GetSiblingIndex());

            ExposeToEditor.Awaked += OnObjectAwaked;
            ExposeToEditor.Started += OnObjectStarted;
            ExposeToEditor.Enabled += OnObjectEnabled;
            ExposeToEditor.Disabled += OnObjectDisabled;
            ExposeToEditor.Destroyed += OnObjectDestroyed;
            ExposeToEditor.ParentChanged += OnParentChanged;
            ExposeToEditor.NameChanged += OnNameChanged;
        }

        private bool CanExposeToEditor(GameObject go)
        {
            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            return exposeToEditor != null;
        }

        private void OnDestroy()
        {
            if (!m_treeView)
            {
                return;
            }
            m_treeView.ItemDataBinding -= OnItemDataBinding;
            m_treeView.SelectionChanged -= OnSelectionChanged;
            m_treeView.ItemsRemoved -= OnItemsRemoved;
            m_treeView.ItemExpanding -= OnItemExpanding;
            m_treeView.ItemBeginDrag -= OnItemBeginDrag;
            m_treeView.ItemDrop -= OnItemDrop;
            m_treeView.ItemEndDrag -= OnItemEndDrag;

            RuntimeSelection.SelectionChanged -= OnRuntimeSelectionChanged;
#if UNITY_EDITOR
            UnityEditor.Selection.selectionChanged -= OnEditorSelectionChanged;
#endif

            ExposeToEditor.Awaked -= OnObjectAwaked;
            ExposeToEditor.Started -= OnObjectStarted;
            ExposeToEditor.Enabled -= OnObjectEnabled;
            ExposeToEditor.Disabled -= OnObjectDisabled;
            ExposeToEditor.Destroyed -= OnObjectDestroyed;
            ExposeToEditor.ParentChanged -= OnParentChanged;
            ExposeToEditor.NameChanged -= OnNameChanged;
        }

        private void OnApplicationQuit()
        {
            ExposeToEditor.Awaked -= OnObjectAwaked;
            ExposeToEditor.Started -= OnObjectStarted;
            ExposeToEditor.Enabled -= OnObjectEnabled;
            ExposeToEditor.Disabled -= OnObjectDisabled;
            ExposeToEditor.Destroyed -= OnObjectDestroyed;
            ExposeToEditor.ParentChanged -= OnParentChanged;
            ExposeToEditor.NameChanged -= OnNameChanged;
        }

        private void OnItemExpanding(object sender, ItemExpandingArgs e)
        {
            GameObject gameObject = (GameObject)e.Item;
            ExposeToEditor exposeToEditor = gameObject.GetComponent<ExposeToEditor>();

            if (exposeToEditor.ChildCount > 0)
            {
                e.Children = exposeToEditor.GetChildren().Select(obj => obj.gameObject);

                //This line is required to syncronize selection, runtime selection and treeview selection
                OnTreeViewSelectionChanged(m_treeView.SelectedItems, m_treeView.SelectedItems);
            }
        }

        private bool m_lockSelection;
        private void OnEditorSelectionChanged()
        {
            if (m_lockSelection)
            {
                return;
            }
            m_lockSelection = true;

            #if UNITY_EDITOR
            RuntimeSelection.activeObject = UnityEditor.Selection.activeGameObject;
            RuntimeSelection.objects = UnityEditor.Selection.objects;
            m_treeView.SelectedItems = UnityEditor.Selection.gameObjects;
            #endif

            m_lockSelection = false;
        }

        private void OnRuntimeSelectionChanged(Object[] unselected)
        {
            if (m_lockSelection)
            {
                return;
            }
            m_lockSelection = true;

            #if UNITY_EDITOR
            if (RuntimeSelection.objects == null)
            {
               UnityEditor.Selection.objects = new Object[0];
            }
            else
            {
                UnityEditor.Selection.activeObject = RuntimeSelection.activeObject;
                UnityEditor.Selection.objects = RuntimeSelection.objects;
            }
            #endif
            m_treeView.SelectedItems = RuntimeSelection.gameObjects;

            m_lockSelection = false;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnTreeViewSelectionChanged(e.OldItems, e.NewItems);
        }

        private void OnTreeViewSelectionChanged(IEnumerable oldItems, IEnumerable newItems)
        {
            if (m_lockSelection)
            {
                return;
            }

            m_lockSelection = true;

            if (newItems == null)
            {
                newItems = new GameObject[0];
            }

            #if UNITY_EDITOR
            UnityEditor.Selection.objects = newItems.OfType<GameObject>().ToArray();
            #endif

            RuntimeSelection.objects = newItems.OfType<GameObject>().ToArray();

            m_lockSelection = false;
        }

        private void OnItemsRemoved(object sender, ItemsRemovedArgs e)
        {
            for (int i = 0; i < e.Items.Length; ++i)
            {
                GameObject go = (GameObject)e.Items[i];
                if (go != null)
                {
                    Destroy(go);
                }
            }
        }

        private void OnItemDataBinding(object sender, TreeViewItemDataBindingArgs e)
        {
            GameObject dataItem = e.Item as GameObject;
            if (dataItem != null)
            {
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = dataItem.name;
                if (dataItem.activeInHierarchy)
                {
                    text.color = EnabledItemColor;
                }
                else
                {
                    text.color = DisabledItemColor;
                }

                e.HasChildren = dataItem.GetComponent<ExposeToEditor>().ChildCount > 0;
            }
        }

        private void OnItemBeginDrag(object sender, ItemDragArgs e)
        {
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
        {
            if (e.IsExternal)
            {
                if (e.DragItems != null)
                {
                    for (int i = 0; i < e.DragItems.Length; ++i)
                    {
                        GameObject prefab = e.DragItems[i] as GameObject;
                        if (prefab != null)
                        {
                            if (RuntimePrefabs.IsPrefab(prefab.transform))
                            {
                                GameObject prefabInstance = Instantiate(prefab);
                                ExposeToEditor exposeToEditor = prefabInstance.GetComponent<ExposeToEditor>();
                                if (exposeToEditor != null)
                                {
                                    exposeToEditor.SetName(prefab.name);
                                }
                                prefabInstance.transform.position = prefab.transform.position;
                                prefabInstance.transform.rotation = prefab.transform.rotation;
                                prefabInstance.transform.localScale = prefab.transform.localScale;
                                RuntimeSelection.activeGameObject = prefabInstance;
                            }
                        }
                    }
                }
            }
            else
            {
                Transform dropT = ((GameObject)e.DropTarget).transform;
                if (e.Action == ItemDropAction.SetLastChild)
                {
                    for (int i = 0; i < e.DragItems.Length; ++i)
                    {
                        Transform dragT = ((GameObject)e.DragItems[i]).transform;
                        dragT.SetParent(dropT, true);
                        dragT.SetAsLastSibling();
                    }
                }
                else if (e.Action == ItemDropAction.SetNextSibling)
                {
                    for (int i = 0; i < e.DragItems.Length; ++i)
                    {
                        Transform dragT = ((GameObject)e.DragItems[i]).transform;
                        if (dragT.parent != dropT.parent)
                        {
                            dragT.SetParent(dropT.parent, true);
                        }

                        int siblingIndex = dropT.GetSiblingIndex();
                        dragT.SetSiblingIndex(siblingIndex + 1);
                    }
                }
                else if (e.Action == ItemDropAction.SetPrevSibling)
                {
                    for (int i = 0; i < e.DragItems.Length; ++i)
                    {
                        Transform dragT = ((GameObject)e.DragItems[i]).transform;
                        if (dragT.parent != dropT.parent)
                        {
                            dragT.SetParent(dropT.parent, true);
                        }

                        int siblingIndex = dropT.GetSiblingIndex();
                        dragT.SetSiblingIndex(siblingIndex);
                    }
                }
            }


        }

        private void OnItemEndDrag(object sender, ItemDragArgs e)
        {
        }

        private void OnObjectAwaked(ExposeToEditor obj)
        {
            GameObject parent = null;
            if (obj.Parent != null)
            {
                parent = obj.Parent.gameObject;
            }
            m_treeView.AddChild(parent, obj.gameObject);

        }

        private void OnObjectStarted(ExposeToEditor obj)
        {

        }

        private void OnObjectEnabled(ExposeToEditor obj)
        {
            TreeViewItem tvItem = (TreeViewItem)m_treeView.GetItemContainer(obj.gameObject);
            if (tvItem == null)
            {
                return;
            }
            Text text = tvItem.GetComponentInChildren<Text>();
            text.color = EnabledItemColor;
        }

        private void OnObjectDisabled(ExposeToEditor obj)
        {
            TreeViewItem tvItem = (TreeViewItem)m_treeView.GetItemContainer(obj.gameObject);
            if (tvItem == null)
            {
                return;
            }
            Text text = tvItem.GetComponentInChildren<Text>();
            text.color = DisabledItemColor;
        }

        private void OnObjectDestroyed(ExposeToEditor obj)
        {
            m_treeView.Remove(obj.gameObject);
        }

        private void OnParentChanged(ExposeToEditor obj, ExposeToEditor oldParent, ExposeToEditor newParent)
        {
            GameObject parent = null;
            if (newParent != null)
            {
                parent = newParent.gameObject;
            }

            m_treeView.ChangeParent(parent, obj.gameObject);
        }

        private void OnNameChanged(ExposeToEditor obj)
        {
            TreeViewItem tvItem = (TreeViewItem)m_treeView.GetItemContainer(obj.gameObject);
            if (tvItem == null)
            {
                return;
            }
            Text text = tvItem.GetComponentInChildren<Text>();
            text.text = obj.gameObject.name;
        }

    }
}

