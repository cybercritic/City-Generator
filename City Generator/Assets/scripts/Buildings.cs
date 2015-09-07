using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct LineSegment
{
    public Vector2 PointA { get; set; }
    public Vector2 PointB { get; set; }

    public LineSegment(Vector2 pointA, Vector2 pointB)
    {
        this.PointA = pointA;
        this.PointB = pointB;
    }
}

public class Buildings : MonoBehaviour {

	public GameObject BuildingStatic;
	public GameObject Roads;
	public GameObject Instances;

	private ControlRoads control { get; set; }

	public List<Building> BuildingsList {get;private set;}

	private List<RoadSegment> splitList {get;set;}
	public static bool Splitting = false;

	void Start () 
	{
		this.control = Roads.GetComponent<ControlRoads> ();
		this.BuildingsList = new List<Building> ();
	}

    /// <summary>
    /// Flips the building generation on
    /// </summary>
	public void PopulateClick()
	{
		//gonna update incrementaly so there is not a long delay to rendering
		if (Buildings.Splitting || this.control.RoadSegments == null)
			return;
		Buildings.Splitting = true;

		this.splitList = new List<RoadSegment> (this.control.RoadSegments);
	}

    /// <summary>
    /// clears all buildings and their objects
    /// </summary>
	public void Clear()
	{
		if (Buildings.Splitting)
			Buildings.Splitting = false;

		this.splitList = new List<RoadSegment> ();
		this.BuildingsList = new List<Building> ();

		foreach(Transform child in this.Instances.transform)
			Destroy(child.gameObject);
	}

	void Update()
	{
        if (Buildings.Splitting) {
			this.checkSegment (this.splitList [0]);
			this.splitList.RemoveAt (0);

			if(this.splitList.Count == 0)
				Buildings.Splitting = false;
		}
	}

    /// <summary>
    /// evaluate segment for potential building placement
    /// </summary>
    /// <param name="segment"></param>
	private void checkSegment(RoadSegment segment)
	{
		Vector2 start = segment.PointA.point;
		Vector2 end = segment.PointB.point;
		Vector2 dir = (end - start).normalized;
		float distance = Vector2.Distance (start, end);

		Vector2 current = start;
		bool side = true;
		for(float f=RoadRenderer.RoadWidth;f<distance || side;f+=4.5f)
		{
			//switch side of the road
			if(f > distance && side)
			{
				side = false;
				f=RoadRenderer.RoadWidth;
			}

			Vector2 per = new Vector2(-dir.y, dir.x);
			if(side)
				per *=-1;

			//try to put some building into the spot
			for(int i=0;i<10;i++)
			{
				//get road level adjustment
				float level = 2.0f - (segment.Level / 3f);//0,0.33,0.66,1

				//get building dimensions
				float width = Random.Range(1.75f,2f) * level;
				float length = Random.Range(1.75f,2f) * level;
				float height = Random.Range(2.5f,10f) * level;

				//get building center
				Vector2 roadOffset = per.normalized * (RoadRenderer.RoadWidth * 1.25f + length);
				Vector2 tc = start + (dir * f) + roadOffset;

				if(f - width < 0 || f + width > distance)
					continue;

				Vector3 center = new Vector3(tc.x,0,tc.y);

				//get building size
				Vector3 size = new Vector3(length,width,height);

				//set building
				GameObject buildingObj = GameObject.Instantiate(this.BuildingStatic);
				buildingObj.transform.parent = this.Instances.transform;
				buildingObj.transform.name = "building_" + this.BuildingsList.Count.ToString("D5");

				Building building = new Building(center,size,this.GetRotation(dir) - (side ? 180 : 0));
				building.AddMyGameObject(buildingObj);
				this.AddBuildingMesh(building);
				building.AddCollider();

				if(this.CheckValidPlacement(building))
				{
					this.BuildingsList.Add(building);
					break;
				}
				else
					GameObject.DestroyImmediate(buildingObj);
			}
		}
	}

