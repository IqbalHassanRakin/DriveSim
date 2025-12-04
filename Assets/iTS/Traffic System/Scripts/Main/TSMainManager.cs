using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// TS main manager.  This class is responsible for holding all the info for the roads
/// </summary>
[RequireComponent(typeof(TSTrafficLightCheck))] 
public class TSMainManager : MonoBehaviour {


	/// <summary>
	/// The lanes.  Variable that contains all the lanes informaction.
	/// </summary>
	[SerializeField]
	[XmlArray("Lanes")]
	[XmlArrayItem("Lane")]
	public TSLaneInfo[] lanes;

	/// <summary>
	/// The menu selection.
	/// </summary>
	public int menuSelection = 0;

	/// <summary>
	/// The lane menu selection.
	/// </summary>
	public int laneMenuSelection = 0;

	/// <summary>
	/// The connections menu selection.
	/// </summary>
	public int connectionsMenuSelection = 0;

	/// <summary>
	/// The settings menu selection.
	/// </summary>
	public int settingsMenuSelection = 0;

	/// <summary>
	/// The width of the visual lines that represents the lanes and connectors on the scene view.
	/// </summary>
	public float visualLinesWidth = 5f;

	/// <summary>
	/// The resolution of the lanes.
	/// </summary>
	public float resolution = 4;

	/// <summary>
	/// The resolution of the connectors.
	/// </summary>
	public float resolutionConnectors = 2.3f;

	/// <summary>
	/// The lane curve speed multiplier.
	/// </summary>
	public float laneCurveSpeedMultiplier = 0.7f;

	/// <summary>
	/// The connectors curve speed multiplier.
	/// </summary>
	public float connectorsCurveSpeedMultiplier = 0.7f;

	/// <summary>
	/// The default type of vehicle.  This would be used as the default vehicle type when new lanes are created.
	/// </summary>
	public TSLaneInfo.VehicleType defaultVehicleType = (TSLaneInfo.VehicleType) (-1);

	/// <summary>
	/// The junctions processed.  This is to know if the junctions have been processed for this TSMainManager instance
	/// </summary>
	public bool junctionsProcessed = false;

	/// <summary>
	/// The vehicle type presets.  This are a list of presets that can be used for quicker access to combinations
	/// of vehicle types.
	/// </summary>
	[SerializeField]
	public List<VehicleTypePresets> vehicleTypePresets = new List<VehicleTypePresets>();

	/// <summary>
	/// The scale factor.  This is used to scale the tool UI that is draw on the scene view, so you could use the 
	/// tool in a miniature world with ease
	/// </summary>
	public float scaleFactor = 1f;


	[System.Serializable]
	public class VehicleTypePresets{
		public TSLaneInfo.VehicleType vehicleType;
		public string name;
	}



    //Bounds bounds = new Bounds();
    //Camera mainC;
    //void OnDrawGizmos()
    //{
    //    if (mainC == null) mainC = GameObject.FindObjectOfType(typeof(Camera)) as Camera;
    //    if (lanes == null) return;
    //    for (int i = 0; i < lanes.Length; i++)
    //    {
    //        bounds = new Bounds(lanes[i].conectorA, Vector3.one);
    //        bounds.Encapsulate(lanes[i].conectorB);
    //        bool draw = false;


