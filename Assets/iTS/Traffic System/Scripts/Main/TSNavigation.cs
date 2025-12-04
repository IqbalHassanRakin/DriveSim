using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// TS navigation.  This clas is responsible for navigating throught the lanes and selecting the next connector/lane
/// the car would go by.
/// </summary>
public class TSNavigation : MonoBehaviour {

	/// <summary>
	/// TS Next lane selection.  This class hold the pointers to the next connector or lane.
	/// </summary>
	[System.Serializable]
	public struct TSNextLaneSelection{
		public int nextLane;
		public int nextConnector;
		public bool isConnector;
	}

	/// <summary>
	/// TS Reserved points.  This struct holds the references to the  connectors and points that have been reserved
	/// by this car.
	/// </summary>
	public struct TSReservedPoints{
//		public TSLaneConnector connector;
//		public TSPoints point;
		public int point;
		public int lane;
		public int connector;

		/// <summary>
		/// Initializes a new instance of the <see cref="TSNavigation+TSReservedPoints"/> struct.
		/// </summary>
		/// <param name="_connector">_connector.</param>
		/// <param name="_point">_point.</param>
//		public  TSReservedPoints(TSLaneConnector _connector, TSPoints _point)
//		{
//			connector = _connector;
//			point = _point;
//		}

		public  TSReservedPoints(int _lane, int _connector, int _point)
		{
			lane = _lane;
			connector = _connector;
			point = _point;
		}
		
	}

	public TSPoints Point(TSReservedPoints point)
	{
		if (point.connector ==-1)
		{
			return lanes[point.lane].points[point.point];
		}
		else
		{
			return lanes[point.lane].connectors[point.connector].points[point.point];
		}
	}




	public TSLaneConnector Connector(TSReservedPoints point)
	{
		if (point.connector !=-1)
		{
			return lanes[point.lane].connectors[point.connector];
		}
		else
		{
			return null;
		}
	}

	// Public Variables

	/// <summary>
	/// The max waiting time.  This value would be compared with the amount of time a car is waiting on a junction
	/// to cross it, this is to avoid or at least try to prevent traffic jams.
	/// </summary>
//	public float maxWaitingTime = 10f; 

	/// <summary>
	/// The current waypoint this car is pointing at, and using for steering.
	/// </summary>
	[HideInInspector]
	public int currentWaypoint = 0;

	/// <summary>
	/// The current waypoint on car.
	/// </summary>
	[HideInInspector]
	public int currentWaypointOnCar = 0;

	/// <summary>
	/// The relative waypoint position.
	/// </summary>
	[HideInInspector]
	public Vector3 RelativeWaypointPosition = Vector3.zero;

	/// <summary>
	/// The relative waypoint position on car.
	/// </summary>
	[HideInInspector]
	public Vector3 RelativeWaypointPositionOnCar = Vector3.zero;

	/// <summary>
	/// The previous waypoint steer.
	/// </summary>
	[HideInInspector]
	public int previousWaypointSteer = 0;

	/// <summary>
	/// The current lane.
	/// </summary>
	[HideInInspector]
	public int currentLane = 0;

	/// <summary>
	/// The next track.
	/// </summary>
	[HideInInspector]
	public TSNextLaneSelection nextTrack = new TSNextLaneSelection();

	/// <summary>
	/// The next track path.  This list hold the next lanes/connectors this car would be traveling to.
	/// </summary>
	[HideInInspector]
	public List<TSNextLaneSelection> nextTrackPath = new List<TSNextLaneSelection>(20);

	/// <summary>
	/// The current max speed.
	/// </summary>
//	[HideInInspector]
	public float currentMaxSpeed = 50f;

	/// <summary>
	/// The waypoints.  This array holds the current lane points reference.
	/// </summary>
	[HideInInspector]
	public TSPoints[] waypoints;

	/// <summary>
	/// The last waypoints.  This array holds the last lane points reference.
	/// </summary>
	[HideInInspector]
	public TSPoints[] lastWaypoints;

	/// <summary>
	/// The lanes.  Reference to all the lanes.
	/// </summary>
	[HideInInspector]
	public TSLaneInfo[]  lanes;

	/// <summary>
	/// The traveling on conector.  Is the car traveling on a connector?.
	/// </summary>
	[HideInInspector]
	public bool travelingOnConector = false;

	/// <summary>
	/// The was traveling on connector.  Was this car traveling on a connector?
	/// </summary>
	[HideInInspector]
	public bool wasTravelingOnConnector = false;

	/// <summary>
	/// The selected connector.
	/// </summary>

	[HideInInspector]
	public int selectedConnector = 0;

	/// <summary>
	/// The last selected connector.
	/// </summary>
	[HideInInspector]
	public TSLaneConnector lastSelectedConnector;

	/// <summary>
	/// The last lane.
	/// </summary>
	[HideInInspector]
	public int lastLane = 0;

	/// <summary>
	/// My ID.  This is a reference to the instance ID.
	/// </summary>
	[HideInInspector]
	public int myID;

	/// <summary>
	/// The relative W position magnitude.
	/// </summary>
	[HideInInspector]
	public float relativeWPosMagnitude = 0;

	/// <summary>
	/// The changing lane.
	/// </summary>
	[HideInInspector]
	public bool changingLane = false;

	/// <summary>
	/// The over taking.
	/// </summary>
	[HideInInspector]
	public bool overTaking = false;

	/// <summary>
	/// The reserved points.  A Queue of all the points that have been reserved by this car.
	/// </summary>
	[HideInInspector]
	public Queue<TSReservedPoints> reservedPoints = new Queue<TSReservedPoints>(100);// List<TSReservedPoints>();

	/// <summary>
	/// The index of the next track.
	/// </summary>
	[HideInInspector]
	public int nextTrackIndex = 0;

	/// <summary>
	/// The reserved change lane points.
	/// </summary>
	[HideInInspector]
	public Queue<TSReservedPoints> reservedChangeLanePoints = new Queue<TSReservedPoints>(100);// List<TSReservedPoints>();

	/// <summary>
	/// The reserved connectors.
	/// </summary>
	[HideInInspector]
	public Queue<TSLaneConnector> reservedConnectors = new Queue<TSLaneConnector>(5);// List<TSLaneConnector>();

	/// <summary>
	/// The starting.  This is for internal use.
	/// </summary>
	[HideInInspector]
	public bool starting = true;

	[HideInInspector]
	public Vector3 pointOffset = Vector3.zero;