    /// <summary>
    /// Adds a building mesh
    /// </summary>
    /// <param name="building"></param>
	private void AddBuildingMesh(Building building)
	{
		building.MyGameObject.GetComponent<MeshFilter> ().mesh = new Mesh ();

		Mesh mesh = building.MyGameObject.GetComponent<MeshFilter>().mesh;
		//Quaternion rot = Quaternion.Euler(0, rotation, 0);

		//get mesh details
        List<int> triangles = mesh.vertexCount == 0 ? new List<int>() : new List<int> (mesh.GetTriangles (0));
		List<Vector3> vertices = new List<Vector3> (mesh.vertices);
		List<Vector3> normals = new List<Vector3> (mesh.normals);
		List<Vector2> uvs = new List<Vector2> (mesh.uv);
		
		//get last triangle
		int last = vertices.Count;

		//front quad
		Vector3 TL = new Vector3 (+building.Size.x, + building.Size.z, -building.Size.y);
		Vector3 TR = new Vector3 (-building.Size.x, + building.Size.z, -building.Size.y);
		Vector3 BL = new Vector3 (+building.Size.x, 0f, -building.Size.y);
		Vector3 BR = new Vector3 (-building.Size.x, 0f, -building.Size.y);

		int[] tris = null;
		Vector3[] norms = null;
		Vector2[] uv = null;
		this.GetQuad(new Vector3[]{TL,TR,BL,BR},ref last,out tris,out norms, out uv);

		triangles.AddRange (tris);
		normals.AddRange (norms);
		vertices.AddRange (new Vector3[]{TL,TR,BL,BR});
		uvs.AddRange (uv);

		//right quad
		TL = new Vector3 (+building.Size.x, +building.Size.z, +building.Size.y);
		TR = new Vector3 (+building.Size.x, +building.Size.z, -building.Size.y);
		BL = new Vector3 (+building.Size.x, 0f, +building.Size.y);
		BR = new Vector3 (+building.Size.x, 0f, -building.Size.y);

		tris = null;
		norms = null;
		uv = null;
		this.GetQuad(new Vector3[]{TL,TR,BL,BR},ref last,out tris,out norms, out uv);
		
		triangles.AddRange (tris);
		normals.AddRange (norms);
		vertices.AddRange (new Vector3[]{TL,TR,BL,BR});
		uvs.AddRange (uv);

		//left quad
		TL = new Vector3 (-building.Size.x, +building.Size.z, -building.Size.y);
		TR = new Vector3 (-building.Size.x, +building.Size.z, +building.Size.y);
		BL = new Vector3 (-building.Size.x, 0f, -building.Size.y);
		BR = new Vector3 (-building.Size.x, 0f, +building.Size.y);

		tris = null;
		norms = null;
		uv = null;
		this.GetQuad(new Vector3[]{TL,TR,BL,BR},ref last,out tris,out norms, out uv);
		
		triangles.AddRange (tris);
		normals.AddRange (norms);
		vertices.AddRange (new Vector3[]{TL,TR,BL,BR});
		uvs.AddRange (uv);

		//back quad
		TL = new Vector3 (-building.Size.x, +building.Size.z, +building.Size.y);
		TR = new Vector3 (+building.Size.x, +building.Size.z, +building.Size.y);
		BL = new Vector3 (-building.Size.x, 0f, +building.Size.y);
		BR = new Vector3 (+building.Size.x, 0f, +building.Size.y);

		tris = null;
		norms = null;
		uv = null;
		this.GetQuad(new Vector3[]{TL,TR,BL,BR},ref last,out tris,out norms, out uv);
		
		triangles.AddRange (tris);
		normals.AddRange (norms);
		vertices.AddRange (new Vector3[]{TL,TR,BL,BR});
		uvs.AddRange (uv);

		//top quad
		TL = new Vector3 (+building.Size.x, +building.Size.z, +building.Size.y);
		TR = new Vector3 (-building.Size.x, +building.Size.z, +building.Size.y);
		BL = new Vector3 (+building.Size.x, +building.Size.z, -building.Size.y);
		BR = new Vector3 (-building.Size.x, +building.Size.z, -building.Size.y);

		tris = null;
		norms = null;
		uv = null;
		this.GetQuad(new Vector3[]{TL,TR,BL,BR},ref last,out tris,out norms, out uv);
		
		triangles.AddRange (tris);
		normals.AddRange (norms);
		vertices.AddRange (new Vector3[]{TL,TR,BL,BR});
		uvs.AddRange (uv);

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.RecalculateNormals ();
		//mesh.normals = normals.ToArray();
	}

