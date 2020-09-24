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
		// Debug.Log("upVertices " + upVertices.Count);
		// Debug.Log("downVertics " + downVertics.Count);
		// Debug.Log("onVertices " + onVertices.Count);
		DebugClearPoint();
		DebugPoint(upVertices, Color.blue, 0.019f);
		DebugPoint(downVertics, Color.green, 0.019f);
		DebugPoint(onVertices, Color.yellow, 0.019f);
		List<int[]> tirs = GetLinePanelTriangles();
		CaculateLinePanelPoint(tirs);
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

	// 2. 计算与面相交的三角形
	// 三角形的三个点不在面的同一侧
	List<int[]> GetLinePanelTriangles(){
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
			if ((upCount == 1 && downCount == 2) || (upCount == 2 && downCount == 1)){
				tris.Add(new int[]{
					triangles[i + 0],
					triangles[i + 1],
					triangles[i + 2],
				});
			}
		}


		// debug
		for (int i = 0; i < tris.Count; i++)
		{
			for (int j = 0; j < tris[i].Length; j++)
			{
				Debug.Log(tris[i][j]);
				DebugPoint(mesh.vertices[tris[i][j]], Color.red, 0.02f);
			}
		}
		return tris;
	}

	void CaculateLinePanelPoint(List<int[]> triangles){
		Vector3 panelNormalVec = panel.transform.up;
		Vector3 panelCenterPos = panel.transform.position;
		List<Vector3> lisePanelPoints = new List<Vector3>();
		for (int i = 0; i < triangles.Count; i++)
		{
			List<Vector3> upPos = new List<Vector3>();
			List<Vector3> downPos = new List<Vector3>();
			for (int j = 0; j < triangles[i].Length; j++)
			{
				Vector3 vert = mesh.vertices[triangles[i][j]];
				Vector3 worldPos = ObjectPosToWorldPos(vert, meshObject);
				float dis = CaculatePos(panelNormalVec, panelCenterPos, worldPos);
				if (dis > 0){
					upPos.Add(worldPos);
				}
				else if (dis < 0){
					downPos.Add(worldPos);
				}
			}
			if (downPos.Count == 1 && upPos.Count == 2){
				lisePanelPoints.Add(GetLinePanelPoint(downPos[0], upPos[0], panelCenterPos, panelNormalVec));
				lisePanelPoints.Add(GetLinePanelPoint(downPos[0], upPos[1], panelCenterPos, panelNormalVec));
			}
			else if (downPos.Count == 2 && upPos.Count == 1){
				lisePanelPoints.Add(GetLinePanelPoint(downPos[0], upPos[0], panelCenterPos, panelNormalVec));
				lisePanelPoints.Add(GetLinePanelPoint(downPos[1], upPos[0], panelCenterPos, panelNormalVec));
			}
		}
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
		// Vector3 p = new Vector3(line_p1.x + lineDir.x * m, line_p1.y + lineDir.y * m, line_p1.z + lineDir.z * m);
		Vector3 p = line_p1 + lineDir * m;
		DebugPoint(p, Color.yellow, 0.02f, false);
		return p;
	}

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

	void DebugPoint(Vector3 point, Color? color = null, float? scale = null, bool translateToWorld = true){
		color = color != null ? color : Color.red;
		scale = scale != null ? scale : 0.05f;
		GameObject go = GameObject.Instantiate(testPoint);
		go.SetActive(true);
		debugPoints.Add(go);
		go.transform.localScale = Vector3.one * (float)scale;
		if (translateToWorld){
			go.transform.position = ObjectPosToWorldPos(point, meshObject);
		}
		else{
			go.transform.position = point;
		}
		go.GetComponent<MeshRenderer>().material.SetColor("_Color", (Color)color);
	}
}
