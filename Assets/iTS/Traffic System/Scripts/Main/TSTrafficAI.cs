using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent (typeof(TSNavigation))]

/// <summary>
/// TS traffic A.  This class is repsonsible for controlling the AI cars on the race
/// </summary>
public class TSTrafficAI : MonoBehaviour
{
	//This is for debugging
	public Color myColor;

	#region properties

	public struct TSBrakeSpeeds{
		public float distance;
		public TSPoints point;
	}




	/// <summary>
	/// The navigate to waypoints.
	/// </summary>
	private TSNavigation _navigateToWaypoints;

	public TSNavigation NavigateToWaypoints {
		get {
			return _navigateToWaypoints;
		}
	}

	/// <summary>
	/// Gets the look ahead distance.
	/// </summary>
	/// <value>The look ahead distance.</value>
	public float LookAheadDistance {
		get {
			return _lookAheadDistance;
		}
	}

	/// <summary>
	/// Gets the wheels center.
	/// </summary>
	/// <value>The wheels center.</value>
	public Transform WheelsCenter {
		get {
			return _wheelsCenter;
		}
	}

	/// <summary>
	/// Gets the car speed.
	/// </summary>
	/// <value>The car speed.</value>
	public float carSpeed {
		get {
			return _carSpeed;
		}
	}
    #endregion

    #region public members
    //Public Variables	

    /// <summary>
    /// The throttle time, this would be a value that the higher it is the more time it takes to apply throttle.
    /// </summary>
    public float throttleTime = 0.6f;

    /// <summary>
    /// The throttle release time, this would be a value that the higher it is the more time it takes to release the throttle.
    /// </summary>
    public float throttleReleasetime = 0.1f;

    /// <summary>
    /// The brake time, this would be a value that the higher it is the more time it takes to apply the brakes.
    /// </summary>
    public float brakeTime = 0.6f;

    /// <summary>
    /// The brake release time, this would be a value that the higher it is the more time it takes to release the brakes.
    /// </summary>
    public float brakeReleaseTime = 0.1f;

	/// <summary>
	/// The max lockahead distance.
	/// </summary>
	[HideInInspector]
	public float maxLockaheadDistance = 0f;

	/// <summary>
	/// The max lockahead distance for full stop.
	/// </summary>
	[HideInInspector]
	public float maxLockaheadDistanceFullStop = 0f;
	
	/// <summary>
	/// The LOOKAHEA d_ FACTOR.  Anticipation for turning on curves
	/// </summary>
	public float LOOKAHEAD_FACTOR = 0.33f;
	
	/// <summary>
	/// The front wheels.  Add only the front wheels here.
	/// </summary>
	public Transform[] frontWheels;

	/// <summary>
	/// The LOOKAHEAD_CONST.
	/// </summary>
	public float LOOKAHEAD_CONST = 2.0f;
	
	/// <summary>
	/// The bodies.
	/// </summary>
	[HideInInspector]
	public Rigidbody[] bodies;
	
	/// <summary>
	/// The car depth.
	/// </summary>
	[HideInInspector]
	public float carDepth = 0.0f;
	
	/// <summary>
	/// The width of the car.
	/// </summary>
	[HideInInspector]
	public float carWidth = 0.0f;
	
	/// <summary>
	/// The height of the car.
	/// </summary>
	[HideInInspector]
	public float carHeight = 0.0f;

	/// <summary>
	/// The type of the my vehicle.
	/// </summary>
	public TSLaneInfo.VehicleType myVehicleType = TSLaneInfo.VehicleType.Light;

	/// <summary>
	/// The brake speeds.  For internal use only.
	/// </summary>
	[HideInInspector]
	public List<TSBrakeSpeeds> brakeSpeeds = new List<TSBrakeSpeeds>(100);

	/// <summary>
	/// The curren point.  For internal use only.
	/// </summary>
	[HideInInspector]
	public TSPoints currenPoint = new TSPoints ();

	/// <summary>
	/// The current connector.  For internal use only.
	/// </summary>
	[HideInInspector]
	public TSLaneConnector currentConnector = null;

	/// <summary>
	/// The next connector.  For internal use only.
	/// </summary>
	[HideInInspector]
	public TSLaneConnector nextConnectorInstance = null;

	/// <summary>
	/// The previous waypoint curve.
	/// </summary>
	[HideInInspector]
	public int previousWaypointCurve = 0;

	/// <summary>
	/// The segment distance.  This is for internal use only.
	/// </summary>
	[HideInInspector]
	public float segDistance = 0f;

	/// <summary>
	/// The index of the next path.  This is for internal use only.
	/// </summary>
	[HideInInspector]
	public int nextPathIndex = 0;

	/// <summary>
	/// The lane.  This is for internal use only.
	/// </summary>
	[HideInInspector]
	public int lane = 0;

	/// <summary>
	/// The change lane time.    This is for internal use only.
	/// </summary>
	[HideInInspector]
	public float changeLaneTime = 0f;

	/// <summary>
	/// The player sensor.
	/// </summary>
	public BoxCollider playerSensor;

	/// <summary>
	/// The player sensor width multiplier.
	/// </summary>
	public float playerSensorWidthMultiplier = 1.05f;

	/// <summary>
	/// The minimum player sensor lenght.
	/// </summary>
	public float minPlayerSensorLenght = 5f;

	/// <summary>
	/// The player sensor length multiplier.  This would be the multiplier of the calculated max distance for the car to make a full stop
	/// This could  be useful for making the player dectection distance greater.
	/// </summary>
	public float playerSensorLengthMultiplier = 1f;

    /// <summary>
    /// Set this to true if you want the player sensor length to be of an static length.
    /// </summary>
	public bool staticPlayerSensorLenght = false;

    /// <summary>
    /// The Reserve Point Distance Multiplier, this is the factor that would be multiplied by the maximum braking distance that would be used to be reserving the points
    /// the greater this value, the greater would be the distance the car would have reserved points when travelling in the road network.
    /// </summary>
    public float reservePointDistanceMultiplier = 2.5f;

    public float minConnectorRequestDistance = 50f;

	/// <summary>
	/// The lenght margin.  This would be the distance to keep from the next car or pedestrian.
	/// </summary>
	[HideInInspector]
	public float lengthMargin = 2f;

	/// <summary>
	/// The length margin minimum.  This is to be used for making the lenght margin be different among the cars, to give a bit more of
	/// realism
	/// </summary>
	public float lengthMarginMin = 1.5f;

	/// <summary>
	/// The length margin maximum.  This is to be used for making the lenght margin be different among the cars, to give a bit more of
	/// realism
	/// </summary>
	public float lengthMarginMax = 3f;

	public float lengthMarginJunctions = 5f;

	/// <summary>
	/// My lane offset minimum.  The minimum offset that this vehicle could have, this is good to give some variation to the
	/// Vehicles position on the lane, so they don't look like they all follow exactly the same line.
	/// </summary>
	public float myLaneOffsetMin = 0f;

