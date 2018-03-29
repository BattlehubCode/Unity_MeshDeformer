using UnityEngine;

namespace Battlehub.MeshTools
{
	[ExecuteInEditMode]
	public class Pivot : MonoBehaviour
	{
        [HideInInspector]
        public Transform Target;

    
		private void Start()
		{
			if(Application.isEditor && !Application.isPlaying)
			{
				if(gameObject.GetComponent<PivotDesignTime>() == null)
				{
					gameObject.AddComponent<PivotDesignTime>();
				}
			}
			else
            {
                PivotDesignTime editor = gameObject.GetComponent<PivotDesignTime>();
                if(editor != null)
                {
                    DestroyImmediate(editor);
                }

            }
        }
        
    }
}