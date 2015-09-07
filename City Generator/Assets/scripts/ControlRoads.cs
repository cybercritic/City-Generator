using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ControlRoads : MonoBehaviour {

	private RoadNetwork network {get;set;}
	private RoadRenderer roadRenderer {get;set;}

	public List<RoadSegment> RoadSegments {get; private set;}
	public List<Intersection> Intersections {get; private set;}

	public Slider GridSlider;
	public Text GridTypeText;

	public GameObject BuildingsGO;
	private Buildings buildings;

	private enum GridType {X_Type, Y_Type, O_Type};
	private GridType currentType = GridType.X_Type;

	// Use this for initialization
	void Start () 
	{
		this.buildings = this.BuildingsGO.GetComponent<Buildings> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void GridTypeClick()
	{
		this.currentType = (GridType)this.GridSlider.value;
		this.GridTypeText.text = this.currentType.ToString ().Replace('_','-');
	}

	public void GenerateClick()
	{
		//if (Buildings.Splitting)
		//	return;

		this.buildings.Clear ();

		this.network = new RoadNetwork (100f);
		if(this.currentType == GridType.X_Type)
			this.network.AddCityCentreX (new Vector2(0,0), 120f);
		else if(this.currentType == GridType.Y_Type)
			this.network.AddCityCentreY (new Vector2(0,0), 120f);
		else if(this.currentType == GridType.O_Type)
			this.network.AddCityCentreO (new Vector2(0,0), 120f);

		this.network.SplitSegments (0);
		this.network.SplitSegments (0);
		this.network.SplitSegments (1);
		this.network.SplitSegments (1);
		this.network.SplitSegments (2);
		this.network.SplitSegments (3);
		
		this.roadRenderer = this.GetComponent<RoadRenderer> ();
		this.roadRenderer.ClearData ();

		foreach (RoadSegment segment in this.network.RoadSegments)
            this.roadRenderer.AddRoadSegments(segment);

		foreach (Intersection inter in this.network.RoadIntersections)
			this.roadRenderer.AddIntersection (inter);

		this.RoadSegments = new List<RoadSegment> (this.network.RoadSegments);
	}
}