	[HideInInspector]
	public int lastConnectorIndex;
	[HideInInspector]
	public int lastLaneIndex;

	// Private Variables

	/// <summary>
	/// The car controller AI.  This is a reference to the TSTrafficAI class.
	/// </summary>
	private TSTrafficAI carControllerAI;

	/// <summary>
	/// My transform.  Cached reference of transform.
	/// </summary>
	private Transform myTransform;

	/// <summary>
	/// The is turning.
	/// </summary>
	private bool isTurning = false;

	/// <summary>
	/// The half depth of this car.
	/// </summary>
//	private float halfDepth;

	/// <summary>
	/// The overtaking from left.
	/// </summary>
	private bool overtakingFromLeft = false;

	/// <summary>
	/// The half depth sqr.
	/// </summary>
//	private float halfDepthSqr = 0;

	private float carOcupation = 0f;

	private List<int> occupiedLanes = new List<int>(10);

	public float CarOcupationLenght {
		get {
			return carOcupation;
		}
	}

	// Use this for initialization
	void Awake () {
		myTransform = transform;
		carControllerAI = GetComponent<TSTrafficAI>();
		myID = GetInstanceID();
		myTransform.name += "-("+ myID.ToString() + ")";
	}

	void Start()
	{
		if (lanes == null)
			lanes = (GameObject.FindObjectOfType(typeof( TSMainManager)) as TSMainManager).lanes;
//		halfDepth = carControllerAI.carDepth/2f;
//		halfDepthSqr = ((-halfDepth)*(-halfDepth));
//		InvokeRepeating("Update1",0,0.02f);

		SetCarOccupationLength();
	}

	public void SetCarOccupationLength(){
		carOcupation = carControllerAI.carDepth+carControllerAI.lengthMargin;
	}

	// Update is called once per frame
	void FixedUpdate () {
		NavigateToWaypoints ();
	}

	/// <summary>
	/// Switchs the track.
	/// </summary>
	/// <returns><c>true</c>, if track was switched, <c>false</c> otherwise.</returns>
	public bool SwitchTrack()
	{
		if (nextTrackIndex >=nextTrackPath.Count){return false;}

		currentWaypoint = 0;
		previousWaypointSteer = 0 ;
		
		if (nextTrackPath[nextTrackIndex].isConnector)
		{
			if(lanes[nextTrackPath[nextTrackIndex].nextLane].connectors.Length == 0){Debug.Log ("not switching2!");return false;}
			currentLane = nextTrackPath[nextTrackIndex].nextLane;
			selectedConnector = nextTrackPath[nextTrackIndex].nextConnector;

			waypoints = lanes[currentLane].connectors[selectedConnector].points;	

			travelingOnConector = true;
			nextTrackIndex ++;
			return true;
		}else {
//			UnReserveNearConnectorPoints(reservedConnectors.Peek());
//			reservedConnectors.Dequeue();
			currentLane = nextTrackPath[nextTrackIndex].nextLane;
			waypoints = lanes[currentLane].points;
//			currentMaxSpeed = lanes[currentLane].maxSpeed;
			GetLaneMaxSpeed();

			travelingOnConector = false;
			nextTrackIndex ++;
			return true;
		}

	}

	public void AddOcupiedLane(int lane){
		occupiedLanes.Add(lane);
	}


	public void GetLaneMaxSpeed()
	{
		currentMaxSpeed = Random.Range(lanes [currentLane].maxSpeed/2f,lanes [currentLane].maxSpeed);
	}

	/// <summary>
	/// The reserv lane.
	/// </summary>
	int reservLane = 0;
	
	/// <summary>
	/// The reserv point.
	/// </summary>
	int reservPoint = 0;
	
	/// <summary>
	/// The reserv connector.
	/// </summary>
	int reservConnector = 0;
	
	/// <summary>
	/// The roolback.
	/// </summary>
	bool roolback = false;
	
	
	/// <summary>
	///Variable for use in all for-loops to avoid memory allocation and share variable
	/// </summary>
	int i =0;
	
	/// <summary>
	/// The index of the point.
	/// </summary>
	int pointIndex = 0;

	///<summary>
	/// Reserves the near connector points.
	/// </summary>
	/// <returns><c>true</c>, if near connector points was reserved, <c>false</c> otherwise.</returns>
	/// <param name="connector">Connector.</param>
	/// <param name="lane">Lane.</param>
	public bool ReserveNearConnectorPoints(ref TSLaneConnector connector, int lane, int currentPoint)
	{
		roolback = false;
		connector.isRequested = true;
		
		//Add something to prevent the cars going to a lane that is already full and blocking the junctions
		if (!connector.isReserved)
		{
			if (lanes[connector.nextLane].totalOcupation > lanes[connector.nextLane].maxTotalOcupation) {
				connector.isRequested = false;
                //Added the method, to make the AI rest their route if the current selected connector is already too crowded
                //this makes it a smarter AI
                ResetRoute();
				return false;
			}
			if (connector.forcedStop && carControllerAI.carSpeed > 1) return false;
			for (i = currentPoint; i < connector.points.Length;i++)
			{
				for (pointIndex = 0; pointIndex < connector.points[i].otherConnectorsPoints.Length; pointIndex++)
				{
					reservLane = connector.points[i].otherConnectorsPoints[pointIndex].lane;
					reservConnector = connector.points[i].otherConnectorsPoints[pointIndex].connector;
					reservPoint = connector.points[i].otherConnectorsPoints[pointIndex].pointIndex;
					if (lanes[reservLane].connectors[reservConnector].isRequested && lanes[reservLane].connectors[reservConnector].priority > connector.priority && reservLane != lane)
					{
						roolback = true;
						break;
					}
//					if (lanes[reservLane].connectors[reservConnector].isReserved && lane != lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID && lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID != -1)
//					{
//						roolback = true;
//						break;
//					}
					//else{
					if( (lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID != -1 && lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID != lane))
					{
						roolback = true;
						break;
					}
				}
				if (roolback) {
					break;}
				if ((connector.points[i].laneReservationID != -1 && connector.points[i].laneReservationID != lane))
				{
					roolback = true;
					break;
				}

			}
			
			if (!roolback)
			{
				for (i = 0; i < connector.points.Length;i++)
				{
					for (pointIndex = 0; pointIndex < connector.points[i].otherConnectorsPoints.Length; pointIndex++)
					{
						reservLane = connector.points[i].otherConnectorsPoints[pointIndex].lane;
						reservConnector = connector.points[i].otherConnectorsPoints[pointIndex].connector;
						reservPoint = connector.points[i].otherConnectorsPoints[pointIndex].pointIndex;
						lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID = lane;
						lanes[reservLane].connectors[reservConnector].points[reservPoint].connectorReservationCount++;
						connector.OtherConnectors.Add(lanes[reservLane].connectors[reservConnector]);
					}

					connector.points[i].laneReservationID = lane;
					connector.points[i].connectorReservationCount++;
				}
				connector.isReserved = true;
//				if (!connector.reservedByID.Contains(myID))
					connector.reservedByID.Add(myID);
				reservedConnectors.Enqueue(connector);
				lanes[connector.nextLane].totalOcupation += Mathf.Round((carOcupation)/lanes[connector.nextLane].totalDistance * 100f);
				occupiedLanes.Add(connector.nextLane);
			}
		}
		else
		{
			if (!connector.reservedByID.Contains(myID)){
				if (CheckConnectorsPriority(ref connector)){
					UnReserveNearConnectorPoints(connector);
					return false;}
				if (connector.forcedStop && carControllerAI.carSpeed > 1) {
					UnReserveNearConnectorPoints(connector);
					return false;}
				if (lanes[connector.nextLane].totalOcupation > lanes[connector.nextLane].maxTotalOcupation) {
					UnReserveNearConnectorPoints(connector);
					return false;}
				connector.reservedByID.Add(myID); 
				reservedConnectors.Enqueue(connector);
				lanes[connector.nextLane].totalOcupation += Mathf.Round((carOcupation)/lanes[connector.nextLane].totalDistance * 100f);
				occupiedLanes.Add(connector.nextLane);
			}
		} 
		return connector.isReserved;
	}

