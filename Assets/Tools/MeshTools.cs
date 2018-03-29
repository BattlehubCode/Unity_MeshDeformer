using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace TrainGame{
	public class MeshTools {

		public static Mesh GetMeshFromFile(string blendFilename, string meshFilename) {
			var guids = AssetDatabase.FindAssets("t:GameObject "+blendFilename, new string[] {"Assets/Models"});
			if(guids.Length == 0){
				Debug.LogError("Blend file named "+blendFilename+" was not found");
				throw new UnityException("Blend file named "+blendFilename+" was not found");
			}

			Mesh mesh = null;

			GameObject blendFileMeshes = (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(GameObject));
			foreach (Transform child in blendFileMeshes.transform){
				MeshFilter mf = child.GetComponent<MeshFilter>();
				if (mf && mf.sharedMesh.name == meshFilename){
					mesh = mf.sharedMesh;
					break;
				}
			}

			if(mesh == null){
				Debug.LogError("Mesh named "+meshFilename+" was not found");
				throw new UnityException("Mesh named "+meshFilename+" was not found");
			}

			return mesh;
		}

		public static Mesh ScaleMesh(Transform transform, Vector3 scale, Mesh sourceMesh = null) {

			Vector3[] _baseVertices;

			//Instantiating mesh if it wasn't already. If I skip this step, all other mesh instances would be affected.
			// if(mesh == null){
			// 	var meshFilter = transform.GetComponent<MeshFilter>();
			// 	Mesh shmesh = meshFilter.sharedMesh;
			// 	string meshName = meshFilter.sharedMesh.name;
			// 	mesh = GameObject.Instantiate(shmesh);
			// 	mesh.name = meshName + " Inverted";
			// 	// meshFilter.sharedMesh = mesh;
			// }

			//Makes a copy of the source mesh and works on it. If I work directly on the source mesh, it will scale the source mesh itself!
			string meshName = sourceMesh.name;
			Mesh mesh = GameObject.Instantiate(sourceMesh);
			mesh.name = meshName + " Inverted";


			//Scaling all vertices
			_baseVertices = mesh.vertices;
			var vertices = new Vector3[_baseVertices.Length];

			// TRYING TO POSITION IT RIGHT -----------------------------

			//Point to use as the pivot of scaling
			Vector3 originalCenter = mesh.bounds.center;
			// TRYING TO POSITION IT RIGHT -----------------------------


			for (var i = 0; i < vertices.Length; i++)
			{
				var vertex = _baseVertices[i];
				vertex.x = vertex.x * scale.x;
				vertex.y = vertex.y * scale.y;
				vertex.z = vertex.z * scale.z;
				vertices[i] = vertex;
			}
			mesh.vertices = vertices;

			//Flipping all normals
			for(int i=0;i<mesh.subMeshCount;i++){
				mesh.SetTriangles(mesh.GetTriangles(i).Reverse().ToArray(),i);
			}

			//Recalculating normals	
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			// /////////////////////

			//POSITIONING IT FOR REAL

			Vector3 newCenter = mesh.bounds.center;

			_baseVertices = mesh.vertices;
			vertices = new Vector3[_baseVertices.Length];

			for (var i = 0; i < vertices.Length; i++)
			{
				var vertex = _baseVertices[i];
				vertices[i] = 
				transform.InverseTransformPoint(transform.TransformPoint(vertex) + (originalCenter - newCenter));
			}
			mesh.vertices = vertices;

			mesh.RecalculateBounds();

			return mesh;
		}
	}
}