	/// <summary>
	/// My lane offset maximum.  The maximum offset that this vehicle could have, this is good to give some variation to the
	/// Vehicles position on the lane, so they don't look like they all follow exactly the same line.
	/// </summary>
	public float myLaneOffsetMax = 0f;

	/// <summary>
	/// The player tag.  This would be used to compare against any object that enters the player sensor trigger
	/// to check if the car needs to react to it.
	/// </summary>
	public string playerTag = "Player";

	/// <summary>
	/// The horn sound clip.  This is the audioSource used for playing the audioClip (randomg if more than one if the playerDetected
	/// Array has more than one clip) on the  of this traffic car when a player or obstacle is blocking their way and are facing them.
	/// This is useful for making the cars sound a horn sound, or for  making people say some complain comments.
	/// </summary>
	public AudioSource playerDetectedSoundAudioSource;

	/// <summary>
	/// The horn sound clips.
	/// </summary>
	public AudioClip[] playerDetectedSoundClips = new AudioClip[0];

	/// <summary>
	/// The minimum horn time.  This is the minimum time interval between clips plays.
	/// </summary>
	public float minplayerDetectedSoundTime = 0.2f;

	/// <summary>
	/// The maximum horn time.  This is the maximum time interval between clips plays.
	/// </summary>
	public float maxplayerDetectedSoundTime = 0.5f;

	/// <summary>
	/// The player detected animator.
	/// </summary>
	public Animator playerDetectedAnimator;

	/// <summary>
	/// The player detected states.
	/// </summary>
	public string[] playerDetectedStates;

	/// <summary>
	/// The minplayer detected animator time.
	/// </summary>
	public float minplayerDetectedAnimatorTime = 0.5f;

	/// <summary>
	/// The maxplayer detected animator time.
	/// </summary>
	public float maxplayerDetectedAnimatorTime = 0.5f;

	/// <summary>
	/// The player detected animation controller.
	/// </summary>
	public Animation playerDetectedAnimationController;

	/// <summary>
	/// The player detected animations.
	/// </summary>
	public string[] playerDetectedAnimations;

	/// <summary>
	/// The minplayer detected animation time.
	/// </summary>
	public float minplayerDetectedAnimationTime = 0.5f;

	/// <summary>
	/// The maxplayer detected animation time.
	/// </summary>
	public float maxplayerDetectedAnimationTime = 0.5f;

	[HideInInspector]
	public bool reservedForEventTrigger = false;

	/// <summary>
	/// The ignore traffic light.
	/// </summary>
	[HideInInspector]
	public bool ignoreTrafficLight = false;

	public float minDistanceToOvertake = 15f;

	public float minBrakingDistRoadblock = 10f;


	/// <summary>
	/// The crashed.  This would get set to true if the car have crashed
	/// </summary>
	[HideInInspector]
	public bool crashed = false;

	#endregion






	// get Methods

	#region Get Methods

	/// <summary>
	/// Gets the current waypoint.
	/// </summary>
	/// <returns>The current waypoint.</returns>
	public int GetCurrentWaypoint ()
	{
			return _navigateToWaypoints.currentWaypoint;
	}


	/// <summary>
	/// Gets the name of the car.
	/// </summary>
	/// <returns>The car name.</returns>
	public string GetCarName ()
	{
			return myTransform.name.Replace ("(Clone)", "");
	}

	/// <summary>
	/// Gets the lookahead_factor.
	/// </summary>
	/// <returns>The lookahead_factor.</returns>
	public float GetLookahead_factor ()
	{
			return LOOKAHEAD_FACTOR;
	}


	/// <summary>
	/// Sets the lookahead_factor.
	/// </summary>
	/// <param name="Lookahead_factor">Lookahead_factor.</param>
	public void SetLookahead_factor (float Lookahead_factor)
	{
			LOOKAHEAD_FACTOR = Lookahead_factor;
	}

	#endregion



	#region private variables

	/// <summary>
	/// The look ahead distance.
	/// </summary>
	private float _lookAheadDistance = 0f;

	
	/// <summary>
	/// The players.
	/// </summary>
	private List<Transform> players = new List<Transform>();
	
	/// <summary>
	/// The car speed.
	/// </summary>
	private float _carSpeed = 0f;

	/// <summary>
	/// The MAXSPEED.
	/// </summary>
	private float MAXSPEED = float.MaxValue;


	
	/// <summary>
	/// The wheels center.
	/// </summary>
	private Transform _wheelsCenter;

	/// <summary>
	/// My transform.
	/// </summary>
	private Transform myTransform;

	/// <summary>
	/// The k friction.
	/// </summary>
	private float[] kFriction;

	/// <summary>
	/// The input torque.
	/// </summary>
	private float inputTorque = 0.0f;

	/// <summary>
	/// The current speed sqr.
	/// </summary>
	public float currentSpeedSqr { get; private set; }

	/// <summary>
	/// The c.
	/// </summary>
	private float c = 0.0f;


	//Inputs variables

	/// <summary>
	/// The brake.
	/// </summary>
	private float brake;

	/// <summary>
	/// The throttle.
	/// </summary>
	private float throttle;

	/// <summary>
	/// The steering.
	/// </summary>
	private float steering;



	/// <summary>
	/// The full stop.  This is enabled if there is an obstacle detected by this car.
	/// </summary>
	private bool fullStop = false;

    /// <summary>
    /// The timer for change lane.  The min time that have to pass between lane changes.
    /// </summary>
    [Range(0.1f,5f)]
    public float timerForChangeLane = 5f;

	/// <summary>
	/// The is up side down.
	/// </summary>
	private bool isUpSideDown = false;

	/// <summary>
	/// The forced respawn.
	/// </summary>
	private bool forcedRespawn = false;



	/// <summary>
	/// Up side down timer.
	/// </summary>
	private float upSideDownTimer = 0f;

	/// <summary>
	/// The width of the half car.
	/// </summary>
	private float halfCarWidth = 0f;

	/// <summary>
	/// The half car depth.
	/// </summary>
	private float halfCarDepth = 0;

	/// <summary>
	/// My lane offset.
	/// </summary>
	private float myLaneOffset = 0f;

	/// <summary>
	/// The player detected sound timming.
	/// </summary>
	private float playerDetectedSoundTimming =0f;

	/// <summary>
	/// The player detected sound next.
	/// </summary>
	private float playerDetectedSoundNext =0f;

	/// <summary>
	/// The player detected animator timming.
	/// </summary>
	private float playerDetectedAnimatorTimming =0f;

	/// <summary>
	/// The player detected animator next.
	/// </summary>
	private float playerDetectedAnimatorNext =0f;

	/// <summary>
	/// The player detected animation timming.
	/// </summary>
	private float playerDetectedAnimationTimming =0f;

	/// <summary>
	/// The player detected animation next.
	/// </summary>
	private float playerDetectedAnimationNext =0f;

	private bool canPlayPlayerDetectedAudio;
	private bool canPlayPlayerDetectedAnimator;
	private bool canPlayPlayerDetectedAnimation;