	public bool CheckConnectorsPriority(ref TSLaneConnector connector)
	{
		foreach(TSLaneConnector otherConnector in connector.OtherConnectors)
		{
			if ( otherConnector.isRequested && otherConnector.priority > connector.priority)
				return true;
		}
		return false;
	}



	/// <summary>
	/// Reserves the near connector points.
	/// </summary>
	/// <returns><c>true</c>, if near connector points was reserved, <c>false</c> otherwise.</returns>
	/// <param name="points">Points.</param>
	/// <param name="connector">Connector.</param>
	public bool ReserveNearConnectorPoints(ref TSPoints points,ref TSLaneConnector connector, int lane, out bool isFromSameLane)
	{
        //Set this bool to true, since we are going to check if there is another car on some of the other connector points
        //and if it is on another lane (not going on same direction or from the same lane)
        isFromSameLane = true;
		if (connector.isReserved)
		{
			roolback = false;
			for (pointIndex = 0; pointIndex < points.otherConnectorsPoints.Length; pointIndex++)
			{
				reservLane = points.otherConnectorsPoints[pointIndex].lane;
				reservConnector = points.otherConnectorsPoints[pointIndex].connector;
				reservPoint = points.otherConnectorsPoints[pointIndex].pointIndex;

                if (lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID == lane)
                {
                    if (lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID == 0)
                    {
                        lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID = myID;
                        lanes[reservLane].connectors[reservConnector].points[reservPoint].carwhoReserved = carControllerAI;
                    }
                    else if (lanes[reservLane].connectors[reservConnector].points[reservPoint].carwhoReserved != null 
                        && lanes[reservLane].connectors[reservConnector].points[reservPoint].carwhoReserved.NavigateToWaypoints.lastLane != lane)
                    {
                        // if there is another car and that car is not on the same lane, set the bool to false to tell the
                        //AI to react properly
                        isFromSameLane = false;
                    }
                }
                else
                {
                    //if the point is taken by another car from another lane set the bool to tru to tell the AI
                    //to react properly
                    isFromSameLane = false;
                }

                

				if (lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID != myID &&
				    !lanes[reservLane].connectors[reservConnector].connectorReservedByTrafficLight){roolback = true; break;};
			}

			if (roolback)
			{
				
				for (pointIndex = 0; pointIndex < points.otherConnectorsPoints.Length; pointIndex++)
				{
					reservLane = points.otherConnectorsPoints[pointIndex].lane;
					reservConnector = points.otherConnectorsPoints[pointIndex].connector;
					reservPoint = points.otherConnectorsPoints[pointIndex].pointIndex;
					if(lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID == myID){
						lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID = 0;
						lanes[reservLane].connectors[reservConnector].points[reservPoint].carwhoReserved = null;
					}
				}	
			}
			return !roolback;
		}else{
            isFromSameLane = false;
            return false;
		}
	}
	
	
	
	
	public void UnReserveNearConnectorPoints(TSLaneConnector connector)
	{
		
		if (connector.isReserved && connector.reservedByID.Count == 1 && connector.reservedByID.Contains(myID)){
			for (int i = 0; i < connector.points.Length;i++)
			{
				for (int pointIndex = 0; pointIndex < connector.points[i].otherConnectorsPoints.Length; pointIndex++)
				{
					reservLane = connector.points[i].otherConnectorsPoints[pointIndex].lane;
					reservConnector = connector.points[i].otherConnectorsPoints[pointIndex].connector;
					reservPoint = connector.points[i].otherConnectorsPoints[pointIndex].pointIndex;
					
					if( lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID == connector.previousLane){
						if (lanes[reservLane].connectors[reservConnector].points[reservPoint].connectorReservationCount < 2){
							lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID = -1;
							lanes[reservLane].connectors[reservConnector].points[reservPoint].connectorReservationCount = 0;
						}else lanes[reservLane].connectors[reservConnector].points[reservPoint].connectorReservationCount--;
					}
				}
				if (connector.points[i].laneReservationID == connector.previousLane)
				{
					if (connector.points[i].connectorReservationCount < 2)
					{
						connector.points[i].laneReservationID = -1;
						connector.points[i].connectorReservationCount = 0;
					}else{
						connector.points[i].connectorReservationCount--;
					}
				}
				
			}
			connector.isReserved = false;
			connector.isRequested = false;
			connector.reservedByID.Remove(myID);
			return;
		}
		if (connector.isReserved)connector.reservedByID.Remove(myID);
	}
	