    //        for (int w = 0; w < lanes[i].points.Length; w++)
    //        {
    //            if (w % 10 == 0)
    //                bounds.Encapsulate(lanes[i].points[w].point);
    //            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Application.isPlaying ? mainC : UnityEditor.SceneView.GetAllSceneCameras()[0]);
    //            if (GeometryUtility.TestPlanesAABB(planes, bounds))
    //                draw = true;
    //            if (draw)
    //            {
    //                if (lanes[i].points[w].carwhoReserved != null) Gizmos.color = lanes[i].points[w].carwhoReserved.myColor;// new Color(1.2f*Mathf.Abs(lanes[i].points[w].reservationID)/20000f,1.5f*Mathf.Abs(lanes[i].points[w].reservationID)/20000f,0.5f *Mathf.Abs(lanes[i].points[w].reservationID)/20000f);
    //                else if (lanes[i].points[w].reservationID ==0)
    //                    Gizmos.color = Color.blue;
    //                else Gizmos.color = Color.red;
    //                Gizmos.DrawCube(lanes[i].points[w].point, Vector3.one);
    //            }
    //        }
    //        if (draw)
    //            for (int c = 0; c < lanes[i].connectors.Length; c++)
    //            {
    //                if (lanes[i].connectors[c].isReserved)
    //                {
    //                    Gizmos.color = Color.green;
    //                    Gizmos.DrawLine(lanes[i].connectors[c].conectorA, lanes[i].connectors[c].conectorB);
    //                }
    //                for (int r = 0; r < lanes[i].connectors[c].points.Length; r++)
    //                {

    //                    if (lanes[i].connectors[c].points[r].laneReservationID != -1) Gizmos.color = Color.red;// new Color(1.2f*Mathf.Abs(lanes[i].connectors[c].points[r].laneReservationID)/200f,1.5f*Mathf.Abs(lanes[i].connectors[c].points[r].laneReservationID)/200f,0.5f *Mathf.Abs(lanes[i].connectors[c].points[r].laneReservationID)/200f);
    //                    else
    //                    {
    //                        if (lanes[i].connectors[c].points[r].carwhoReserved != null) Gizmos.color = lanes[i].connectors[c].points[r].carwhoReserved.myColor;//new Color(1.2f*Mathf.Abs(lanes[i].connectors[c].points[r].reservationID)/20000f,1.5f*Mathf.Abs(lanes[i].connectors[c].points[r].reservationID)/20000f,0.5f *Mathf.Abs(lanes[i].connectors[c].points[r].reservationID)/20000f);
    //                        else Gizmos.color = new Color((float)c / (float)lanes[i].connectors.Length, (float)i / (float)lanes.Length, (1f - ((float)c / (float)lanes[i].connectors.Length)) * 0.5f);
    //                    }
    //                    Gizmos.DrawCube(lanes[i].connectors[c].points[r].point, Vector3.one);

    //                }
    //            }
    //    }

    //}
}

/// <summary>
/// TS connetor other points.  This class holds the reference to the surrounding points of a connector
/// </summary>
[System.Serializable]
public class TSConnectorOtherPoints{
	/// <summary>
	/// The lane.
	/// </summary>
	[XmlElement("a1")]
	public int lane = -1;

	/// <summary>
	/// The connector.
	/// </summary>
	[XmlElement("a2")]
	public int connector = -1;

	/// <summary>
	/// The index of the point.
	/// </summary>
	[XmlElement("a3")]
	public int pointIndex = -1;
}


/// <summary>
/// TS points.  The class holds the information about a point.
/// </summary>
[System.Serializable]
public class TSPoints{
	/// <summary>
	/// The point position in world space.
	/// </summary>
	[XmlElement("a4")]
	public Vector3 point;

	/// <summary>
	/// The reservation ID.  This variable holds the ID of the car that is reserving this point if any, if not it holds 0.
	/// </summary>
	[XmlElement("a5")]
	public int reservationID = 0;

	/// <summary>
	/// The carwho reserved.  this is a reference to the class TSTrafficAI of the car that have reserved this point
	/// </summary>
	[XmlIgnore]
	public TSTrafficAI carwhoReserved;

	/// <summary>
	/// The lane reservation ID.  If this is a connector point, and have been reserved by a lane, then this value is
	/// different from -1
	/// </summary>
	[XmlElement("a6")]
	public int laneReservationID = -1;

	/// <summary>
	/// The connector reservation count.  This value increases if more than one car with different lane target reserves this
	/// point.
	/// </summary>
	[XmlElement("a7")]
	public int connectorReservationCount = 0;