	private int currentLaneIndex;
	private int currentConnectorIndex;

	private Transform frontPoint;
	private Transform rearPoint;

	private bool _Initialized = false;

    bool otherCarPresentInJunction = false;
    #endregion



    #region delegates


    public delegate void OnUpdateAIDelegate(float steering, float brake, float throttle, bool isUpSideDown);
	public delegate void GetCarSpeedDelegate( out float carSpeed);
	public delegate void OnTurnRigthDelegate(bool isTurning);
	public delegate void OnTurnLeftDelegate(bool isTurning);
	public delegate void OnCloserRangeDelegate();
	public delegate void OnFarRangeDelegate();

	/// <summary>
	/// The on far range callback.  This is called if the car is in the far range.
	/// </summary>
	public OnFarRangeDelegate OnFarRange;

	/// <summary>
	/// The on closer range callback.  This is called if the car is on the closer range.
	/// </summary>
	public OnCloserRangeDelegate OnCloserRange;

	/// <summary>
	/// The on turn right callback.
	/// </summary>
	public OnTurnRigthDelegate OnTurnRight;

	/// <summary>
	/// The on turn left callback.
	/// </summary>
	public OnTurnLeftDelegate OnTurnLeft;

	/// <summary>
	/// The update car speed callback.
	/// </summary>
	public GetCarSpeedDelegate UpdateCarSpeed;

	/// <summary>
	/// The on update AI callback.
	/// </summary>
	public OnUpdateAIDelegate OnUpdateAI;

	#endregion


	#region Properties

	public bool earlyBrakePoint { get; private set; }

	public Transform RearPoint {
		get {
			return rearPoint;
		}
	}