	public void UnReserveNearConnectorPoints(TSPoints points)
	{
		for (int pointIndex = 0; pointIndex < points.otherConnectorsPoints.Length; pointIndex++)
		{
			reservLane = points.otherConnectorsPoints[pointIndex].lane;
			reservConnector = points.otherConnectorsPoints[pointIndex].connector;
			reservPoint = points.otherConnectorsPoints[pointIndex].pointIndex;
			if(lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID == myID)
			{
                lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID = 0;
                lanes[reservLane].connectors[reservConnector].points[reservPoint].carwhoReserved= null;
//				if (lanes[reservLane].connectors[reservConnector].points[reservPoint].connectorReservationCount < 2){
//					lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID = -1;
//					lanes[reservLane].connectors[reservConnector].points[reservPoint].connectorReservationCount = 0;
//				}else lanes[reservLane].connectors[reservConnector].points[reservPoint].connectorReservationCount--;
            }
		}
//		if (points.connectorReservationCount < 2)
//		{
//			points.laneReservationID = -1;
//			points.connectorReservationCount = 0;
//		}else{
//			points.connectorReservationCount--;
//		}

	}
	
	
	void  NavigateToWaypoints ()
	{
		RelativeWaypointPosition =  myTransform.InverseTransformPoint(waypoints[currentWaypoint].point + pointOffset);
		if (reservedPoints.Count >0)
			RelativeWaypointPositionOnCar =  carControllerAI.RearPoint.InverseTransformPoint( Point(reservedPoints.Peek()).point + pointOffset);
		relativeWPosMagnitude = RelativeWaypointPosition.magnitude;
		if (!overTaking){
			AddNextTrackToPath();

			if ( RelativeWaypointPosition.z < carControllerAI.LookAheadDistance && Mathf.Abs(RelativeWaypointPosition.x) < carControllerAI.carWidth*5f)
			{
				//Put a variable here to be able to tweak the distance to make the turn lights start working
				if (nextTrackPath.Count > 1){
					if (!isTurning && nextTrackPath[1].isConnector && waypoints.Length -currentWaypoint < 15){
						if (carControllerAI.OnTurnLeft != null && lanes[nextTrackPath[1].nextLane].connectors[nextTrackPath[1].nextConnector].direction == TSLaneConnector.Direction.Left)
						{
							isTurning = true;
							carControllerAI.OnTurnLeft(true);
						}
						if (carControllerAI.OnTurnRight != null && lanes[nextTrackPath[1].nextLane].connectors[nextTrackPath[1].nextConnector].direction == TSLaneConnector.Direction.Right)
						{
							isTurning = true;
							carControllerAI.OnTurnRight(true);
						}
					}
				}

				previousWaypointSteer = currentWaypoint;
				currentWaypoint ++;
				if ( currentWaypoint >= waypoints.Length ) 
				{
					if (lanes[currentLane].connectors.Length >0) {

						if (SwitchTrack())
							currentWaypoint = 0;
						else currentWaypoint--;
					}
					else {
						currentWaypoint --;
						gameObject.SetActive(false);
					}
			    }
				if ( previousWaypointSteer >= waypoints.Length ) 
	    			previousWaypointSteer = 0;
				RelativeWaypointPosition =  myTransform.InverseTransformPoint(waypoints[currentWaypoint].point + pointOffset);
				relativeWPosMagnitude = RelativeWaypointPosition.magnitude;
			}
		}else
		{
			//Overtaking, things are done backwards
			if (RelativeWaypointPosition.z < carControllerAI.LookAheadDistance && Mathf.Abs(RelativeWaypointPosition.x) < carControllerAI.carWidth*5f)
			{
				previousWaypointSteer = currentWaypoint;
				currentWaypoint --;
				if ( currentWaypoint <= 0 ) 
						currentWaypoint = 0;
				if ( previousWaypointSteer <= 0 ) 
					previousWaypointSteer = 0;
				RelativeWaypointPosition =  myTransform.InverseTransformPoint(waypoints[currentWaypoint].point + pointOffset);
				relativeWPosMagnitude = RelativeWaypointPosition.magnitude;
			}
		}
		CheckLastPoints();
	}
	bool changed;
	bool isConnector;
	float distanceForCheck;
	void CheckLastPoints()
	{
		distanceForCheck = Mathf.Sign(RelativeWaypointPositionOnCar.z) * RelativeWaypointPositionOnCar.sqrMagnitude;
		if (distanceForCheck < 0 && Mathf.Abs(RelativeWaypointPositionOnCar.x) < carControllerAI.carWidth*5f && reservedPoints.Count >1)// 
		{
			TSPoints cachedReservedPoint = Point(reservedPoints.Peek());
			changed = false;
			isConnector =Connector(reservedPoints.Peek()) != null;
			if(!wasTravelingOnConnector){
				totalDistance -= cachedReservedPoint.distanceToNextPoint;
				if (isConnector)
					changed = true;
			}else
			{
				totalDistance -= cachedReservedPoint.distanceToNextPoint;
				if (!isConnector)
					changed = true;
			}
			
			if ((changingLane && !overTaking) || (overTaking))
			{
				if (reservedChangeLanePoints.Count == 0) {
					if (changingLane)changingLane = false;
//					if (overTaking) overTaking = false;
				}else{
					changingLane = true;
					TSPoints cachedReservedChangeLanePoints =Point(reservedChangeLanePoints.Peek());
					if(cachedReservedChangeLanePoints.reservationID == myID){
						cachedReservedChangeLanePoints.reservationID = 0;
						cachedReservedChangeLanePoints.carwhoReserved = null;
						reservedChangeLanePoints.Dequeue();
					}
				}
				
			}

			if (cachedReservedPoint.reservationID == myID){
				cachedReservedPoint.reservationID = 0;
				cachedReservedPoint.carwhoReserved = null;
			}
			
			carControllerAI.segDistance -= cachedReservedPoint.distanceToNextPoint;
			UnReserveNearConnectorPoints(cachedReservedPoint);

			if (changed)
			{
				if (nextTrackPath.Count<1 && !overTaking) AddNextTrackToPath();
				if (nextTrackPath.Count>0){
					if (isConnector){
//						lastWaypoints = lanes[nextTrackPath[0].nextLane].connectors[nextTrackPath[0].nextConnector].points;
						lastLane = nextTrackPath[0].nextLane;
						lastSelectedConnector =Connector(reservedPoints.Peek());
						wasTravelingOnConnector = true;
						lanes[lastLane].totalOcupation -= Mathf.Round( (carOcupation)/lanes[lastLane].totalDistance*100f);
						if (lanes[lastLane].totalOcupation < 0) lanes[lastLane].totalOcupation = 0;
						occupiedLanes.Remove(lastLane);
					}
					else{ 
						if (carControllerAI.OnTurnLeft != null){
							isTurning = false;
							carControllerAI.OnTurnLeft(false);
						}
						if (carControllerAI.OnTurnRight != null)
						{
							isTurning = false;
							carControllerAI.OnTurnRight(false);
						}
						wasTravelingOnConnector = false;

//						UnReserveNearConnectorPoints(reservedConnectors.Peek());
//						reservedConnectors.Dequeue();
//						lastWaypoints = lanes[nextTrackPath[0].nextLane].points;
						lastSelectedConnector = null;
						
					}
					carControllerAI.nextPathIndex--;
					if (carControllerAI.nextPathIndex < 0) carControllerAI.nextPathIndex =0;
					nextTrackPath.RemoveAt(0);
					nextTrackIndex--;
				}
			}

			reservedPoints.Dequeue();
		}
	}


