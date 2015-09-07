
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoadNetwork 
{
	public List<RoadSegment> RoadSegments { get; private set; }
	public List<Intersection> RoadIntersections { get; private set; }

	public float scale { get; private set; }

	public const float ShortCutoff = 5.5f;
	public const float CloseCutoff = 7.5f;

	public RoadNetwork(float scale)
	{
		this.RoadSegments = new List<RoadSegment> ();
		this.RoadIntersections = new List<Intersection> ();

		this.scale = scale;
	}

	public void SplitSegments(int level = 0)
	{
		List<RoadSegment> segments = new List<RoadSegment> (this.RoadSegments);

		int c = 0;
		//foreach (RoadSegment segment in segments) {
		for(int i=0;i<segments.Count;i++){
			if(segments[i].Level == level)
			{	
				this.splitSegment (segments[i]);
			//	if(++c == 2)
			//		break;
			}
		}
	}

	private void splitSegment(RoadSegment segment)
	{
		//get split ratio
		float splitDistance = Random.Range (0.33f, 0.66f);

		//get split distance
		Vector3 p1 = new Vector3 (segment.PointA.point.x, 0, segment.PointA.point.y);
		Vector3 p2 = new Vector3 (segment.PointB.point.x, 0, segment.PointB.point.y);
		float length = Vector3.Distance (p1, p2);
		length *= splitDistance;

		//get direction vector for segment
		Vector3 direction = (p1 - p2).normalized;

		//get new point and patch the segment
		Vector3 newPoint = p2 + (direction * length);

		//calaculate other new point
		Vector3 per = Vector3.Cross (p1 - p2, Vector3.down).normalized;// * (Random.Range (0f, 1f) < 0.5f ? -1 : 1);
		float newLength = this.scale / ((segment.Level + 1) * Random.Range(1f,2f));
		Vector3 newPointEnd = newPoint + (per * newLength);

		//add new segment
		RoadSegment newSegment = new RoadSegment (new RoadPoint(new Vector2 (newPoint.x, newPoint.z),null), new RoadPoint(new Vector2 (newPointEnd.x, newPointEnd.z),null), segment.Level + 1);
  		
		//calaculate other new point
		Vector3 perA = Vector3.Cross (p1 - p2, Vector3.down).normalized * -1;
		Vector3 newPointEndOther = newPoint + (perA * newLength);
		RoadSegment newSegmentOther = new RoadSegment (new RoadPoint (new Vector2 (newPoint.x, newPoint.z), null), new RoadPoint (new Vector2 (newPointEndOther.x, newPointEndOther.z), null), segment.Level + 1);

		//check what segments to add and add them
		bool seg1 = false;
		bool seg2 = false;

		bool with1 = this.SegmentWithin (newSegment, CloseCutoff);
		bool with2 = this.SegmentWithin (newSegmentOther, CloseCutoff);

		if (!with1) 
		{
			Vector2 intersection = Vector3.zero;
			RoadSegment other = null;

			int iCount = segmentIntersection(newSegment,out intersection,out other,segment);

			if(iCount <= 1)
			{
				this.RoadSegments.RemoveAll(p => p.IsEqual(newSegment));
				this.RoadSegments.Add (newSegment);
				seg1 = true;
			}

			if(iCount == 1)
			{
				RoadSegment[] segmentsA = this.patchSegment (other, new RoadPoint (intersection, other));
				RoadSegment[] segmentsB = this.patchSegment (newSegment, new RoadPoint (intersection, newSegment));

				//kill very short dead-ends
				bool sa = segmentsA[0].SegmentLength() > ShortCutoff;
				bool sb = segmentsA[1].SegmentLength() > ShortCutoff;
				bool sc = segmentsB[0].SegmentLength() > ShortCutoff;
				bool sd = segmentsB[1].SegmentLength() > ShortCutoff;
				
				List<RoadPoint> points = new List<RoadPoint>();
				if(sa)
					points.Add(segmentsA [0].PointB);
				else
					this.RoadSegments.RemoveAll(p => p.IsEqual(segmentsA[0]));
				
				if(sb)
					points.Add(segmentsA [1].PointB);
				else
					this.RoadSegments.RemoveAll(p => p.IsEqual(segmentsA[1]));
				
				if(sc)
					points.Add(segmentsB [0].PointB);
				else
					this.RoadSegments.RemoveAll(p => p.IsEqual(segmentsB[0]));
				
				if(sd)
					points.Add(segmentsB [1].PointB);
				else
					this.RoadSegments.RemoveAll(p => p.IsEqual(segmentsB[1]));
				
				Intersection inter = new Intersection (points);
				this.RoadIntersections.Add (inter);
			}
		}

		//other side of intersection
		if (!with2) 
		{
			Vector2 intersection = Vector3.zero;
			RoadSegment other = null;

			int iCount = segmentIntersection(newSegmentOther,out intersection,out other,segment);

			if(iCount <= 1)
			{
				this.RoadSegments.RemoveAll(p => p.IsEqual(newSegmentOther));
				this.RoadSegments.Add (newSegmentOther);
				seg2 = true;
			}

			if(iCount == 1)
			{
				RoadSegment[] segmentsA = this.patchSegment (other, new RoadPoint (intersection, other));
				RoadSegment[] segmentsB = this.patchSegment (newSegmentOther, new RoadPoint (intersection, newSegmentOther));

				//kill very short dead-ends
				bool sa = segmentsA[0].SegmentLength() > ShortCutoff;
				bool sb = segmentsA[1].SegmentLength() > ShortCutoff;
				bool sc = segmentsB[0].SegmentLength() > ShortCutoff;
				bool sd = segmentsB[1].SegmentLength() > ShortCutoff;

				List<RoadPoint> points = new List<RoadPoint>();
				if(sa)
					points.Add(segmentsA [0].PointB);
				else
					this.RoadSegments.RemoveAll(p => p.IsEqual(segmentsA[0]));

				if(sb)
					points.Add(segmentsA [1].PointB);
				else
					this.RoadSegments.RemoveAll(p => p.IsEqual(segmentsA[1]));

				if(sc)
					points.Add(segmentsB [0].PointB);
				else
					this.RoadSegments.RemoveAll(p => p.IsEqual(segmentsB[0]));

				if(sd)
					points.Add(segmentsB [1].PointB);
				else
					this.RoadSegments.RemoveAll(p => p.IsEqual(segmentsB[1]));

				Intersection inter = new Intersection (points);
				this.RoadIntersections.Add (inter);
			}
		}

		if (seg1 || seg2) {
			RoadSegment[] segments = this.patchSegment (segment, new RoadPoint (new Vector2 (newPoint.x, newPoint.z), segment));

			if (seg1 && seg2) {
				Intersection inter = new Intersection (new List<RoadPoint>{segments [0].PointB,segments [1].PointB,newSegment.PointA,newSegmentOther.PointA});
				this.RoadIntersections.Add (inter);
			} else if (seg1) {
				Intersection inter = new Intersection (new List<RoadPoint>{segments [0].PointB,segments [1].PointB,newSegment.PointA});
				this.RoadIntersections.Add (inter);
			} else if (seg2) {
				Intersection inter = new Intersection (new List<RoadPoint>{segments [0].PointB,segments [1].PointB,newSegmentOther.PointA});
				this.RoadIntersections.Add (inter);
			}
		}
	}

	/// <summary>
	/// http://geomalgorithms.com/a02-_lines.html#Distance-to-Ray-or-Segment
	/// </summary>
	/// <returns>Distance between segments.</returns>
	/// <param name="P">P.</param>
	/// <param name="S">S.</param>
	private float distPointSegment( RoadPoint P, RoadSegment S)
	{
		Vector2 v = S.PointB.point - S.PointA.point;
		Vector2 w = P.point - S.PointA.point;
		
		float c1 = Vector2.Dot(w,v);
		if ( c1 <= 0 )
			return Vector2.Distance(P.point, S.PointA.point);
		
		float c2 = Vector2.Dot(v,v);
		if ( c2 <= c1 )
			return Vector2.Distance(P.point, S.PointB.point);
		
		float b = c1 / c2;
		Vector2 Pb = S.PointA.point + (v * b);
		return Vector2.Distance(P.point, Pb);
	}

	private int segmentIntersection(RoadSegment segment, out Vector2 intersection, out RoadSegment other, RoadSegment skip)
	{
		intersection = Vector2.zero;
		other = null;

		Vector2 tmp = Vector2.zero;
		Vector2 interTmp = Vector3.zero;

		int count = 0;
		// foreach (RoadSegment seg in this.RoadSegments)
		for (int i=0; i<this.RoadSegments.Count; i++) {
			RoadSegment seg = this.RoadSegments[i];
			if (seg.IsEqual(skip))
				continue;
			else if (Vector2.Distance (seg.PointA.point, segment.PointA.point) < 0.01f || Vector2.Distance (seg.PointB.point, segment.PointB.point) < 0.01f)
				continue;
			else if (Vector2.Distance (seg.PointA.point, segment.PointB.point) < 0.01f || Vector2.Distance (seg.PointB.point, segment.PointA.point) < 0.01f)
				continue;
			else if (inter2Segments (segment, seg, out interTmp, out tmp) != 0) {
				other = seg;
				intersection = new Vector2(interTmp.x,interTmp.y);
				count++;
			}
		}

		return count;
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
	int inter2Segments(RoadSegment S1, RoadSegment S2, out Vector2 I0, out Vector2 I1)
	{
		Vector2 u = S1.PointB.point - S1.PointA.point;
		Vector2 v = S2.PointB.point - S2.PointA.point;
		Vector2 w = S1.PointA.point - S2.PointA.point;
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
				if (S1.PointA.point !=  S2.PointA.point)         // they are distinct  points
					return 0;
				I0 = S1.PointA.point;                 // they are the same point
				return 1;
			}
			if (du==0) {                     // S1 is a single point
				if  (inSegment(S1.PointA, S2) == 0)  // but is not in S2
					return 0;
				I0 = S1.PointA.point;
				return 1;
			}
			if (dv==0) {                     // S2 a single point
				if  (inSegment(S2.PointA, S1) == 0)  // but is not in S1
					return 0;
				I0 = S2.PointA.point;
				return 1;
			}
			// they are collinear segments - get  overlap (or not)
			float t0, t1;                    // endpoints of S1 in eqn for S2
			Vector2 w2 = S1.PointB.point - S2.PointA.point;
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
				I0 = S2.PointA.point +  t0 * v;
				return 1;
			}
			
			// they overlap in a valid subsegment
			I0 = S2.PointA.point + t0 * v;
			I1 = S2.PointA.point + t1 * v;
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
		
		I0 = S1.PointA.point + sI * u;                // compute S1 intersect point
		return 1;
	}

	// inSegment(): determine if a point is inside a segment
	//    Input:  a point P, and a collinear segment S
	//    Return: 1 = P is inside S
	//            0 = P is  not inside S
	int inSegment( RoadPoint P, RoadSegment S)
	{
		if (S.PointA.point.x != S.PointB.point.x) {    // S is not  vertical
			if (S.PointA.point.x <= P.point.x && P.point.x <= S.PointB.point.x)
				return 1;
			if (S.PointA.point.x >= P.point.x && P.point.x >= S.PointB.point.x)
				return 1;
		}
		else {    // S is vertical, so test y  coordinate
			if (S.PointA.point.y <= P.point.y && P.point.y <= S.PointB.point.y)
				return 1;
			if (S.PointA.point.y >= P.point.y && P.point.y >= S.PointB.point.y)
				return 1;
		}
		return 0;
	}

	private bool SegmentWithin(RoadSegment segment, float max)
	{
		foreach (RoadSegment seg in this.RoadSegments) {
			bool amax = distPointSegment (seg.PointA, segment) < max;
			bool bmax = distPointSegment (seg.PointB, segment) < max;

			bool amin = MinPointDistance(seg,segment,max / 1.0f);

			if(amax || bmax || amin)
				return true;
		}

		return false;
	}

	private bool MinPointDistance(RoadSegment a, RoadSegment b, float min)
	{
		if (Vector2.Distance (a.PointA.point, b.PointA.point) < min)
			return true;
		if (Vector2.Distance (a.PointA.point, b.PointB.point) < min)
			return true;
		if (Vector2.Distance (a.PointB.point, b.PointA.point) < min)
			return true;
		if (Vector2.Distance (a.PointB.point, b.PointB.point) < min)
			return true;

		return false;
	}

	private bool PointWithin(RoadPoint point, float distance)
	{
		foreach (RoadSegment segment in this.RoadSegments)
			if (Vector2.Distance (point.point, segment.PointA.point) < distance)
				return true;
			else if (Vector2.Distance (point.point, segment.PointB.point) < distance)
				return true;

		return false;
	}

	private RoadSegment[] patchSegment(RoadSegment segment, RoadPoint newPoint)
	{
		this.RoadSegments.RemoveAll(p => p.IsEqual(segment));

		RoadSegment left = new RoadSegment (segment.PointA, new RoadPoint(newPoint.point), segment.Level);
		RoadSegment right = new RoadSegment (segment.PointB, new RoadPoint(newPoint.point), segment.Level);

		this.RoadSegments.Add (left);
		this.RoadSegments.Add (right);

		return new RoadSegment[] {left,right};
	}

    /// <summary>
    /// starting city blueprint O-type
    /// </summary>
    /// <param name="center"></param>
    /// <param name="angle"></param>
	public void AddCityCentreO(Vector2 center, float angle)
	{
		angle = 360f / 8f;
		RoadSegment last = null;
		RoadSegment first = null;
		for (int i=0; i<8; i++) 
		{
			Quaternion rotationA = Quaternion.Euler (0, 0, angle * i);
			Quaternion rotationB = Quaternion.Euler (0, 0, angle * (i + 1));

			RoadPoint a = new RoadPoint ();
			a.point = rotationA * (new Vector2 (this.scale / 2.5f, 0) + center);
			RoadPoint b = new RoadPoint ();
			b.point = rotationB * (new Vector2 (this.scale / 2.5f, 0) + center);

			RoadSegment rA = new RoadSegment (a, b, 0);
			this.RoadSegments.Add(rA);
			if(first == null)
				first = rA;

			if(last != null)
			{
				Intersection iA = new Intersection (new List<RoadPoint> (){rA.PointA,last.PointA});
				this.RoadIntersections.Add (iA);
			}
			last = rA;
		}

		Intersection iB = new Intersection (new List<RoadPoint> (){first.PointA,last.PointA});
		this.RoadIntersections.Add (iB);
	}

    /// <summary>
    /// starting city blueprint Y-type
    /// </summary>
    /// <param name="center"></param>
    /// <param name="angle"></param>
	public void AddCityCentreY(Vector2 center, float angle)
	{
		Quaternion rotation = Quaternion.Euler (0, 0, angle);

		RoadPoint a = new RoadPoint ();
		a.point = rotation * center;
		RoadPoint b = new RoadPoint ();
		b.point = rotation * (new Vector2 (this.scale, 0) + center);

		Quaternion localA = Quaternion.Euler (0, 0, 120f);
		RoadPoint c = new RoadPoint ();
		c.point = rotation * center;
		RoadPoint d = new RoadPoint ();
		d.point = localA * rotation * (new Vector2 (this.scale, 0) + center);

		Quaternion localB = Quaternion.Euler (0, 0, 240f);
		RoadPoint e = new RoadPoint ();
		e.point = rotation * center;
		RoadPoint f = new RoadPoint ();
		f.point = localB * rotation * (new Vector2 (this.scale, 0) + center);

		RoadSegment rA = new RoadSegment (a, b, 0);
		RoadSegment rB = new RoadSegment (c, d, 0);
		RoadSegment rC = new RoadSegment (e, f, 0);

		this.RoadSegments.AddRange (new RoadSegment[]{rA,rB,rC});
		
		Intersection iA = new Intersection (new List<RoadPoint> (){rA.PointA,rB.PointA,rC.PointA});
		this.RoadIntersections.Add (iA);
	}

	/// <summary>
	/// Adds the city centre x.
	/// </summary>
	/// <param name="center">Center.</param>
	/// <param name="angle">Angle.</param>
	public void AddCityCentreX(Vector2 center, float angle)
	{
		Quaternion rotation = Quaternion.Euler (0, 0, angle);

		RoadPoint a = new RoadPoint ();
		a.point = rotation * center;
		RoadPoint b = new RoadPoint ();
		b.point = rotation * (new Vector2 (this.scale, 0) + center);
		
		RoadPoint c = new RoadPoint ();
		c.point = rotation * center;
		RoadPoint d = new RoadPoint ();
		d.point = rotation * (new Vector2 (0, this.scale) + center);
		
		RoadPoint e = new RoadPoint ();
		e.point = rotation * center;
		RoadPoint f = new RoadPoint ();
		f.point = rotation * (new Vector2 (-this.scale, 0) + center);
		
		RoadPoint g = new RoadPoint ();
		g.point = rotation * center;
		RoadPoint h = new RoadPoint ();
		h.point = rotation * (new Vector2 (0, -this.scale) + center);
		
		RoadSegment rA = new RoadSegment (a, b, 0);
		RoadSegment rB = new RoadSegment (c, d, 0);
		RoadSegment rC = new RoadSegment (e, f, 0);
		RoadSegment rD = new RoadSegment (g, h, 0);

		this.RoadSegments.AddRange (new RoadSegment[]{rA,rB,rC,rD});

		Intersection iA = new Intersection (new List<RoadPoint> (){rA.PointA,rB.PointA,rC.PointA,rD.PointA});
		this.RoadIntersections.Add (iA);
	}
}