	public Transform FrontPoint {
		get {
			return frontPoint;
		}
	}
	/// <summary>
	/// Gets a value indicating whether this instance is up side down.
	/// </summary>
	/// <value><c>true</c> if this instance is up side down; otherwise, <c>false</c>.</value>
	public bool IsUpSideDown {
		set{
			isUpSideDown = value;
		}
		get {
			return isUpSideDown;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="TSTrafficAI"/> forced respawn.
	/// </summary>
	/// <value><c>true</c> if forced respawn; otherwise, <c>false</c>.</value>
	public bool ForcedRespawn {
		set{
			forcedRespawn = value;
		}
		get {
			return forcedRespawn;
		}
	}

	#endregion

	// Initialize
	void Awake ()
	{
		//Early exit if it has been initialized already.
		if (_Initialized)return;
		//This is for debugging
		myColor = new Color(UnityEngine.Random.Range(0.0f,1.0f),UnityEngine.Random.Range(0.0f,1.0f),UnityEngine.Random.Range(0.0f,1.0f));
		myTransform = transform; 
		_navigateToWaypoints = gameObject.GetComponent<TSNavigation> ();
		IRDSWheelsCenter ();
		bodies = GetComponentsInChildren<Rigidbody>();
		myLaneOffset = UnityEngine.Random.Range (myLaneOffsetMin,myLaneOffsetMax);

		canPlayPlayerDetectedAudio = playerDetectedSoundAudioSource != null && (playerDetectedSoundClips.Length >0 || playerDetectedSoundAudioSource.clip != null);
		canPlayPlayerDetectedAnimator = playerDetectedAnimator != null && playerDetectedStates.Length >0;
		canPlayPlayerDetectedAnimation = playerDetectedAnimationController != null && playerDetectedAnimations.Length>0;
		lengthMargin = UnityEngine.Random.Range(lengthMarginMin, lengthMarginMax);
		SetCarSizeAndReferencePoints();
		CreatePlayerSensor();
	}

	void CreatePlayerSensor()
	{
		if (playerSensor != null && playerSensor.gameObject == this.gameObject)
		{		Destroy(playerSensor);
			GameObject newSensor = new GameObject("Player Sensor",typeof(BoxCollider));
			playerSensor = newSensor.GetComponent<BoxCollider>();
			playerSensor.transform.parent =(myTransform);
		}
		playerSensor.transform.localPosition = Vector3.zero;
		playerSensor.transform.localRotation = Quaternion.identity;
		playerSensor.isTrigger = true;
		Vector3 size = new Vector3 (carWidth*playerSensorWidthMultiplier, carHeight, minPlayerSensorLenght);
		playerSensor.size = size;
		playerSensor.center = new Vector3 (0, carHeight / 2f, minPlayerSensorLenght/2f);
	}
		
	void SetCarSizeAndReferencePoints()
	{
		Bounds sizeB = CarSize();
		carWidth = sizeB.size.x;
		carDepth = sizeB.size.z;
		carHeight = sizeB.size.y;
		frontPoint = new GameObject("frontPoint").transform;
		frontPoint.parent =(myTransform);
		rearPoint = new GameObject("rearPoint").transform;
		rearPoint.parent =(myTransform);
		frontPoint.localPosition = sizeB.center + new Vector3(0,0,sizeB.extents.z);
		frontPoint.localRotation = Quaternion.identity;
		rearPoint.localPosition = sizeB.center - new Vector3(0,0,sizeB.extents.z);
		rearPoint.localRotation = Quaternion.identity;
	}

	void Start ()
	{	
		//Early exit if it has been initialized already.
		if (_Initialized)return;
		_Initialized = true;
		halfCarWidth = carWidth/2f;
		halfCarDepth = carDepth/2f;
		kFriction = new float[2];
		kFriction[0] = 1f;
		kFriction[1] = 0.4f;
		c = kFriction [1] * -Physics.gravity.y;
//		InvokeRepeating("Update1",0,0.02f);
	}

	public void InitializeMe()
	{
		Awake();
		Start ();
	}


	void FixedUpdate ()
	{
		if (UpdateCarSpeed != null)
			UpdateCarSpeed(out _carSpeed);
		GeneralCalculations ();
		controllerAI ();
	}


	void controllerAI ()
	{
		//Steering Calculation for the AI cars
		steering = GetSteer (_navigateToWaypoints.waypoints [_navigateToWaypoints.currentWaypoint].point + _navigateToWaypoints.pointOffset, _navigateToWaypoints.waypoints [_navigateToWaypoints.previousWaypointSteer].point + _navigateToWaypoints.pointOffset, true);

		playerSensor.transform.localEulerAngles = new Vector3(0,steering * 45f,0);
//		playerSensor.transform.rotation = Quaternion.LookRotation(_navigateToWaypoints.waypoints [_navigateToWaypoints.currentWaypoint].point-myTransform.position);

		// Throttle/Brake
		float brake1 = GetBrake ();
		inputTorque = (Mathf.Clamp01 (GetAccel ()));
		if (inputTorque == 1)
			inputTorque += 1;
		if (inputTorque > throttle)
			throttle += Time.deltaTime / throttleTime;
		else
			throttle -= Time.deltaTime / throttleReleasetime;
		
		if (brake1 == 1)
			brake1 += 1;
		if (brake1 > brake)
			brake += Time.deltaTime / brakeTime;
		else
			brake -= Time.deltaTime / brakeReleaseTime;

		brake = Mathf.Clamp01 (brake1);
		throttle = Mathf.Clamp01 (throttle);

		CheckUpsideDown();

		if (OnUpdateAI!=null)
			OnUpdateAI(steering, brake, throttle, isUpSideDown);
	}

	/// <summary>
	/// The accel result.
	/// </summary>
	float accelResult = 0;

	/// <summary>
	/// Gets the accel.
	/// </summary>
	/// <returns>The accel.</returns>
	float GetAccel ()
	{	
		if (carSpeed < MAXSPEED - 2f)
			accelResult = 1f;
		else
			accelResult = 2 - carSpeed / MAXSPEED;
		if (brake != 0)
			accelResult =0;

		return accelResult;
	}

	/// <summary>
	/// The distance to run.
	/// </summary>
	float distanceToRun;

	/// <summary>
	/// The next connector distance.
	/// </summary>
	float nextConnectorDistance;



	/// <summary>
	/// Gets the max lock ahead distance.
	/// </summary>
	void GetMaxLockAheadDistance()
	{
		if (nextConnectorInstance != null && nextConnectorInstance.remainingGreenLightTime != -1)
		{
			distanceToRun = carSpeed * nextConnectorInstance.remainingGreenLightTime;
			float nextConnectorDistance = (((nextConnectorInstance.points[0].point + _navigateToWaypoints.pointOffset) - frontPoint.position).magnitude + nextConnectorInstance.totalDistance);

			if (distanceToRun < nextConnectorDistance)
				nextCarSpeedSqr =0;
		}
		maxLockaheadDistance = (((currentSpeedSqr - nextCarSpeedSqr) / (2.0f * c/*Mathf.Lerp (c, c / 2f, carSpeed / 10f)*/)));
		maxLockaheadDistanceFullStop = ((currentSpeedSqr) / (2.0f * c));
		if (maxLockaheadDistance < 0)
			maxLockaheadDistance =0;
		if (maxLockaheadDistanceFullStop < 0)
			maxLockaheadDistanceFullStop =0;
		maxLockaheadDistance += lengthMargin;
		maxLockaheadDistanceFullStop += halfCarDepth+lengthMargin;//_lookAheadDistance+1f,
	}


	/// <summary>
	/// Gets the max speed.
	/// </summary>
	void GetMaxSpeed()
	{
		if (_navigateToWaypoints.nextTrackPath.Count >0 && nextPathIndex!=-1)
		{
			MAXSPEED = _navigateToWaypoints.lanes [_navigateToWaypoints.nextTrackPath [nextPathIndex].nextLane].maxSpeed/3.6f;

		}
		else 
		{
			MAXSPEED = _navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].maxSpeed / 3.6f;
		}
		if (MAXSPEED*3.6f > _navigateToWaypoints.currentMaxSpeed)
			MAXSPEED =_navigateToWaypoints.currentMaxSpeed/3.6f;
	}

	/// <summary>
	/// Gets the brake for corners.
	/// </summary>
	void GetBrakeForCorners()
	{
		if (brakeSpeeds.Count >0){
			for(int i =0;i < brakeSpeeds.Count;i++){
				TSBrakeSpeeds newSPD = new TSBrakeSpeeds();
				newSPD.point = brakeSpeeds[i].point;
				newSPD.distance = (frontPoint.position - (newSPD.point.point + _navigateToWaypoints.pointOffset)).sqrMagnitude;
				brakeSpeeds[i] = newSPD;
				
				if ( brakeSpeeds[i].distance < sqrMaxLockaheadDistance && carSpeed > brakeSpeeds[i].point.maxSpeedLimit/3.6f){
					returningValue += (carSpeed-brakeSpeeds[i].point.maxSpeedLimit/3.6f)/5f;
				}
				if (brakeSpeeds[i].point.reservationID != _navigateToWaypoints.myID)brakeSpeeds.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// The next car speed sqr.
	/// </summary>
	float nextCarSpeedSqr = 0;
	float returningValue = 0;
	float sqrMaxLockaheadDistance = 0;
	private bool reservedConnector = false;	/// <summary>
	/// Gets the brake.
	/// </summary>
	/// <returns>The brake.</returns>
	float GetBrake()
	{

		returningValue = 0;
		GetMaxLockAheadDistance();
		
		sqrMaxLockaheadDistance = maxLockaheadDistanceFullStop*maxLockaheadDistanceFullStop;

		GetMaxSpeed();

		if (!staticPlayerSensorLenght){
			float sensorLenght = Mathf.Max(minPlayerSensorLenght,maxLockaheadDistanceFullStop*playerSensorLengthMultiplier);
			playerSensor.size = new Vector3 (playerSensor.size.x, playerSensor.size.y, sensorLenght);
			playerSensor.center = new Vector3 (0, playerSensor.center.y, sensorLenght/2f);
		}
		
		segDistance = segDistance < 0? 0:segDistance; 
		GetBrakeForCorners();

		bool stillMine = true;
//		bool enterOnce = true;

		if (fullStop){
			float pDistance = GetNearestPlayerDistance();
			if (pDistance < maxLockaheadDistanceFullStop * playerSensorLengthMultiplier+2f){
				returningValue += 1-((pDistance-maxLockaheadDistanceFullStop * playerSensorLengthMultiplier)/2f);
			}
		}
		float tempMaxLookAhead = (currenPoint.carwhoReserved==null?maxLockaheadDistanceFullStop:maxLockaheadDistance);
		float distanceChecking = minBrakingDistRoadblock+(carDepth>tempMaxLookAhead?carDepth:tempMaxLookAhead)* reservePointDistanceMultiplier;
		while (((segDistance  <= distanceChecking || _navigateToWaypoints.reservedPoints.Count<=2) && stillMine))
			//|| (enterOnce && (currenPoint.reservationID == 0  || currenPoint.reservationID == _navigateToWaypoints.myID ))) 
		{
            
//			enterOnce = false;

            
			//Check if we are overtaking on a countrary lane and try to get change back.
			if (_navigateToWaypoints.overTaking){
				if( Time.time - changeLaneTime > 1f)
				{
					changeLaneTime = Time.time;
					_navigateToWaypoints.LaneChange(false);
				}
			}
			if (_navigateToWaypoints.nextTrackPath.Count !=0){
				if (!GetCurrentPoint()){
					return returningValue;
				}
                
			} else {
				if (previousWaypointCurve >=0 && previousWaypointCurve < _navigateToWaypoints.lastWaypoints.Length)
				{
					currenPoint = _navigateToWaypoints.lastWaypoints [previousWaypointCurve];
					currentLaneIndex = _navigateToWaypoints.lastLaneIndex;
					currentConnectorIndex = _navigateToWaypoints.lastConnectorIndex;
				}

				else {
					return returningValue;
				}
			}
			//Check if the actual max speed is zero
			if (MAXSPEED == 0){
				returningValue += 1;
			}

			//Check if the car is going faster than the actual max speed
			if (carSpeed >= (_navigateToWaypoints.overTaking?MAXSPEED*1.2f:MAXSPEED)) {
				returningValue += (carSpeed-MAXSPEED)/5f;
			}
			if(!TryToReservePoint(ref stillMine))
			{
				break;
			}
		}

		CheckIfNeedsToBrake(stillMine);
		if (_navigateToWaypoints.overTaking && previousWaypointCurve < 25)
			returningValue +=1;
		return Mathf.Clamp01(returningValue);
	}

	void OnDrawGizmosSelected ()
	{
		if (!Application.isPlaying)return;
		Gizmos.color = currenPoint.reservationID==0?Color.green:Color.red;
		Gizmos.DrawCube(currenPoint.point,Vector3.one * carWidth);
	}


	/// <summary>
	/// Checks if needs to brake.
	/// </summary>
	void CheckIfNeedsToBrake(bool stillMine)
	{
		//If the currenpoint at this point of the code is not ours, it means that either
		//The distance was enough so we didnt needed to reserve more points, or there is
		//Another car/traffic light that is owning another point and we shouldn't keep going
		if ((currenPoint.reservationID != _navigateToWaypoints.myID && currenPoint.reservationID !=0 || !stillMine)
		    || fullStop)
		{

            bool overtake = false;
			Vector3 localPointPos =frontPoint.InverseTransformPoint(currenPoint.point + _navigateToWaypoints.pointOffset);
			float currentPointDistance = localPointPos.magnitude * Mathf.Sign(localPointPos.z);
			if (Mathf.Abs(localPointPos.x)>carDepth) currentPointDistance =Mathf.Abs(currentPointDistance);

            //Added on version 1.1.3, this would help on stopping correctly on the junctions if another car from another lane is
            //already crossing the junction to avoid crashes
            if (otherCarPresentInJunction)
            {
                currentPointDistance -= lengthMarginJunctions;
            }


            if ((!fullStop
                 && currenPoint.carwhoReserved != null))
//                 && ((currentConnector == null ) || (currentConnector != null
//                && !currentConnector.connectorReservedByTrafficLight))))
            {
				float otherCarDir =Vector3.Dot(myTransform.forward,currenPoint.carwhoReserved.myTransform.forward);
				nextCarSpeedSqr = currenPoint.carwhoReserved.nextCarSpeedSqr;// * otherCarDir;
				if (otherCarDir <=0 && _navigateToWaypoints.currentLane != currenPoint.carwhoReserved._navigateToWaypoints.lastLane) {
					nextCarSpeedSqr =0;
					maxLockaheadDistance = maxLockaheadDistanceFullStop+lengthMargin;
				}
	               
				if (currenPoint.carwhoReserved.earlyBrakePoint)
	            {
	            	nextCarSpeedSqr = 0;
	                maxLockaheadDistance = maxLockaheadDistanceFullStop+minDistanceToOvertake/2f;
	            }
				if(_navigateToWaypoints.overTaking && Time.time - changeLaneTime > 1f)
				{
					changeLaneTime = Time.time;
					_navigateToWaypoints.LaneChange(false);

						returningValue = 1;
					return;
				}
				if (currenPoint.carwhoReserved._navigateToWaypoints.overTaking){

						returningValue = 1;
					return;}
				float currentPointDistance1 = (currenPoint.carwhoReserved.RearPoint.position - frontPoint.position).magnitude;
				if (currentPointDistance1 < currentPointDistance )
					currentPointDistance = currentPointDistance1;
	               overtake = nextCarSpeedSqr < currentSpeedSqr && currentPointDistance < maxLockaheadDistance+lengthMargin;

           }
            else{ 
				nextCarSpeedSqr =0;
//				maxLockaheadDistance = maxLockaheadDistanceFullStop;
                overtake = true;
			}

			if (currenPoint.reservationID !=0 
			    || fullStop)
			{
				if (!_navigateToWaypoints.travelingOnConector 
				    && overtake && (fullStop 
				    || (currentPointDistance > minDistanceToOvertake  
				    && Time.time - changeLaneTime > timerForChangeLane)))
				{
					changeLaneTime = Time.time;
					if (_navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].points[_navigateToWaypoints.currentWaypoint].leftParalelLaneIndex != -1)
					{
						int nextLane = _navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].laneLinkLeft;
						if (_navigateToWaypoints.lanes[nextLane].laneLinkRight == _navigateToWaypoints.currentLane)
							_navigateToWaypoints.LaneChange(true);
						else if (!_navigateToWaypoints.overTaking && _navigateToWaypoints.lanes[nextLane].laneLinkLeft == _navigateToWaypoints.currentLane)
						{
							if (carSpeed < MAXSPEED && (currenPoint.carwhoReserved == null || !currenPoint.carwhoReserved._navigateToWaypoints.overTaking)){
								_navigateToWaypoints.OverTaking(true);
							}
						}
					}
					if (_navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].points[_navigateToWaypoints.currentWaypoint].rightParalelLaneIndex != -1)
					{
						int nextLane = _navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].laneLinkRight;
						if (_navigateToWaypoints.lanes[nextLane].laneLinkLeft == _navigateToWaypoints.currentLane)
							_navigateToWaypoints.LaneChange(false);
						else if (!_navigateToWaypoints.overTaking && _navigateToWaypoints.lanes[nextLane].laneLinkRight == _navigateToWaypoints.currentLane)
						{
							if (carSpeed < MAXSPEED  && (currenPoint.carwhoReserved == null || !currenPoint.carwhoReserved._navigateToWaypoints.overTaking)){
								_navigateToWaypoints.OverTaking(false);
							}
						}
					}
				}
				
			}
            if (!_navigateToWaypoints.changingLane)
                earlyBrakePoint = currenPoint.reservationID != 0 && 
					(currenPoint.carwhoReserved == null && (currentConnector == null 
					 || (currentConnector != null && !currentConnector.connectorReservedByTrafficLight)));
			if ( currentPointDistance  < (earlyBrakePoint? maxLockaheadDistance + minBrakingDistRoadblock:maxLockaheadDistance)
                || fullStop )
			{

				if (currenPoint.reservationID !=_navigateToWaypoints.myID || !stillMine){
					returningValue +=  Mathf.Max(lengthMargin,maxLockaheadDistance)/(currentPointDistance<=0?0.0001f:currentPointDistance) ;// Mathf.Max(1-((((currentPointDistance<0?0:currentPointDistance)-maxLockaheadDistance)/lengthMargin)),returningValue);
					if (currentPointDistance <= lengthMargin)returningValue = 1;
				}

				return;
			}
		}
	}