	private Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle){
		//return angle * ( point - pivot) + pivot;
		return point;
	}

    /// <summary>
    /// check if a given building intersecs a road
    /// </summary>
    /// <param name="building"></param>
    /// <returns></returns>
	private bool IntersectsRoad(Building building)
	{
        //can be made more efficient, currently evaluates all roads, check distance if needed
		foreach (RoadSegment segment in this.control.RoadSegments) {
			Ray ray = new Ray(segment.GetVector3(true),segment.GetVector3(false) - segment.GetVector3(true));
			RaycastHit hit = new RaycastHit();
			float distance = Vector2.Distance(segment.PointA.point,segment.PointB.point);
			building.MyCollider.Raycast(ray,out hit,distance);

			if(building.MyCollider.Raycast(ray,out hit,distance))
				return true;
		}

		return false;
	}

    /// <summary>
    /// get 360 rotation of vector
    /// </summary>
    /// <param name="segDir"></param>
    /// <returns></returns>
	private float GetRotation(Vector3 segDir)
	{
		float a1 = Mathf.Atan2 (segDir.x, segDir.y) * Mathf.Rad2Deg;
		a1 += a1 < 0 ? 360 : 0;

		return a1;
	}

    /// <summary>
    /// construct a building quad given 4 points and last indice
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="last"></param>
    /// <param name="triangles"></param>
    /// <param name="normals"></param>
    /// <param name="uvs"></param>
	private void GetQuad(Vector3[] vertices,ref int last, out int[] triangles,out Vector3[] normals, out Vector2[] uvs)
	{
		//create vertices
		Vector3 vertTL = vertices [0];
		Vector3 vertTR = vertices [1];
		Vector3 vertBL = vertices [2];
		Vector3 vertBR = vertices [3];

		//set triangles
		triangles = new int[]{ last, last + 2, last + 1, last + 1, last + 2, last + 3};
		last += 4;

		//get normals... gonna auto calculate
		Vector3 nTL = Vector3.Cross ((vertTR - vertTL), (vertBL - vertTL));

		//set normals
		normals = new Vector3[]{};

		//set uvs
		uvs = new Vector2[]{ new Vector2 (0, 1), new Vector2 (1, 1), new Vector2 (0, 0), new Vector2 (1, 0)};
	}
	
    /// <summary>
    /// check if new building placement is ok
    /// </summary>
    /// <param name="building"></param>
    /// <returns></returns>
	private bool CheckValidPlacement(Building building)
	{
        //check this building with all other withing given distance
		foreach (Building other in this.BuildingsList)
			if (Vector3.Distance (building.Center, other.Center) > 25f)
				continue;
			else if (building.Intersects (other))
				return false;

		if (this.IntersectsRoad (building))
			return false;

		return true;
	}
}

public class Building
{
	public Vector3 Center { get; private set; }
	public Vector3 Size { get; private set; }
	public float Rotation { get; private set; }

	public BoxCollider MyCollider { get; private set; } 

	public GameObject MyGameObject { get; set; }

	public Building(Vector3 center, Vector3 size, float rotation)
	{
		this.Center = new Vector3 (center.x, center.y, center.z);
		this.Size = new Vector3 (size.x, size.y, size.z);

		this.Rotation = rotation > 360 ? rotation - 360 : rotation;
		this.Rotation = rotation < 0 ? rotation + 360 : rotation;
	}

