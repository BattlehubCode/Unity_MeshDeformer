using UnityEngine;
using UnityEditor;
using System.Linq;
using Battlehub.MeshTools;
using Battlehub.Integration;

namespace Battlehub.MeshDeformer2
{
    public class MeshDeformerMenu
    {
        const string root = "Battlehub/MeshDeformer2/";

        static MeshDeformerMenu()
        {
            MeshCombinerIntegration.BeginEditPivot += OnBeginEditPivot;
            MeshCombinerIntegration.Combined += OnCombined;
        }

        private static void OnBeginEditPivot(IntegrationArgs args)
        {
            GameObject go = args.GameObject;
            MeshDeformer deformer = go.GetComponentInParent<MeshDeformer>();
            if (deformer != null && deformer.GetType() == typeof(MeshDeformer))
            {
                if(!Rollback(deformer.gameObject))
                {
                    args.Cancel = true;
                }
            }
        }

        private static void OnCombined(IntegrationArgs args)
        {
            GameObject go = args.GameObject;
            if (go.GetComponent<MeshDeformer>() != null)
            {
                CleanupCombined(go);
            }
        }

        [MenuItem("Tools/Mesh Deformer/Create Runtime Editor", validate = true)]
        public static bool CanCreateRuntimeEditor()
        {
            return SplineMenu.CanCreateRuntimeEditor();
        }

        [MenuItem("Tools/Mesh Deformer/Create Runtime Editor")]
        public static void CreateRuntimeEditor()
        {
            GameObject commandsPanelGO = InstantiatePrefab("CommandsPanel.prefab");
            SplineMenu.CreateRuntimeEditor(commandsPanelGO, "Mesh Deformer Runtime Component");
        }