	/// <summary>
	/// Gets the nearest player.
	/// </summary>
	float GetNearestPlayerDistance()
	{
		float playerDistance =float.MaxValue;
		int selectedPlayer = -1;
		for (int i = 0 ; i < players.Count;i++)
		{
			if (players[i] == null){
				players.Remove(players[i]);
				break;
			}
			float tempDistance = (_wheelsCenter.position - players[i].position).sqrMagnitude;
			if (tempDistance < playerDistance) {
				playerDistance = tempDistance;
				selectedPlayer = i;
			}
		}
		if (selectedPlayer != -1)
		{
			RaycastHit hit;

			//DEBUGING CAST
//			RenderVolume(_wheelsCenter.position, _wheelsCenter.position + _wheelsCenter.forward,carWidth/2f,( currenPoint.point-new Vector3( _wheelsCenter.position.x,currenPoint.point.y,_wheelsCenter.position.z)).normalized,(currenPoint.point-_wheelsCenter.position).magnitude);
			if (Physics.CapsuleCast(_wheelsCenter.position, _wheelsCenter.position + _wheelsCenter.forward,halfCarWidth>0.7f?halfCarWidth:0.7f,( (currenPoint.point + _navigateToWaypoints.pointOffset)-new Vector3( _wheelsCenter.position.x,currenPoint.point.y + _navigateToWaypoints.pointOffset.y,_wheelsCenter.position.z)).normalized,out hit,((currenPoint.point + _navigateToWaypoints.pointOffset)-_wheelsCenter.position).magnitude, 1<<players[selectedPlayer].gameObject.layer))
			{
				if (hit.transform.root == players[selectedPlayer].root){

					float factor = Mathf.Clamp01( Vector3.Dot(myTransform.forward, players[selectedPlayer].forward));
					if (factor < 0.5f){
						PlayPlayerDetectedAudio();
						PlayPlayerDetectedAnimator();
						PlayPlayerDetectedAnimation();
					}

					playerDistance = hit.distance * factor;
				}else playerDistance = float.MaxValue;

			}else {

				if (playerDistance > (carDepth/2f + lengthMargin) * (carDepth/2f + lengthMargin))
				{
					playerDistance = float.MaxValue;
				}else playerDistance = 0;
				//DEBUGING CAST
//				HideVolume();
			}


		}else  playerDistance = float.MaxValue;
		return playerDistance;
	}