	public void AddMyGameObject(GameObject gameObject)
	{
		this.MyGameObject = gameObject;

		this.MyGameObject.transform.localPosition = this.Center;
		this.MyGameObject.transform.localRotation = Quaternion.Euler (0, this.Rotation, 0);
	}

	public void AddCollider()
	{
		this.MyCollider = this.MyGameObject.GetComponent<BoxCollider> ();
		this.MyCollider.size = new Vector3 (this.Size.x * 2 + RoadRenderer.RoadWidth, this.Size.z + 0.75f, this.Size.y * 2 + RoadRenderer.RoadWidth);
		this.MyCollider.center = new Vector3 (0, this.Size.z / 2.0f, 0);
	}

	public bool Intersects(Building building)
	{
        Vector3[] MyPoints = this.getWorldBase ();
        Vector3[] OtherPoints = building.getWorldBase ();

        //new code, does a quicker line intersect check
        for (int me=0; me<4; me++)
        {
            LineSegment myLine = new LineSegment(new Vector2(MyPoints[me%4].x,MyPoints[me%4].z),
                                                 new Vector2(MyPoints[(me+1)%4].x,MyPoints[(me+1)%4].z));

            for (int other=0; other<4; other++)
            {
                LineSegment otherLine = new LineSegment(new Vector2(OtherPoints[other%4].x,OtherPoints[other%4].z),
                                                        new Vector2(OtherPoints[(other+1)%4].x,OtherPoints[(other+1)%4].z));
                Vector2 I0,I1;
                if(this.inter2Segments(myLine,otherLine,out I0,out I1) != 0)
                    return true;
            }
        }

        return false;

        //old code, checks bounding boxes
		/*for(int i=0;i<4;i++){
			Vector3 dir = points[(i+1)%4] - points[i%4];
			Ray ray = new Ray(points[i%4]-(dir*RoadRenderer.RoadWidth),dir);
			RaycastHit hit = new RaycastHit();
			float distance = Vector3.Distance(points[i%4],points[(i+1)%4]);

			if(building.MyCollider.Raycast(ray,out hit,distance * 1.35f))
				return true;
		}

		for(int i=0;i<4;i+=2){
			Vector3 dir = points[(i+2)%4] - points[i%4];
			Ray ray = new Ray(points[i%4]-(dir*RoadRenderer.RoadWidth),dir);
			RaycastHit hit = new RaycastHit();
			float distance = Vector3.Distance(points[i%4],points[(i+2)%4]);
			
			if(building.MyCollider.Raycast(ray,out hit,distance * 1.35f))
				return true;
		}*/

		//Physics.OverlapSphere(building.
		//if (this.MyCollider. .bounds.Intersects (building.MyCollider.bounds))
		//	return true;

		//return false;
	}

	public Vector3[] getWorldBase()
	{
		Vector3[] result = new Vector3[4];
		Quaternion rot = Quaternion.Euler (0,this.Rotation, 0);
		
		result [0] = (rot * new Vector3 (-Size.x, 0, -Size.y)) + this.Center;
		result [1] = (rot * new Vector3 (+Size.x, 0, -Size.y)) + this.Center;
		result [2] = (rot * new Vector3 (-Size.x, 0, +Size.y)) + this.Center;
		result [3] = (rot * new Vector3 (+Size.x, 0, +Size.y)) + this.Center;

		result [0].y = 0;
		result [1].y = 0;
		result [2].y = 0;
		result [3].y = 0;

		return result;
	}


    private float perp (Vector2 u, Vector2 v)
    {
        return ((u).x * (v).y - (u).y * (v).x);
    }