	int newCurrentWaypointOnCar = 0;
	int newLane = 0;
	int initialIndex;
	int point = 0;
	int lane = 0;
	bool found = false;
	float segDistance ;
	int counter = 0;
//	int reservedCLIndex = 0;
	TSReservedPoints newPoint1 = new TSReservedPoints();
	TSReservedPoints newPoint = new TSReservedPoints();
	/// <summary>
	/// Change to another lane.
	/// </summary>
	/// <param name="left">If set to <c>true</c> left.</param>
	public void LaneChange(bool left)
	{
		if (!changingLane && reservedPoints.Count >0)
		{
			newCurrentWaypointOnCar = 0;
			newLane = 0;
			TSReservedPoints P = reservedPoints.Peek();
			initialIndex = P.point;
			if (overTaking)
			{ 
				left = overtakingFromLeft;
				newCurrentWaypointOnCar = (left?Point(P).rightParalelLaneIndex:Point(P).leftParalelLaneIndex);
				newLane = (left?lanes[currentLane].laneLinkRight:lanes[currentLane].laneLinkLeft);

			}
			else
			{
				point = 0;
				lane = 0;
				found = false;
				if (left)
				{
					if (Point(P).leftParalelLaneIndex != -1)
					{
						found = true;
						point = Point(P).leftParalelLaneIndex;
						lane = lanes[currentLane].laneLinkLeft;
//						initialIndex = P.point;// lanes[lane].points[point].rightParalelLaneIndex;
					}
					
				}else
				{
					if (Point(P).rightParalelLaneIndex != -1)
					{
						found = true;
						point = Point(P).rightParalelLaneIndex;
						lane = lanes[currentLane].laneLinkRight;
//						initialIndex = P.point;// lanes[lane].points[point].leftParalelLaneIndex;
					}
				}
				if (!found) return;
				newCurrentWaypointOnCar = point;
				newLane = lane;
				
			}

			if (!(((((int)lanes[newLane ].vehicleType) & (1<<(int)carControllerAI.myVehicleType))) > 0))
			{
				return;
			}
			totalDistance =0;
			segDistance = (myTransform.position - (Point(P).point + pointOffset)).magnitude;
			counter = 0;
			reservedChangeLanePoints.Clear();
			while (segDistance < Mathf.Clamp(carControllerAI.maxLockaheadDistanceFullStop ,
			                                 carControllerAI.carDepth,float.MaxValue)|| counter < 2)
			{
				if ((newCurrentWaypointOnCar+counter >= lanes[newLane].points.Length)
				    ||(lanes[newLane].points[newCurrentWaypointOnCar+counter].reservationID != 0 
				 && lanes[newLane].points[newCurrentWaypointOnCar+counter].reservationID != myID)
				    || lanes[newLane].points[newCurrentWaypointOnCar+counter].roadBlockAhead)
				{
					if (reservedChangeLanePoints.Count > reservedPoints.Count)
					{
						reservedPoints = reservedChangeLanePoints;
					}
					reservedChangeLanePoints.Clear();
					return;
				}
				if (segDistance < carControllerAI.carDepth*2f+3 || counter < 2){
					
					if ((overTaking && initialIndex-counter >= 0) 
					    || (!overTaking && initialIndex+counter < lanes[currentLane].points.Length))
					{
						if (lanes[currentLane].points[(overTaking?initialIndex-counter:initialIndex+counter)].reservationID ==0 
						    || lanes[currentLane].points[(overTaking?initialIndex-counter:initialIndex+counter)].reservationID ==myID)
						{
							newPoint1.lane = currentLane;
							newPoint1.connector = -1;
							newPoint1.point = (overTaking?initialIndex-counter:initialIndex+counter);//lanes[currentLane].points[(overTaking?initialIndex-counter:initialIndex+counter)] ;
							TSPoints cachedNewPoint = Point(newPoint1);
							cachedNewPoint.reservationID = myID;
							cachedNewPoint.carwhoReserved = carControllerAI;
							reservedChangeLanePoints.Enqueue(newPoint1);
						}
					}
				}
				segDistance+=lanes[newLane].points[newCurrentWaypointOnCar+counter].distanceToNextPoint;
				counter++;
			}
			counter--;
			UnreserveAll(false, true);
			reservedPoints.Clear();
			for (i = 0; i < counter;i++)
			{
				lanes[newLane].points[newCurrentWaypointOnCar+i].reservationID = myID;
				lanes[newLane].points[newCurrentWaypointOnCar+i].carwhoReserved = carControllerAI;
				newPoint.lane = newLane;
				newPoint.connector = -1;
				newPoint.point = newCurrentWaypointOnCar+i;//lanes[newLane].points[newCurrentWaypointOnCar+i];
				reservedPoints.Enqueue(newPoint);
			}
			if (counter == 0) {
				lanes[newLane].points[newCurrentWaypointOnCar].reservationID = myID;
				lanes[newLane].points[newCurrentWaypointOnCar].carwhoReserved = carControllerAI;
				newPoint.lane = newLane;
				newPoint.connector = -1;
				newPoint.point = newCurrentWaypointOnCar;//lanes[newLane].points[newCurrentWaypointOnCar];
				reservedPoints.Enqueue(newPoint);
			}
			changingLane = true;

			currentLane = newLane;
			if (overTaking) {
				overTaking = false;
				carControllerAI.changeLaneTime = Time.time;
			}


			waypoints = lanes [newLane].points;

			wasTravelingOnConnector = false;
			travelingOnConector = false;
			lastLane = 0;
			lastWaypoints = waypoints;
			lastLaneIndex = currentLane;
			lastConnectorIndex = -1;

			int tempInt= newCurrentWaypointOnCar + counter-1;
			if (tempInt >= lastWaypoints.Length)
				tempInt = lastWaypoints.Length-1;
			if (tempInt <0)tempInt = 0;

			carControllerAI.previousWaypointCurve = tempInt;
			currentWaypoint = tempInt;
			currentWaypointOnCar = newCurrentWaypointOnCar;
			previousWaypointSteer = currentWaypoint;

			RelativeWaypointPositionOnCar =  carControllerAI.RearPoint.InverseTransformPoint(Point(P).point + pointOffset);
			relativeWPosMagnitude = 0;
			carControllerAI.nextPathIndex = 0;
			carControllerAI.segDistance = segDistance-carControllerAI.carDepth-3;
			carControllerAI.currenPoint = lastWaypoints[carControllerAI.previousWaypointCurve];
			carControllerAI.currentConnector = null;
			carControllerAI.lane = 0;

			carControllerAI.brakeSpeeds.Clear();// = new List<TSTrafficAI.TSBrakeSpeeds>();
			selectedConnector = 0;
			lastSelectedConnector = null;
			starting = true;
			currentMaxSpeed = Random.Range(lanes [newLane].maxSpeed/2f,lanes [newLane].maxSpeed);
			AddNextTrackToPath();
			return;
		}
		return;
	}



