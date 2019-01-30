using UnityEngine;
using Battlehub.RTEditor;

namespace Battlehub.MeshDeformer2
{
    public class MeshDeformerRuntimeCmd : SplineRuntimeCmd
    {
        public override void Append()
        {
            RunAction<SplineBase>((spline, go) =>
            {
                MeshDeformer deformer = spline as MeshDeformer;
                if(deformer != null)
                {
                    deformer.Append();
                }
                else
                {
                    base.Append();
                }
            });
        }

        public override void Insert()
        {
            RunAction<SplineBase>((spline, go) =>
            {
                MeshDeformer deformer = spline as MeshDeformer;
                if (deformer != null)
                {
                    GameObject selection = RuntimeSelection.activeGameObject;
                    if (selection != null)
                    {
                        ControlPoint ctrlPoint = selection.GetComponent<ControlPoint>();
                        if (ctrlPoint != null)
                        {
                            deformer.Insert((ctrlPoint.Index + 2) / 3);
                        }
                    }
                }
                else
                {
                    base.Insert();
                }
            });
        }

        public override void Prepend()
        {
            RunAction<SplineBase>((spline, go) =>
            {
                MeshDeformer deformer = spline as MeshDeformer;
                if (deformer != null)
                {
                    deformer.Prepend();
                }
                else
                {
                    base.Prepend();
                }
            });
        }

        public override void Remove()
        {
            RunAction<SplineBase>((spline, go) =>
            {
                MeshDeformer deformer = spline as MeshDeformer;
                if (deformer != null)
                {
                    GameObject selection = RuntimeSelection.activeGameObject;
                    if (selection != null)
                    {
                        SplineControlPoint ctrlPoint = selection.GetComponent<SplineControlPoint>();
                        if (ctrlPoint != null)
                        {
                            deformer.Remove((ctrlPoint.Index - 1) / 3);
                        }
                        RuntimeSelection.activeGameObject = deformer.gameObject;
                    }
                }
                else
                {
                    base.Remove();
                }
            });
        }

        public override void Smooth()
        {
            base.Smooth();
        }

        public override void SetMirroredMode()
        {
            base.SetMirroredMode();
        }

        public override void SetAlignedMode()
        {
            base.SetAlignedMode();
        }

        public override void SetFreeMode()
        {
            base.SetFreeMode();
        }

        public override void Load()
        {
            base.Load();
        }

        public override void Save()
        {
            base.Save();
        }
    }
}