	/// <summary>
	/// The distance to next point.
	/// </summary>
	[XmlElement("a8")]
	public float distanceToNextPoint = 0f;

	/// <summary>
	/// The index of the right paralel lane.
	/// </summary>
	[XmlElement("a9")]
	public int rightParalelLaneIndex = -1;

	/// <summary>
	/// The index of the left paralel lane.
	/// </summary>
	[XmlElement("a10")]
	public int leftParalelLaneIndex = -1;

	/// <summary>
	/// The max speed limit.  This is for internal use, for calculating the max
	/// </summary>
	[XmlElement("a11")]
	public float maxSpeedLimit = 1000f;

	/// <summary>
	/// The other connectors points.  Array that points to the opoints of other connectors that intersects with this
	/// point or are too close to it.
	/// </summary>
	[SerializeField]
	[XmlArray("a12")]
	[XmlArrayItem("a13")]
	public TSConnectorOtherPoints[] otherConnectorsPoints;

	[SerializeField]
	[XmlIgnore]
	public TSConnectorOtherPoints[] nearbyPoints;

	/// <summary>
	/// The road block ahead.  This would tell the AI not to get into this lane (overtake into this lane) since ahead is a road block.  This is to prevent traffic jams
	/// from AI that would overtake into a lane that has a road block in it.
	/// </summary>
	[XmlElement("a13a")]
	public bool roadBlockAhead = false;
}


/// <summary>
/// TS lane info.  This class is responsible for holding all the lane information.
/// </summary>
[System.Serializable]
public class TSLaneInfo{

	/// <summary>
	/// The conector a.  This is the starting point of the lane.
	/// </summary>
	[XmlElement("a14")]
	public Vector3 conectorA;

	/// <summary>
	/// The conector b.  This is the ending point of the lane.
	/// </summary>
	[XmlElement("a15")]
	public Vector3 conectorB;

	/// <summary>
	/// The middle points.  This are the middle control points of the spline that creates the lane
	/// shape.
	/// </summary>
	[XmlArray("a16")]
	[XmlArrayItem("a17")]
	public List<Vector3> middlePoints = new List<Vector3>();

	/// <summary>
	/// The width of the lane.
	/// </summary>
	[XmlElement("a18")]
	public float laneWidth = 2.5f;

	/// <summary>
	/// The type of the vehicle that can transit this lane.
	/// </summary>
	[XmlElement("a19",typeof(int))]
	public VehicleType vehicleType = ((VehicleType) (-1));

	/// <summary>
	/// The max speed set for this lane.
	/// </summary>
	[XmlElement("a20")]
	public float maxSpeed = 50;

	/// <summary>
	/// The points.  All the points that conforms this lane.
	/// </summary>
	[SerializeField]
	[XmlArray("a21")]
	[XmlArrayItem("a22")]
	public TSPoints[] points;

	/// <summary>
	/// The connectors.  All the connectors that are created from this lane.
	/// </summary>
	[SerializeField]
	[XmlArray("a23")]
	[XmlArrayItem("a24")]
	public TSLaneConnector[] connectors;

	/// <summary>
	/// The lane link right.
	/// </summary>
	[XmlElement("a25")]
	public int laneLinkRight = -1;

	/// <summary>
	/// The lane link left.
	/// </summary>
	[XmlElement("a26")]
	public int laneLinkLeft = -1;

	/// <summary>
	/// The traffic density.
	/// </summary>
	[XmlElement("a27")]
	public float trafficDensity = 1f;

	/// <summary>
	/// The total ocupation of this lane.
	/// </summary>
	[XmlElement("a28")]
	public float totalOcupation = 0f;

	/// <summary>
	/// The total distance.
	/// </summary>
	[XmlElement("a29")]
	public float totalDistance = 0f;


    /// <summary>
    /// The traffic light.  This field is not available on the Lite version.
    /// </summary>
    [XmlIgnore]
    public TSTrafficLight trafficLight;

