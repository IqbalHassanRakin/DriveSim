
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class TSTrafficSpawner : MonoBehaviour
{
	[System.Serializable]
	public class TransformsSpawningCheck{
		public Transform transform;
		public float initialNotSpawningRadius = 10f;
		float sqrRadius=0;
		float lastinitialNotSpawningRadius =0;
		public float InitialNotSpawningRadiusSQR {
			get {
				if (initialNotSpawningRadius != lastinitialNotSpawningRadius)
				{	
					sqrRadius = initialNotSpawningRadius * initialNotSpawningRadius;
					lastinitialNotSpawningRadius = initialNotSpawningRadius;
				}

				return sqrRadius;
			}
		}
	}

	public class OtherSpawners{
		public TSTrafficSpawner spawnerReference;
		public bool isInRange = false;
	}

	[System.Serializable]
	public class TSSpawnVehicles
	{
		public GameObject cars;
		public int frequency = 1;
	}
	
	public struct PointsIndex
	{
		public int lane;
		public int point;

		public PointsIndex(int l, int p)
		{
			lane = l;
			point = p;
		}
	}

	#region public members

	/// <summary>
	/// The initialize on start.  If enabled the spawner would be initialized when the Awake is called.  If not it wont be initialized and
	/// The spawner would need to be initialized by script.
	/// </summary>
	public bool initializeOnStart = true;

	/// <summary>
	/// The cars.  This is the array that contains the source traffic cars that would be used to spawn on the scene.
	/// </summary>
	[SerializeField]
	public TSSpawnVehicles[] cars;

	/// <summary>
	/// The total amount of cars.  This would be the max amount of cars the pool would have.
	/// </summary>
	public int totalAmountOfCars = 50;

	/// <summary>
	/// The amount of cars that would  be on the scene.  This is the maximum amount of cars on the scene at the same time.
	/// </summary>
	public int amount = 50;

	/// <summary>
	/// The max distance from the spawner object the cars are spawned into, if the traffic cars are farther from this
	/// distance the cars would be disabled and respawned within this distance
	/// </summary>
	public float maxDistance = 150f;

	/// <summary>
	/// The offset.  This is the offset to make the area for spawning cars, it is the max distance minus this offset what
	/// makes the spawning area or radius
	/// </summary>
	public float offset = 140f;

	/// <summary>
	/// The closer range.  This is to make a callback triggered when the traffic car or pedestrians is near the spawning object
	/// from certain distance, usefull to activate other stuff on the traffic car or AI
	/// </summary>
	public float closerRange = 25f;

	/// <summary>
	/// The refresh time for spawning cars.
	/// </summary>
	public float refreshTime = 0.02f;

	/// <summary>
	/// The manager system which contains all the lanes info.
	/// </summary>
	public TSMainManager manager;


	
	/// <summary>
	/// The unused cars position.
	/// </summary>
	public Vector3 unusedCarsPosition = new Vector3 (50000, 50000, 50000);

	/// <summary>
	/// The respawn if flipped.
	/// </summary>
	public bool respawnIfUpSideDown = false;

	/// <summary>
	/// The respawn up side down timer.
	/// </summary>
	public float respawnUpSideDownTime = 2f;

	/// <summary>
	/// The respawn altitude.  This is the altitude from the spawning point the car would get spanwed to.
	/// </summary>
	public float respawnAltitude = 0.3f;

	/// <summary>
	/// The disbale multi threading.
	/// </summary>
	public bool disableMultiThreading = false;

	/// <summary>
	/// The global point offset.  This would be the offset of the world with respect to the origin, this is useful in case you are in the need of having to shift the worlds game objects position
	/// to avoid floating precision issues on the physics.  The cars wont get moved automatically by the spawner to the new offset, this needs to be done separately.
	/// </summary>
	public Vector3 globalPointOffset = Vector3.zero;

	/// <summary>
	/// The cars checked per frame.  This would be the amount of cars that would be checked to see if they are outside the spawning are to despawn them
	/// </summary>
	public int carsCheckedPerFrame = 50;

	/// <summary>
	/// The initial not spawning radius.  This would be a radius that the spawner would check aroung its own position to avoid spanwing cars initially over the center or inside this
	/// radius, so you would be able to avoid cars to get spawned into the players car.
	/// </summary>
	public float initialNotSpawningRadius = 10f;

	/// <summary>
	/// The transform spawning check.  The spawner would check against all the listed Transforms to avoid spawning cars from their position with the radius given for each transform.
	/// </summary>
	[SerializeField] 
	public TransformsSpawningCheck[] transformSpawningCheck;

	#endregion

	#region private members

	/// <summary>
	/// The traffic volumes.
	/// </summary>
	private TSTrafficVolume[] trafficVolumes;

	/// <summary>
	/// The traffic cars reference.
	/// </summary>
	private TSTrafficAI[] trafficCars;



	/// <summary>
	/// The traffic cars transform reference.
	/// </summary>
	private Transform[] trafficCarsTransform;

	/// <summary>
	/// The traffic cars positions vector.
	/// </summary>
	private Vector3[] trafficCarsPositions;

	/// <summary>
	/// The traffic cars far indexes.
	/// </summary>
	private bool[] trafficCarsFarIndexes;

	/// <summary>
	/// The index of the points.
	/// </summary>
	PointsIndex[] pointsIndex = new PointsIndex[100];

	/// <summary>
	/// The next action time.
	/// </summary>
	private float nextActionTime = 0f;

	/// <summary>
	/// My position.
	/// </summary>
	private Vector3 myPosition = Vector3.zero;
	// Use this for initialization

#if UNITY_EDITOR || (!UNITY_METRO && !UNITY_WP8)

	/// <summary>
	/// The threads.
	/// </summary>
	private System.Threading.Thread[] threads;

	/// <summary>
	/// The job available.
	/// </summary>
	private AutoResetEvent[] jobAvailable;

	/// <summary>
	/// The thread idle.
	/// </summary>
	private ManualResetEvent[] threadIdle;

#endif

	#if !UNITY_EDITOR && (UNITY_WP8 || UNITY_METRO)
	LegacySystem.Thread[] threads;
#endif

	/// <summary>
	/// The threads count.
	/// </summary>
	private int threadsCount = 0;

	/// <summary>
	/// The close.
	/// </summary>
	private bool close = false;

	/// <summary>
	/// The lock2.  This is used to sync the threads
	/// </summary>
	private Object lock2 = new Object ();



	/// <summary>
	/// The current volume.
	/// </summary>
	private int currentVolume = 0;

	/// <summary>
	/// The trafficar last added.
	/// </summary>
	private int trafficarLastAdded = 0;

	/// <summary>
	/// The total far cars.
	/// </summary>
	private int _totalFarCars = 0;



	/// <summary>
	/// The max distance SQR max.
	/// </summary>
	private float maxDistanceSQRMax = 0f;

	/// <summary>
	/// The max distance SQR minimum.
	/// </summary>
	private float maxDistanceSQRMin = 0f;

	private Transform trafficCarsParent;

	public static TSTrafficSpawner mainInstance;


	private OtherSpawners[] otherSpawners;
	private bool otherSpawnersPresent = false;

	private TSEventTrigger[] eventTriggers;
	private bool weHaveEventTriggers = false;
	private bool _Initialized = false;
	#endregion


	#region properties

	/// <summary>
	/// Gets or sets the max distance.  Use this for changing the spawner max distance at runtime by script, since
	/// this distance is converted into square distance for better performance calculations.
	/// </summary>
	/// <value>The max distance.</value>
	public float MaxDistance {
		get {
			return maxDistance;
		}
		set {
			maxDistance = value;
			maxDistanceSQRMax = (maxDistance+offset)*(maxDistance+offset);
			maxDistanceSQRMin = (maxDistance-offset)*(maxDistance-offset);
		}
	}

	/// <summary>
	/// Gets or sets the offset.  Use this for changing the spawner offset at runtime by script, since this offset is used 
	/// to talculate the min distance in squared values for better perfomance calculations.
	/// </summary>
	/// <value>The offset.</value>
	public float Offset {
		get {
			return offset;
		}
		set {
			offset = value;
			maxDistanceSQRMax = (maxDistance+offset)*(maxDistance+offset);
			maxDistanceSQRMin = (maxDistance-offset)*(maxDistance-offset);
		}
	}

	/// <summary>
	/// Gets the traffic cars.
	/// </summary>
	/// <value>The traffic cars.</value>
	public TSTrafficAI[] TrafficCars {
		get {
			return trafficCars;
		}
	}

	/// <summary>
	/// Gets the total far cars.
	/// </summary>
	/// <value>The total far cars.</value>
	public int totalFarCars {
		get {
			return _totalFarCars;
		}
	}

	/// <summary>
	/// Gets the traffic cars transform.
	/// </summary>
	/// <value>The traffic cars transform.</value>
	public Transform[] TrafficCarsTransform {
		get {
			return trafficCarsTransform;
		}
	}

	/// <summary>
	/// Gets the traffic cars positions.
	/// </summary>
	/// <value>The traffic cars positions.</value>
	public Vector3[] TrafficCarsPositions {
		get {
			return trafficCarsPositions;
		}
	}

	public static float RespawnUpSideDownTime{
		get{
			return mainInstance.respawnUpSideDownTime;
		}
	}

	/// <summary>
	/// Gets or sets the amount.
	/// </summary>
	/// <value>The amount.</value>
	public int Amount {
		get {
			return amount;
		}
		set {
			amount = value;
		}
	}

    public Transform TrafficCarsParent
    {
        get
        {
            return trafficCarsParent;
        }
    }

  
    #endregion properties

    void Awake ()
	{

        if (initializeOnStart)
            InitializeMe();
        else
        {
            trafficCars = new TSTrafficAI[0];
            trafficCarsPositions = new Vector3[0];
            trafficCarsFarIndexes = new bool[0];
        }
        //Initialize the threads and start a coroutine if there is only 1 thread.
        Initialize();
    }

	public void InitializeMe()
	{
		if (this.enabled && !_Initialized){
			myPosition = transform.position;
			GetEventTriggers();
			GetOtherSpawners();
			mainInstance = this;
			if (manager == null)
				manager = GameObject.FindObjectOfType (typeof(TSMainManager))as TSMainManager;
			trafficCarsParent = new GameObject("TrafficCarsContainer").transform;
			PopulateInitialPoints();
			AddCarsStart();
			_Initialized = true;
		}
	}

	void GetEventTriggers()
	{
		eventTriggers = GameObject.FindObjectsOfType<TSEventTrigger>();
		if (eventTriggers != null && eventTriggers.Length !=0)
			weHaveEventTriggers = true;
		for (int i =0; i < eventTriggers.Length; i++)
		{
			eventTriggers[i].InitializeMe();
		}
	}

	void GetOtherSpawners(){
		TSTrafficSpawner[] tempSpawners = GameObject.FindObjectsOfType<TSTrafficSpawner>();
		otherSpawners = new OtherSpawners[tempSpawners.Length-1];
		int otherSpCounter =0;
		if (tempSpawners.Length <=1) return;
		otherSpawnersPresent = true;
		for (int i = 0; i < tempSpawners.Length;i++)
		{
			if (tempSpawners[i] != this)
			{
				otherSpawners[otherSpCounter] = new OtherSpawners();
				otherSpawners[otherSpCounter].spawnerReference = tempSpawners[i];
				otherSpCounter++;
			}
		}

	}


	void PopulateInitialPoints()
	{
		while (currentPointIndex < pointsIndex.Length)
		{
			for (int ii =0; ii < manager.lanes.Length; ii++)
			{
				for (int iii = 0; iii< manager.lanes[ii].points.Length;iii++)
				{
					pointsIndex[currentPointIndex] = new PointsIndex(ii,iii);
				}
			}
			currentPointIndex++;
		}
		currentPointIndex = 0;
	}

	/// <summary>
	/// Adds the cars at start.  this methods tries to add all the cars available at the start of the scene
	/// if the cars cant be spawned at the first attempt, they get on hold.
	/// </summary>
	void AddCarsStart ()
	{
		if (totalAmountOfCars < amount)totalAmountOfCars = amount;
		//Get the spawning area squared for using it later.
		maxDistanceSQRMax = (maxDistance+offset)*(maxDistance+offset);
		maxDistanceSQRMin = (maxDistance-offset)*(maxDistance-offset);

		//Get the traffic volumes if they are available
		trafficVolumes = GameObject.FindObjectsOfType (typeof(TSTrafficVolume)) as TSTrafficVolume[];




		//Eventriggers initialization
		int evenTriggersCounter =0;
		if (weHaveEventTriggers)evenTriggersCounter = eventTriggers.Length;
		int currentEnventTrigger =0;

		//Variables initialization
		bool dontCreateAgain = false;
		GameObject car = null;
		int selectedCar = 0;
		int pointIndex = 0;
		int laneIndex = 0;
		Bounds bounds = new Bounds ();
		float carLength = 0;
		int frequencyAmount = 0;
		for (int i = 0; i < cars.Length;i++)
		{
			frequencyAmount+= cars[i].frequency;
		}
		int tempAmount = (totalAmountOfCars < cars.Length ? (cars.Length < frequencyAmount + cars.Length?frequencyAmount+cars.Length:cars.Length) : (totalAmountOfCars < frequencyAmount+cars.Length?frequencyAmount+cars.Length:totalAmountOfCars));
		trafficCars = new TSTrafficAI[tempAmount];
		trafficCarsTransform = new Transform[tempAmount];
		trafficCarsFarIndexes = new bool[tempAmount];
		trafficCarsPositions = new Vector3[tempAmount];



		bool gotAllFrequency = false;
		int frequencyIndex = 0;
		//Spawning the cars loop
		for (int i = 0; i < tempAmount; i++) 
		{

			if (!dontCreateAgain) {
				if (amount < cars.Length)
					selectedCar = i;
				else {

					if (!gotAllFrequency)
					{
						bool gotOne = false;
						while(!gotOne){
							gotOne = GetCarByFrequency(ref frequencyIndex,out selectedCar);
						}
						if (frequencyIndex >= cars.Length){
							gotAllFrequency = true;

						}
					}
					else{
						selectedCar = Random.Range (0, cars.Length);
					}
				}
				selectedCar = Mathf.Clamp(selectedCar,0,cars.Length-1);
				car = Instantiate (cars [selectedCar].cars) as GameObject;
				car.transform.parent = trafficCarsParent;
				bounds = CarSize (car);
				laneIndex = Random.Range (0, manager.lanes.Length - 1);
				carLength = bounds.size.z + 3;
				pointIndex = Random.Range (0, manager.lanes [laneIndex].points.Length - 1);
			}
			if (pointIndex >= manager.lanes [laneIndex].points.Length - carLength) {
				laneIndex = Random.Range (0, manager.lanes.Length - 1);
				pointIndex = Random.Range (0, manager.lanes [laneIndex].points.Length - 1);
			}




			TSTrafficAI AI = car.GetComponent<TSTrafficAI> ();

			//code for checking the triggers
			if (weHaveEventTriggers)
			{
				while(currentEnventTrigger < evenTriggersCounter && !eventTriggers[currentEnventTrigger].spawnCarOnStartingPoint )
				{
					currentEnventTrigger++;
				}
				if (currentEnventTrigger < evenTriggersCounter)
				{
					laneIndex = eventTriggers[currentEnventTrigger].startingPoint.lane;
					pointIndex = eventTriggers[currentEnventTrigger].startingPoint.point;
				}
			}


			float segDistance = pointIndex > 0 ? manager.lanes[laneIndex].points [pointIndex - 1].distanceToNextPoint - manager.lanes [laneIndex].points [pointIndex].distanceToNextPoint : 0;

			int pointIndexOffset = 0;
			bool checkFree = true;

			if ((myPosition - manager.lanes[laneIndex].points[pointIndex].point + globalPointOffset).magnitude < initialNotSpawningRadius)
				checkFree = false;
			if (!CheckAgainstTransformList(manager.lanes[laneIndex].points[pointIndex].point + globalPointOffset))
				checkFree = false;

			if (i >= amount )checkFree = false;
			if (!CheckTrafficVolume (out currentVolume, manager.lanes [laneIndex].points [pointIndex].point + globalPointOffset))
				checkFree = false;
			if (!((((int)manager.lanes [laneIndex].vehicleType) & (1 << (int)AI.myVehicleType)) > 0))
				checkFree = false;
			while (segDistance < carLength + (carLength/2f)) {
				segDistance += manager.lanes [laneIndex].points [pointIndex + pointIndexOffset].distanceToNextPoint;
				pointIndexOffset++;
				if (pointIndex + pointIndexOffset > manager.lanes [laneIndex].points.Length - 1 
				    || manager.lanes [laneIndex].totalOcupation > Mathf.Round(manager.lanes [laneIndex].trafficDensity*100f)) {
					checkFree = false;
					break;
				}
			}
			TSNavigation nav = car.GetComponent<TSNavigation> ();
			nav.lanes = manager.lanes;
			for (int y = 0; y < pointIndexOffset; y++) {
				if (checkFree && manager.lanes [laneIndex].points [pointIndex + y].reservationID != 0)
					checkFree = false;
			}
			if (checkFree) {
				if (currentVolume != -1)
					trafficVolumes [currentVolume].carsOnThisSection.Add (AI);
				for (int y = 0; y < pointIndexOffset; y++) {
					manager.lanes [laneIndex].points [pointIndex + y].reservationID = nav.GetInstanceID ();
					manager.lanes [laneIndex].points [pointIndex + y].carwhoReserved = AI;
					TSNavigation.TSReservedPoints newPoint = new TSNavigation.TSReservedPoints ();
					newPoint.point = pointIndex + y;//manager.lanes [laneIndex].points [pointIndex + y];
					newPoint.lane = laneIndex;
					newPoint.connector = -1;
					nav.reservedPoints.Enqueue (newPoint);
				}
				Quaternion tempDir = Quaternion.LookRotation (((manager.lanes [laneIndex].points [pointIndex + pointIndexOffset / 2 + 1].point + globalPointOffset) - (manager.lanes [laneIndex].points [pointIndex + pointIndexOffset / 2].point + globalPointOffset)));
				car.transform.position = manager.lanes [laneIndex].points [pointIndex + pointIndexOffset / 2].point + globalPointOffset + Vector3.up*respawnAltitude ;
				car.transform.rotation = tempDir;
				AddTrafficCar (ref AI);
				trafficCarsPositions [i] = (AI.transform.position);
				nav.lanes = manager.lanes;
				nav.wasTravelingOnConnector = false;
				nav.travelingOnConector = false;
				nav.lastLane = nav.currentLane;
				nav.RelativeWaypointPosition = Vector3.zero;
				nav.RelativeWaypointPositionOnCar = Vector3.zero;
				nav.relativeWPosMagnitude = 0;
				AI.nextPathIndex = 0;
				AI.segDistance = 0f;
				AI.currentConnector = null;
				AI.lane = 0;
				AI.brakeSpeeds.Clear();// = new List<TSTrafficAI.TSBrakeSpeeds> ();
				nav.selectedConnector = 0;
				nav.lastSelectedConnector = null;
				nav.starting = true;
				nav.currentWaypoint = pointIndex + pointIndexOffset - 1;
				nav.currentLane = laneIndex;
				nav.currentWaypointOnCar = pointIndex;
				nav.previousWaypointSteer = pointIndex;
				nav.waypoints = manager.lanes [laneIndex].points;
				nav.lastWaypoints = nav.waypoints;
				nav.lastLaneIndex = laneIndex;
				nav.lastConnectorIndex = -1;
				nav.currentMaxSpeed = manager.lanes [laneIndex].maxSpeed;
				AI.carDepth = bounds.size.z;	
				AI.carWidth = bounds.size.x;
				AI.carHeight = bounds.size.y;
				AI.previousWaypointCurve = pointIndex + pointIndexOffset;
				AI.currenPoint = nav.lastWaypoints [AI.previousWaypointCurve];
				nav.AddNextTrackToPath ();
				dontCreateAgain = false;
				AI.Enable (true);
				nav.GetLaneMaxSpeed();
				nav.SetCarOccupationLength();
				nav.lanes [nav.currentLane].totalOcupation += Mathf.Round((nav.CarOcupationLenght) / nav.lanes [nav.currentLane].totalDistance*100f);
				nav.AddOcupiedLane(nav.currentLane);
				trafficCarsFarIndexes [i] = false;
				AssignCarToEvenTrigger(AI,currentEnventTrigger);
			} else {
				nav.lanes = manager.lanes;
				AddTrafficCar (ref AI);
				trafficCarsPositions [i] = (AI.transform.position);
				AI.carDepth = bounds.size.z;	
				AI.carWidth = bounds.size.x;
				AI.carHeight = bounds.size.y;
				nav.SetCarOccupationLength();
				AI.Enable (false);
				AI.transform.position = unusedCarsPosition;
				trafficCarsFarIndexes [i] = true;
				_totalFarCars++;
				dontCreateAgain = false;
			}
			currentEnventTrigger++;
		}


		
	}

	bool CheckAgainstTransformList(Vector3 point)
	{
		if (transformSpawningCheck != null && transformSpawningCheck.Length >0)
		{
			for (int i =0; i < transformSpawningCheck.Length;i++)
			{
				if ((transformSpawningCheck[i].transform.position - point).sqrMagnitude < transformSpawningCheck[i].InitialNotSpawningRadiusSQR)
					return false;
			}
		}
		return true;
	}

	void AssignCarToEvenTrigger(TSTrafficAI car, int currentEnventTrigger)
	{
		if (weHaveEventTriggers)
		{
			if (currentEnventTrigger < eventTriggers.Length)
			{
				eventTriggers[currentEnventTrigger].SetCar(car);
			}
		}
	}

//	Coroutine checkNearLanesThread;
	void OnEnable()
	{
		if (threadsCount ==0)
		{
			StopCoroutine(CheckNearLanesSingleThread());
			StartCoroutine(CheckNearLanesSingleThread());
		}
	}

	int currentAmountOfCar = 0;
	bool GetCarByFrequency(ref int currentIndex, out int returnedIndex)
	{
		returnedIndex = 0;
		if (currentIndex < cars.Length){
			if (cars[currentIndex].frequency !=0 && currentAmountOfCar < cars[currentIndex].frequency )
			{
				returnedIndex = currentIndex;
				currentAmountOfCar++;
				return true;
			}else {
				currentIndex++;
				currentAmountOfCar =0;
				return false;
			}
		}else{
			return true;
		}
	}


	/// <summary>
	/// Adds the traffic car.
	/// </summary>
	/// <param name="AI">A.</param>
	void AddTrafficCar (ref TSTrafficAI AI)
	{
		trafficCars [trafficarLastAdded] = (AI);
		trafficCarsTransform [trafficarLastAdded] = AI.transform;
		trafficarLastAdded++;
	}

	/// <summary>
	/// Checks the traffic volume.  This method checks if a spawning point is inside a traffic volume and
	/// checks if there is room for more cars according to the volume settings and remaning cars slot.
	/// </summary>
	/// <returns><c>true</c>, if traffic volume was checked, <c>false</c> otherwise.</returns>
	/// <param name="volume">Volume.</param>
	/// <param name="point">Point.</param>
	bool CheckTrafficVolume (out int volume, Vector3 point)
	{
		volume = -1;
		for (int i = 0; i < trafficVolumes.Length; i++) {
			if (trafficVolumes [i].GetComponent<Collider>().bounds.Contains (point)) {
				if (trafficVolumes [i].carsOnThisSection.Count < trafficVolumes [i].maxAllowedCars) {
					volume = i;
					return true;
				} else {
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Gets the Car size.
	/// </summary>
	/// <returns>The size.</returns>
	/// <param name="car">Car.</param>
	Bounds CarSize (GameObject car)
	{	
		Bounds bounds;
		Quaternion temprotation = car.transform.rotation;
		Vector3 tempPosition = car.transform.position;
		car.transform.rotation = Quaternion.Euler (Vector3.zero);
		car.transform.position = Vector3.zero;
		bounds = new Bounds (car.transform.position, Vector3.zero);
		Collider[] renderers = car.GetComponentsInChildren<Collider> ();
		
		foreach (Collider renderer in renderers) {
			if (!renderer.isTrigger)
				bounds.Encapsulate (renderer.bounds);
		}
		car.transform.rotation = temprotation;
		car.transform.position = tempPosition;
		return bounds;
	}



	void Update ()
	{
		//*************JUST TESTING CODE
//        if (Input.GetKeyUp(KeyCode.A))
//            InitializeMe();
		//*************JUST TESTING CODE

		UpdateCarPositions ();
		myPosition = transform.position;
		if (Time.time - nextActionTime > refreshTime) {
			nextActionTime = Time.time;
			CheckFarCarsSingleThread ();
			AddCar();
		}
	}

	/// <summary>
	/// Updates the car positions.
	/// </summary>
	void UpdateCarPositions ()
	{
		for (int i = 0; i < trafficCars.Length; i++) {
			trafficCarsPositions [i] = trafficCarsTransform [i].position;
		}
	}

	/// <summary>
	/// The index of the lane.
	/// </summary>
	int laneIndex1 = 0;
	int pointIndex1 =0;
	/// <summary>
	/// The points index2.
	/// </summary>


	int currentPointIndex = 0;
	int checkNearLanesTimer = 0;
	/// <summary>
	/// Checks the near lanes.
	/// </summary>
	/// <param name="maxTicks">Max ticks.</param>
	void CheckNearLanes ()
	{
//		int timer = 0;
		for ( ; laneIndex1 < manager.lanes.Length;laneIndex1++ ) {

			for (; pointIndex1 < manager.lanes[laneIndex1].points.Length; pointIndex1++) {

				float distance3 =  ((manager.lanes [laneIndex1].points [pointIndex1].point + globalPointOffset)- myPosition).sqrMagnitude;
				if (distance3 > maxDistanceSQRMin && distance3 < maxDistanceSQRMax && manager.lanes [laneIndex1].points [pointIndex1].reservationID ==0) 
				{
					if (pointIndex1+3 < manager.lanes[laneIndex1].points.Length &&
					    manager.lanes[laneIndex1].points[pointIndex1+1].reservationID ==0 &&
					    manager.lanes[laneIndex1].points[pointIndex1+2].reservationID ==0 &&
					    manager.lanes[laneIndex1].points[pointIndex1+3].reservationID ==0)
					{
                        if (currentPointIndex >= pointsIndex.Length) currentPointIndex = 0;
                        lock (lock2){
							pointsIndex[currentPointIndex].lane = laneIndex1;
							pointsIndex[currentPointIndex].point = pointIndex1;
						}
						currentPointIndex++;
						
					}
				}
				if (checkNearLanesTimer > 500)
				{
					checkNearLanesTimer = 0;
					return;
				}
			}
			pointIndex1 =0;
		}
		if (laneIndex1 >= manager.lanes.Length) laneIndex1 =0;
	}

	/// <summary>
	/// Checks the far cars.  This method checks the cars distance and put them on the far cars array
	/// so the system can disable them and respawn on another near point.
	/// </summary>
	void CheckFarCars ()
	{
		for (int i = 0; i < trafficCarsPositions.Length; i++) {
			trafficCars[i].NavigateToWaypoints.pointOffset = globalPointOffset;
			float distance =  (trafficCarsPositions [i]- myPosition).sqrMagnitude;
			if (distance > maxDistanceSQRMax * 1.2f || (respawnIfUpSideDown && trafficCars[i].ForcedRespawn) || !trafficCars[i].enabled) {

				if (!trafficCarsFarIndexes [i] && !CheckFarCarsOnOtherSpawners(trafficCarsPositions [i])){
					_totalFarCars++;
					trafficCarsFarIndexes [i] = true;
				}
				
			} else{
				if (trafficCarsFarIndexes [i]){
					_totalFarCars--;
					trafficCarsFarIndexes [i] = false;
				}
			}
			if (distance > closerRange*closerRange)
			{
				if (trafficCars [currentFarCar].OnFarRange != null)
					trafficCars [currentFarCar].OnFarRange();
			}else
			{
				if (trafficCars [currentFarCar].OnCloserRange != null)
					trafficCars [currentFarCar].OnCloserRange();
			}
		}
	}

	bool CheckFarCarsOnOtherSpawners(Vector3 carPosition){
		
		if (!otherSpawnersPresent)return false;  //Early exit if there is no other spawner on the scene
		for (int i =0; i < otherSpawners.Length;i++)
		{
			if ((carPosition - otherSpawners[i].spawnerReference.myPosition).sqrMagnitude < otherSpawners[i].spawnerReference.maxDistanceSQRMax)
				return true;
		}
		return false;
	}





	/// <summary>
	/// Checks the near lanes (single thread).
	/// </summary>
	/// <returns>The near lanes single thread.</returns>
	IEnumerator CheckNearLanesSingleThread ()
	{
		int time = 0;
		while (true) {
			for (int w = 0; w < manager.lanes.Length; w++) {
				for (int i = 0; i < manager.lanes[w].points.Length; i++) {
					float distance3 =  ((manager.lanes [w].points [i].point + globalPointOffset) - myPosition).sqrMagnitude;
					if (distance3 > maxDistanceSQRMin && distance3 < maxDistanceSQRMax && manager.lanes [w].points [i].reservationID ==0) {
                        if (currentPointIndex >= pointsIndex.Length) currentPointIndex = 0;
                        pointsIndex[currentPointIndex].lane = w;
						pointsIndex[currentPointIndex].point = i;
						currentPointIndex++;
						
					} 
					time ++;
					if (time > 250) {
						time = 0;
						yield return null;
					}
				}
			}
		}
	}

	/// <summary>
	/// The current far car.
	/// </summary>
	int currentFarCar = 0;

	/// <summary>
	/// The time.
	/// </summary>
	int time = 0;

	/// <summary>
	/// Checks the far cars (single thread).
	/// </summary>
	void CheckFarCarsSingleThread ()
	{
		for ( ; currentFarCar < trafficCarsPositions.Length; currentFarCar++) {
			trafficCars[currentFarCar].NavigateToWaypoints.pointOffset = globalPointOffset;
			if (time > carsCheckedPerFrame) {
				time = 0;
				return;
			}
			float distance = (trafficCarsPositions [currentFarCar]- myPosition).sqrMagnitude;
			if (!trafficCars[currentFarCar].reservedForEventTrigger &&
				(distance > maxDistanceSQRMax*1.2f  
			    || (respawnIfUpSideDown && trafficCars[currentFarCar].ForcedRespawn)
			    || !trafficCarsTransform[currentFarCar].gameObject.activeSelf)) 
			{
				if (!trafficCarsFarIndexes [currentFarCar] && !CheckFarCarsOnOtherSpawners(trafficCarsPositions [currentFarCar])){
					trafficCarsFarIndexes [currentFarCar] = true;
					_totalFarCars++;
					trafficCars [currentFarCar].NavigateToWaypoints.UnreserveAll (true, false);
					trafficCars [currentFarCar].Enable (false);
					trafficCars [currentFarCar].ForcedRespawn = false;
					trafficCarsTransform [currentFarCar].position = unusedCarsPosition;
				}
			}else{ 
				if (trafficCarsFarIndexes [currentFarCar]){
					_totalFarCars--;
					trafficCarsFarIndexes [currentFarCar] = false;
				}
			}
			if (distance > closerRange*closerRange)
			{
				if (trafficCars [currentFarCar].OnFarRange != null)
					trafficCars [currentFarCar].OnFarRange();
			}else
			{
				if (trafficCars [currentFarCar].OnCloserRange != null)
					trafficCars [currentFarCar].OnCloserRange();
			}
			time++;
		}
		currentFarCar = 0;
		time = 0;
	}

	/// <summary>
	/// Checks the far cars loop.
	/// </summary>
	/// <param name="obj">Object.</param>
//	void CheckFarCarsLoop (object obj)
//	{
//		while (true) {
//			CheckFarCars ();
//			if (close)
//				break;
//		}
//	}

	/// <summary>
	/// Checks the near lanes (loop - Multithreading).
	/// </summary>
	/// <param name="obj">Object.</param>
	void CheckNearLanesLoop (object obj)
	{
		while (true) {
			CheckNearLanes ();

#if (UNITY_EDITOR || (!UNITY_METRO && !UNITY_WP8 && !NETFX_CORE))
			Thread.Sleep(0);
#else
			LegacySystem.Thread.Sleep(0);
#endif

			if (close)
				break;
		}
	}

	/// <summary>
	/// Checks the both.
	/// </summary>
	/// <param name="obj">Object.</param>
//	void CheckBoth (object obj)
//	{
//		while (true) {
//			CheckNearLanes ();
//#if (UNITY_EDITOR || (!UNITY_METRO && !UNITY_WP8 && !NETFX_CORE))
//			Thread.Sleep(0);
//#else
//			LegacySystem.Thread.Sleep(0);
//#endif
//			if (close)
//				break;
//		}
//	}

	/// <summary>
	/// Removes the car.  This method makes a specific car to drop any locked points on the lanes.
	/// </summary>
	/// <param name="carIndex">Car index.</param>
	void RemoveCar (int carIndex)
	{
		trafficCars [carIndex].NavigateToWaypoints.UnreserveAll (true, false);
		
	}

	/// <summary>
	/// Gets the index of the next far car.
	/// </summary>
	/// <returns>The next car far index.</returns>
	int GetNextCarFarIndex()
	{
		int i = Random.Range(0,trafficCarsFarIndexes.Length);
		int counter = 0;
		while(counter < trafficCarsFarIndexes.Length)
		{
			if (trafficCarsFarIndexes[i])
				return i;
			i++;
			if (i >= trafficCarsFarIndexes.Length)i=0;
			counter++;
		}
		return -1;
	}


	/// <summary>
	/// Gets the index of the next far car next to a specific index.
	/// </summary>
	/// <returns>The next car far index.</returns>
	/// <param name="currentIndex">Current index.</param>
	int GetNextCarFarIndex(int currentIndex)
	{
		for (int i = currentIndex; i < trafficCarsFarIndexes.Length;i++)
		{
			if (trafficCarsFarIndexes[i])
				return i;
		}
		return -1;
	}

	/// <summary>
	/// A.
	/// </summary>
	TSTrafficAI AI;

	/// <summary>
	/// The nav.
	/// </summary>
	TSNavigation nav;

	/// <summary>
	/// Adds the car.  This method is responsible for the car spawning on the lanes that are closer to the spawner object
	/// </summary>
	void AddCar ()
	{
		int nextCarFar = GetNextCarFarIndex();
		{
			if (nextCarFar == -1 || amount ==0 ) {
				goto a1;
			}
			{
				if (trafficCars.Length - _totalFarCars > amount || _totalFarCars < 1)
					goto a1;
				int pointer = 0;
				int pointIndex = 0;
				int laneIndex =  0;
				lock(lock2){
					pointer = Random.Range (0, pointsIndex.Length - 1);
					pointIndex = pointsIndex[ pointer].point;
					laneIndex = pointsIndex[ pointer].lane;
				}
				bool checkFree = true;
				if (!CheckAgainstTransformList(manager.lanes[laneIndex].points[pointIndex].point + globalPointOffset))
					checkFree = false;

				if (!CheckTrafficVolume (out currentVolume, manager.lanes [laneIndex].points [pointIndex].point + globalPointOffset))
					checkFree = false;
				if (CheckFarCarsOnOtherSpawners(manager.lanes [laneIndex].points [pointIndex].point + globalPointOffset))
					checkFree = false;
				int carToSpawn = nextCarFar;
				float segDistance = pointIndex > 0 ? manager.lanes [laneIndex].points [pointIndex - 1].distanceToNextPoint - manager.lanes [laneIndex].points [pointIndex].distanceToNextPoint : 0;
				int pointIndexOffset = 0;
				
				int tFarIndex = nextCarFar;
				AI = trafficCars [carToSpawn];
				float distanceToCompare = maxDistanceSQRMin;
				float pointDistance = (this.transform.position - (manager.lanes [laneIndex].points [pointIndex].point + globalPointOffset)).sqrMagnitude; 
				if (pointDistance < distanceToCompare)
					goto a1;
				if (pointDistance > maxDistanceSQRMax)
					goto a1;
				int counter = 0;
				while (counter < _totalFarCars && checkFree) {
					carToSpawn = tFarIndex;
					AI = trafficCars [carToSpawn];
					
					if (((((int)manager.lanes [laneIndex].vehicleType) & (1 << (int)AI.myVehicleType)) > 0)) {
						break;
					} else {
						tFarIndex = GetNextCarFarIndex(tFarIndex);
					}
					
					counter++;
					if (tFarIndex ==-1 || counter >= _totalFarCars)
						checkFree = false;
					
				}
				if (!checkFree)goto a1;
				nav = trafficCars [carToSpawn].NavigateToWaypoints;
				float carLength = trafficCars [carToSpawn].carDepth;
				if (manager.lanes [laneIndex].points [pointIndex].reservationID != 0 ||
				    manager.lanes [laneIndex].points [pointIndex].carwhoReserved != null) {
					goto a1;
				}
				while (segDistance < carLength+(carLength/2f)) {
					pointIndexOffset++;
					if (pointIndex + pointIndexOffset >= manager.lanes [laneIndex].points.Length - 1 
					    || manager.lanes [laneIndex].totalOcupation > Mathf.Round(manager.lanes [laneIndex].trafficDensity*100f)) {
						checkFree = false;
						goto a1;
					}
					segDistance += manager.lanes [laneIndex].points [pointIndex + pointIndexOffset].distanceToNextPoint;
					if (manager.lanes [laneIndex].points [pointIndex + pointIndexOffset].reservationID != 0 ||
					    manager.lanes [laneIndex].points [pointIndex + pointIndexOffset].carwhoReserved != null) {
						goto a1;
					}


				}
				if (checkFree && pointIndex + pointIndexOffset < manager.lanes [laneIndex].points.Length) {
					checkFree = false;
					if (currentVolume != -1)
						trafficVolumes [currentVolume].carsOnThisSection.Add (AI);
					if (nav.lanes.Length == 0)
						nav.lanes = manager.lanes;
					Quaternion tempDir = Quaternion.LookRotation (((manager.lanes [laneIndex].points [pointIndex + pointIndexOffset / 2 + 1].point + globalPointOffset) - (manager.lanes [laneIndex].points [pointIndex + pointIndexOffset / 2].point + globalPointOffset)));

					for (int i =0; i < AI.bodies.Length;i++){
						AI.bodies[i].velocity = Vector3.zero;
						AI.bodies[i].angularVelocity = Vector3.zero;
						AI.bodies[i].transform.localRotation = Quaternion.identity;
						AI.bodies[i].transform.localPosition = Vector3.zero;
					}
					trafficCars [carToSpawn].transform.position = (manager.lanes [laneIndex].points [pointIndex + pointIndexOffset / 2].point + globalPointOffset) + Vector3.up*respawnAltitude ;
					trafficCars [carToSpawn].transform.rotation = tempDir;
					trafficCarsFarIndexes[carToSpawn] = false;
					_totalFarCars--;
					this.trafficCarsPositions [carToSpawn] = trafficCars [carToSpawn].transform.position;
					
					for (int y = 0; y < pointIndexOffset; y++) {
						manager.lanes [laneIndex].points [pointIndex + y].reservationID = nav.myID;
						manager.lanes [laneIndex].points [pointIndex + y].carwhoReserved = AI;
						TSNavigation.TSReservedPoints newPoint = new TSNavigation.TSReservedPoints ();
						newPoint.point = pointIndex + y;//manager.lanes [laneIndex].points [pointIndex + y];
						newPoint.lane = laneIndex;
						newPoint.connector = -1;
						nav.reservedPoints.Enqueue (newPoint);
					}
					
					nav.currentWaypoint = pointIndex + pointIndexOffset;
					nav.currentLane = laneIndex;
					nav.currentWaypointOnCar = pointIndex;
					nav.previousWaypointSteer = pointIndex;
					AI.previousWaypointCurve = pointIndex + pointIndexOffset;
					nav.waypoints = manager.lanes [laneIndex].points;
					nav.wasTravelingOnConnector = false;
					nav.travelingOnConector = false;
					nav.lastLane = nav.currentLane;
					nav.lastWaypoints = nav.waypoints;
					nav.lastLaneIndex = laneIndex;
					nav.RelativeWaypointPosition = Vector3.zero;
					nav.RelativeWaypointPositionOnCar = Vector3.zero;
					nav.relativeWPosMagnitude = 0;
					AI.nextPathIndex = 0;
					AI.segDistance = 0f;
					AI.currenPoint = nav.lastWaypoints [AI.previousWaypointCurve];
					AI.currentConnector = null;
					AI.lane = 0;
					AI.brakeSpeeds.Clear();
					nav.selectedConnector = 0;
					nav.lastSelectedConnector = null;
					nav.starting = true;
					nav.currentMaxSpeed = manager.lanes [laneIndex].maxSpeed;
					nav.AddNextTrackToPath ();
					nav.lanes [nav.currentLane].totalOcupation += Mathf.Round((nav.CarOcupationLenght) / nav.lanes [nav.currentLane].totalDistance*100f);
					nav.AddOcupiedLane(nav.currentLane);
					nav.GetLaneMaxSpeed();
					trafficCars [carToSpawn].Enable (true);
					
				}
			}
		}
	a1:
			return;
	}

	/// <summary>
	/// Initialize the threads if they are available.
	/// </summary>
	private void Initialize ()
	{
		threadsCount = System.Environment.ProcessorCount;
		if (disableMultiThreading)
			threadsCount = 0;
		//No point starting new threads for a single core computer
		if (threadsCount <= 1) {
			//						notThreading = true;
			threadsCount = 0;
			return;
		}
		threadsCount = Mathf.Clamp (threadsCount, 0, 1);
#if (UNITY_EDITOR || (!UNITY_METRO && !UNITY_WP8 && !NETFX_CORE))
		// array of events, which signal about available job
		jobAvailable = new AutoResetEvent[1];
		// array of events, which signal about available thread
		threadIdle = new ManualResetEvent[1];
		// array of threads
		threads = new Thread[1];

		jobAvailable [0] = new AutoResetEvent (false);
		threadIdle [0] = new ManualResetEvent (true);
		threads [0] = new Thread (new ParameterizedThreadStart (CheckNearLanesLoop));
		threads [0].IsBackground = false;
		threads [0].Start (0);
		#else
		threads = new LegacySystem.Thread[1];
		threads[0] = new LegacySystem.Thread(new LegacySystem.ParameterizedThreadStart(CheckNearLanesLoop));
		threads [0].Start (0);
		#endif
	}

	/// <summary>
	/// Close any threads if any.
	/// </summary>
	public void Close ()
	{
#if (UNITY_EDITOR || (!UNITY_METRO && !UNITY_WP8 && !NETFX_CORE))
		//Exit all threads
		for (int i = 0; i < threadsCount; i++)
			jobAvailable [i].Set ();

#endif
	}
	
	void OnDisable ()
	{

		close = true;
	}

	void OnDestroy()
	{
		close = true;
	}

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (transform.position, maxDistance + offset);
		Gizmos.DrawWireSphere (transform.position, maxDistance - offset);
	}



}
