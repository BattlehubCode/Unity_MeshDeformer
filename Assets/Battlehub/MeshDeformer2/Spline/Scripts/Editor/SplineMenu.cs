using UnityEngine;
using UnityEditor;
using System.Linq;
using Battlehub.RTEditor;
using UnityEngine.EventSystems;
using UnityEditor.Events;
using UnityEngine.Events;

namespace  Battlehub.MeshDeformer2
{
    public static class SplineMenu
    {
        const string root = "Battlehub/MeshDeformer2/Spline/";


        [MenuItem("Tools/Mesh Deformer/Spline/Create Runtime Editor", validate = true)]
        public static bool CanCreateRuntimeEditor()
        {
            return !Object.FindObjectOfType<SplineRuntimeEditor>() && SplineRuntimeEditor.Instance == null;
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Create Runtime Editor")]
        public static void CreateRuntimeEditor()
        {
            GameObject uiCommandsGo = InstantiatePrefab("CommandsPanel.prefab");
            CreateRuntimeEditor(uiCommandsGo, "Spline Runtime Editor");
        }

        public static void CreateRuntimeEditor(GameObject commandsPanel, string name)
        {
            if (!Object.FindObjectOfType<EventSystem>())
            {
                GameObject es = new GameObject();
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
                es.name = "EventSystem";
            }

            GameObject go = new GameObject();
            go.name = name;
            SplineRuntimeEditor srtEditor = go.AddComponent<SplineRuntimeEditor>();

            GameObject uiEditorGO = RTEditorMenu.InstantiateRuntimeEditor();
            uiEditorGO.transform.SetParent(go.transform, false);

            RuntimeEditor rtEditor = uiEditorGO.GetComponent<RuntimeEditor>();
            UnityEventTools.AddPersistentListener(rtEditor.Closed, new UnityAction(srtEditor.OnClosed));


            Placeholder[] placeholders = uiEditorGO.GetComponentsInChildren<Placeholder>(true);
            Placeholder cmd = placeholders.Where(p => p.Id == Placeholder.CommandsPlaceholder).First();

            commandsPanel.transform.SetParent(cmd.transform, false);

            Undo.RegisterCreatedObjectUndo(go, "Battlehub.Spline.CreateRuntimeEditor");
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Create")]
        public static void Create()
        {            
            GameObject spline = new GameObject();
            spline.name = "Spline";

            Undo.RegisterCreatedObjectUndo(spline, "Battlehub.Spline.Create");

            Spline splineComponent = spline.AddComponent<Spline>();
            splineComponent.SetControlPointMode(ControlPointMode.Mirrored);

            Camera sceneCam = SceneView.lastActiveSceneView.camera;
            spline.transform.position = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 5f));

            Selection.activeGameObject = spline.gameObject;
        }

      
      
        public static GameObject InstantiatePrefab(string name)
        {
            Object prefab = AssetDatabase.LoadAssetAtPath("Assets/" + root + "Prefabs/" + name, typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Set Mode/Free", validate = true)]
        private static bool CanSetFreeMode()
        {
            return CanSetMode();
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Set Mode/Aligned", validate = true)]
        private static bool CanSetAlignedMode()
        {
            return CanSetMode();
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Set Mode/Mirrored", validate = true)]
        private static bool CanSetMirroredMode()
        {
            return CanSetMode();
        }

        private static bool CanSetMode()
        {
            GameObject[] selected = Selection.gameObjects;
            return selected.Any(s => s.GetComponentInParent<Spline>());
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Set Mode/Free")]
        private static void SetFreeMode()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                SetMode(gameObjects[i], ControlPointMode.Free);
            }

        }

        [MenuItem("Tools/Mesh Deformer/Spline/Set Mode/Aligned")]
        private static void SetAlignedMode()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                SetMode(gameObjects[i], ControlPointMode.Aligned);
            }
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Set Mode/Mirrored")]
        private static void SetMirroredMode()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                SetMode(gameObjects[i], ControlPointMode.Mirrored);
            }
        }

        private static void SetMode(GameObject selected, ControlPointMode mode)
        {
            Spline spline = selected.GetComponentInParent<Spline>();
            if (spline == null)
            {
                return;
            }

            SplineControlPoint selectedControlPoint = selected.GetComponent<SplineControlPoint>();
            Undo.RecordObject(spline, "Battlehub.Spline.SetMode");
            EditorUtility.SetDirty(spline);

            if (selectedControlPoint != null)
            {
                spline.SetControlPointMode(selectedControlPoint.Index, mode);
            }
            else
            {
                spline.SetControlPointMode(mode);
            }
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Append _&3", validate = true)]
        private static bool CanAppend()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponentInParent<Spline>();
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Append _&3")]
        private static void Append()
        {
            GameObject selected = Selection.activeObject as GameObject;
            Spline spline = selected.GetComponentInParent<Spline>();
            Undo.RecordObject(spline, "Battlehub.Spline.Append");
            spline.Append();
            EditorUtility.SetDirty(spline);
            Selection.activeGameObject = spline.GetComponentsInChildren<SplineControlPoint>(true).Last().gameObject;
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Insert _&4", validate = true)]
        private static bool CanInsert()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponent<SplineControlPoint>();
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Insert _&4")]
        private static void Insert()
        {
            GameObject selected = Selection.activeObject as GameObject;
            Spline spline = selected.GetComponentInParent<Spline>();
            SplineControlPoint ctrlPoint = selected.GetComponent<SplineControlPoint>();
            Undo.RecordObject(spline, "Battlehub.Spline.Insert");
           
            spline.Insert((ctrlPoint.Index + 2) / 3);
            
            EditorUtility.SetDirty(spline);
            Selection.activeGameObject = spline.GetComponentsInChildren<SplineControlPoint>(true).ElementAt(ctrlPoint.Index + 3).gameObject;

        }

        [MenuItem("Tools/Mesh Deformer/Spline/Prepend _&5", validate = true)]
        private static bool CanPrepend()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponentInParent<Spline>();
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Prepend _&5")]
        private static void Prepend()
        {
            GameObject selected = Selection.activeObject as GameObject;
            Spline spline = selected.GetComponentInParent<Spline>();
            Undo.RecordObject(spline, "Battlehub.Spline.Prepend");
            spline.Prepend();
            EditorUtility.SetDirty(spline);
            Selection.activeGameObject = spline.GetComponentsInChildren<SplineControlPoint>(true).First().gameObject;
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Remove Curve", validate = true)]
        private static bool CanRemove()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponent<SplineControlPoint>() && selected.GetComponentInParent<Spline>();
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Remove Curve")]
        private static void Remove()
        {
            GameObject selected = Selection.activeObject as GameObject;
            SplineControlPoint ctrlPoint = selected.GetComponent<SplineControlPoint>();
            Spline spline = selected.GetComponentInParent<Spline>();
            Selection.activeGameObject = spline.gameObject;
            Undo.RecordObject(spline, "Battlehub.Spline.Remove");
            spline.Remove((ctrlPoint.Index - 1) / 3);
            EditorUtility.SetDirty(spline);
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Smooth", validate = true)]
        private static bool CanSmooth()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponentInParent<Spline>();
        }

        [MenuItem("Tools/Mesh Deformer/Spline/Smooth")]
        private static void Smooth()
        {
            GameObject selected = Selection.activeObject as GameObject;
            Spline spline = selected.GetComponentInParent<Spline>();
            Undo.RecordObject(spline, "Battlehub.Spline.Remove");
            spline.Smooth();
            EditorUtility.SetDirty(spline);
        }



    }
}