    /// <summary>
    /// The max total ocupation this lane would allow.
    /// </summary>
    [XmlElement("a28a")]
	public float maxTotalOcupation = 75f;

	public enum VehicleType{
		Taxi, 
		Bus,
		Light,
		Medium,
		Heavy,
		Train,
		Heavy_Machinery,
		Pedestrians,
	}
}

/// <summary>
/// TS lane connector class.  This class holds all the information regarding the connectors.
/// </summary>
[System.Serializable]
public class TSLaneConnector{

	/// <summary>
	/// The conector a.  The starting point of this connector.
	/// </summary>
	[XmlElement("a30")]
	public Vector3 conectorA = Vector3.zero;

	/// <summary>
	/// The conector b.  The ending point of this connector.
	/// </summary>
	[XmlElement("a31")]
	public Vector3 conectorB = Vector3.zero;

	/// <summary>
	/// The middle points.  The control points of the spline for this connector.
	/// </summary>
	[XmlArray("a32")]
	[XmlArrayItem("a33")]
	public List<Vector3> middlePoints = new List<Vector3>();

	/// <summary>
	/// The points.  All the points that conforms this connector.
	/// </summary>
	[SerializeField]
	[XmlArray("a34")]
	[XmlArrayItem("a35")]
	public TSPoints[] points;

	/// <summary>
	/// The next lane.
	/// </summary>
	[XmlElement("a36")]
	public int nextLane;

	/// <summary>
	/// The previous lane.
	/// </summary>
	[XmlElement("a37")]
	public int previousLane;

	/// <summary>
	/// The forced stop.  The cars have to stop on this junction if there is no "convoy" crossing at the moment?.
	/// </summary>
	[XmlElement("a38")]
	public bool forcedStop = false;

	/// <summary>
	/// Is reserved.  Is this connector already reserved, this is for internal use only.
	/// </summary>
	[XmlElement("a39")]
	public bool isReserved = false;

	/// <summary>
	/// Is requested.  Have been this connector requested?
	/// </summary>
	[XmlElement("a40")]
	public bool isRequested = false;

	/// <summary>
	/// The reserved by ID.  List of all the cars ID's that have reserved this connector.
	/// </summary>
	[XmlArray("a41")]
	[XmlArrayItem("a42")]
	public HashSet<int> reservedByID = new HashSet<int>();

	/// <summary>
	/// The priority.
	/// </summary>
	[XmlElement("a43")]
	public int priority = 1;

	/// <summary>
	/// The direction.  This is to know if this connector is a straight, left or right connector.
	/// </summary>
	[XmlElement("a44")]
	public Direction direction =  Direction.Straight;

	/// <summary>
	/// The total distance of this connector.
	/// </summary>
	[XmlElement("a45")]
	public float totalDistance = 0f;

	/// <summary>
	/// The random weight.  This is for internal use.
	/// </summary>
	[XmlElement("a46")]
	public float randomWeight = 0;

	/// <summary>
	/// The type of the vehicle.
	/// </summary>
//	[XmlElement("a47")]
	[XmlElement("a47",typeof(int))]
	public TSLaneInfo.VehicleType vehicleType = ((TSLaneInfo.VehicleType) (-1));

	/// <summary>
	/// The connector reserved by traffic light.
	/// </summary>
	[XmlElement("a48")]
	public bool connectorReservedByTrafficLight = false;

	/// <summary>
	/// The remaining green light time.  By default is -1, which means this connector have no trafficlight
	/// </summary>
	[XmlElement("a49")]
	public float remainingGreenLightTime = -1;

	public enum Direction{
		Left,
		Right,
		Straight
	}

	/// <summary>
	/// The other connectors.  This is used internally to know faster which other connectors that are
	/// near this one are currently reserved, and also to be able to yield for those connectors that
	/// have greater pass priority
	/// </summary>
	[XmlElement("a50")]
	public HashSet<TSLaneConnector> OtherConnectors = new HashSet<TSLaneConnector>();

}
