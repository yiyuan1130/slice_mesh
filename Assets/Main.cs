using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

	public GameObject meshObject;
	public Transform panel;
	public LineRenderer lineRenderer;

	public GameObject testPoint;
	public Mesh mesh;

	void Start () {
		mesh = meshObject.GetComponent<MeshFilter>().mesh;
	}
	
	Vector3 downPos = Vector3.one * 1000;
	Vector3 upPos = Vector3.one * 1000;
	bool drawing = false;
	void Update () {
		if (Input.GetMouseButtonDown(0)){
			downPos = MousePostionToWorld(Input.mousePosition, meshObject.transform);
			drawing = true;
		}
		if (Input.GetMouseButtonUp(0)){
			drawing = false;
			DoSplit();
		}
		if (drawing) {
			upPos = MousePostionToWorld(Input.mousePosition, meshObject.transform);
			lineRenderer.startWidth = 0.05f;
			lineRenderer.endWidth = 0.05f;
			lineRenderer.SetPositions(new Vector3[]{
				downPos, upPos
			});
			DrawPanel();
		}
	}

	void DoSplit(){
		List<Vector3> upVertices = new List<Vector3>();
		List<Vector3> downVertics = new List<Vector3>();
		List<Vector3> onVertices = new List<Vector3>();
		SplitVerticesByPanel(out upVertices, out downVertics, out onVertices);
		DebugClearPoint();
		DebugPoint(upVertices, Color.blue, 0.019f);
		DebugPoint(downVertics, Color.green, 0.019f);
		DebugPoint(onVertices, Color.yellow, 0.019f);
		List<int[]> upTris = new List<int[]>();
		List<int[]> downTris = new List<int[]>();
		List<int[]> midTirs = GetLinePanelTriangles(out upTris, out downTris);

		List<List<Vector3>> midAndUpVerts = new List<List<Vector3>>();
		List<List<Vector3>> midAndDownVerts = new List<List<Vector3>>();
		List<Vector3> midVertices = CaculateLinePanelPoint(midTirs, out midAndUpVerts, out midAndDownVerts);
		// CreateNewMesh(upTris, midAndDownVerts);
		// CreateNewMesh(downTris, midAndUpVerts);
		// HideOther();
	}

	void HideOther(){
		DebugClearPoint();
		meshObject.SetActive(false);
		lineRenderer.gameObject.SetActive(false);
		panel.gameObject.SetActive(false);
	}

	void DrawPanel(){
		panel.transform.position = new Vector3((downPos.x + upPos.x)/2, (downPos.y + upPos.y) / 2, meshObject.transform.position.z);
		panel.transform.right = Vector3.Normalize(upPos - downPos);
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
		for (int i = 0; i < tris.Count; i++)
		{
			for (int j = 0; j < tris[i].Length; j++)
			{
				DebugPoint(mesh.vertices[tris[i][j]], Color.red, 0.02f);
			}
		}
		return tris;
	}

	List<Vector3> CaculateLinePanelPoint(List<int[]> triangles, out List<List<Vector3>> midAndDownVerts, out List<List<Vector3>> midAndUpVerts){
		Vector3 panelNormalVec = panel.transform.up;
		Vector3 panelCenterPos = panel.transform.position;
		midAndDownVerts = new List<List<Vector3>>();
		midAndUpVerts = new List<List<Vector3>>();
		List<Vector3> linePanelPoints = new List<Vector3>();
		for (int i = 0; i < triangles.Count; i++)
		{
			List<Vector3> perUp = new List<Vector3>();
			List<Vector3> perdown = new List<Vector3>();
			List<Vector3> perUp_vert = new List<Vector3>();
			List<Vector3> downPos_vert = new List<Vector3>();

			List<Vector3> upPos = new List<Vector3>();
			List<Vector3> downPos = new List<Vector3>();
			for (int j = 0; j < triangles[i].Length; j++)
			{
				Vector3 vert = mesh.vertices[triangles[i][j]];
				Vector3 worldPos = ObjectPosToWorldPos(vert, meshObject);
				float dis = CaculatePos(panelNormalVec, panelCenterPos, worldPos);
				if (dis > 0){
					upPos.Add(worldPos);
					perUp_vert.Add(vert);
				}
				else if (dis < 0){
					downPos.Add(worldPos);
					downPos_vert.Add(vert);
				}
			}

			if (downPos.Count == 1 && upPos.Count == 2){
				Vector3 vert1 = WorldPosToObjectPos(GetLinePanelPoint(downPos[0], upPos[0], panelCenterPos, panelNormalVec), meshObject);
				linePanelPoints.Add(vert1);
				Vector3 vert2 = WorldPosToObjectPos(GetLinePanelPoint(upPos[1], downPos[0], panelCenterPos, panelNormalVec), meshObject);
				linePanelPoints.Add(vert2);
				perUp.Add(vert1);
				perUp.Add(perUp_vert[0]);
				perUp.Add(perUp_vert[1]);
				perUp.Add(vert2);

				perdown.Add(vert1);
				perdown.Add(vert2);
				perdown.Add(downPos_vert[0]);
			}
			else if (downPos.Count == 2 && upPos.Count == 1){
				Vector3 vert1 = WorldPosToObjectPos(GetLinePanelPoint(downPos[0], upPos[0], panelCenterPos, panelNormalVec), meshObject);
				linePanelPoints.Add(vert1);
				Vector3 vert2 = WorldPosToObjectPos(GetLinePanelPoint(upPos[0], downPos[1], panelCenterPos, panelNormalVec), meshObject);
				linePanelPoints.Add(vert2);
				perUp.Add(vert1);
				perUp.Add(perUp_vert[0]);
				perUp.Add(vert2);


				perdown.Add(downPos_vert[0]);
				perdown.Add(vert1);
				perdown.Add(vert2);
				perdown.Add(downPos_vert[1]);
			}
			midAndDownVerts.Add(perdown);
			midAndUpVerts.Add(perUp);
		}
		foreach (var pos in linePanelPoints)
		{
			DebugPoint(pos, Color.yellow, 0.02f);
		}
		return linePanelPoints;
	}
	Vector3 GetLinePanelPoint(Vector3 line_p1, Vector3 line_p2, Vector3 panel_p, Vector3 panel_normal){
		Vector3 lineDir = (line_p2 - line_p1).normalized;
		float t = lineDir.x * panel_normal.x + lineDir.y * panel_normal.y + lineDir.z * panel_normal.z;
		if (t == 0){
			//方向向量与平面平行，没有交点
			return Vector3.zero;
        }
		float m = ((panel_p.x - line_p1.x) * panel_normal.x +
					(panel_p.y - line_p1.y) * panel_normal.y +
					(panel_p.z - line_p1.z) * panel_normal.z) / t;
		Vector3 p = line_p1 + lineDir * m;
		return p;
	}

	void CreateNewMesh(List<int[]> tris, List<List<Vector3>> midPartVerts){
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<Color> colors = new List<Color>();

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

		// 通过交点及相应的顶点构造mesh
		for (int i = 0; i < midPartVerts.Count; i++)
		{
			List<Vector3> verts = midPartVerts[i];
			List<Vector3> subVertices = new List<Vector3>();
			List<int> subTriangles = new List<int>();
			List<Vector3> subNormals = new List<Vector3>();
			List<Vector2> subUVs = new List<Vector2>();

			subVertices.AddRange(verts);
			if (verts.Count == 3){
				subTriangles.AddRange(new int[]{
					0, 1, 2,
				});
			}
			else if (verts.Count == 4){
				subTriangles.AddRange(new int[]{
					0, 1, 2,
					1, 2, 3,
				});
			}
			for (int j = 0; j < verts.Count; j++)
			{
				subNormals.Add(Vector3.one);
				subUVs.Add(Vector2.one);
			}

			// 分别添加到真正的mesh中
			for (int j = 0; j < subTriangles.Count; j++)
			{
				triangles.Add(subTriangles[j] + vertices.Count - 1);
			}
			vertices.AddRange(verts);
			normals.AddRange(subNormals);
			uvs.AddRange(subUVs);
		}


		Mesh newMesh = new Mesh();
		newMesh.vertices = vertices.ToArray();
		newMesh.triangles = triangles.ToArray();
		newMesh.normals = normals.ToArray();
		newMesh.uv = uvs.ToArray();
		// newMesh.colors = colors.ToArray();

		GameObject go = new GameObject("part_x");
		go.AddComponent<MeshFilter>().mesh = newMesh;
		go.AddComponent<MeshRenderer>().material = meshObject.GetComponent<MeshRenderer>().material;
		go.transform.position = meshObject.transform.position;
		go.transform.localScale = meshObject.transform.localScale;
		go.transform.localRotation = meshObject.transform.localRotation;
	}
	
	// void SortVertices(List<Vector3> verts){
	// 	Vector3 baseVerts = verts[0];
	// 	verts.Sort((Vector3 a, Vector3 b) => {
	// 		return 
	// 	});
	// }

	List<GameObject> debugPoints = new List<GameObject>();
	void DebugClearPoint(){
		foreach (GameObject item in debugPoints)
		{
			GameObject.Destroy(item);
		}
	}
	void DebugPoint(List<Vector3> points, Color? color = null, float? scale = null){
		color = color != null ? color : Color.red;
		scale = scale != null ? scale : 0.05f;
		for (int i = 0; i < points.Count; i++)
		{
			GameObject go = GameObject.Instantiate(testPoint);
			go.SetActive(true);
			debugPoints.Add(go);
			go.transform.localScale = Vector3.one * (float)scale;
			go.transform.position = ObjectPosToWorldPos(points[i], meshObject);
			go.GetComponent<MeshRenderer>().material.SetColor("_Color", (Color)color);
		}
	}

	void DebugPoint(Vector3 point, Color? color = null, float? scale = null){
		color = color != null ? color : Color.red;
		scale = scale != null ? scale : 0.05f;
		GameObject go = GameObject.Instantiate(testPoint);
		go.SetActive(true);
		debugPoints.Add(go);
		go.transform.localScale = Vector3.one * (float)scale;
		go.transform.position = ObjectPosToWorldPos(point, meshObject);
		go.GetComponent<MeshRenderer>().material.SetColor("_Color", (Color)color);
	}
}