    //http://geomalgorithms.com/a05-_intersect-1.html
    // intersect2D_2Segments(): find the 2D intersection of 2 finite segments
    //    Input:  two finite segments S1 and S2
    //    Output: *I0 = intersect point (when it exists)
    //            *I1 =  endpoint of intersect segment [I0,I1] (when it exists)
    //    Return: 0=disjoint (no intersect)
    //            1=intersect  in unique point I0
    //            2=overlap  in segment from I0 to I1
    int inter2Segments(LineSegment S1, LineSegment S2, out Vector2 I0, out Vector2 I1)
    {
        Vector2 u = S1.PointB - S1.PointA;
        Vector2 v = S2.PointB - S2.PointA;
        Vector2 w = S1.PointA - S2.PointA;
        float D = perp(u,v);
        
        I0 = Vector2.zero;
        I1 = Vector2.zero;
        
        // test if  they are parallel (includes either being a point)
        if (Mathf.Abs(D) < 0.01f) {           // S1 and S2 are parallel
            if (perp(u,w) != 0 || perp(v,w) != 0)  {
                return 0;                    // they are NOT collinear
            }
            // they are collinear or degenerate
            // check if they are degenerate  points
            float du = Vector2.Dot(u,u);
            float dv = Vector2.Dot(v,v);
            if (du==0 && dv==0) {            // both segments are points
                if (S1.PointA !=  S2.PointA)         // they are distinct  points
                    return 0;
                I0 = S1.PointA;                 // they are the same point
                return 1;
            }
            if (du==0) {                     // S1 is a single point
                if  (inSegment(S1.PointA, S2) == 0)  // but is not in S2
                    return 0;
                I0 = S1.PointA;
                return 1;
            }
            if (dv==0) {                     // S2 a single point
                if  (inSegment(S2.PointA, S1) == 0)  // but is not in S1
                    return 0;
                I0 = S2.PointA;
                return 1;
            }
            // they are collinear segments - get  overlap (or not)
            float t0, t1;                    // endpoints of S1 in eqn for S2
            Vector2 w2 = S1.PointB - S2.PointA;
            if (v.x != 0) {
                t0 = w.x / v.x;
                t1 = w2.x / v.x;
            }
            else {
                t0 = w.y / v.y;
                t1 = w2.y / v.y;
            }
            if (t0 > t1) {                   // must have t0 smaller than t1
                float t=t0; t0=t1; t1=t;    // swap if not
            }
            if (t0 > 1 || t1 < 0) {
                return 0;      // NO overlap
            }
            t0 = t0<0? 0 : t0;               // clip to min 0
            t1 = t1>1? 1 : t1;               // clip to max 1
            if (t0 == t1) {                  // intersect is a point
                I0 = S2.PointA +  t0 * v;
                return 1;
            }
            
            // they overlap in a valid subsegment
            I0 = S2.PointA + t0 * v;
            I1 = S2.PointA + t1 * v;
            return 2;
        }
        
        // the segments are skew and may intersect in a point
        // get the intersect parameter for S1
        float     sI = perp(v,w) / D;
        if (sI < 0 || sI > 1)                // no intersect with S1
            return 0;
        
        // get the intersect parameter for S2
        float     tI = perp(u,w) / D;
        if (tI < 0 || tI > 1)                // no intersect with S2
            return 0;
        
        I0 = S1.PointA + sI * u;                // compute S1 intersect point
        return 1;
    }

    // inSegment(): determine if a point is inside a segment
    //    Input:  a point P, and a collinear segment S
    //    Return: 1 = P is inside S
    //            0 = P is  not inside S
    int inSegment( Vector2 P, LineSegment S)
    {
        if (S.PointA.x != S.PointB.x) {    // S is not  vertical
            if (S.PointA.x <= P.x && P.x <= S.PointB.x)
                return 1;
            if (S.PointA.x >= P.x && P.x >= S.PointB.x)
                return 1;
        }
        else {    // S is vertical, so test y  coordinate
            if (S.PointA.y <= P.y && P.y <= S.PointB.y)
                return 1;
            if (S.PointA.y >= P.y && P.y >= S.PointB.y)
                return 1;
        }
        return 0;
    }
}