	/// <summary>
	/// Overtakes if there is free lanes.
	/// </summary>
	/// <param name="left">If set to <c>true</c> left.</param>
	public void OverTaking(bool left)
	{
		if (!overTaking && reservedPoints.Count >0){
			TSReservedPoints P = reservedPoints.Peek();
			newCurrentWaypointOnCar = (left?Point(P).leftParalelLaneIndex:Point(P).rightParalelLaneIndex);
            
			newLane = (left?lanes[currentLane].laneLinkLeft:lanes[currentLane].laneLinkRight);
			if (newCurrentWaypointOnCar == -1 || !(((((int)lanes[newLane ].vehicleType) & (1<<(int)carControllerAI.myVehicleType))) > 0))
			{
				return;
			}
			if (newCurrentWaypointOnCar < 25) return;
			totalDistance =0;
			segDistance = (myTransform.position - (Point(P).point + pointOffset)).magnitude;
			counter = 0;
			initialIndex =  P.point;//  (left?lanes[newLane].points[newCurrentWaypointOnCar].leftParalelLaneIndex:lanes[newLane].points[newCurrentWaypointOnCar].rightParalelLaneIndex);
//			UnreserveAll(false, true);
			if (!wasTravelingOnConnector){
				lanes[lastLane].totalOcupation -= Mathf.Round((carOcupation)/lanes[lastLane].totalDistance*100f);
				occupiedLanes.Remove(lastLane);
			}
			reservedChangeLanePoints.Clear();
			int actualPointTouse = 0;
			while (segDistance < Mathf.Clamp(carControllerAI.maxLockaheadDistanceFullStop*5f,carControllerAI.carDepth * 10f + 3f,float.MaxValue) || counter <2)
			{
				if ((newCurrentWaypointOnCar-counter < 0) || 
				    (lanes[newLane].points[newCurrentWaypointOnCar-counter].reservationID != 0 
				 && lanes[newLane].points[newCurrentWaypointOnCar-counter].reservationID != myID)
				    ||
				    (initialIndex+counter >= lanes[currentLane].points.Length-1 
				 || lanes[currentLane].points.Length ==0)
				    || lanes[newLane].points[newCurrentWaypointOnCar-counter].roadBlockAhead)
				{
					if (reservedChangeLanePoints.Count > reservedPoints.Count)
					{
						reservedPoints = reservedChangeLanePoints;
					}
					reservedChangeLanePoints.Clear();
					return;
				} 
				

				if (segDistance < carControllerAI.maxLockaheadDistanceFullStop)
				{
					actualPointTouse = counter;
				}
				if (segDistance < carControllerAI.carDepth*2f+3f){
					if (lanes[currentLane].points[initialIndex+counter].reservationID ==0 
					    || lanes[currentLane].points[initialIndex+counter].reservationID == myID){
						newPoint1.connector = -1;
						newPoint1.lane = currentLane;
						newPoint1.point = initialIndex+counter;//lanes[currentLane].points[initialIndex+counter] ;
							Point(newPoint1).reservationID = myID;
							Point(newPoint1).carwhoReserved = carControllerAI;
						reservedChangeLanePoints.Enqueue(newPoint1);
					}
				}
				segDistance+=lanes[newLane].points[newCurrentWaypointOnCar-counter].distanceToNextPoint;
				counter++;
			}
			counter--;
//			if (counter ==0)return;
			UnreserveAll(false, true);
			reservedPoints.Clear();
			for (i = 0; i < counter;i++)
			{
				lanes[newLane].points[newCurrentWaypointOnCar-i].reservationID = myID;
				lanes[newLane].points[newCurrentWaypointOnCar-i].carwhoReserved = carControllerAI;
				newPoint.connector = -1;
				newPoint.lane = newLane;
				newPoint.point = newCurrentWaypointOnCar-i;//lanes[newLane].points[newCurrentWaypointOnCar-i];
				reservedPoints.Enqueue(newPoint);
			}

			if (counter == 0) {
				lanes[newLane].points[newCurrentWaypointOnCar].reservationID = myID;
				lanes[newLane].points[newCurrentWaypointOnCar].carwhoReserved = carControllerAI;
				newPoint.lane = newLane;
				newPoint.connector = -1;
				newPoint.point = newCurrentWaypointOnCar;//lanes[newLane].points[newCurrentWaypointOnCar];
				reservedPoints.Enqueue(newPoint);
			}

			overTaking = true;
			changingLane = true;
			overtakingFromLeft = !left;
			currentLane = newLane;
			waypoints = lanes [newLane].points;
			lastWaypoints = waypoints;
			lastLaneIndex = currentLane;
			lastConnectorIndex = -1;

			int tempInt= newCurrentWaypointOnCar - counter/2;
			if (tempInt >= lastWaypoints.Length)
				tempInt = lastWaypoints.Length-1;
			if (tempInt <0)tempInt = 0;

			currentWaypoint = newCurrentWaypointOnCar -actualPointTouse;
			currentWaypointOnCar = newCurrentWaypointOnCar;
			previousWaypointSteer = currentWaypoint;
			carControllerAI.previousWaypointCurve = newCurrentWaypointOnCar-counter;

			wasTravelingOnConnector = false;
			travelingOnConector = false;
			lastLane = 0;

			RelativeWaypointPositionOnCar =  carControllerAI.RearPoint.InverseTransformPoint(Point(P).point + pointOffset);
			relativeWPosMagnitude = 0;
			carControllerAI.nextPathIndex = 0;
			carControllerAI.segDistance = segDistance-carControllerAI.carDepth-3;
			carControllerAI.currenPoint = lastWaypoints[carControllerAI.previousWaypointCurve];
			carControllerAI.currentConnector = null;
			carControllerAI.lane = 0;
			carControllerAI.brakeSpeeds.Clear();// = new List<TSTrafficAI.TSBrakeSpeeds>();
			selectedConnector = 0;
			lastSelectedConnector = null;
			starting = true;
			nextTrackPath.Clear();
			currentMaxSpeed = Random.Range(lanes [newLane].maxSpeed/2f,lanes [newLane].maxSpeed);
			return;
		}
		return;
	}
	public void TurnOffTurningLights()
	{
		isTurning = false;
		if (carControllerAI.OnTurnLeft != null)
			carControllerAI.OnTurnLeft(false);
		if (carControllerAI.OnTurnRight != null)
			carControllerAI.OnTurnRight(false);
	}
	
	
	public void UnreserveAll(bool all, bool checkExistence)
	{
		if (all)
		{
			changingLane = false;
			while (reservedChangeLanePoints.Count >0){
				if (Point(reservedChangeLanePoints.Peek()).reservationID == myID){
					Point(reservedChangeLanePoints.Peek()).reservationID = 0;
					Point(reservedChangeLanePoints.Peek()).carwhoReserved = null;
				}
				reservedChangeLanePoints.Dequeue();
			}
			reservedChangeLanePoints.Clear();
			overTaking = false;
		}

		while(reservedConnectors.Count >0)
		{
			UnReserveNearConnectorPoints(reservedConnectors.Peek());
			reservedConnectors.Dequeue();
		}

		for(int i = 0; i < occupiedLanes.Count;i++)
		{
			lanes[occupiedLanes[i]].totalOcupation -= Mathf.Round( (carOcupation)/lanes[occupiedLanes[i]].totalDistance*100f);
			if (lanes[occupiedLanes[i]].totalOcupation < 0) lanes[occupiedLanes[i]].totalOcupation = 0;
		}
		occupiedLanes.Clear();

		int pointCounter =0;
		while (reservedPoints.Count >0)
		{
			if (Point(reservedPoints.Peek()).reservationID == myID && (!checkExistence
			    || (checkExistence && pointCounter >= reservedChangeLanePoints.Count)))
			{
				Point(reservedPoints.Peek()).reservationID = 0;
				Point(reservedPoints.Peek()).carwhoReserved = null;

			}
			pointCounter++;
			if (reservedPoints.Peek().connector != -1)
				UnReserveNearConnectorPoints(Point(reservedPoints.Peek()));
			reservedPoints.Dequeue();
		}
		reservedPoints.Clear();
		carControllerAI.nextPathIndex = 0;
		carControllerAI.segDistance = 0;
		totalDistance = 0;
		nextTrackIndex =0;

		nextTrackPath.Clear();
	}

