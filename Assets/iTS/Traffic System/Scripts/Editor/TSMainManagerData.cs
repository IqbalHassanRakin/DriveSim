using UnityEngine;
using System;
public class TSMainManagerData : ScriptableObject{
	
	public Vector3 MultipleSelectionOrigin = Vector3.zero;
	public float MultipleSelectionRadius = 10f;
	public Bounds MultipleSelectionBounds = new Bounds(Vector3.zero,new Vector3(100,100,100));
	public bool useSphereSelection = false;
	public bool enableUndoForLanesDeletion = true;
	public bool enableUndoForCleaingRoadData = true;
	public bool allowDeadEndLanes = false;
	public float nearbyPointsRadius = 10f;
	public float maxLaneCachedPerFrame = 50f;
	public bool calculateNerbyPointsForPlayerFinding = false;
/// <summary>
	/// The batch Lane and Connectors settings.  This would be the settings that could be applied to more than one lane/connector
	/// </summary>
	[SerializeField]
	public BatchLaneConnectorsSettings batchLCSettings;
	
	[System.Serializable]
	public class BatchLaneConnectorsSettings{
		
		public TSLaneInfo.VehicleType defaultVehicleType;
		public bool setVehicleTypeLane = false;
		public bool setVehicleTypeConnector = false;
		public float defaultLaneWidth = 2.5f;
		public bool setLaneWidth = false;
		public float defaultMaxSpeed = 50f;
		public bool setMaxSpeed = false;
		public float defaultMaxDensity = 1f;
		public bool setMaxDensity = false;
		public TSLaneConnector.Direction defaultDirection = TSLaneConnector.Direction.Straight;
		public bool setDirection = false;
		public bool defaultForcedStop = false;
		public bool setForcedStop = false;
		public int defaultPassPriority = 1;
		public bool setPassPriority = false;
		public Vector3 selectionPosition;
	}

}