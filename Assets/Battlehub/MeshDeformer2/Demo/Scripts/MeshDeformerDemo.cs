using UnityEngine;

namespace Battlehub.MeshDeformer2
{
    public class MeshDeformerDemo : MonoBehaviour
    {
        
        // Use this for initialization
        void Start()
        {
            
			MeshDeformer deformer = gameObject.AddComponent<MeshDeformer>();
			deformer.Axis = Axis.Y;
			deformer.ResetDeformer();
			deformer.Append ();
			deformer.Append ();
			deformer.CurvesPerMesh = 2;
			deformer.Spacing = 1;
			deformer.Approximation = 50;
			deformer.SetControlPointLocal(0, new Vector3(-1, -4, -1));
        }

    }
}

