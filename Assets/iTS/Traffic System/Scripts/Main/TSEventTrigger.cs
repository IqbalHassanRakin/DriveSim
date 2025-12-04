using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class TSEventTrigger : MonoBehaviour {

	[System.Serializable]
	public class TSPointReference{
		/// <summary>
		/// The lane.
		/// </summary>
		public int lane;
		
		/// <summary>
		/// The connector.
		/// </summary>
		public int connector;
		
		/// <summary>
		/// The point.
		/// </summary>
		public int point;
	}

	[HideInInspector]
	public TSPointReference startingPoint;
	[HideInInspector]
	public TSPointReference eventEndingPoint;
	[HideInInspector]
	public bool spawnCarOnStartingPoint = false;


	[HideInInspector]
	[SerializeField]
	public List<TSNavigation.TSNextLaneSelection> carPredefinedPath = new List<TSNavigation.TSNextLaneSelection>();
	[HideInInspector]
	public TSTrafficAI tAI;
	[HideInInspector]
	public TSNavigation nav;


	public float range = 25f;


	#region private members
	protected bool isTriggered = false;
	protected TSMainManager manager;

	#endregion


	public virtual void Awake()
	{
		manager = GameObject.FindObjectOfType<TSMainManager>();
	}


	public abstract void InitializeMe();


	public virtual void SetCar(TSTrafficAI car)
	{
		tAI = car;
		nav = car.GetComponent<TSNavigation>();
		tAI.InitializeMe();
		if (spawnCarOnStartingPoint)
		{
			tAI.reservedForEventTrigger = true;
		}
		if (carPredefinedPath !=null &&  carPredefinedPath.Count >0)
		{
			nav.AddNextTrackToPath(carPredefinedPath);
		}
	}

	protected void DisableCarAI()
	{
		tAI.enabled = false;
		nav.enabled = false;
	}

	protected void EnableCarAI()
	{
		tAI.enabled = true;
		nav.enabled = true;
	}


	public TSPoints Point(TSPointReference point)
	{
		if (point.connector ==-1)
		{
			return manager.lanes[point.lane].points[point.point];
		}
		else
		{
			return manager.lanes[point.lane].connectors[point.connector].points[point.point];
		}
	}

}