	void PlayPlayerDetectedAudio()
	{
		if ( canPlayPlayerDetectedAudio && Time.time - playerDetectedSoundTimming > playerDetectedSoundNext)
		{
			playerDetectedSoundNext = UnityEngine.Random.Range(minplayerDetectedSoundTime,maxplayerDetectedSoundTime);
			playerDetectedSoundTimming = Time.time;
			if (playerDetectedSoundClips.Length >0)
				playerDetectedSoundAudioSource.PlayOneShot(playerDetectedSoundClips[UnityEngine.Random.Range(0,playerDetectedSoundClips.Length)]);
			else if (playerDetectedSoundAudioSource.clip != null)
				playerDetectedSoundAudioSource.PlayOneShot(playerDetectedSoundAudioSource.clip);
		}
	}

	void PlayPlayerDetectedAnimator()
	{
		if ( canPlayPlayerDetectedAnimator && Time.time - playerDetectedAnimatorTimming > playerDetectedAnimatorNext)
		{
			playerDetectedAnimatorNext = UnityEngine.Random.Range(minplayerDetectedAnimatorTime,maxplayerDetectedAnimatorTime);
			playerDetectedAnimatorTimming = Time.time;
			playerDetectedAnimator.Play(playerDetectedStates[UnityEngine.Random.Range(0,playerDetectedStates.Length)]);
		}
	}

	void PlayPlayerDetectedAnimation()
	{
		if ( canPlayPlayerDetectedAnimation && Time.time - playerDetectedAnimationTimming > playerDetectedAnimationNext)
		{
			playerDetectedAnimationNext = UnityEngine.Random.Range(minplayerDetectedAnimationTime,maxplayerDetectedAnimationTime);
			playerDetectedAnimationTimming = Time.time;
			playerDetectedAnimationController.Play(playerDetectedAnimations[UnityEngine.Random.Range(0,playerDetectedAnimations.Length)]);
		}
	}


	private Transform shape; 

	void RenderVolume( Vector3 p1, Vector3 p2 ,float radius, Vector3 dir,float distance){
		if (!shape){ // if shape doesn't exist yet, create it
			shape = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
			shape.GetComponent<BoxCollider>().isTrigger = true;
			Destroy(shape.GetComponent<BoxCollider>()); // no collider, please!
		}
		Vector3 scale; // calculate desired scale
		float diam = 2 * radius; // calculate capsule diameter
		scale.x = diam; // width = capsule diameter
		scale.y = Vector3.Distance(p2, p1) + diam; // capsule height
		scale.z = distance + diam; // volume length
		shape.localScale = scale; // set the rectangular volume size
		// set volume position and rotation
		shape.position = (p1 + p2 + dir.normalized * distance) / 2;
		shape.rotation = Quaternion.LookRotation(dir, p2 - p1);
		shape.GetComponent<Renderer>().enabled = true; // show it
	}
	
	void HideVolume(){ // hide the volume
		if (shape) shape.GetComponent<Renderer>().enabled = false;
	}



    
//    float otherCarOnConnectorSpdSqr = 0f;
	/// <summary>
	/// Tries to reserve point.
	/// </summary>
	bool TryToReservePoint(ref bool stillMine)
	{
        otherCarPresentInJunction = false;  //Initialize this variable to false, if there is another car blocking the junction it gets set to true later
        TSNavigation.TSReservedPoints newPoint = new TSNavigation.TSReservedPoints();
		newPoint.connector = -1;
		newPoint.point = previousWaypointCurve;
		newPoint.lane = currentLaneIndex;

		bool trafficLightOverride =ignoreTrafficLight &&  currenPoint.carwhoReserved == null && currenPoint.reservationID != 0;
		//(currenPoint.carwhoReserved == null) 		||
		if (( currenPoint.reservationID == 0 || 
		     currenPoint.reservationID == _navigateToWaypoints.myID) && 
		    stillMine)
		{
			if (!trafficLightOverride)
			{
				currenPoint.reservationID = _navigateToWaypoints.myID;
				currenPoint.carwhoReserved = this;
			}
			if (currentConnector != null) 
			{
				bool canCrossConnector = true;
				if (currentConnector.remainingGreenLightTime !=-1)
				{
					float distanceToRun1 = Mathf.Max(10f,carSpeed) * currentConnector.remainingGreenLightTime;
					float currentConnectorDistance = (((currentConnector.points[0].point + _navigateToWaypoints.pointOffset) - frontPoint.position).magnitude + currentConnector.totalDistance);
					canCrossConnector = distanceToRun1 > currentConnectorDistance;
				}
				if (ignoreTrafficLight)canCrossConnector = true;

				if (!reservedConnector && canCrossConnector)reservedConnector = _navigateToWaypoints.ReserveNearConnectorPoints (ref currentConnector, lane, previousWaypointCurve);
//				bool goOn = _navigateToWaypoints.CheckOtherPointsAreFromSameLane(currenPoint, lane);
				if ((reservedConnector && canCrossConnector)) 
				{
                    //Added this bool to check if there is another car on the junction from another lane and stop properly
                    bool isFromSameLane = false;
					if (!_navigateToWaypoints.ReserveNearConnectorPoints (ref currenPoint, ref currentConnector, lane, out isFromSameLane))
					{

						if (currenPoint.reservationID == _navigateToWaypoints.myID)
						{
							currenPoint.reservationID = 0;
							currenPoint.carwhoReserved = null;
						}
						stillMine = false;

                        //Added this bool to check if there is another car on the junction from another lane and stop properly
                        if (!isFromSameLane) otherCarPresentInJunction = true;
                    }
                    else
					{
//						if (reservedConnector && !_navigateToWaypoints.reservedConnectors.Contains(currentConnector))
//							_navigateToWaypoints.reservedConnectors.Enqueue(currentConnector);
						newPoint.connector = currentConnectorIndex;

					}
				}
	 			else
				{  //we may not need this brackets **************************************************************CHECK!****************************************************************************
					if (currenPoint.reservationID == _navigateToWaypoints.myID)
					{
						currenPoint.reservationID = 0;
						currenPoint.carwhoReserved = null;
					}
					stillMine = false;
                }//we may not need this brackets **************************************************************CHECK!****************************************************************************
			}

		} 
		else 
		{
			return stillMine = false;
		}
		
		if (currenPoint.reservationID != _navigateToWaypoints.myID && !trafficLightOverride)
		{
			stillMine = false;
			return false;
		}else {
			nextCarSpeedSqr =0;
			_navigateToWaypoints.reservedPoints.Enqueue(newPoint);
			CheckPointSpeed();
			segDistance += currenPoint.distanceToNextPoint;
			if (_navigateToWaypoints.overTaking)
				previousWaypointCurve --;
			else
				previousWaypointCurve ++;
		}
		return true;
	}


