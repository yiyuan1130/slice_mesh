using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SliceMesh {
	private static Panel panel;
	private static Mesh mesh;
	private static GameObject sliceObj;
	public static void DoSlice(Panel slice_panel, GameObject slice_go){
		Debug.Log("====================== do slice ======================");
		DebugPoint.Init(slice_go);
		panel = slice_panel;
		mesh = slice_go.GetComponent<MeshFilter>().mesh;
		Debug.Log("mesh vert -> " + mesh.vertices.Length);
		Debug.Log("mesh triangles -> " + mesh.triangles.Length);
		sliceObj = slice_go;

		// List<int[]>[] partsOfTriangles = GetPartOfTriangles();
		// List<int[]> upTris = partsOfTriangles[0];
		// List<int[]> downTris = partsOfTriangles[1];
		// List<int[]> midTris = partsOfTriangles[2];
		// DebugTris(upTris, Color.blue, 0.02f);
		// DebugTris(downTris, Color.green, 0.02f);
		// DebugTris(midTris, Color.red, 0.02f);
		Mesh[] meshs = _slice();
		// Mesh upMesh = CreateMesh(upTris, midTris);
		DebugNewGameObject(meshs[0], "part_up");
		DebugNewGameObject(meshs[1], "part_down");
	}

	static Mesh[] _slice(){
		List<Vector3> vertives1 = new List<Vector3>();
		List<int> triangles1 = new List<int>();
		List<Vector3> normals1 = new List<Vector3>();
		
		List<Vector3> vertives2 = new List<Vector3>();
		List<int> triangles2 = new List<int>();
		List<Vector3> normals2 = new List<Vector3>();
		for (int i = 0; i < mesh.triangles.Length; i+=3)
		{
			int index0 = mesh.triangles[i + 0];
			int index1 = mesh.triangles[i + 1];
			int index2 = mesh.triangles[i + 2];
			Vector3 vert0 = mesh.vertices[index0];
			Vector3 vert1 = mesh.vertices[index1];
			Vector3 vert2 = mesh.vertices[index2];
			Vector3 normal0 = mesh.normals[index0];
			Vector3 normal1 = mesh.normals[index1];
			Vector3 normal2 = mesh.normals[index2];
			Vector3 worldPos0 = Utility.ObjectToWorldPoint(vert0, sliceObj.transform);
			Vector3 worldPos1 = Utility.ObjectToWorldPoint(vert1, sliceObj.transform);
			Vector3 worldPos2 = Utility.ObjectToWorldPoint(vert2, sliceObj.transform);
			float dis0 = panel.DistanceToPoint(worldPos0);
			float dis1 = panel.DistanceToPoint(worldPos1);
			float dis2 = panel.DistanceToPoint(worldPos2);
			if (dis0 >= 0 && dis1 >= 0 && dis2 >= 0){
				int count = vertives1.Count;
				vertives1.Add(vert0);
				vertives1.Add(vert1);
				vertives1.Add(vert2);
				normals1.Add(normal0);
				normals1.Add(normal1);
				normals1.Add(normal2);
				triangles1.Add(count + 0);
				triangles1.Add(count + 1);
				triangles1.Add(count + 2);
			}
			else if (dis0 < 0 && dis1 < 0 && dis2 < 0){
				int count = vertives2.Count;
				vertives2.Add(vert0);
				vertives2.Add(vert1);
				vertives2.Add(vert2);
				normals2.Add(normal0);
				normals2.Add(normal1);
				normals2.Add(normal2);
				triangles2.Add(count + 0);
				triangles2.Add(count + 1);
				triangles2.Add(count + 2);
			}
			else{
				// Vector3[] verts = new Vector3[]{};
				if(dis0 * dis1 > 0){
					// 2单独
					index0 = mesh.triangles[i + 2];
					index1 = mesh.triangles[i + 0];
					index2 = mesh.triangles[i + 1];
				}
				else
				{
					if(dis0 * dis2 < 0){
						// 0 单独
						index0 = mesh.triangles[i + 0];
						index1 = mesh.triangles[i + 1];
						index2 = mesh.triangles[i + 2];
					}
					else{
						// 1
						index0 = mesh.triangles[i + 1];
						index1 = mesh.triangles[i + 2];
						index2 = mesh.triangles[i + 0];
					}
				}

				vert0 = mesh.vertices[index0];
				vert1 = mesh.vertices[index1];
				vert2 = mesh.vertices[index2];
				normal0 = mesh.normals[index0];
				normal1 = mesh.normals[index1];
				normal2 = mesh.normals[index2];
				worldPos0 = Utility.ObjectToWorldPoint(vert0, sliceObj.transform);
				worldPos1 = Utility.ObjectToWorldPoint(vert1, sliceObj.transform);
				worldPos2 = Utility.ObjectToWorldPoint(vert2, sliceObj.transform);
				dis0 = panel.DistanceToPoint(worldPos0);
				dis1 = panel.DistanceToPoint(worldPos1);
				dis2 = panel.DistanceToPoint(worldPos2);
				// 01
				float w1 = (0 - dis0) / (dis1 - dis0);
				Vector3 p1 = Vector3.Lerp(vert0, vert1, w1);
				// 02
				float w2 = (0 - dis0) / (dis2 - dis0);
				Vector3 p2 = Vector3.Lerp(vert0, vert2, w2);

				if (dis0 > 0){
					int count1 = vertives1.Count;
					vertives1.Add(vert0);
					vertives1.Add(p1);
					vertives1.Add(p2);
					triangles1.Add(count1 + 0);
					triangles1.Add(count1 + 1);
					triangles1.Add(count1 + 2);

					int count2 = vertives2.Count;
					vertives2.Add(p1);
					vertives2.Add(vert1);
					vertives2.Add(vert2);
					vertives2.Add(p2);
					triangles2.Add(count2 + 0);
					triangles2.Add(count2 + 1);
					triangles2.Add(count2 + 2);
					triangles2.Add(count2 + 0);
					triangles2.Add(count2 + 2);
					triangles2.Add(count2 + 3);
				}
				else{
					int count2 = vertives2.Count;
					vertives2.Add(vert0);
					vertives2.Add(p1);
					vertives2.Add(p2);
					triangles2.Add(count2 + 0);
					triangles2.Add(count2 + 1);
					triangles2.Add(count2 + 2);

					int count1 = vertives1.Count;
					vertives1.Add(p1);
					vertives1.Add(vert1);
					vertives1.Add(vert2);
					vertives1.Add(p2);
					triangles1.Add(count1 + 0);
					triangles1.Add(count1 + 1);
					triangles1.Add(count1 + 2);
					triangles1.Add(count1 + 0);
					triangles1.Add(count1 + 2);
					triangles1.Add(count1 + 3);
				}
			}
		}

		Mesh mesh1 = new Mesh();
		mesh1.vertices = vertives1.ToArray();
		mesh1.triangles = triangles1.ToArray();
		// mesh1.normals = normals1.ToArray();

		Mesh mesh2 = new Mesh();
		mesh2.vertices = vertives2.ToArray();
		mesh2.triangles = triangles2.ToArray();
		// mesh2.normals = normals2.ToArray();

		return new Mesh[]{mesh1, mesh2};
	}

	// ret [0]up [1]down [2]mid
	static List<int[]>[] GetPartOfTriangles(){
		List<int[]>[] ret = new List<int[]>[3];
		List<int[]> upTriangles = new List<int[]>();
		List<int[]> downTriangles = new List<int[]>();
		List<int[]> midTriangles = new List<int[]>();

		int[] triangles = mesh.triangles;
		for (int i = 0; i < triangles.Length; i+=3)
		{
			int[] triangle = new int[]{
				triangles[i + 0],
				triangles[i + 1],
				triangles[i + 2],
			};
			Vector3 vert0 = mesh.vertices[triangle[0]];
			Vector3 vert1 = mesh.vertices[triangle[1]];
			Vector3 vert2 = mesh.vertices[triangle[2]];
			Vector3 worldPos0 = Utility.ObjectToWorldPoint(vert0, sliceObj.transform);
			Vector3 worldPos1 = Utility.ObjectToWorldPoint(vert1, sliceObj.transform);
			Vector3 worldPos2 = Utility.ObjectToWorldPoint(vert2, sliceObj.transform);
			float dis0 = panel.DistanceToPoint(worldPos0);
			float dis1 = panel.DistanceToPoint(worldPos1);
			float dis2 = panel.DistanceToPoint(worldPos2);
			if (dis0 > 0 && dis1 > 0 && dis2 > 0){
				upTriangles.Add(triangle);
			}
			else if (dis0 < 0 && dis1 < 0 && dis2 < 0){
				downTriangles.Add(triangle);
			}
			else {
				midTriangles.Add(triangle);
			}
		}
		ret[0] = upTriangles;
		ret[1] = downTriangles;
		ret[2] = midTriangles;
		return ret;
	}

	static Mesh CreateMesh(List<int[]> partTriangle, List<int[]> midTriangles){
		Mesh newMesh = new Mesh();
		List<Vector3> vertices = new List<Vector3>();
		List<int> tirangles = new List<int>();

		for (int i = 0; i < partTriangle.Count; i++)
		{
			int[] tirangle = partTriangle[i];
			for (int j = 0; j < tirangle.Length; j++)
			{
				Vector3 vert = mesh.vertices[tirangle[j]];
				vertices.Add(vert);	
				tirangles.Add(i * 3 + j);	
			}
		}

		newMesh.vertices = vertices.ToArray();
		newMesh.triangles = tirangles.ToArray();
		return newMesh;
	}

	// upOrDown 1:up -1:down
	static void MidVertAndTri(List<int[]> midTriangles, int upOrDown){
		for (int i = 0; i < midTriangles.Count; i++)
		{
			int[] triangle = midTriangles[i];
			Vector3 vert0 = mesh.vertices[triangle[0]];
			Vector3 vert1 = mesh.vertices[triangle[1]];
			Vector3 vert2 = mesh.vertices[triangle[2]];
			Vector3 worldPos0 = Utility.ObjectToWorldPoint(vert0, sliceObj.transform);
			Vector3 worldPos1 = Utility.ObjectToWorldPoint(vert1, sliceObj.transform);
			Vector3 worldPos2 = Utility.ObjectToWorldPoint(vert2, sliceObj.transform);
			float dis0 = panel.DistanceToPoint(worldPos0);
			float dis1 = panel.DistanceToPoint(worldPos1);
			float dis2 = panel.DistanceToPoint(worldPos2);
		}
	}

	static void DebugNewGameObject(Mesh mesh, string name){
		GameObject obj = new GameObject(name);
		obj.AddComponent<MeshFilter>().mesh = mesh;
		obj.AddComponent<MeshRenderer>().material = sliceObj.GetComponent<MeshRenderer>().material;
		obj.transform.position = sliceObj.transform.position;
		obj.transform.rotation = sliceObj.transform.rotation;
		obj.transform.localScale = sliceObj.transform.localScale;
	}

	static void DebugTris(List<int[]> triangles, Color color, float size){
		for (int i = 0; i < triangles.Count; i++)
		{
			int[] triangle = triangles[i];
			for (int j = 0; j < triangle.Length; j++)
			{
				DebugPoint.Show(mesh.vertices[triangle[j]], color, size);
			}
		}
	}

}
