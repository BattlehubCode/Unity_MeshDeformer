using UnityEngine;
using System.Collections;
using Battlehub.UIControls;
using System;
using System.Linq;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class RuntimePrefabs : MonoBehaviour
    {
        public GameObject ListBoxPrefab;
        private ListBox m_listBox;
        public Type TypeCriteria = typeof(GameObject);
        public RuntimeEditor Editor;
        //public Texture2D DragDropIcon;

        public static bool IsPrefab(Transform This)
        {
            #if UNITY_EDITOR
            return UnityEditor.PrefabUtility.GetPrefabType(This.gameObject) == UnityEditor.PrefabType.Prefab;
            #else
            if (Application.isEditor && !Application.isPlaying)
            {
                throw new InvalidOperationException("Does not work in edit mode");
            }
            return This.gameObject.scene.buildIndex < 0;
            #endif
        }

        private void Start()
        {
            if (!ListBoxPrefab)
            {
                Debug.LogError("Set ListBoxPrefab field");
                return;
            }

            m_listBox = Instantiate(ListBoxPrefab).GetComponent<ListBox>();
            m_listBox.CanDrag = false;
            m_listBox.MultiselectKey = KeyCode.None;
            m_listBox.RangeselectKey = KeyCode.None;
            m_listBox.RemoveKey = KeyCode.None;
            m_listBox.transform.SetParent(transform, false);

            m_listBox.ItemDataBinding += OnItemDataBinding;
            m_listBox.SelectionChanged += OnSelectionChanged;
            m_listBox.ItemsRemoved += OnItemsRemoved;
            m_listBox.ItemBeginDrag += OnItemBeginDrag;
            m_listBox.ItemDrop += OnItemDrop;
            m_listBox.ItemEndDrag += OnItemEndDrag;

            RuntimeSelection.SelectionChanged += OnRuntimeSelectionChanged;
#if UNITY_EDITOR
            UnityEditor.Selection.selectionChanged += OnEditorSelectionChanged;
#endif

            if(Editor != null)
            {
                if(Editor.Prefabs != null)
                {
                    for (int i = 0; i < Editor.Prefabs.Length; ++i)
                    {
                        GameObject prefab = Editor.Prefabs[i];
                        if (prefab != null)
                        {
                            if (!prefab.GetComponent<ExposeToEditor>())
                            {
                                prefab.AddComponent<ExposeToEditor>();
                            }

                        }
                    }
                }
              
                m_listBox.Items = Editor.Prefabs;
            }

         
            ExposeToEditor.Destroyed += OnObjectDestroyed;
        }


        private void OnDestroy()
        {
            if (!m_listBox)
            {
                return;
            }
            m_listBox.ItemDataBinding -= OnItemDataBinding;
            m_listBox.SelectionChanged -= OnSelectionChanged;
            m_listBox.ItemsRemoved -= OnItemsRemoved;
            m_listBox.ItemBeginDrag -= OnItemBeginDrag;
            m_listBox.ItemDrop -= OnItemDrop;
            m_listBox.ItemEndDrag -= OnItemEndDrag;

            RuntimeSelection.SelectionChanged -= OnRuntimeSelectionChanged;
#if UNITY_EDITOR
            UnityEditor.Selection.selectionChanged -= OnEditorSelectionChanged;
#endif

            ExposeToEditor.Destroyed -= OnObjectDestroyed;
        }

        private void OnApplicationQuit()
        {
            ExposeToEditor.Destroyed -= OnObjectDestroyed;
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
            m_listBox.SelectedItems = UnityEditor.Selection.gameObjects;
#endif

            m_lockSelection = false;
        }

        private void OnRuntimeSelectionChanged(UnityEngine.Object[] unselected)
        {
            if (m_lockSelection)
            {
                return;
            }
            m_lockSelection = true;

#if UNITY_EDITOR
            if (RuntimeSelection.objects == null)
            {
                UnityEditor.Selection.objects = new UnityEngine.Object[0];
            }
            else
            {
                UnityEditor.Selection.activeObject = RuntimeSelection.activeObject;
                UnityEditor.Selection.objects = RuntimeSelection.objects;
            }
#endif
            m_listBox.SelectedItems = RuntimeSelection.gameObjects;

            m_lockSelection = false;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnListBoxSelectionChanged(e.OldItems, e.NewItems);
        }

        private void OnListBoxSelectionChanged(IEnumerable oldItems, IEnumerable newItems)
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

        private void OnItemDataBinding(object sender, ItemDataBindingArgs e)
        {
            GameObject dataItem = e.Item as GameObject;
            if (dataItem != null)
            {
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = dataItem.name;
            }
        }

        private void OnItemBeginDrag(object sender, ItemDragArgs e)
        {

            //Cursor.SetCursor(DragDropIcon, Vector2.zero, CursorMode.Auto);
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
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

        private void OnItemEndDrag(object sender, ItemDragArgs e)
        {
            //Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void OnObjectDestroyed(ExposeToEditor obj)
        {
            m_listBox.Remove(obj.gameObject);
        }
    }

}
