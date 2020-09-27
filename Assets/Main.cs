using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

	public GameObject meshObject;
	public Transform panel;
	public LineRenderer lineRenderer;
	private Mesh mesh;

	void Start () {
		mesh = meshObject.GetComponent<MeshFilter>().mesh;
		Debug.Log(Vector3.Cross(new Vector3(1, 0, 0), new Vector3(0, 1, 0)));
	}
	
	Vector3 downPos = Vector3.one * 1000;
	Vector3 upPos = Vector3.one * 1000;
	bool drawing = false;
	void Update () {
		if (Input.GetMouseButtonDown(0)){
			downPos = Utility.MousePostionToWorld(Input.mousePosition, meshObject.transform);
			drawing = true;
		}
		if (Input.GetMouseButtonUp(0)){
			drawing = false;
			SliceMesh.DoSlice(new Panel(panel.up, panel.position), meshObject);
		}
		if (drawing) {
			upPos = Utility.MousePostionToWorld(Input.mousePosition, meshObject.transform);
			lineRenderer.startWidth = 0.05f;
			lineRenderer.endWidth = 0.05f;
			lineRenderer.SetPositions(new Vector3[]{
				downPos, upPos
			});
			DrawPanel();
		}
	}

	void DrawPanel(){
		panel.transform.position = new Vector3((downPos.x + upPos.x)/2, (downPos.y + upPos.y) / 2, meshObject.transform.position.z);
		panel.transform.right = Vector3.Normalize(upPos - downPos);
	}