    /// <summary>
    /// Resets the route.  This method when is called gets the actual path cleared and a new path is build.
    /// </summary>
    public void ResetRoute()
    {
        TSNextLaneSelection nextPath = nextTrackPath[carControllerAI.nextPathIndex];
        nextPath.nextConnector = GetNextConnector(nextTrackPath[carControllerAI.nextPathIndex].nextLane);
        nextTrackPath[carControllerAI.nextPathIndex] = nextPath;

        if (nextTrackPath.Count-1 > carControllerAI.nextPathIndex)
        {
            while(nextTrackPath.Count - 1 > carControllerAI.nextPathIndex)
            {
                if (nextTrackPath[nextTrackPath.Count - 1].isConnector)
                {
                    totalDistance -= lanes[nextTrackPath[nextTrackPath.Count - 1].nextLane].connectors[nextTrackPath[nextTrackPath.Count - 1].nextConnector].totalDistance;
                }
                else
                {
                    totalDistance -= lanes[nextTrackPath[nextTrackPath.Count - 1].nextLane].totalDistance;
                }
                nextTrackPath.RemoveAt(nextTrackPath.Count - 1);
            }
        }
        
        wasConnector = false;
        TurnOffTurningLights();
        AddNextTrackToPath();
    }


    bool wasConnector = false;
	float totalDistance = 0f;
	float dist;
	public void AddNextTrackToPath()
	{
		
		dist = Mathf.Max(carControllerAI.maxLockaheadDistanceFullStop,carControllerAI.LookAheadDistance) * carControllerAI.reservePointDistanceMultiplier+carControllerAI.carDepth+2;
		
		if (totalDistance < 0) totalDistance = 0;
		if (nextTrackPath.Count >0 && lanes[nextTrackPath[nextTrackPath.Count-1].nextLane].connectors.Length == 0)return;
		while (totalDistance < dist)
		{
			if (starting)
			{
				wasConnector = true;
				starting = false;
				nextTrack.isConnector = false;
				nextTrack.nextLane = currentLane;
				nextTrackIndex = 1;
				//Debug.Log(currentLane+ " "+ lanes.Count + " " + name);
				if (currentWaypointOnCar> lanes[currentLane].points.Length)currentWaypointOnCar = lanes[currentLane].points.Length-1;
				totalDistance = lanes[currentLane].totalDistance;
				for (int i =0; i < currentWaypointOnCar;i++)
				{
					totalDistance -= lanes[currentLane].points[i].distanceToNextPoint;
				}
			}else{
				if (!wasConnector)
				{
					if (nextTrackPath.Count >0){
						wasConnector = true;
						if (lanes[nextTrackPath[nextTrackPath.Count-1].nextLane].connectors.Length ==0)return;
						nextTrack.nextLane = lanes[nextTrackPath[nextTrackPath.Count-1].nextLane].connectors[nextTrackPath[nextTrackPath.Count-1].nextConnector].nextLane;
						nextTrack.isConnector = false;
					}else {
						wasConnector = true;
						nextTrack.nextLane = lanes[currentLane].connectors[selectedConnector].nextLane;
						nextTrack.isConnector = false;
					}
					totalDistance += lanes[nextTrack.nextLane].totalDistance;
				}else
				{
					wasConnector =false;
					if (nextTrackPath.Count >0){
						nextTrack.nextLane = nextTrackPath[nextTrackPath.Count-1].nextLane;
						nextTrack.nextConnector = GetNextConnector(nextTrackPath[nextTrackPath.Count-1].nextLane);
						
					}else
					{
						
						nextTrack.nextLane = currentLane;
						nextTrack.nextConnector = GetNextConnector(currentLane);
					}
					nextTrack.isConnector = true;
					if (lanes[nextTrack.nextLane].connectors.Length ==0)return;
					totalDistance += lanes[nextTrack.nextLane].connectors[nextTrack.nextConnector].totalDistance;
				}
			}
			nextTrackPath.Add(nextTrack);
		}
	}