public class RoadSegment
{
	public RoadPoint PointA { get; private set; }
	public RoadPoint PointB { get; private set; }

	public int Level { get; private set; }

	public RoadSegment(RoadPoint a, RoadPoint b, int level)
	{
		this.PointA = new RoadPoint (a.point, this);
		this.PointB = new RoadPoint (b.point, this);

		this.Level = level;
	}

    /// <summary>
    /// get other segment point given one of them
    /// </summary>
    /// <param name="main"></param>
    /// <returns></returns>
	public RoadPoint GetOther(RoadPoint main)
	{
		return this.PointA.Equals(main) ? this.PointB : this.PointA;
	}

    public float SegmentLength()
	{
		return Vector2.Distance (this.PointA.point, this.PointB.point);
	}

    /// <summary>
    /// convert Vector2 roadpoint position to Vector3
    /// </summary>
    /// <param name="first">first or second point return?</param>
    /// <returns></returns>
    public Vector3 GetVector3(bool first)
	{
		if (first)
			return new Vector3 (this.PointA.point.x, 0, this.PointA.point.y);
		else
			return new Vector3 (this.PointB.point.x, 0, this.PointB.point.y);
	}

	/// <summary>
	/// Determines whether this instance is equal the specified segment.
	/// </summary>
	/// <returns><c>true</c> if this instance is equal the specified segment; otherwise, <c>false</c>.</returns>
	/// <param name="segment">Segment.</param>
	public bool IsEqual(RoadSegment segment)
	{
		if (this.PointA.Equals(segment.PointA) && this.PointB.Equals(segment.PointB))
			return true;
		else if (this.PointA.Equals(segment.PointB) && this.PointB.Equals(segment.PointA))
			return true;

		return false;
	}
}

public class RoadPoint
{
	public Vector2 point { get; set; }
	public RoadSegment mySegement { get; set; }

	public RoadPoint(){	}

	public RoadPoint(Vector2 point, RoadSegment segment = null)
	{
		this.point = new Vector2(point.x, point.y);
		this.mySegement = segment;
	}

	public override bool Equals(object other) 
	{
        //just check distance, ignore segment
		if (Vector2.Distance ((other as RoadPoint).point, this.point) < 0.01f)
			return true;

		return false;
	}
}
