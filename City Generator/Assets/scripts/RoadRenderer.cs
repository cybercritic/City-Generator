using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoadRenderer : MonoBehaviour 
{
	private List<RoadSegment> roadSegments {get;set;}
	private float roadWidth { get; set; }
	private float intersectionOffset { get; set; }

	public Material RoadMaterial;
	public Material IntersectionMaterial;

	private MeshRenderer meshRenderer {get;set;}
	private MeshFilter meshFilter { get; set; }

	public float Scale;
	public float Tiling;

	public GameObject Intersections;
	public List<Intersection> IntersectionList { get; set; }

	public static float RoadWidth {get;private set;}

	public RoadRenderer()
	{
		this.roadSegments = new List<RoadSegment> ();
		this.IntersectionList = new List<Intersection> ();
	}

	public void ClearData()
	{
		this.roadSegments = new List<RoadSegment> ();
		this.IntersectionList = new List<Intersection> ();

		//setup mesh filters
		this.meshFilter = this.GetComponent<MeshFilter> ();
		this.meshFilter.mesh = new Mesh ();
		
		this.Intersections.GetComponent<MeshFilter> ().mesh = new Mesh ();
		this.Intersections.GetComponent<MeshRenderer> ().material = new Material (this.IntersectionMaterial);
	}

	void Start()
	{
		//set road width and intersection
		this.roadWidth = 1.0f * this.Scale;
		this.intersectionOffset = 0.5f * this.Scale;

		RoadRenderer.RoadWidth = this.roadWidth;

		//setup mesh renderer
		this.meshRenderer = this.GetComponent<MeshRenderer> ();
		this.meshRenderer.sharedMaterial = new Material (this.RoadMaterial);
	}

	/// <summary>
	/// Adds the road segments.
	/// </summary>
	/// <param name="segment">Segment.</param>
	public void AddRoadSegments(RoadSegment segment)
	{
		this.addSegmentQuad (segment);
		this.roadSegments.Add (segment);
	}

	public void AddIntersection(Intersection inter)
	{
		List<Vector3[]> interPoints = new List<Vector3[]> ();
		foreach (RoadPoint point in inter.Points) {
			interPoints.Add(this.getVerticeOffset(point, point.mySegement.GetOther(point)));
		}

		//intersection already exists
		if (this.IntersectionList.Exists (p => p.IsThisOne (inter)))
			return;

		//add this intersection to existing
		this.IntersectionList.Add (inter);

		Mesh mesh = this.Intersections.GetComponent<MeshFilter>().sharedMesh;

		//get mesh details
        List<int> triangles = mesh.vertexCount == 0 ? new List<int>() : new List<int> (mesh.triangles);
		List<Vector3> vertices = new List<Vector3> (mesh.vertices);
		List<Vector3> normals = new List<Vector3> (mesh.normals);
		List<Vector2> uvs = new List<Vector2> (mesh.uv);

		//get last triangle
		int last = vertices.Count - 1;

		List<Vector3> interVecs = new List<Vector3> ();
		foreach(Vector3[] points in interPoints)
			interVecs.AddRange(points);

		Vector3 center = new Vector3 (interVecs.Average (p => p.x), 0, interVecs.Average (p => p.z));

		IComparer<Vector3> comparer = new CircleSort (center);

		interVecs.Sort(comparer);

		interVecs.Reverse ();

		//interVecs.OrderBy (p => Mathf.Atan2 (p.z - center.z, p.x - center.x) * Mathf.Rad2Deg).ThenBy (p => Vector3.Distance (p, center));

		interVecs.Add (interVecs [0]);


		for(int i=0;i<interVecs.Count - 1;i++)
		{
			//create vertices
			Vector3 vertA = interVecs[i];
			Vector3 vertB = interVecs[i+1];
			Vector3 vertC = center;

			//add vertices
			vertices.AddRange (new Vector3[]{vertA,vertB,vertC});
			
			//add triangles
			triangles.AddRange (new int[]{ ++last, ++last, ++last});

			//add normals
			normals.AddRange (new Vector3[]{ Vector3.up, Vector3.up, Vector3.up});
			
			//add uvs
			uvs.AddRange (new Vector2[]{ new Vector2 (0, 1), new Vector2 (1, 1), new Vector2 (0.5f, 0.5f)});
		}

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		//mesh.normals = normals.ToArray();
		mesh.RecalculateNormals ();
	}

	private Vector3[] getVerticeOffset(RoadPoint main, RoadPoint other)
	{
		Vector3[] result = new Vector3[2];

		//get the road start and end
		Vector3 pointA = new Vector3 (main.point.x, 0, main.point.y);
		Vector3 pointB = new Vector3 (other.point.x, 0, other.point.y);
		
		//adjust for intersection
		Vector3 segvec = (pointA - pointB).normalized;
		pointA -= segvec * this.intersectionOffset;
		pointB += segvec * this.intersectionOffset;
		
		//get perpendicular vector
		Vector3 per = Vector3.Cross (pointA - pointB, Vector3.down).normalized;
		
		//create result
		result[0] = pointA + per * (0.5f * this.roadWidth);
		result[1] = pointA - per * (0.5f * this.roadWidth);

		return result;
	}

	private void addSegmentQuad(RoadSegment segment)
	{
        Mesh mesh = this.meshFilter.mesh;

        //get mesh details
        List<int> triangles = mesh.vertexCount == 0 ? new List<int>() : new List<int> (mesh.triangles);
		List<Vector3> vertices = new List<Vector3> (mesh.vertices);
		List<Vector3> normals = new List<Vector3> (mesh.normals);
		List<Vector2> uvs = new List<Vector2> (mesh.uv);

		//get last triangle
		int last = vertices.Count;

		//get the road start and end
		Vector3 pointA = new Vector3 (segment.PointA.point.x, 0, segment.PointA.point.y);
		Vector3 pointB = new Vector3 (segment.PointB.point.x, 0, segment.PointB.point.y);

		//adjust for intersection
		Vector3 segvec = (pointA - pointB).normalized;
		pointA -= segvec * this.intersectionOffset;
		pointB += segvec * this.intersectionOffset;

		//get perpendicular vector
		Vector3 per = Vector3.Cross (pointA - pointB, Vector3.down).normalized;
	
		//create vertices
		Vector3 vertTL = pointA + per * (0.5f * this.roadWidth);
		Vector3 vertTR = pointA - per * (0.5f * this.roadWidth);
		Vector3 vertBL = pointB + per * (0.5f * this.roadWidth);
		Vector3 vertBR = pointB - per * (0.5f * this.roadWidth);

		//add vertices
		vertices.AddRange (new Vector3[]{vertTL,vertTR,vertBL,vertBR});

		//add triangles
		triangles.AddRange (new int[]{ last, last + 2, last + 1});
		triangles.AddRange (new int[]{ last + 1, last + 2, last + 3});

		//add normals
		normals.AddRange (new Vector3[]{ Vector3.up, Vector3.up, Vector3.up, Vector3.up});

		//add uvs
		float length = Vector3.Distance (pointA, pointB) * this.Tiling;
		uvs.AddRange (new Vector2[]{ new Vector2 (0, length), new Vector2 (1, length), new Vector2 (0, 0), new Vector2 (1, 0)});

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		//mesh.normals = vertices.ToArray();
		mesh.RecalculateNormals ();
	}
}

public class CircleSort : IComparer<Vector3>
{
	public Vector3 center { get; set; }

	public CircleSort(Vector3 center)
	{
		this.center = center;
	}

	public int Compare(Vector3 a, Vector3 b)
	{
		float a1 = Mathf.Atan2 (a.x - center.x, a.z - center.z) * Mathf.Rad2Deg;
		float a2 = Mathf.Atan2 (b.x - center.x, b.z - center.z) * Mathf.Rad2Deg;

		a1 += a1 < 0 ? 360 : 0;
		a2 += a2 < 0 ? 360 : 0;

		return a1 > a2 ? -1 : 1;
	}
}

public struct Intersection
{
	public List<RoadPoint> Points;

	public Intersection(List<RoadPoint> points)
	{
		this.Points = points;
	}

	public bool IsThisOne(Intersection inter)
	{
		int c = 0;
		foreach (RoadPoint p in inter.Points)
			if (this.Points.Exists (f => f == p))
				c++;

		if (c == this.Points.Count && c == inter.Points.Count)
			return true;

		return false;
	}
}