	/// <summary>
	/// Checks the point speed.
	/// </summary>
	void CheckPointSpeed()
	{
		float pointMaxSpeed = currenPoint.maxSpeedLimit / 3.6f;
		if (pointMaxSpeed < MAXSPEED)
		{
			TSBrakeSpeeds newSPD = new TSBrakeSpeeds();
			newSPD.point = currenPoint;
			newSPD.distance = (frontPoint.position - (newSPD.point.point + _navigateToWaypoints.pointOffset)).sqrMagnitude;
			brakeSpeeds.Add(newSPD);
			if (newSPD.distance < sqrMaxLockaheadDistance && pointMaxSpeed < carSpeed ) returningValue +=1;
			
		}
	}


	/// <summary>
	/// Checks the upside down.
	/// </summary>
	void CheckUpsideDown()
	{
		if ((myTransform.localEulerAngles.z > 60 && myTransform.localEulerAngles.z < 310) || crashed)
		{
			upSideDownTimer += Time.deltaTime;
			isUpSideDown = true;
			if (upSideDownTimer > TSTrafficSpawner.RespawnUpSideDownTime )
				forcedRespawn = true;
		}
		else 
		{
			upSideDownTimer = 0;
			forcedRespawn = isUpSideDown = false;
		}
	}

	/// <summary>
	/// Gets the current point.
	/// </summary>
	/// <returns><c>true</c>, if current point was gotten, <c>false</c> otherwise.</returns>
	bool GetCurrentPoint()
	{
		if (_navigateToWaypoints.nextTrackPath [nextPathIndex].isConnector) {
			bool changed = false;	
			if (previousWaypointCurve >= _navigateToWaypoints.lanes [_navigateToWaypoints.nextTrackPath [nextPathIndex].nextLane].connectors [_navigateToWaypoints.nextTrackPath [nextPathIndex].nextConnector].points.Length) {
				if (nextPathIndex+1 >= _navigateToWaypoints.nextTrackPath.Count)
					return false;
				changed = true;
				previousWaypointCurve = 0;
				nextPathIndex++;
			}
			if (!changed) {
				lane = _navigateToWaypoints.nextTrackPath [nextPathIndex].nextLane;
				currenPoint = _navigateToWaypoints.lanes [lane].connectors [_navigateToWaypoints.nextTrackPath [nextPathIndex].nextConnector].points [previousWaypointCurve];
				currentConnector = _navigateToWaypoints.lanes [lane].connectors [_navigateToWaypoints.nextTrackPath [nextPathIndex].nextConnector];
				currentLaneIndex = lane;
				currentConnectorIndex = _navigateToWaypoints.nextTrackPath [nextPathIndex].nextConnector;
				nextConnectorInstance = null;
                
			} else {
				reservedConnector = false;
				lane = _navigateToWaypoints.nextTrackPath [nextPathIndex].nextLane;
				currenPoint = _navigateToWaypoints.lanes [lane].points [previousWaypointCurve];
				currentLaneIndex = lane;
				currentConnectorIndex = -1;
				currentConnector = null;
				if (_navigateToWaypoints.reservedConnectors.Count >0){
					_navigateToWaypoints.UnReserveNearConnectorPoints(_navigateToWaypoints.reservedConnectors.Peek());
					_navigateToWaypoints.reservedConnectors.Dequeue();
				}
				if (nextPathIndex+1 < _navigateToWaypoints.nextTrackPath.Count)
					nextConnectorInstance = _navigateToWaypoints.lanes [_navigateToWaypoints.nextTrackPath [nextPathIndex+1].nextLane].connectors [_navigateToWaypoints.nextTrackPath [nextPathIndex+1].nextConnector];
			}
		} else {
			bool changed = false;
			if (previousWaypointCurve >= _navigateToWaypoints.lanes [_navigateToWaypoints.nextTrackPath [nextPathIndex].nextLane].points.Length) {
				if (nextPathIndex+1 >= _navigateToWaypoints.nextTrackPath.Count){
					return false;
				}
				changed = true;
				previousWaypointCurve = 0;
				nextPathIndex++;
			}
			if (!changed) {
				lane = _navigateToWaypoints.nextTrackPath [nextPathIndex].nextLane;
				currenPoint = _navigateToWaypoints.lanes [lane].points [previousWaypointCurve];
				currentLaneIndex = lane;
				currentConnectorIndex = -1;
				currentConnector = null;
                if (nextPathIndex + 1 < _navigateToWaypoints.nextTrackPath.Count)
                {
                    nextConnectorInstance = _navigateToWaypoints.lanes[_navigateToWaypoints.nextTrackPath[nextPathIndex + 1].nextLane].connectors[_navigateToWaypoints.nextTrackPath[nextPathIndex + 1].nextConnector];
                    
                }
                if (nextConnectorInstance != null
                    
                    && _navigateToWaypoints.lanes[nextConnectorInstance.nextLane].totalOcupation < _navigateToWaypoints.lanes[nextConnectorInstance.nextLane].maxTotalOcupation
                    && _navigateToWaypoints.lanes[currentLaneIndex].totalDistance * (1-(float)previousWaypointCurve / (float)_navigateToWaypoints.lanes[currentLaneIndex].points.Length) < minConnectorRequestDistance)
                {
                    if (carSpeed > 0.01f)
                        nextConnectorInstance.isRequested = true;
                    else
                    {
                        if (currenPoint.carwhoReserved == null)
                            nextConnectorInstance.isRequested = true;
                        else if (!nextConnectorInstance.isReserved)
                            nextConnectorInstance.isRequested = false;
                    }
                }
                    
            } else {
				lane = _navigateToWaypoints.nextTrackPath [nextPathIndex].nextLane;
				currenPoint = _navigateToWaypoints.lanes [lane].connectors [_navigateToWaypoints.nextTrackPath [nextPathIndex].nextConnector].points [previousWaypointCurve];
				currentConnector = _navigateToWaypoints.lanes [lane].connectors [_navigateToWaypoints.nextTrackPath [nextPathIndex].nextConnector];
				currentLaneIndex = lane;
				currentConnectorIndex = _navigateToWaypoints.nextTrackPath [nextPathIndex].nextConnector;
				reservedConnector = false;
				nextConnectorInstance = null;
			}
		}
		return true;
	}