/*
	void DoSplit(){
		List<Vector3> upVertices = new List<Vector3>();
		List<Vector3> downVertics = new List<Vector3>();
		List<Vector3> onVertices = new List<Vector3>();
		SplitVerticesByPanel(out upVertices, out downVertics, out onVertices);
		// DebugClearPoint();
		// DebugPoint(upVertices, Color.blue, 0.019f);
		// DebugPoint(downVertics, Color.green, 0.019f);
		// DebugPoint(onVertices, Color.yellow, 0.019f);
		List<int[]> upTris = new List<int[]>();
		List<int[]> downTris = new List<int[]>();
		List<int[]> midTirs = GetLinePanelTriangles(out upTris, out downTris);
		List<Vector3[]>[] upAndDownVerts = MidTirangleAddPoint(midTirs);
		CreateNewMesh(upTris, upAndDownVerts[0], "part1");
		CreateNewMesh(downTris, upAndDownVerts[1], "part2");
		HideOther();
	}

	void DestroyParts(){
		GameObject g1 = GameObject.Find("part1");
		if (g1){
			GameObject.Destroy(g1);
		}
		GameObject g2 = GameObject.Find("part2");
		if (g2){
			GameObject.Destroy(g2);
		}
	}

	void ShowMeshObject(){
		meshObject.SetActive(true);
		lineRenderer.gameObject.SetActive(true);
		panel.gameObject.SetActive(true);
	}

	void HideOther(){
		// DebugClearPoint();
		meshObject.SetActive(false);
		lineRenderer.gameObject.SetActive(false);
		panel.gameObject.SetActive(false);
	}

	Vector3 MousePostionToWorld(Vector3 mousePos, Transform targetTransform)
    {
        Vector3 dir = targetTransform.position - Camera.main.transform.position;
        Vector3 normardir = Vector3.Project(dir, Camera.main.transform.forward);
        return Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, normardir.magnitude));
    }

	// 1. 顶点分类成面上面下
	void SplitVerticesByPanel(out List<Vector3> upVertices, out List<Vector3> downVertics, out List<Vector3> onVertics){
		// 面的法线
		Vector3 panelNormalVec = panel.transform.up;

		// 面上任意一点
		Vector3 panelCenterPos = panel.transform.position;

		// Mesh mesh = meshObject.GetComponent<MeshFilter>().mesh;
		upVertices = new List<Vector3>();
		downVertics = new List<Vector3>();
		onVertics = new List<Vector3>();
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			Vector3 vert_mesh = mesh.vertices[i];
			Vector3 vert_worldPos = ObjectPosToWorldPos(vert_mesh, meshObject);
			
			float v = CaculatePos(panelNormalVec, panelCenterPos, vert_worldPos);
			if (v == 0)
				onVertics.Add(vert_mesh);
			else if (v < 0)
				downVertics.Add(vert_mesh);
			else if (v > 0)
				upVertices.Add(vert_mesh);
		}
	}
	float CaculatePos(Vector3 panelNormalVec, Vector3 panelCenterPos, Vector3 worldPos){
		Vector3 posToCenterVec = worldPos - panelCenterPos;
		float cosVal = Vector3.Dot(posToCenterVec, panelNormalVec) / (posToCenterVec.magnitude * panelNormalVec.magnitude);
		return cosVal;
	}
	Vector3 ObjectPosToWorldPos(Vector3 posInObj, GameObject obj){
		return obj.transform.TransformPoint(posInObj);
	}
	Vector3 WorldPosToObjectPos(Vector3 posInWorld, GameObject obj){
		return obj.transform.InverseTransformPoint(posInWorld);
	}

	// 2. 计算与面相交的三角形
	// 三角形的三个点不在面的同一侧
	List<int[]> GetLinePanelTriangles(out List<int[]> upTris, out List<int[]> downTris){
		upTris = new List<int[]>();
		downTris = new List<int[]>();
		Vector3 panelNormalVec = panel.transform.up;
		Vector3 panelCenterPos = panel.transform.position;

		int[] triangles = mesh.triangles;
		List<int[]> tris = new List<int[]>();
		for (int i = 0; i < triangles.Length - 3; i+=3)
		{
			float upCount = 0;
			float downCount = 0;
			for (int j = 0; j < 3; j++)
			{
				Vector3 vert = mesh.vertices[triangles[i + j]];	
				Vector3 worldPos = ObjectPosToWorldPos(vert, meshObject);
				float dis = CaculatePos(panelNormalVec, panelCenterPos, worldPos);
				if (dis < 0){
					downCount ++;
				}
				else{
					upCount ++;
				}
			}
			int[] tri = new int[3]{
				triangles[i + 0],
				triangles[i + 1],
				triangles[i + 2],
			};
			if ((upCount == 1 && downCount == 2) || (upCount == 2 && downCount == 1)){
				tris.Add(tri);
			}
			if (upCount == 3 && downCount == 0){
				upTris.Add(tri);
			}
			else if (upCount == 0 && downCount == 3){
				downTris.Add(tri);
			}
		}


		// debug
		// for (int i = 0; i < tris.Count; i++)
		// {
		// 	for (int j = 0; j < tris[i].Length; j++)
		// 	{
		// 		DebugPoint(mesh.vertices[tris[i][j]], Color.red, 0.02f);
		// 	}
		// }
		return tris;
	}

	List<Vector3[]>[] MidTirangleAddPoint(List<int[]> midTirs){
		List<Vector3[]> up = new List<Vector3[]>();
		List<Vector3[]> down = new List<Vector3[]>();

		Vector3 panelNormalVec = panel.transform.up;
		Vector3 panelCenterPos = panel.transform.position;
		for (int i = 0; i < midTirs.Count; i++)
		{
			int[] tirs = midTirs[i];
			Vector3 vert0 = ObjectPosToWorldPos(mesh.vertices[tirs[0]], meshObject);
			Vector3 vert1 = ObjectPosToWorldPos(mesh.vertices[tirs[1]], meshObject);
			Vector3 vert2 = ObjectPosToWorldPos(mesh.vertices[tirs[2]], meshObject);
			Vector3[] line1 = new Vector3[]{vert0, vert1};
			Vector3[] line2 = new Vector3[]{vert1, vert2};
			Vector3[] line3 = new Vector3[]{vert2, vert0};

			bool line1HavePoint = HavePointBtweenPanelLine(line1, panelNormalVec, panelCenterPos);
			bool line2HavePoint = HavePointBtweenPanelLine(line2, panelNormalVec, panelCenterPos);
			bool line3HavePoint = HavePointBtweenPanelLine(line3, panelNormalVec, panelCenterPos);

			Vector3 intersectP1 = Vector3.zero;
			if (line1HavePoint){
				intersectP1 = GetIntersectWithLineAndPlane(line1, panelNormalVec, panelCenterPos);
			}
			Vector3 intersectP2 = Vector3.zero;
			if (line2HavePoint){
				intersectP2 = GetIntersectWithLineAndPlane(line2, panelNormalVec, panelCenterPos);
			}
			Vector3 intersectP3 = Vector3.zero;
			if (line3HavePoint){
				intersectP3 = GetIntersectWithLineAndPlane(line3, panelNormalVec, panelCenterPos);
			}

			List<Vector3> verts = new List<Vector3>();
			verts.Add(mesh.vertices[tirs[0]]);
			if (line1HavePoint){
				verts.Add(WorldPosToObjectPos(intersectP1, meshObject));
				// DebugPoint(WorldPosToObjectPos(intersectP1, meshObject), Color.black, 0.02f);
			}
			verts.Add(mesh.vertices[tirs[1]]);
			if (line2HavePoint){
				verts.Add(WorldPosToObjectPos(intersectP2, meshObject));
				// DebugPoint(WorldPosToObjectPos(intersectP2, meshObject), Color.black, 0.02f);
			}
			verts.Add(mesh.vertices[tirs[2]]);
			if (line3HavePoint){
				verts.Add(WorldPosToObjectPos(intersectP3, meshObject));
				// DebugPoint(WorldPosToObjectPos(intersectP3, meshObject), Color.black, 0.02f);
			}

			List<Vector3> upPos = new List<Vector3>();
			List<Vector3> downPos = new List<Vector3>();
			if (!line1HavePoint){
				float dis = CaculatePos(panelNormalVec, panelCenterPos, vert2);
				if (dis > 0) {
					upPos.Add(WorldPosToObjectPos(intersectP2, meshObject));
					upPos.Add(mesh.vertices[tirs[2]]);
					upPos.Add(WorldPosToObjectPos(intersectP3, meshObject));

					downPos.Add(mesh.vertices[tirs[1]]);
					downPos.Add(WorldPosToObjectPos(intersectP2, meshObject));
					downPos.Add(WorldPosToObjectPos(intersectP3, meshObject));
					downPos.Add(mesh.vertices[tirs[0]]);
				}
				else{
					downPos.Add(WorldPosToObjectPos(intersectP2, meshObject));
					downPos.Add(mesh.vertices[tirs[2]]);
					downPos.Add(WorldPosToObjectPos(intersectP3, meshObject));

					upPos.Add(mesh.vertices[tirs[1]]);
					upPos.Add(WorldPosToObjectPos(intersectP2, meshObject));
					upPos.Add(WorldPosToObjectPos(intersectP3, meshObject));
					upPos.Add(mesh.vertices[tirs[0]]);
				}
			}else if(!line2HavePoint){
				float dis = CaculatePos(panelNormalVec, panelCenterPos, vert0);
				if (dis > 0) {
					upPos.Add(WorldPosToObjectPos(intersectP3, meshObject));
					upPos.Add(mesh.vertices[tirs[0]]);
					upPos.Add(WorldPosToObjectPos(intersectP1, meshObject));

					downPos.Add(mesh.vertices[tirs[2]]);
					downPos.Add(WorldPosToObjectPos(intersectP3, meshObject));
					downPos.Add(WorldPosToObjectPos(intersectP1, meshObject));
					downPos.Add(mesh.vertices[tirs[1]]);
				}
				else{
					downPos.Add(WorldPosToObjectPos(intersectP3, meshObject));
					downPos.Add(mesh.vertices[tirs[0]]);
					downPos.Add(WorldPosToObjectPos(intersectP1, meshObject));

					upPos.Add(mesh.vertices[tirs[2]]);
					upPos.Add(WorldPosToObjectPos(intersectP3, meshObject));
					upPos.Add(WorldPosToObjectPos(intersectP1, meshObject));
					upPos.Add(mesh.vertices[tirs[1]]);
				}
			}else if (!line3HavePoint){
				float dis = CaculatePos(panelNormalVec, panelCenterPos, vert1);
				if (dis > 0) {
					upPos.Add(WorldPosToObjectPos(intersectP1, meshObject));
					upPos.Add(mesh.vertices[tirs[1]]);
					upPos.Add(WorldPosToObjectPos(intersectP2, meshObject));

					downPos.Add(mesh.vertices[tirs[0]]);
					downPos.Add(WorldPosToObjectPos(intersectP1, meshObject));
					downPos.Add(WorldPosToObjectPos(intersectP2, meshObject));
					downPos.Add(mesh.vertices[tirs[2]]);
				}
				else{
					downPos.Add(WorldPosToObjectPos(intersectP1, meshObject));
					downPos.Add(mesh.vertices[tirs[1]]);
					downPos.Add(WorldPosToObjectPos(intersectP2, meshObject));

					upPos.Add(mesh.vertices[tirs[0]]);
					upPos.Add(WorldPosToObjectPos(intersectP1, meshObject));
					upPos.Add(WorldPosToObjectPos(intersectP2, meshObject));
					upPos.Add(mesh.vertices[tirs[2]]);
				}
			}
			up.Add(upPos.ToArray());
			down.Add(downPos.ToArray());
		}
		return new List<Vector3[]>[] {up, down};
	}

	bool HavePointBtweenPanelLine(Vector3[] line, Vector3 panelNormalVec, Vector3 panelCenterPos){
		float dis1 = CaculatePos(panelNormalVec, panelCenterPos, line[0]);
		float dis2 = CaculatePos(panelNormalVec, panelCenterPos, line[1]);
		return dis1 * dis2 < 0;
	}
    private Vector3 GetIntersectWithLineAndPlane(Vector3[] line, Vector3 panelNormalVec, Vector3 panelCenterPos)
    {
		Vector3 direct = (line[1] - line[0]).normalized;
        float d = Vector3.Dot(panelCenterPos - line[0], panelNormalVec) / Vector3.Dot(direct, panelNormalVec);
 
        return d * direct.normalized + line[0];
    }

	List<Vector3[]>[] SpratorMidPartPoints(List<Vector3[]> midPartVerts){
		List<Vector3[]> up = new List<Vector3[]>();
		List<Vector3[]> down = new List<Vector3[]>();

		Vector3 panelNormalVec = panel.transform.up;
		Vector3 panelCenterPos = panel.transform.position;

		int count = 0;
		for (int i = 0; i < midPartVerts.Count; i++)
		{
			Vector3[] tirsAndIntersect = midPartVerts[i];
			List<Vector3> upPoints = new List<Vector3>();
			List<Vector3> downPoints = new List<Vector3>();
			for (int j = 0; j < tirsAndIntersect.Length; j++)
			{
				float dis = CaculatePos(panelNormalVec, panelCenterPos, ObjectPosToWorldPos(tirsAndIntersect[j], meshObject));
				if (Mathf.Abs(dis - 0) < 0.001f){
					count ++;
				}
				if (dis >= 0){
					upPoints.Add(tirsAndIntersect[j]);
				}
				else if (dis <= 0){
					downPoints.Add(tirsAndIntersect[j]);
				}
			}
			up.Add(upPoints.ToArray());
			down.Add(downPoints.ToArray());
		}
		Debug.Log("count === " + count);

		return new List<Vector3[]>[]{up, down};
	}

	void CreateNewMesh(List<int[]> tris, List<Vector3[]> partOfVerts, string name){
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		// List<Color> colors = new List<Color>();

		// 通过原模型构造mesh
		for (int i = 0; i < tris.Count; i++)
		{
			int[] preTir = tris[i];
			for (int j = 0; j < preTir.Length; j++)
			{
				int index = preTir[j];
				Vector3 vert = mesh.vertices[index];
				int triIndex = i * 3 + j;
				Vector3 normal = mesh.normals[index];
				Vector2 uv = mesh.uv[index];
				// Color color = mesh.colors[index];

				vertices.Add(vert);
				triangles.Add(triIndex);
				normals.Add(normal);
				uvs.Add(uv);
				// colors.Add(color);

			}
		}

		// Debug.LogFormat("verts count = {0}, vert count * 3 = {1}, tris count = {2}", vertices.Count, vertices.Count * 3, triangles.Count);
		// for (int i = 0; i < partOfVerts.Count; i++)
		// {
		// 	Vector3[] triVerts = partOfVerts[i];
		// 	if (triVerts.Length != 3 && triVerts.Length != 4){
		// 		Debug.LogWarning("xxxxxxxxxxxxxxxxxx");
		// 	}
		// 	// for (int j = 0; j < triVerts.Length; j++)
		// 	// {
		// 	// 	// vertices.Add(triVerts[j]);
		// 	// 	normals.Add(Vector3.one);
		// 	// 	uvs.Add(Vector2.one);
		// 	// }
		// 	int count = vertices.Count - 1;
		// 	if (triVerts.Length == 3){
		// 		vertices.Add(triVerts[0]);
		// 		vertices.Add(triVerts[1]);
		// 		vertices.Add(triVerts[2]);
		// 		for (int k = 0; k < 3; k++)
		// 		{
		// 			normals.Add(Vector3.one);
		// 			uvs.Add(Vector2.one);
		// 		}
		// 		triangles.Add(count + 0);
		// 		triangles.Add(count + 1);
		// 		triangles.Add(count + 2);
		// 	}
		// 	else if (triVerts.Length == 4){
		// 		vertices.Add(triVerts[0]);
		// 		vertices.Add(triVerts[1]);
		// 		vertices.Add(triVerts[2]);
		// 		vertices.Add(triVerts[0]);
		// 		vertices.Add(triVerts[2]);
		// 		vertices.Add(triVerts[3]);
		// 		for (int k = 0; k < 6; k++)
		// 		{
		// 			normals.Add(Vector3.one);
		// 			uvs.Add(Vector2.one);
		// 		}
		// 		triangles.Add(count + 0);
		// 		triangles.Add(count + 1);
		// 		triangles.Add(count + 2);
		// 		triangles.Add(count + 0);
		// 		triangles.Add(count + 2);
		// 		triangles.Add(count + 3);
		// 	}
		// }
		// Debug.LogFormat("verts count = {0}, vert count * 3 = {1}, tris count = {2}", vertices.Count, vertices.Count * 3, triangles.Count);



		Mesh newMesh = new Mesh();
		newMesh.vertices = vertices.ToArray();
		newMesh.triangles = triangles.ToArray();
		newMesh.normals = normals.ToArray();
		newMesh.uv = uvs.ToArray();
		// newMesh.colors = colors.ToArray();

		GameObject go = new GameObject(name);
		go.AddComponent<MeshFilter>().mesh = newMesh;
		go.AddComponent<MeshRenderer>().material = meshObject.GetComponent<MeshRenderer>().material;
		go.transform.position = meshObject.transform.position;
		go.transform.localScale = meshObject.transform.localScale;
		go.transform.localRotation = meshObject.transform.localRotation;
		MeshCollider meshCollider = go.AddComponent<MeshCollider>();
		meshCollider.convex = true;
		go.AddComponent<Rigidbody>();
	}

	// List<GameObject> debugPoints = new List<GameObject>();
	// void DebugClearPoint(){
	// 	foreach (GameObject item in debugPoints)
	// 	{
	// 		GameObject.Destroy(item);
	// 	}
	// }
	// void DebugPoint(List<Vector3> points, Color? color = null, float? scale = null){
	// 	color = color != null ? color : Color.red;
	// 	scale = scale != null ? scale : 0.05f;
	// 	for (int i = 0; i < points.Count; i++)
	// 	{
	// 		GameObject go = GameObject.Instantiate(testPoint);
	// 		go.SetActive(true);
	// 		debugPoints.Add(go);
	// 		go.transform.localScale = Vector3.one * (float)scale;
	// 		go.transform.position = ObjectPosToWorldPos(points[i], meshObject);
	// 		go.GetComponent<MeshRenderer>().material.SetColor("_Color", (Color)color);
	// 	}
	// }

	// void DebugPoint(Vector3 point, Color? color = null, float? scale = null){
	// 	color = color != null ? color : Color.red;
	// 	scale = scale != null ? scale : 0.05f;
	// 	GameObject go = GameObject.Instantiate(testPoint);
	// 	go.SetActive(true);
	// 	debugPoints.Add(go);
	// 	go.transform.localScale = Vector3.one * (float)scale;
	// 	go.transform.position = ObjectPosToWorldPos(point, meshObject);
	// 	go.GetComponent<MeshRenderer>().material.SetColor("_Color", (Color)color);
	// }
*/
}