	public void AddNextTrackToPath(List<TSNextLaneSelection> newPath)
	{
		starting = true;
		if (totalDistance < 0) totalDistance = 0;
		nextTrackPath.Clear();
		for (int ii = 0; ii < newPath.Count;ii++)
		{
			if (starting)
			{
				wasConnector = true;
				starting = false;
				nextTrack.isConnector = false;
				nextTrack.nextLane = newPath[ii].nextLane;
				nextTrackIndex = 1;
				if (currentWaypointOnCar> lanes[currentLane].points.Length)currentWaypointOnCar = lanes[currentLane].points.Length-1;
				totalDistance = lanes[nextTrack.nextLane].totalDistance;
				for (int i =0; i < currentWaypointOnCar;i++)
				{
					totalDistance -= lanes[currentLane].points[i].distanceToNextPoint;
				}
			}else{
				if (!wasConnector)
				{
					if (nextTrackPath.Count >0){
						wasConnector = true;
						if (lanes[nextTrackPath[nextTrackPath.Count-1].nextLane].connectors.Length ==0)return;
						nextTrack.nextLane = lanes[nextTrackPath[nextTrackPath.Count-1].nextLane].connectors[nextTrackPath[nextTrackPath.Count-1].nextConnector].nextLane;
						nextTrack.isConnector = false;
					}else {
						wasConnector = true;
						nextTrack.nextLane = lanes[currentLane].connectors[selectedConnector].nextLane;
						nextTrack.isConnector = false;
					}
					totalDistance += lanes[nextTrack.nextLane].totalDistance;
				}else
				{
					wasConnector =false;
					if (nextTrackPath.Count >0){
						nextTrack.nextLane = newPath[ii].nextLane;
						nextTrack.nextConnector = newPath[ii].nextConnector;
						
					}else
					{
						
						nextTrack.nextLane = currentLane;
						nextTrack.nextConnector = GetNextConnector(currentLane);
					}
					nextTrack.isConnector = true;
					if (lanes[nextTrack.nextLane].connectors.Length ==0)return;
					totalDistance += lanes[nextTrack.nextLane].connectors[nextTrack.nextConnector].totalDistance;
				}
			}
			nextTrackPath.Add(nextTrack);
		}
	}






	int totalConnectors = 0;
	float maxInt = 0;
	int index = 0;

	int GetNextConnector(int lane){
		totalConnectors = lanes[lane].connectors.Length;
		maxInt = float.MaxValue;
		index = 0;
		for (int i = 0; i <totalConnectors;i++)
		{
			if (!(((((int)lanes[lanes[lane ].connectors[i].nextLane].vehicleType) & (1<<(int)carControllerAI.myVehicleType))) > 0) || !(((((int)lanes[lane ].connectors[i].vehicleType) & (1<<(int)carControllerAI.myVehicleType))) > 0))
			{
				lanes[lane ].connectors[i].randomWeight = float.MaxValue - Random.Range(1,lanes[lane ].connectors.Length+1);
			}else
			{
				lanes[lane ].connectors[i].randomWeight =  (lanes[lanes[lane ].connectors[i].nextLane].totalOcupation)* 100 * Random.Range(1f,lanes[lane ].connectors.Length*Random.Range(1f,100f));
			}
			if (lanes[lane ].connectors[i].randomWeight < maxInt && lanes[lanes[lane ].connectors[i].nextLane].totalOcupation < lanes[lanes[lane ].connectors[i].nextLane].maxTotalOcupation){
				maxInt = lanes[lane].connectors[i].randomWeight;
				index = i;
			}
		}
		return index;
	}

	void OnDrawGizmosSelected ()
	{
		if (!Application.isPlaying)return;
		Gizmos.color = Color.green;
		foreach (TSReservedPoints resP in reservedPoints)
		{
			if (Point(resP).carwhoReserved == null && Point(resP).reservationID ==0)Gizmos.color = Color.blue;
			else 
				if (Point(resP).carwhoReserved != carControllerAI)Gizmos.color = Color.red;
			else
				Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(Point(resP).point + pointOffset,carControllerAI.carWidth);
		}
		Gizmos.color = Color.yellow;
		foreach (TSReservedPoints resP in reservedChangeLanePoints)
		{
			if (Point(resP).carwhoReserved == null && Point(resP).reservationID ==0)Gizmos.color = Color.cyan;
			else 
				if (Point(resP).carwhoReserved != carControllerAI)Gizmos.color = Color.magenta;
			else
				Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Point(resP).point + pointOffset,carControllerAI.carWidth);
		}
		Gizmos.color = Color.red;
		foreach(TSLaneConnector connector in reservedConnectors)
		{
			Gizmos.DrawLine(connector.conectorA + pointOffset,connector.conectorB + pointOffset);
		}

		foreach (TSNextLaneSelection nextLane in nextTrackPath)
		{
			if (nextLane.isConnector)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(lanes[nextLane.nextLane].connectors[nextLane.nextConnector].conectorA + pointOffset,lanes[nextLane.nextLane].connectors[nextLane.nextConnector].conectorB + pointOffset);
			}
			else{
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine(lanes[nextLane.nextLane].conectorA + pointOffset,lanes[nextLane.nextLane].conectorB+pointOffset);
			}

		}
		Gizmos.DrawSphere(waypoints[currentWaypoint].point + pointOffset,carControllerAI.carWidth);

	}

}