	/// <summary>
	/// Gets the steer.
	/// </summary>
	/// <returns>The steer.</returns>
	/// <param name="targetPoint">Target point.</param>
	/// <param name="previousT">Previous t.</param>
	/// <param name="useOvertakes">If set to <c>true</c> use overtakes.</param>
	float GetSteer (Vector3 targetPoint, Vector3 previousT, bool useOvertakes)
	{
		Vector2 localTarget = GetTargetPoint (targetPoint, previousT, useOvertakes);
		float x = localTarget.x;
		float temp = x / ((localTarget.magnitude));
		return temp;
	}

	/// <summary>
	/// Gets the target point.
	/// </summary>
	/// <returns>The target point.</returns>
	/// <param name="Point">Point.</param>
	/// <param name="prev">Previous.</param>
	/// <param name="useOvertakes">If set to <c>true</c> use overtakes.</param>
	Vector2 GetTargetPoint (Vector3 Point, Vector3 prev, bool useOvertakes)
	{
		if (!float.IsNaN (Vector3.SqrMagnitude (prev))) {
			float distanceBPoints = Vector3.Distance (_navigateToWaypoints.waypoints [_navigateToWaypoints.currentWaypoint].point, _navigateToWaypoints.waypoints [_navigateToWaypoints.previousWaypointSteer].point);
			Point = Vector3.Lerp (prev, Point, ((Mathf.Abs ((GetLookahead () - (_navigateToWaypoints.relativeWPosMagnitude - distanceBPoints)) / distanceBPoints))));
		}
		Vector3 localTarget1 = _wheelsCenter.InverseTransformPoint (Point);
		float x = localTarget1.x + myLaneOffset;
		float z = Mathf.Max(localTarget1.z,2);
		Vector2 s = new Vector2 (x, z);
		return s;
	}

	/// <summary>
	/// Gets the lookahead.
	/// </summary>
	/// <returns>The lookahead.</returns>
	public float GetLookahead ()
	{
		_lookAheadDistance = LOOKAHEAD_CONST + (carSpeed * LOOKAHEAD_FACTOR * Mathf.Clamp ((carSpeed / (150f)), 0.1f, 1f));
		return _lookAheadDistance;
	}

	/// <summary>
	/// Raises the trigger enter event.
	/// </summary>
	/// <param name="colInfo">Col info.</param>
	void OnTriggerEnter (Collider colInfo)
	{
		if (colInfo.tag == playerTag) {
			players.Add(colInfo.transform);
			fullStop = true;
		}
	}

	/// <summary>
	/// Raises the trigger exit event.
	/// </summary>
	/// <param name="colInfo">Col info.</param>
	void OnTriggerExit (Collider colInfo)
	{
		if (colInfo.tag == playerTag) {
			players.Remove(colInfo.transform);
			fullStop = false;
			HideVolume();
		}
	}
		
	/// <summary>
	/// General calculations.
	/// </summary>
	void GeneralCalculations ()
	{
		currentSpeedSqr = ((carSpeed) * (carSpeed));
	}
	
	
//	*********************************************************************
//	Utility methods	
//	*********************************************************************
	
	/// <summary>
	/// Gets the wheels center.
	/// </summary>
	void IRDSWheelsCenter ()
	{	
		Bounds bounds;
		Quaternion temprotation = myTransform.rotation;
		Vector3 tempPosition = myTransform.position;
		myTransform.rotation = Quaternion.Euler (Vector3.zero);
		myTransform.position = Vector3.zero;
		Vector3 initialBoundPos = Vector3.zero;
		initialBoundPos = frontWheels[0].position;
		bounds = new Bounds (initialBoundPos, Vector3.zero);
		_wheelsCenter = new GameObject ().transform;
		_wheelsCenter.transform.position = Vector3.zero;
		_wheelsCenter.transform.eulerAngles = Vector3.zero;
		foreach (Transform wheel in frontWheels) {
//			Renderer[] renderers = wheel.GetComponentsInChildren<Renderer> (); 
//			foreach (Renderer renderer in renderers) {
				bounds.Encapsulate (wheel.position);
//			}
		}
		_wheelsCenter.transform.position = new Vector3 (bounds.center.x, 0f, bounds.center.z);
		_wheelsCenter.transform.parent = myTransform;
		_wheelsCenter.name = "FrontIRDSWheelsCenter";
		myTransform.rotation = temprotation;
		myTransform.position = tempPosition;
	}

	/// <summary>
	/// Gets the car size.
	/// </summary>
	/// <returns>The size.</returns>
	Bounds CarSize()
	{	
		Bounds bounds;
		Quaternion temprotation = myTransform.rotation;
		Vector3 tempPosition=myTransform.position;
		myTransform.rotation = Quaternion.Euler(Vector3.zero);
		myTransform.position = Vector3.zero;
		bounds = new Bounds (myTransform.position, Vector3.zero);
		Collider[] renderers = GetComponentsInChildren<Collider> ();
		foreach (Collider renderer in renderers)
		{
			if (!renderer.isTrigger)
				bounds.Encapsulate (renderer.bounds);
		}
		myTransform.rotation = temprotation;
		myTransform.position = tempPosition;
		return bounds;
	}

	/// <summary>
	/// Enable or disables the car components.
	/// </summary>
	/// <param name="flag">If set to <c>true</c> flag.</param>
	public void Enable(bool flag)
	{
		crashed = false;
		if (flag){
			gameObject.SetActive(true);

			for (int i = 0; i < bodies.Length;i++){
				bodies[i].constraints = RigidbodyConstraints.None;
				bodies[i].velocity = Vector3.zero;
			}

		}else{
			this.throttle = 0;
			this.steering = 0;
			this.brake = 0;
			if (
				this.OnUpdateAI != null)OnUpdateAI(0,0,0, isUpSideDown);
			for (int i = 0; i < bodies.Length;i++)
				bodies[i].constraints = RigidbodyConstraints.FreezeAll;
			_navigateToWaypoints.TurnOffTurningLights();
			gameObject.SetActive(false);
		}
	}	


	
//end line
}