        public static GameObject InstantiatePrefab(string name)
        {
            Object prefab = AssetDatabase.LoadAssetAtPath("Assets/" + root + "Prefabs/" + name, typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        [MenuItem("Tools/Mesh Deformer/Deform/Z", validate = true)]
        public static bool CanDeformZAxis()
        {
            return CanDeformAxis(Axis.Z);
        }



        [MenuItem("Tools/Mesh Deformer/Deform/Z")]
        public static void DeformZAxis()
        {
            DeformAxis(Axis.Z);
        }

        [MenuItem("Tools/Mesh Deformer/Deform/X", validate = true)]
        public static bool CanDeformXAxis()
        {
            return CanDeformAxis(Axis.X);
        }

        [MenuItem("Tools/Mesh Deformer/Deform/X")]
        public static void DeformXAxis()
        {
            DeformAxis(Axis.X);
        }


        [MenuItem("Tools/Mesh Deformer/Deform/Y", validate = true)]
        public static bool CanDeformYAxis()
        {
            return CanDeformAxis(Axis.Y);
        }

        [MenuItem("Tools/Mesh Deformer/Deform/Y")]
        public static void DeformYAxis()
        {
            DeformAxis(Axis.Y);
        }

        private static bool CanDeformAxis(Axis axis)
        {
            GameObject gameObject = Selection.activeObject as GameObject;
            if (gameObject != null)
            {
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    Mesh mesh = meshFilter.sharedMesh;
                    if (mesh != null)
                    {
                        Vector3 from;
                        Vector3 to;
                        mesh.GetBounds(axis, out from, out to);
                        return to != from;
                    }
                }
            }
            return false;
        }

        private static void DeformAxis(Axis axis)
        {
            GameObject selected = (GameObject)Selection.activeObject;
            Mesh colliderMesh = null;
            MeshCollider collider = selected.GetComponent<MeshCollider>();
            if (collider != null)
            {
                colliderMesh = collider.sharedMesh;
            }
            
            Mesh mesh = null;
            MeshFilter filter = selected.GetComponent<MeshFilter>();
            MeshDeformer deformer = selected.GetComponent<MeshDeformer>();
            if (filter != null)
            {
                mesh = filter.sharedMesh;
            }

            bool deformerIsNull = (deformer == null);
            
            bool ok = MeshDeformerIntegration.RaiseBeforeDeform(selected, mesh);
            if(!ok)
            {
                return;
            }

            if(!Rollback(selected))
            {
                return;
            }

            deformer = selected.AddComponent<MeshDeformer>();

            if (filter != null)
            {
                if (selected.transform.localScale != Vector3.one * selected.transform.localScale.x)
                {
                    Debug.LogWarning("Object with Non-uniform scale!");

                    CombineResult result = Battlehub.MeshTools.MeshUtils.Combine(new[] { selected });
                    selected = result.GameObject;
                    filter = selected.GetComponent<MeshFilter>();
                    mesh = filter.sharedMesh;

                    MeshCollider combinedCollider = selected.GetComponent<MeshCollider>();
                    if(combinedCollider != null)
                    {
                        GameObject.DestroyImmediate(combinedCollider);
                    }
                }

                if (collider == null)
                {
                    bool result = false;
                    if (deformerIsNull)
                    {
                        result = EditorUtility.DisplayDialog("Mesh Collider?", "Create MeshCollider using MeshFilter's mesh?", "Yes", "No");
                    }

                    if (result)
                    {
                        collider = selected.AddComponent<MeshCollider>();
                        Undo.RegisterCreatedObjectUndo(collider, "Battlehub.MeshDeformer.Deform");
                        collider.sharedMesh = GameObject.Instantiate(mesh);
                        collider.sharedMesh.name = mesh.name + " Collider";
                        deformer.ColliderOriginal = collider.sharedMesh;
                    }
                }  
                else
                {
                    deformer.ColliderOriginal = colliderMesh;
                }
            }

           

            deformer.Axis = axis;
            deformer.ResetDeformer();

            Undo.RegisterCreatedObjectUndo(deformer, "Battlehub.MeshDeformer.Deform");

            Scaffold scaffold = selected.GetComponent<Scaffold>();
            if (scaffold != null)
            {
                Undo.RegisterCreatedObjectUndo(scaffold, "Battlehub.MeshDeformer.Deform");
            }


            EditorUtility.SetDirty(deformer);
        }

        [MenuItem("Tools/Mesh Deformer/Set Mode/Rigid", validate = true)]
        public static bool CanSetRigidMode()
        {
            return CanModifyDeformer();
        }

        [MenuItem("Tools/Mesh Deformer/Set Mode/Free", validate = true)]
        public static bool CanSetFreeMode()
        {
            return CanModifyDeformer();
        }

        [MenuItem("Tools/Mesh Deformer/Set Mode/Aligned", validate = true)]
        public static bool CanSetAlignedMode()
        {
            return CanModifyDeformer();
        }

        [MenuItem("Tools/Mesh Deformer/Set Mode/Mirrored", validate = true)]
        public static bool CanSetMirroredMode()
        {
            return CanModifyDeformer();
        }

        public static bool CanModifyDeformer()
        {
            GameObject[] selected = Selection.gameObjects;
            return selected.Any(s => s.GetComponentInParent<MeshDeformer>());
        }

        [MenuItem("Tools/Mesh Deformer/Set Mode/Rigid")]
        public static void SetRigidMode()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                SetMode(gameObjects[i], true, ControlPointMode.Free);
            }
        }

        [MenuItem("Tools/Mesh Deformer/Set Mode/Free")]
        public static void SetFreeMode()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                SetMode(gameObjects[i], false, ControlPointMode.Free);
            }

        }

        [MenuItem("Tools/Mesh Deformer/Set Mode/Aligned")]
        public static void SetAlignedMode()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                SetMode(gameObjects[i], false, ControlPointMode.Free);
            }
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                SetMode(gameObjects[i], false, ControlPointMode.Aligned);
            }
        }

        [MenuItem("Tools/Mesh Deformer/Set Mode/Mirrored")]
        public static void SetMirroredMode()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                SetMode(gameObjects[i], false, ControlPointMode.Free);
            }
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                SetMode(gameObjects[i], false, ControlPointMode.Mirrored);
            }
        }

        private static void SetMode(GameObject selected, bool isRigid, ControlPointMode mode)
        {
            MeshDeformer meshDeformer = selected.GetComponentInParent<MeshDeformer>();
            if (meshDeformer == null)
            {
                return;
            }
            Scaffold selectedScaffold = selected.GetComponent<Scaffold>();
            ControlPoint selectedControlPoint = selected.GetComponent<ControlPoint>();


            Undo.RecordObject(meshDeformer, "Battlehub.MeshDeformer.SetMode");
            MeshDeformerEditor.RecordScaffolds(meshDeformer, "Battlehub.MeshDeformer.SetMode");
            EditorUtility.SetDirty(meshDeformer);

            if (selectedScaffold != null && selectedScaffold.gameObject != meshDeformer.gameObject)
            {
                ScaffoldWrapper scaffold = meshDeformer.Scaffolds.Where(s => s.Obj == selectedScaffold).FirstOrDefault();
                if (scaffold != null)
                {
                    for (int i = 0; i < scaffold.CurveIndices.Length; ++i)
                    {
                        int curveIndex = scaffold.CurveIndices[i];
                        if (mode == ControlPointMode.Free)
                        {
                            meshDeformer.SetIsRigid(curveIndex * 3, isRigid);
                        }

                        if (!isRigid)
                        {
                            meshDeformer.SetControlPointMode(curveIndex * 3, mode);
                            meshDeformer.SetControlPointMode(curveIndex * 3 + 3, mode);
                        }
                    }
                }
                else
                {
                    Debug.LogError("scaffold not found");
                }

            }
            else if (selectedControlPoint != null)
            {
                if (mode == ControlPointMode.Free)
                {
                    meshDeformer.SetIsRigid(selectedControlPoint.Index, isRigid);
                }

                if (!isRigid)
                {
                    meshDeformer.SetControlPointMode(selectedControlPoint.Index, mode);
                }
            }
            else
            {
                MeshDeformerEditor.SetMode(meshDeformer, mode, isRigid);
            }
        }


        [MenuItem("Tools/Mesh Deformer/Subdivide Mesh", validate = true)]
        public static bool CanSubdivide()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            MeshFilter meshFilter = selected.GetComponent<MeshFilter>();
            return meshFilter != null;
        }

        [MenuItem("Tools/Mesh Deformer/Subdivide Mesh")]
        public static void Subdivide()
        {
            GameObject selected = Selection.activeObject as GameObject;
            MeshDeformer deformer = selected.GetComponentInParent<MeshDeformer>();
            MeshFilter meshFilter = null;
            if (deformer != null && deformer.Original != null)
            {
                if (!EditorUtility.DisplayDialog("Are you sure?", "This action is irreversible. Are you sure you want to subdivide mesh?", "Ok", "Cancel"))
                {
                    return;
                }

                string name = deformer.Original.name;
                deformer.Original = MeshSubdivider.Subdivide4(deformer.Original);
                deformer.Original.name = name;

                if(deformer.ColliderOriginal != null)
                {
                    name = deformer.ColliderOriginal.name;
                    deformer.ColliderOriginal = MeshSubdivider.Subdivide4(deformer.ColliderOriginal);
                    deformer.ColliderOriginal.name = name;
                }

                RecalculateNormals(deformer);

                //for (int i = 0; i < deformer.Scaffolds.Length; ++i)
                //{
                //    ScaffoldWrapper scaffold = deformer.Scaffolds[i];
                //    if (scaffold.Obj == null)
                //    {
                //        continue;
                //    }

                //    MeshFilter filter = scaffold.Obj.GetComponent<MeshFilter>();
                //    MeshCollider collider = scaffold.Obj.GetComponent<MeshCollider>();

                //    if (filter != null)
                //    {
                //        Mesh colliderMesh = null;
                //        if (collider != null)
                //        {
                //            collider.sharedMesh = Object.Instantiate(deformer.ColliderOriginal);
                //            collider.sharedMesh.name = deformer.ColliderOriginal.name + " Deformed";
                //            colliderMesh = collider.sharedMesh;
                //        }

                //        filter.sharedMesh = Object.Instantiate(deformer.Original);
                //        filter.sharedMesh.name = deformer.Original.name + " Deformed";
                //        scaffold.Wrap(filter.sharedMesh, colliderMesh, deformer.Axis, scaffold.CurveIndices, scaffold.SliceCount);
                //        scaffold.Deform(deformer, deformer.Original, deformer.ColliderOriginal);
                //        scaffold.RecalculateNormals();
                //    }
                //}

                //ScaffoldWrapper prev = null;
                //if (deformer.Loop)
                //{
                //    prev = deformer.Scaffolds[deformer.Scaffolds.Length - 1];
                //}

                //for (int i = 0; i < deformer.Scaffolds.Length; ++i)
                //{
                //    ScaffoldWrapper scaffold = deformer.Scaffolds[i];
                //    scaffold.SlerpContacts(deformer, deformer.Original, deformer.ColliderOriginal, prev, null);
                //    scaffold = prev;
                //}

            }
            else
            {
                meshFilter = selected.GetComponent<MeshFilter>();
                Mesh mesh = meshFilter.sharedMesh;
                Undo.RecordObject(selected, "Battlehub.MeshDeformer Subdivide");
                Undo.RecordObject(meshFilter, "Battlehub.MeshDeformer Subdivide");
                meshFilter.sharedMesh = MeshSubdivider.Subdivide4(mesh);
                meshFilter.sharedMesh.name = mesh.name;

                MeshCollider collider = selected.GetComponent<MeshCollider>();
                if(collider != null)
                {
                    Mesh colliderMesh = collider.sharedMesh;
                    Undo.RecordObject(selected, "Battlehub.MeshDeformer Subdivide");
                    Undo.RecordObject(collider, "Battlehub.MeshDeformer Subdivide");
                    collider.sharedMesh = MeshSubdivider.Subdivide4(colliderMesh);
                    collider.sharedMesh.name = colliderMesh.name;
                }
            }
        }

        public static void RecalculateNormals(MeshDeformer deformer)
        {
            for (int i = 0; i < deformer.Scaffolds.Length; ++i)
            {
                ScaffoldWrapper scaffold = deformer.Scaffolds[i];
                if (scaffold.Obj == null)
                {
                    continue;
                }

                MeshFilter filter = scaffold.Obj.GetComponent<MeshFilter>();
                MeshCollider collider = scaffold.Obj.GetComponent<MeshCollider>();

                if (filter != null)
                {
                    Mesh colliderMesh = null;
                    if (collider != null)
                    {
                        collider.sharedMesh = Object.Instantiate(deformer.ColliderOriginal);
                        collider.sharedMesh.name = deformer.ColliderOriginal.name + " Deformed";
                        colliderMesh = collider.sharedMesh;
                    }

                    filter.sharedMesh = Object.Instantiate(deformer.Original);
                    filter.sharedMesh.name = deformer.Original.name + " Deformed";
                    scaffold.Wrap(filter.sharedMesh, colliderMesh, deformer.Axis, scaffold.CurveIndices, scaffold.SliceCount);
                    scaffold.Deform(deformer, deformer.Original, deformer.ColliderOriginal);
                    scaffold.RecalculateNormals();
                }
            }

            ScaffoldWrapper prev = null;
            if (deformer.Loop)
            {
                prev = deformer.Scaffolds[deformer.Scaffolds.Length - 1];
            }

            for (int i = 0; i < deformer.Scaffolds.Length; ++i)
            {
                ScaffoldWrapper scaffold = deformer.Scaffolds[i];
                scaffold.SlerpContacts(deformer, deformer.Original, deformer.ColliderOriginal, prev, null);
                scaffold = prev;
            }
        }

        [MenuItem("Tools/Mesh Deformer/Append _%1", validate = true)]
        public static bool CanAppend()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponentInParent<MeshDeformer>();
        }

        [MenuItem("Tools/Mesh Deformer/Append _%1")]
        public static void Append()
        {
            GameObject selected = Selection.activeObject as GameObject;
            MeshDeformer deformer = selected.GetComponentInParent<MeshDeformer>();
            Undo.RecordObject(deformer, "Battlehub.MeshDeformer.Append");
            Undo.RegisterCreatedObjectUndo(deformer.Append(), "Battlehub.MeshDeformer.Append");
            EditorUtility.SetDirty(deformer);
            Selection.activeGameObject = deformer.GetComponentsInChildren<ControlPoint>(true).Last().gameObject;
        }

        [MenuItem("Tools/Mesh Deformer/Insert _%2", validate = true)]
        public static bool CanInsert()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponent<ControlPoint>();
        }

        [MenuItem("Tools/Mesh Deformer/Insert _%2")]
        public static void Insert()
        {
            GameObject selected = Selection.activeObject as GameObject;
            MeshDeformer deformer = selected.GetComponentInParent<MeshDeformer>();
            Undo.RecordObject(deformer, "Battlehub.MeshDeformer.Prepend");
            Scaffold[] scaffolds = deformer.GetComponentsInChildren<Scaffold>();
            foreach (Scaffold scaffold in scaffolds)
            {
                Undo.RecordObject(scaffold, "Battlehub.MeshDeformer.Prepend");
            }
            SplineControlPoint ctrlPoint = selected.GetComponent<ControlPoint>();
            Undo.RegisterCreatedObjectUndo(deformer.Insert((ctrlPoint.Index + 2) / 3), "Battlehub.MeshDeformer.Insert");

            EditorUtility.SetDirty(deformer);
            Selection.activeGameObject = deformer.GetComponentsInChildren<ControlPoint>(true).ElementAt(ctrlPoint.Index + 3).gameObject;
        }

        [MenuItem("Tools/Mesh Deformer/Prepend _%3", validate = true)]
        public static bool CanPrepend()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponentInParent<MeshDeformer>();
        }

        [MenuItem("Tools/Mesh Deformer/Prepend _%3")]
        public static void Prepend()
        {
            GameObject selected = Selection.activeObject as GameObject;
            MeshDeformer deformer = selected.GetComponentInParent<MeshDeformer>();
            Undo.RecordObject(deformer, "Battlehub.MeshDeformer.Prepend");
            Scaffold[] scaffolds = deformer.GetComponentsInChildren<Scaffold>();
            foreach (Scaffold scaffold in scaffolds)
            {
                Undo.RecordObject(scaffold, "Battlehub.MeshDeformer.Prepend");
            }
            Undo.RegisterCreatedObjectUndo(deformer.Prepend(), "Battlehub.MeshDeformer.Prepend");
            EditorUtility.SetDirty(deformer);
            Selection.activeGameObject = deformer.GetComponentsInChildren<ControlPoint>(true).First().gameObject;
        }

        [MenuItem("Tools/Mesh Deformer/Straighten _%4", validate = true)]
        public static bool CanStraighten()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return (selected.GetComponent<Scaffold>() != null || selected.GetComponent<ControlPoint>() != null) && selected.GetComponentInParent<MeshDeformer>() != null;
        }


        [MenuItem("Tools/Mesh Deformer/Straighten _%4")]
        public static void Straighten()
        {
            GameObject selected = Selection.activeObject as GameObject;
            ControlPoint ctrlPoint = selected.GetComponent<ControlPoint>();
            MeshDeformer deformer = selected.GetComponentInParent<MeshDeformer>();
            int index = 0;
            if (ctrlPoint == null)
            {
                Scaffold scaffold = selected.GetComponent<Scaffold>();
                for (int i = 0; i < deformer.Scaffolds.Length; ++i)
                {
                    if (deformer.Scaffolds[i].Obj == scaffold)
                    {
                        index = deformer.Scaffolds[i].CurveIndices.Min() * 3 + 1;
                        break;
                    }
                }
            }
            else
            {
                index = ctrlPoint.Index;
            }


            Undo.RecordObject(deformer, "Battlehub.MeshDeformer.Straighten");
            MeshDeformerEditor.RecordScaffolds(deformer, "Battlehub.MeshDeformer.Straighten");
            EditorUtility.SetDirty(deformer);
            deformer.Straighten(index);
        }

        [MenuItem("Tools/Mesh Deformer/Remove Curve", validate = true)]
        public static bool CanRemove()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponent<ControlPoint>() && selected.GetComponentInParent<MeshDeformer>();
        }

        [MenuItem("Tools/Mesh Deformer/Remove Curve")]
        public static void Remove()
        {
            GameObject selected = Selection.activeObject as GameObject;
            ControlPoint ctrlPoint = selected.GetComponent<ControlPoint>();
            MeshDeformer deformer = selected.GetComponentInParent<MeshDeformer>();
            Selection.activeGameObject = deformer.gameObject;

            Undo.RecordObject(deformer, "Battlehub.MeshDeformer.Remove");
            MeshDeformerEditor.RecordScaffolds(deformer, "Battlehub.MeshDeformer.Remove");

            GameObject removeObject;
            deformer.Remove((ctrlPoint.Index - 1) / 3, out removeObject);
            if (removeObject != null)
            {
                Undo.DestroyObjectImmediate(removeObject);
            }

            EditorUtility.SetDirty(deformer);
        }

        [MenuItem("Tools/Mesh Deformer/Duplicate", validate = true)]
        public static bool CanDuplicate()
        {
            GameObject selected = Selection.activeObject as GameObject;
            return selected != null && selected.GetComponentInParent<MeshDeformer>() != null;
        }

        [MenuItem("Tools/Mesh Deformer/Duplicate")]
        public static void Duplicate()
        {
            GameObject selected = Selection.activeObject as GameObject;
            GameObject copy = GameObject.Instantiate(selected.GetComponentInParent<MeshDeformer>().gameObject);

            Undo.RegisterCreatedObjectUndo(copy, "Battlehub.MeshDeformer.Duplicate");
            MeshDeformer deformer = copy.GetComponentInParent<MeshDeformer>();
            deformer.WrapAndDeformAll();

            Selection.activeGameObject = copy;
        }


        [MenuItem("Tools/Mesh Deformer/Postprocessing/Smooth Spline", validate = true)]
        public static bool CanFit()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponentInParent<SplineBase>();
        }

        [MenuItem("Tools/Mesh Deformer/Postprocessing/Smooth Spline")]
        public static void Fit()
        {
            GameObject selected = Selection.activeObject as GameObject;
            SplineBase spline = selected.GetComponentInParent<SplineBase>();
            if (spline is MeshDeformer)
            {
                MeshDeformer deformer = (MeshDeformer)spline;
                Undo.RecordObject(deformer, "Battlehub.MeshDeformer.Fit");
                MeshDeformerEditor.RecordScaffolds(deformer, "Battlehub.MeshDeformer.Fit");
                EditorUtility.SetDirty(deformer);
            }
            else
            {
                Undo.RecordObject(spline, "Battlehub.MeshDeformer.Fit");
                EditorUtility.SetDirty(spline);
            }


            spline.Smooth();
        }

        [MenuItem("Tools/Mesh Deformer/Postprocessing/Extract Spline", validate = true)]
        public static bool CanExtractSpline()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponentInParent<MeshDeformer>();
        }

        [MenuItem("Tools/Mesh Deformer/Postprocessing/Extract Spline")]
        public static void ExtractSpline()
        {
            GameObject selected = Selection.activeObject as GameObject;
            MeshDeformer deformer = selected.GetComponentInParent<MeshDeformer>();

            GameObject splineObj = new GameObject();
            splineObj.name = "Spline";
            splineObj.transform.position = deformer.transform.position;
            splineObj.transform.rotation = deformer.transform.rotation;
            splineObj.transform.localScale = deformer.transform.localScale;
            Spline spline = splineObj.AddComponent<Spline>();
            Undo.RegisterCreatedObjectUndo(spline, "Battlehub.MeshDeformer.Extract Spline");
            spline.Load(deformer.Save());
            EditorUtility.SetDirty(splineObj);
            Selection.activeGameObject = splineObj;
        }


        [MenuItem("Tools/Mesh Deformer/Postprocessing/Remove Deformer", validate = true)]
        public static bool CanRemoveDeformer()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponentInParent<MeshDeformer>();
        }

        [MenuItem("Tools/Mesh Deformer/Postprocessing/Remove Deformer")]
        public static void RemoveDeformer()
        {
            GameObject selected = Selection.activeObject as GameObject;
            MeshDeformer deformer = selected.GetComponentInParent<MeshDeformer>();
            ControlPoint[] controlPoints = deformer.GetComponentsInChildren<ControlPoint>(true);
            for (int i = 0; i < controlPoints.Length; ++i)
            {
                Undo.DestroyObjectImmediate(controlPoints[i].gameObject);
            }

            Scaffold[] scaffolds = deformer.GetComponentsInChildren<Scaffold>();
            for (int i = 0; i < scaffolds.Length; ++i)
            {
                Undo.DestroyObjectImmediate(scaffolds[i]);
            }

            Undo.DestroyObjectImmediate(deformer);
        }


        [MenuItem("Tools/Mesh Deformer/Postprocessing/Rollback", validate = true)]
        public static bool CanRollback()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponentInParent<MeshDeformer>();
        }

        [MenuItem("Tools/Mesh Deformer/Postprocessing/Rollback")]
        public static void Rollback()
        {
            GameObject selected = Selection.activeObject as GameObject;
            Rollback(selected);
        }

        public static bool Rollback(GameObject selected)
        {
            MeshDeformer deformer = selected.GetComponentInParent<MeshDeformer>();
            if (deformer != null)
            {
                selected = deformer.gameObject;
                Selection.activeGameObject = selected;
            }
            MeshFilter meshFilter = selected.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                EditorUtility.DisplayDialog("MeshFilter required", "Select object with MeshFilter component", "OK");
                return false;
            }
          
            if (deformer != null)
            {
                if (!EditorUtility.DisplayDialog("Are you sure?", "This action is irreversible. Deformation will be lost", "Ok", "Cancel"))
                {
                    return false;
                }

                ControlPoint[] controlPoints = deformer.GetComponentsInChildren<ControlPoint>(true);
                for (int i = 0; i < controlPoints.Length; ++i)
                {
                    Object.DestroyImmediate(controlPoints[i].gameObject);
                }

                Scaffold[] scaffolds = deformer.GetComponentsInChildren<Scaffold>();
                for (int i = 0; i < scaffolds.Length; ++i)
                {
                    if (scaffolds[i].gameObject != deformer.gameObject)
                    {
                        Object.DestroyImmediate(scaffolds[i].gameObject);
                    }
                }

                Mesh original = deformer.Original;
                meshFilter.sharedMesh = original;

                Mesh colliderOriginal = deformer.ColliderOriginal;
                if(colliderOriginal != null)
                {
                    MeshCollider collider = deformer.GetComponent<MeshCollider>();
                    if(collider != null)
                    {
                        collider.sharedMesh = colliderOriginal;
                    }
                }

                Object.DestroyImmediate(deformer);
            }

            
            Scaffold scaffold = selected.GetComponent<Scaffold>();
            if (scaffold != null)
            {
                Object.DestroyImmediate(scaffold);
            }

            return true;
        }

        [MenuItem("Tools/Mesh Deformer/Postprocessing/Combine And Save", validate = true)]
        public static bool CanCombineAndSave()
        {
            GameObject selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                return false;
            }

            return selected.GetComponentInParent<MeshDeformer>();
        }

        [MenuItem("Tools/Mesh Deformer/Postprocessing/Combine And Save")]
        public static void CombineAndSave()
        {
            GameObject selected = Selection.activeObject as GameObject;
            MeshDeformer deformer = selected.GetComponentInParent<MeshDeformer>();
            GameObject[] gameObjects = deformer.GetComponentsInChildren<Scaffold>().Select(s => s.gameObject).ToArray();

            CombineResult combineResult = Battlehub.MeshTools.MeshUtils.Combine(gameObjects, deformer.gameObject);
            if (combineResult != null)
            {
                CleanupCombined(combineResult.GameObject);
                Battlehub.MeshTools.MeshUtils.SaveMesh(new[] { combineResult.GameObject }, "Battlehub/");
            }
            else
            {
                Debug.LogWarning("Unable to Combine and Save");
            }
        }

        private static void CleanupCombined(GameObject gameObject)
        {
            MeshDeformer deformer = gameObject.GetComponent<MeshDeformer>();
            if (deformer != null)
            {
                UnityEngine.Object.DestroyImmediate(deformer);
            }
            Scaffold scaffold = gameObject.GetComponent<Scaffold>();
            if (scaffold != null)
            {
                UnityEngine.Object.DestroyImmediate(scaffold);
            }
        }
    }
}
