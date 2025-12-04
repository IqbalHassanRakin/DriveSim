using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

[CustomEditor(typeof(TSMainManager))]
public class TSMainManagerEditor : Editor {

	public delegate void OnLaneDeleted(int lane);
	public OnLaneDeleted onLaneDeleted;

	HashSet<int> tempLanes;
	private TSMainManagerData mainData;
	private TSMainManager manager;
	private string[] toolbar1Contents = new string[]{"Lanes","Lane Connectors","Settings"};
	private string[] toolbar2Contents = new string[]{"Lane","Lane Linking","Edit Points","Batch Settings"}; 
	private string[] toolbar3Contents = new string[]{"Connector","Edit Points","Batch Settings"};
	private bool addLane = false;
	private bool addConnector = false;
	private int currentLaneSelected = -1;
	private int currentConnectorSelected = -1;
	private bool addLane1 = false; 
	private bool removeLane = false;
	private TSLaneConnector newConnector;
	private bool addConnector1 = false;
	private bool removeConnector = false;
	private bool removeConnectorPoint = false;
	private bool removeLanePoint = false;
	private bool addLaneLink = false;
	private bool removeLaneLink = false;
	private int linkLane1 = -1;
	private int linkLane2 = -1;
	private bool linkLane1Set = false;
	private bool linkLane2Set = false;
	private bool linkLane1Right = false;
	private bool linkLane2Right = false;
	private Vector3 laneLinkPos = Vector3.zero;
	private Texture2D iTSLogo;

	private HashSet<TSLaneInfo> selectedLanes = new HashSet<TSLaneInfo>();
	private HashSet<TSLaneConnector> selectedConnectors = new HashSet<TSLaneConnector>();


	public void OnEnable()
	{ 
		tempLanes = new HashSet<int>();
		GetMainManagerData();
		manager = (TSMainManager)target;
		if (manager.GetComponent<TSTrafficLightCheck>()==null)
			manager.gameObject.AddComponent<TSTrafficLightCheck>();
		if (manager.lanes == null || manager.lanes.Length == 0){ 
			ResetLanes();
		}
		vehicleTypesNames = System.Enum.GetNames(typeof(TSLaneInfo.VehicleType));
		vehicleTypesSelected = new bool[vehicleTypesNames.Length];
		iTSLogo = AssetDatabase.LoadAssetAtPath("Assets/iTS/Traffic System/Required/iTSLogo/iTSLogo.png", typeof( Texture2D))as Texture2D;

		MultipleConnectorsSelection();
		MultipleLanesSelection();
		EditorApplication.update += CheckLanes;
	}

	void OnDisable()
	{
		EditorApplication.update -= CheckLanes;
	}

	//Not used code, was thinking on implementing a scriptable object for storing some editor tool settings
	//But i think is better to keep them on the GameObject, in case users want to have different Default settings
	//Stored on different iTSManager objects and not having the same on all
	void GetMainManagerData()
	{
		string path = GetiTSDirectory();
		mainData =(TSMainManagerData) AssetDatabase.LoadAssetAtPath(path + "iTSMainData.asset", typeof( TSMainManagerData));
		if(!mainData){
			mainData = ScriptableObject.CreateInstance<TSMainManagerData>();
			
			AssetDatabase.CreateAsset(mainData, path + "iTSMainData.asset");
		}
	}


	void ResetLanes()
	{
		manager.lanes = new TSLaneInfo[0];
	}


	public override void OnInspectorGUI(){


		GUILayout.Space(10);
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.alignment = TextAnchor.UpperCenter;
		style.imagePosition = ImagePosition.ImageAbove;
		Rect logoRect = GUILayoutUtility.GetRect(0,100);
		EditorGUI.LabelField(logoRect,new GUIContent(iTSLogo),style);
		GUILayout.Label("Version 1.1.3", style);
		GUILayout.Space(10);
		GUILayout.BeginVertical("Road Data",GUI.skin.box);
		GUILayout.Space(15);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Save", GUILayout.Width(60)))
		{
			string path = EditorUtility.SaveFilePanel("Save Road Data as",Application.dataPath,"RoadData","xml");
			if (path.Length !=0)
				Save(path);
		}
		if (GUILayout.Button("Load", GUILayout.Width(60)))
		{
			string path = EditorUtility.OpenFilePanel("Load Road Data",Application.dataPath,"xml");
			if (path.Length!=0){
				manager.lanes = Load(path);
				ProcessJunctions();
			}
		}

		if (GUILayout.Button("Clear", GUILayout.Width(60)))
		{
			ClearDataChecked();
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(5);
		GUILayout.EndVertical();

		Color defaultGuiColor = GUI.backgroundColor;
		if (!manager.junctionsProcessed)
		{
			GUI.backgroundColor = Color.red;
		}else GUI.backgroundColor = Color.green;


		if (GUILayout.Button("Process junctions",GUILayout.Height(25)))
		{
			ProcessJunctions();
		}

		GUI.backgroundColor = defaultGuiColor;

		manager.menuSelection = GUILayout.Toolbar(manager.menuSelection, toolbar1Contents);
		
		switch (manager.menuSelection)
		{
		case 0:
			addConnector1 = false;
			manager.connectionsMenuSelection =0;
			removeConnector = false;
			removeConnectorPoint = false;

			LanesStuff();
			break;
		case 1:
			addLane1 = false;
			removeLane = false;
			removeLanePoint = false;
			manager.laneMenuSelection = 0;
			ConnectionsStuff();
			break;
		case 2:
			Settings();
			break;
		}

		GUILayout.Label("Total lanes: "+ manager.lanes.Length.ToString());





		/*************************************************************************************************
		 ************************************DEBUG CODE***************************************************
		 *************************************************************************************************/
//		if (currentConnectorSelected >=0 && currentConnectorSelected < manager.lanes[currentLaneSelected].connectors.Length && currentLaneSelected < manager.lanes.Length){
//			SerializedProperty property = this.serializedObject.FindProperty("lanes");
//			
//			SerializedProperty connector = property.GetArrayElementAtIndex(currentLaneSelected).FindPropertyRelative("connectors");
//			EditorGUILayout.PropertyField(connector.GetArrayElementAtIndex(currentConnectorSelected),true); 
//
//		}
//		else if (currentLaneSelected < manager.lanes.Length){
//			SerializedProperty property = this.serializedObject.FindProperty("lanes");
//			
//			EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(currentLaneSelected),true);
//		}
//		serializedObject.ApplyModifiedProperties();
//		serializedObject.Update();
		/*************************************************************************************************
		 ************************************DEBUG CODE***************************************************
		 *************************************************************************************************/


		if (GUI.changed){
		 	EditorUtility.SetDirty(manager);
		}
		
	}

	void ClearDataChecked(){
		int decision = EditorUtility.DisplayDialogComplex("Warning!","You are about to delete all the road data of this scene, if you have not saved the road data and don't want to loose your work press NO and save your data first!,\n Do you want to continue to clear all road data?","Yes","No","Cancel");
		if (mainData.enableUndoForCleaingRoadData){
			if (manager.lanes.Length > 25)
			{
				int result = EditorUtility.DisplayDialogComplex("Undo Warning!","The undo process would take several seconds to minutes, would you like to continue with Undo?","Yes","No","Cancel");
				switch(result)
				{
				case 0:
					Undo.RecordObject(manager,"Clear road data");
					break;
				case 1:
					break;
				case 2:
					return;
				}
			}else{
				Undo.RecordObject(manager,"Clear road data");
			}
		}
		if (decision == 0)
			manager.lanes = new TSLaneInfo[0];
	}

	void ProcessJunctions()
	{
		RemoveBadConnectors();
		float progress = 0f;
		float totalProgress = manager.lanes.Length;
		EditorUtility.DisplayProgressBar("Processing Roads Data","Processing Junctions",progress);
		for (int laneIndexP =0; laneIndexP < manager.lanes.Length; laneIndexP++)
		{
			
			//Speed Limit of lanes Inclusion on the track data
			for (int point = 0; point < manager.lanes[laneIndexP].points.Length; point++){
				manager.lanes[laneIndexP].points[point].nearbyPoints = new TSConnectorOtherPoints[0];
				if (point +2 < manager.lanes[laneIndexP].points.Length){
					Vector3 point1 = manager.lanes[laneIndexP].points[point].point;
					Vector3 point2 = manager.lanes[laneIndexP].points[point+1].point;
					Vector3 point3 = manager.lanes[laneIndexP].points[point+2].point;
					Quaternion tempDir = Quaternion.LookRotation(point2-point1);
					Quaternion tempDir1 = Quaternion.LookRotation(point3-point2);
					float angle2 = Quaternion.Angle(tempDir1,tempDir);
					if (angle2 < 5){
						manager.lanes[laneIndexP].points[point].maxSpeedLimit = 1000f;
					}
					else if (angle2 < 10)
						manager.lanes[laneIndexP].points[point].maxSpeedLimit = 50f * manager.laneCurveSpeedMultiplier;
					else if (angle2 < 15)
						manager.lanes[laneIndexP].points[point].maxSpeedLimit = 40f* manager.laneCurveSpeedMultiplier;
					else if (angle2 < 20)
						manager.lanes[laneIndexP].points[point].maxSpeedLimit = 30f* manager.laneCurveSpeedMultiplier;
					else if (angle2 >= 20)
						manager.lanes[laneIndexP].points[point].maxSpeedLimit = 25f* manager.laneCurveSpeedMultiplier;
				}
				manager.lanes[laneIndexP].points[point].leftParalelLaneIndex = -1;
				manager.lanes[laneIndexP].points[point].rightParalelLaneIndex = -1;
				manager.lanes[laneIndexP].points[point].connectorReservationCount = 0;
				
			}
			

			if (!mainData.allowDeadEndLanes && manager.lanes[laneIndexP].connectors.Length == 0 && manager.lanes[laneIndexP].laneLinkLeft == -1 &&manager.lanes[laneIndexP].laneLinkRight == -1 ){
				GameObject tempFocus = new GameObject();
				tempFocus.transform.position = manager.lanes[laneIndexP].conectorA;
				if (SceneView.sceneViews != null && SceneView.sceneViews.Count >0)
				{
					for (int i=0; i < SceneView.sceneViews.Count;i++)
					{
						SceneView sceneView = (SceneView)SceneView.sceneViews[i]; 
						sceneView.Focus(); 
						sceneView.AlignViewToObject(tempFocus.transform);
						
					}
				}
				DestroyImmediate(tempFocus);
				EditorUtility.ClearProgressBar();
				EditorUtility.DisplayDialog("Important!", "All lanes needs to have at least 1 connector, the lane number "+ laneIndexP + " does not have any connector, please add a connector to this lane and process the road data again","Ok");
				int decision = EditorUtility.DisplayDialogComplex("Warning!", "Do you want to delete the lane "+ laneIndexP + ", since this lane have no connector?","Yes","No","");
				if (decision == 0)
				{
					RemoveLane(laneIndexP);

				}
				return;
			}


			for (int connectorIndexP = 0; connectorIndexP < manager.lanes[laneIndexP].connectors.Length; connectorIndexP++)
			{
				manager.lanes[laneIndexP].connectors[connectorIndexP].previousLane = laneIndexP;
				for (int pointIndexP =0; pointIndexP < manager.lanes[laneIndexP].connectors[connectorIndexP].points.Length; pointIndexP++)
				{
					manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].otherConnectorsPoints = new TSConnectorOtherPoints[0];// List<TSConnetorOtherPoints>();
					manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].nearbyPoints = new TSConnectorOtherPoints[0];
					manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].connectorReservationCount = 0;
					
					//Speed Limit of connectors Inclusion on the track data
					
					if (pointIndexP +2 < manager.lanes[laneIndexP].connectors[connectorIndexP].points.Length){
						Vector3 point1 = manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].point;
						Vector3 point2 = manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP+1].point;
						Vector3 point3 = manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP+2].point;
						Quaternion tempDir = Quaternion.LookRotation(point2-point1);
						Quaternion tempDir1 = Quaternion.LookRotation(point3-point2);
						float angle2 = Quaternion.Angle(tempDir1,tempDir);
						if (angle2 < 5){
							manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].maxSpeedLimit = 1000f;
						}
						else if (angle2 < 10)
							manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].maxSpeedLimit = 50f * manager.connectorsCurveSpeedMultiplier;
						else if (angle2 < 15)
							manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].maxSpeedLimit = 40f * manager.connectorsCurveSpeedMultiplier;
						else if (angle2 < 20)
							manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].maxSpeedLimit = 30f * manager.connectorsCurveSpeedMultiplier;
						else if (angle2 >= 20)
							manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].maxSpeedLimit = 25f * manager.connectorsCurveSpeedMultiplier;
					}
					
					
					//Compare fors
					for (int laneIndexC =0; laneIndexC < manager.lanes.Length; laneIndexC++)
					{

						for (int connectorIndexC = 0; connectorIndexC < manager.lanes[laneIndexC].connectors.Length; connectorIndexC++)
						{
							if (connectorIndexP == connectorIndexC && laneIndexP == laneIndexC) continue;
							for (int pointIndexC =0; pointIndexC < manager.lanes[laneIndexC].connectors[connectorIndexC].points.Length; pointIndexC++)
							{
								float distance = Vector3.Distance(manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].point,    manager.lanes[laneIndexC].connectors[connectorIndexC].points[pointIndexC].point);
								if (distance < manager.lanes[laneIndexP].laneWidth)
								{
									TSConnectorOtherPoints otherPoint = new TSConnectorOtherPoints();
									otherPoint.lane = laneIndexC;
									otherPoint.connector = connectorIndexC;
									otherPoint.pointIndex = pointIndexC;
									manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].otherConnectorsPoints = manager.lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].otherConnectorsPoints.Add(otherPoint);
								}
							}
						}
					}

				}

			}
			EditorUtility.DisplayProgressBar("Processing Roads Data","Processing Junctions",laneIndexP / totalProgress);
		}
		
		
		
		
		//Processing parallel Lanes
		progress = 0f;
		EditorUtility.DisplayProgressBar("Processing Roads Data","Processing parallel lanes",progress);
		for (int laneIndex1 =0; laneIndex1 < manager.lanes.Length; laneIndex1++)
		{
			for (int laneIndex2 = 0; laneIndex2 < manager.lanes.Length; laneIndex2++)
			{
				bool left = false;
				bool right = false;
				bool otherIsLeft = false;
				bool otherIsRigth = false;
				if (manager.lanes[laneIndex1].laneLinkLeft == laneIndex2)
					left = true;
				if (manager.lanes[laneIndex1].laneLinkRight == laneIndex2)
					right = true;

				if (manager.lanes[laneIndex2].laneLinkLeft == laneIndex1)
					otherIsLeft = true;
				if (manager.lanes[laneIndex2].laneLinkRight == laneIndex1)
					otherIsRigth = true;

				if (left || right){
					for (int pointIndex1 =0; pointIndex1 < manager.lanes[laneIndex1].points.Length; pointIndex1++)
					{
						int pointIndexParallel = getNearestWayppoint(manager.lanes[laneIndex2].points, manager.lanes[laneIndex1].points[pointIndex1].point);
						if (left)
							manager.lanes[laneIndex1].points[pointIndex1].leftParalelLaneIndex = pointIndexParallel;
						else 
							if (right)
							manager.lanes[laneIndex1].points[pointIndex1].rightParalelLaneIndex = pointIndexParallel;
						if (otherIsLeft)
							manager.lanes[laneIndex2].points[pointIndexParallel].leftParalelLaneIndex = pointIndex1;
						else if(otherIsRigth)
							manager.lanes[laneIndex2].points[pointIndexParallel].rightParalelLaneIndex = pointIndex1;
					}
				}
				
			}
			EditorUtility.DisplayProgressBar("Processing Roads Data","Removing duplicates",laneIndex1 / totalProgress);
		}



		//Eliminate Duplicates
		progress=0;
		EditorUtility.DisplayProgressBar("Processing Roads Data","Removing duplicates",progress);
		for (int laneIndexP =0; laneIndexP < manager.lanes.Length; laneIndexP++)
		{
			TSLaneInfo lane = manager.lanes[laneIndexP];
			for (int connectorIndexP = 0; connectorIndexP < lane.connectors.Length; connectorIndexP++)
			{
				TSLaneConnector connector = lane.connectors[connectorIndexP];
				for (int pointIndexC =0; pointIndexC < connector.points.Length; pointIndexC++)
				{
					TSPoints point = connector.points[pointIndexC];

					for (int otherPointsIndex = point.otherConnectorsPoints.Length-1; otherPointsIndex >=0; otherPointsIndex--)
					{
						TSConnectorOtherPoints otherPoint = point.otherConnectorsPoints[otherPointsIndex];
						for (int SecondPointsIndex = 0; SecondPointsIndex < connector.points.Length;SecondPointsIndex++){
							TSPoints pointC = connector.points[SecondPointsIndex];
							if (pointIndexC != SecondPointsIndex){
								for (int otherPointsIndexC = pointC.otherConnectorsPoints.Length-1; otherPointsIndexC >= 0; otherPointsIndexC--)
								{
									TSConnectorOtherPoints otherPointC = pointC.otherConnectorsPoints[otherPointsIndexC];
									if (otherPointsIndex < point.otherConnectorsPoints.Length){
										if (otherPoint.connector == otherPointC.connector && otherPoint.lane == otherPointC.lane  &&  otherPoint.pointIndex == otherPointC.pointIndex)
										{
											float pDistance = Vector3.Distance( point.point, manager.lanes[otherPoint.lane].connectors[otherPoint.connector].points[otherPoint.pointIndex].point);
											float pCDistance = Vector3.Distance( pointC.point, manager.lanes[otherPointC.lane].connectors[otherPointC.connector].points[otherPointC.pointIndex].point);
											if (pDistance < pCDistance)
												pointC.otherConnectorsPoints = pointC.otherConnectorsPoints.Remove(otherPointC);
											else 
												point.otherConnectorsPoints = point.otherConnectorsPoints.Remove(otherPointC);
										}
									}
								}
							}
						}
					}
					
				}
			}
			EditorUtility.DisplayProgressBar("Processing Roads Data","Removing duplicates",laneIndexP / totalProgress);
		}






		if (mainData.calculateNerbyPointsForPlayerFinding)
			FindNearbyPoints();






		EditorUtility.ClearProgressBar();
		manager.junctionsProcessed = true;
		EditorUtility.SetDirty(manager);
	}

	void FindNearbyPoints()
	{
		float progress=0;
		float totalProgress = manager.lanes.Length;
		EditorUtility.DisplayProgressBar("Processing Roads Data","Finding nearby points",progress);
		for (int laneIndexP =0; laneIndexP < manager.lanes.Length; laneIndexP++)
		{
			TSLaneInfo lane = manager.lanes[laneIndexP];

			for (int lanePointIndex =0;lanePointIndex < lane.points.Length; lanePointIndex++)
			{
				TSPoints currenP = lane.points[lanePointIndex];

				for (int laneIndexS =0; laneIndexS < manager.lanes.Length; laneIndexS++)
				{
					TSLaneInfo laneS = manager.lanes[laneIndexS];
					for (int lanePointIndexS =0;lanePointIndexS< laneS.points.Length; lanePointIndexS++)
					{
						TSPoints searchP = laneS.points[lanePointIndexS];
						if (currenP != searchP)
						{
							float distance = Vector3.Distance(currenP.point,searchP.point);
							if (distance < mainData.nearbyPointsRadius)
							{
								TSConnectorOtherPoints otherPoint = new TSConnectorOtherPoints();
								otherPoint.lane = laneIndexS;
								otherPoint.connector = -1;
								otherPoint.pointIndex = lanePointIndexS;
								currenP.nearbyPoints = currenP.nearbyPoints.Add(otherPoint);
							}
						}
					}
				
					for (int connectorIndexP = 0; connectorIndexP < laneS.connectors.Length; connectorIndexP++)
					{
						TSLaneConnector connector = laneS.connectors[connectorIndexP];
						for (int pointIndexC =0; pointIndexC < connector.points.Length; pointIndexC++)
						{
							TSPoints point = connector.points[pointIndexC];
							if (currenP != point)
							{
								float distance = Vector3.Distance(currenP.point,point.point);
								if (distance < mainData.nearbyPointsRadius)
								{
									TSConnectorOtherPoints otherPoint = new TSConnectorOtherPoints();
									otherPoint.lane = laneIndexS;
									otherPoint.connector = connectorIndexP;
									otherPoint.pointIndex = pointIndexC;
									currenP.nearbyPoints = currenP.nearbyPoints.Add(otherPoint);
								}
							}
						}
					}
				}
			}


			for (int connectorIndexP = 0; connectorIndexP < lane.connectors.Length; connectorIndexP++)
			{
				TSLaneConnector connector = lane.connectors[connectorIndexP];
				for (int pointIndexC =0; pointIndexC < connector.points.Length; pointIndexC++)
				{
					TSPoints currenP = connector.points[pointIndexC];

					for (int laneIndexS =0; laneIndexS < manager.lanes.Length; laneIndexS++)
					{
						TSLaneInfo laneS = manager.lanes[laneIndexS];
						for (int lanePointIndexS =0;lanePointIndexS< laneS.points.Length; lanePointIndexS++)
						{
							TSPoints searchP = laneS.points[lanePointIndexS];
							if (currenP != searchP)
							{
								float distance = Vector3.Distance(currenP.point,searchP.point);
								if (distance < mainData.nearbyPointsRadius)
								{
									TSConnectorOtherPoints otherPoint = new TSConnectorOtherPoints();
									otherPoint.lane = laneIndexS;
									otherPoint.connector = -1;
									otherPoint.pointIndex = lanePointIndexS;
									currenP.nearbyPoints = currenP.nearbyPoints.Add(otherPoint);
								}
							}
						}
						
						for (int connectorIndexS = 0; connectorIndexS < laneS.connectors.Length; connectorIndexS++)
						{
							TSLaneConnector connectorS = laneS.connectors[connectorIndexS];
							for (int pointIndexS =0; pointIndexS < connectorS.points.Length; pointIndexS++)
							{
								TSPoints pointS = connectorS.points[pointIndexS];
								if (currenP != pointS)
								{
									float distance = Vector3.Distance(currenP.point,pointS.point);
									if (distance < mainData.nearbyPointsRadius)
									{
										TSConnectorOtherPoints otherPoint = new TSConnectorOtherPoints();
										otherPoint.lane = laneIndexS;
										otherPoint.connector = connectorIndexS;
										otherPoint.pointIndex = pointIndexS;
										currenP.nearbyPoints = currenP.nearbyPoints.Add(otherPoint);
									}
								}
							}
						}
					}

				}
			}
			EditorUtility.DisplayProgressBar("Processing Roads Data","Finding nearby points",laneIndexP / totalProgress);
		}
	}



	void RemoveBadConnectors()
	{
		float totalProgress = manager.lanes.Length;
		float progress = 0f;
		EditorUtility.DisplayProgressBar("Processing Roads Data","Removing invalid connectors",progress);
		for (int laneIndexP =0; laneIndexP < manager.lanes.Length; laneIndexP++)
		{
			for (int connectorIndexP = 0; connectorIndexP < manager.lanes[laneIndexP].connectors.Length; connectorIndexP++)
			{
				if(manager.lanes[laneIndexP].connectors[connectorIndexP].points.Length == 0){
					RemoveConnector(laneIndexP,connectorIndexP);
					Debug.LogWarning ("Removing bad connector at lane->"+laneIndexP + " Connector->" + connectorIndexP);
					laneIndexP--;
					break;
				}
			}
			EditorUtility.DisplayProgressBar("Processing Roads Data","Removing invalid connectors",laneIndexP / totalProgress);
		}
	}






	bool[] vehicleTypesSelected;
	string[] vehicleTypesNames;

	void LanesStuff()
	{
		GUILayout.Space(25);

		GUILayout.BeginHorizontal();
		GUILayout.Space(Screen.width/2 - 210);
		manager.laneMenuSelection = GUILayout.Toolbar(manager.laneMenuSelection, toolbar2Contents, GUILayout.Width(400));
		GUILayout.EndHorizontal();
		
		switch(manager.laneMenuSelection){
		case 0:
			removeLanePoint = false;
			addLane1 = GUILayout.Toggle(addLane1, "Create", EditorStyles.miniButton);
			if (addLane1)removeLane = false;
			
			removeLane = GUILayout.Toggle(removeLane, "Remove", EditorStyles.miniButton);
				if (removeLane)
					addLane1 = false;
			break;
		case 1:
			removeLanePoint = false;
			addLane1 = false;
			removeLane = false;
			addLaneLink = GUILayout.Toggle(addLaneLink, "Create", EditorStyles.miniButton);
			if (addLaneLink)removeLaneLink = false;
			
			removeLaneLink = GUILayout.Toggle(removeLaneLink, "Remove", EditorStyles.miniButton);
				if (removeLaneLink)
					addLaneLink = false;
			break;
		case 2:
			EditorGUILayout.HelpBox("Hold Shift to add points at the end of the lane", MessageType.Info);
			removeLanePoint = GUILayout.Toggle(removeLanePoint, "Remove", EditorStyles.miniButton);
			addLane1 = false;
			removeLane = false;
			break;
		case 3:
			EditorGUILayout.HelpBox("Hold Shift + left mouse button (drag also) to position the selection bounding box/sphere", MessageType.Info);
			if (GUI.changed)
				MultipleLanesSelection();
			EditSettingsMultipleLanes();
			break;
		}
		//Lane selection
		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Current Selected Lane: ");
		currentLaneSelected = EditorGUILayout.IntField(currentLaneSelected);
		if (currentLaneSelected >= manager.lanes.Length)currentLaneSelected = manager.lanes.Length-1;
		if (GUILayout.Button("Goto selected lane") && currentLaneSelected != -1)
		{
			if (SceneView.sceneViews.Count>0)
			{
				SceneView currenScene =(SceneView) SceneView.sceneViews[0];
				int middlePoint = manager.lanes[currentLaneSelected].middlePoints.Count/2;
				currenScene.pivot = new Vector3(manager.lanes[currentLaneSelected].middlePoints[middlePoint].x,currenScene.pivot.y,manager.lanes[currentLaneSelected].middlePoints[middlePoint].z);
			}
			
		}
		if (GUILayout.Button("Delete") && currentLaneSelected != -1)
		{
			DeleteLanesCheck();
		}
		if (GUILayout.Button("Swap") && currentLaneSelected !=-1)
		{
			if (manager.lanes[currentLaneSelected].connectors.Length>0){
				if (EditorUtility.DisplayDialog("Warning!","If you swap this lane direction, all connectors of this lane would be deleted, do you want to continue?","Yes","No"))
				{
					Undo.RecordObject(manager,"Swap Lane Direction");
					while (manager.lanes[currentLaneSelected].connectors.Length>0)
						RemoveConnector(currentLaneSelected,0);
					SwapLaneDirection(currentLaneSelected);
				}

			}else
			{
				Undo.RecordObject(manager,"Swap Lane Direction");
				SwapLaneDirection(currentLaneSelected);
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		if (manager.laneMenuSelection != 3 && currentLaneSelected != -1 && currentLaneSelected < manager.lanes.Length)
		{
			manager.lanes[currentLaneSelected].laneWidth =  EditorGUILayout.Slider("Width",manager.lanes[currentLaneSelected].laneWidth,0f,10f);
			manager.lanes[currentLaneSelected].maxSpeed = EditorGUILayout.FloatField("Max Speed",manager.lanes[currentLaneSelected].maxSpeed);
			manager.lanes[currentLaneSelected].trafficDensity = EditorGUILayout.Slider("Max Density",manager.lanes[currentLaneSelected].trafficDensity,0f,1f);
			manager.lanes[currentLaneSelected].maxTotalOcupation = EditorGUILayout.Slider(new GUIContent( "Max Total Ocupation (%)","This values is in percentage, so to have full max Ocupation should be 100 (100%)"),manager.lanes[currentLaneSelected].maxTotalOcupation,50f,1000f);
			VehicleTypeGUI(ref manager.lanes[currentLaneSelected].vehicleType);
			GUILayout.Label("Total lane distance: "+manager.lanes[currentLaneSelected].totalDistance);

		}



		if (GUI.changed){
			SceneView.RepaintAll();
			EditorUtility.SetDirty(manager);
		}
		
	}

	void DeleteLanesCheck()
	{
		if (mainData.enableUndoForLanesDeletion){
			if (manager.lanes.Length-currentLaneSelected > 5)
			{
				int result = EditorUtility.DisplayDialogComplex("Undo Warning!","The undo process would take several seconds to minutes, would you like to continue with Undo?","Yes","No","Cancel");
				switch(result)
				{
				case 0:
					Undo.RecordObject(manager,"Delete Lane");
					RemoveLane(currentLaneSelected);

					break;
				case 1:
					RemoveLane(currentLaneSelected);
					break;
				case 2:
					break;
				}
			}else{
				Undo.RecordObject(manager,"Delete Lane");
				RemoveLane(currentLaneSelected);
			}
		}else
		{
			RemoveLane(currentLaneSelected);
		}
	}


	void EditSettingsMultipleLanes()
	{
		SphereBoxSelection();
		GUILayout.BeginHorizontal();
		mainData.batchLCSettings.setLaneWidth = EditorGUILayout.Toggle(mainData.batchLCSettings.setLaneWidth, GUILayout.Width(15));
		mainData.batchLCSettings.defaultLaneWidth =  EditorGUILayout.Slider("Width",mainData.batchLCSettings.defaultLaneWidth,0f,10f);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		mainData.batchLCSettings.setMaxSpeed = EditorGUILayout.Toggle(mainData.batchLCSettings.setMaxSpeed, GUILayout.Width(15));
		mainData.batchLCSettings.defaultMaxSpeed = EditorGUILayout.FloatField("Max Speed",mainData.batchLCSettings.defaultMaxSpeed);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		mainData.batchLCSettings.setMaxDensity = EditorGUILayout.Toggle(mainData.batchLCSettings.setMaxDensity, GUILayout.Width(15));
		mainData.batchLCSettings.defaultMaxDensity = EditorGUILayout.Slider("Max Density",mainData.batchLCSettings.defaultMaxDensity,0f,1f);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		mainData.batchLCSettings.setVehicleTypeLane = EditorGUILayout.Toggle(mainData.batchLCSettings.setVehicleTypeLane, GUILayout.Width(15));
		VehicleTypeGUI(ref mainData.batchLCSettings.defaultVehicleType);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Apply to all", GUILayout.Width(100)))
		{
			ApplyBatchSettingsToAllLanes();
		}
		if (GUILayout.Button("Apply to selection", GUILayout.Width(140)))
		{
			ApplyBatchSettingsToSelectedLanes();
		}
		GUILayout.EndHorizontal();
	}

	void ApplyBatchSettingsToAllLanes()
	{
		Undo.RecordObject(manager,"Apply Lanes Batch Settings");
		for (int i = 0; i < manager.lanes.Length;i++)
		{
			if (mainData.batchLCSettings.setLaneWidth)
			{
				manager.lanes[i].laneWidth = mainData.batchLCSettings.defaultLaneWidth;
			}
			if (mainData.batchLCSettings.setMaxSpeed)
			{
				manager.lanes[i].maxSpeed = mainData.batchLCSettings.defaultMaxSpeed;
			}
			if (mainData.batchLCSettings.setMaxDensity)
			{
				manager.lanes[i].trafficDensity = mainData.batchLCSettings.defaultMaxDensity;
			}
			if (mainData.batchLCSettings.setVehicleTypeLane)
			{
				manager.lanes[i].vehicleType = mainData.batchLCSettings.defaultVehicleType;
			}
			
		}
	}

	void ApplyBatchSettingsToSelectedLanes()
	{
		Undo.RecordObject(manager,"Apply Lanes Batch Settings");
		foreach (TSLaneInfo lane in selectedLanes)
		{
			if (mainData.batchLCSettings.setLaneWidth)
			{
				lane.laneWidth = mainData.batchLCSettings.defaultLaneWidth;
			}
			if (mainData.batchLCSettings.setMaxSpeed)
			{
				lane.maxSpeed = mainData.batchLCSettings.defaultMaxSpeed;
			}
			if (mainData.batchLCSettings.setMaxDensity)
			{
				lane.trafficDensity = mainData.batchLCSettings.defaultMaxDensity;
			}
			if (mainData.batchLCSettings.setVehicleTypeLane)
			{
				lane.vehicleType = mainData.batchLCSettings.defaultVehicleType;
			}
			
		}
	}



	void SwapLaneDirection(int selectedLane)
	{
		Vector3 connectorA = manager.lanes[selectedLane].conectorA;
		Vector3 connectorB = manager.lanes[selectedLane].conectorB;
		manager.lanes[selectedLane].conectorA = connectorB;
		manager.lanes[selectedLane].conectorB = connectorA;
		manager.lanes[selectedLane].middlePoints.Reverse ();
		System.Array.Reverse(manager.lanes[selectedLane].points);
	}

	void VehicleTypeGUI(ref TSLaneInfo.VehicleType vehicleType)
	{
		GUILayout.BeginVertical("Vehicle Type:",GUI.skin.box);
		GUILayout.Space(15);
		GetEnumBools(vehicleType);

		for (int i =0; i < vehicleTypesNames.Length;i++){
			vehicleTypesSelected[i] = EditorGUILayout.Toggle(vehicleTypesNames[i],vehicleTypesSelected[i]);
		}
		
		SetEnumBools(ref vehicleType);

		if (manager.vehicleTypePresets.Count >0){
			GUILayout.BeginVertical("Presets", GUI.skin.box);
			GUILayout.Space(15);
			for (int i =0; i < manager.vehicleTypePresets.Count;i++)
			{
				if (GUILayout.Button(manager.vehicleTypePresets[i].name))
				{
					vehicleType = manager.vehicleTypePresets[i].vehicleType;
					GUILayout.EndVertical();
					GUILayout.EndVertical();
					EditorUtility.SetDirty(manager);
					return;
				}
			}
			GUILayout.EndVertical();
		}

		GUILayout.EndVertical();
	}


	void GetEnumBools(TSLaneInfo.VehicleType myEnum)
	{
		if (myEnum.Has(TSLaneInfo.VehicleType.Taxi))
		{
			vehicleTypesSelected[0] = true;
		}else vehicleTypesSelected[0] = false;
		if (myEnum.Has(TSLaneInfo.VehicleType.Bus))
		{
			vehicleTypesSelected[1] = true;
		}else vehicleTypesSelected[1] = false;

		if (myEnum.Has(TSLaneInfo.VehicleType.Light))
		{
			vehicleTypesSelected[2] = true;
		}else vehicleTypesSelected[2] = false;

		if (myEnum.Has(TSLaneInfo.VehicleType.Medium))
		{
			vehicleTypesSelected[3] = true;
		}else vehicleTypesSelected[3] = false;

		if (myEnum.Has(TSLaneInfo.VehicleType.Heavy))
		{
			vehicleTypesSelected[4] = true;
		}else vehicleTypesSelected[4] = false;

		if (myEnum.Has(TSLaneInfo.VehicleType.Train))
		{
			vehicleTypesSelected[5] = true;
		}else vehicleTypesSelected[5] = false;

		if (myEnum.Has(TSLaneInfo.VehicleType.Heavy_Machinery))
		{
			vehicleTypesSelected[6] = true;
		}else vehicleTypesSelected[6] = false;

		if (myEnum.Has(TSLaneInfo.VehicleType.Pedestrians))
		{
			vehicleTypesSelected[7] = true;
		}else vehicleTypesSelected[7] = false;
		
	}

	void SetEnumBools(ref TSLaneInfo.VehicleType myEnum)
	{
		if (vehicleTypesSelected[0])
		{
			myEnum = myEnum.Add(TSLaneInfo.VehicleType.Taxi);
		}else myEnum =myEnum.Remove(TSLaneInfo.VehicleType.Taxi);

		if (vehicleTypesSelected[1])
		{
			myEnum = myEnum.Add(TSLaneInfo.VehicleType.Bus);
		}else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Bus);

		if (vehicleTypesSelected[2])
		{
			myEnum = myEnum.Add(TSLaneInfo.VehicleType.Light);
		}else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Light);
		if (vehicleTypesSelected[3])
		{
			myEnum = myEnum.Add(TSLaneInfo.VehicleType.Medium);
		}else myEnum =myEnum.Remove(TSLaneInfo.VehicleType.Medium);

		if (vehicleTypesSelected[4])
		{
			myEnum =myEnum.Add(TSLaneInfo.VehicleType.Heavy);
		}else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Heavy);

		if (vehicleTypesSelected[5])
		{
			myEnum = myEnum.Add(TSLaneInfo.VehicleType.Train);
		}else myEnum =myEnum.Remove(TSLaneInfo.VehicleType.Train);

		if (vehicleTypesSelected[6])
		{
			myEnum = myEnum.Add(TSLaneInfo.VehicleType.Heavy_Machinery);
		}else myEnum =myEnum.Remove(TSLaneInfo.VehicleType.Heavy_Machinery);

		if (vehicleTypesSelected[7])
		{
			myEnum = myEnum.Add(TSLaneInfo.VehicleType.Pedestrians);
		}else myEnum =myEnum.Remove(TSLaneInfo.VehicleType.Pedestrians);
		
	}
	
	void ConnectionsStuff()
	{
		GUILayout.Space(25);
		GUILayout.BeginHorizontal();
		GUILayout.Space(Screen.width/2 - 175);
		manager.connectionsMenuSelection = GUILayout.Toolbar(manager.connectionsMenuSelection, toolbar3Contents, GUILayout.Width(350));
		GUILayout.EndHorizontal();

		switch(manager.connectionsMenuSelection)
		{
		case 0:
			bool lastAddConnector = addConnector;
			addConnector1 = GUILayout.Toggle(addConnector1, "Create", EditorStyles.miniButton);
			if (lastAddConnector != addConnector1)SceneView.RepaintAll();
			if (addConnector1){
				if (newConnector == null)
					newConnector = new TSLaneConnector();
				removeConnector = false;
			}
			
			removeConnector = GUILayout.Toggle(removeConnector, "Remove", EditorStyles.miniButton);
			if (removeConnector)
				addConnector1 = false;
			break;
		case 1:
			EditorGUILayout.HelpBox("Hold Shift to add points at the end of the connector", MessageType.Info);
			removeConnectorPoint = GUILayout.Toggle(removeConnectorPoint, "Remove", EditorStyles.miniButton);
			break;
		case 2:
			EditorGUILayout.HelpBox("Hold Shift + left mouse button (drag also) to position the selection bounding box/sphere", MessageType.Info);
			if (GUI.changed)
				MultipleConnectorsSelection();
			EditSettingsMultipleConnectors();
			break;
		}
		


		GUILayout.BeginVertical(GUI.skin.box);
		//Lane Selection
		GUILayout.BeginHorizontal();
		GUILayout.Label("Current Selected Lane: ");
		currentLaneSelected = EditorGUILayout.IntField(currentLaneSelected);
		if (currentLaneSelected >= manager.lanes.Length)currentLaneSelected = manager.lanes.Length-1;
		if (GUILayout.Button("Goto selected lane") && currentLaneSelected != -1)
		{
			if (SceneView.sceneViews.Count>0)
			{
				SceneView currenScene =(SceneView) SceneView.sceneViews[0];
				currenScene.pivot = new Vector3(manager.lanes[currentLaneSelected].conectorB.x,currenScene.pivot.y,manager.lanes[currentLaneSelected].conectorB.z);
			}
			
		}
		if (GUILayout.Button("Delete") && currentLaneSelected !=-1)
		{
			DeleteLanesCheck();
		}
		GUILayout.EndHorizontal();

		//Connector Selection
		GUILayout.BeginHorizontal();
		GUILayout.Label("Current Selected Connector: ");
		currentConnectorSelected = EditorGUILayout.IntField(currentConnectorSelected);
		if (manager.lanes.Length >0 && currentLaneSelected !=-1 && currentConnectorSelected >= manager.lanes[currentLaneSelected].connectors.Length)
		{
			if (currentLaneSelected > manager.lanes.Length)currentLaneSelected = manager.lanes.Length-1;
			currentConnectorSelected = manager.lanes[currentLaneSelected].connectors.Length-1;
		}
		if (GUILayout.Button("Goto selected connector")&& currentLaneSelected !=-1  && currentConnectorSelected != -1)
		{
			if (SceneView.sceneViews.Count>0)
			{
				SceneView currenScene =(SceneView) SceneView.sceneViews[0];
				int middlePoint = manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].middlePoints.Count/2;
				currenScene.pivot = new Vector3(manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].middlePoints[middlePoint].x,currenScene.pivot.y,manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].middlePoints[middlePoint].z);
			}
			
		}
		if (GUILayout.Button("Delete") && currentLaneSelected != -1 && currentConnectorSelected != -1)
		{
			Undo.RecordObject(manager,"Delete Connector");
			RemoveConnector(currentLaneSelected,currentConnectorSelected);
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		if (manager.connectionsMenuSelection != 2 && currentLaneSelected != -1 && currentLaneSelected < manager.lanes.Length && currentConnectorSelected != -1 && currentConnectorSelected < manager.lanes[currentLaneSelected].connectors.Length)
		{
			manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].direction =  (TSLaneConnector.Direction) EditorGUILayout.EnumPopup("Direction",manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].direction);
			manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].forcedStop = EditorGUILayout.Toggle(new GUIContent("Forced stop","If this is enabled, all cars would always stop at the start of this connector, and then would continue, simulating an stop sign"),manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].forcedStop);
			manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].priority = EditorGUILayout.IntSlider(new GUIContent("Pass priority","The priority the cars would have if they are waiting on this connector"),manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].priority,0,100);

			VehicleTypeGUI(ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].vehicleType);
		}



		if (GUI.changed){
			EditorUtility.SetDirty(manager);
			SceneView.RepaintAll();
		}
	}

	void SphereBoxSelection(){
		GUILayout.BeginHorizontal(GUI.skin.box);
		GUILayout.Label("Selection type");
		mainData.useSphereSelection = GUILayout.Toggle(mainData.useSphereSelection,"Sphere", EditorStyles.miniButtonLeft, GUILayout.Width(70));
		mainData.useSphereSelection = !GUILayout.Toggle(!mainData.useSphereSelection,"Box", EditorStyles.miniButtonRight, GUILayout.Width(70));
		GUILayout.EndVertical();
	}

	void EditSettingsMultipleConnectors()
	{
		SphereBoxSelection();

		GUILayout.BeginHorizontal();
		mainData.batchLCSettings.setDirection = EditorGUILayout.Toggle(mainData.batchLCSettings.setDirection, GUILayout.Width(15));
		mainData.batchLCSettings.defaultDirection =  (TSLaneConnector.Direction)EditorGUILayout.EnumPopup("Direction",mainData.batchLCSettings.defaultDirection);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		mainData.batchLCSettings.setForcedStop = EditorGUILayout.Toggle(mainData.batchLCSettings.setForcedStop, GUILayout.Width(15));
		mainData.batchLCSettings.defaultForcedStop = EditorGUILayout.Toggle("Forced Stop",mainData.batchLCSettings.defaultForcedStop);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		mainData.batchLCSettings.setPassPriority = EditorGUILayout.Toggle(mainData.batchLCSettings.setPassPriority, GUILayout.Width(15));
		mainData.batchLCSettings.defaultPassPriority = EditorGUILayout.IntSlider("Pass Priority",mainData.batchLCSettings.defaultPassPriority,0,100);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		mainData.batchLCSettings.setVehicleTypeConnector = EditorGUILayout.Toggle(mainData.batchLCSettings.setVehicleTypeConnector, GUILayout.Width(15));
		VehicleTypeGUI(ref mainData.batchLCSettings.defaultVehicleType);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Apply to all", GUILayout.Width(100)))
		{
			ApplyBatchSettingsToAllConnectors();
		}
		if (GUILayout.Button("Apply to Selection", GUILayout.Width(130)))
		{
			ApplyBatchSettingsToSelectedConnectors();
		}
		GUILayout.EndHorizontal();
	}


	void ApplyBatchSettingsToAllConnectors()
	{
		Undo.RecordObject(manager,"Apply Connectors Batch Settings");
		for (int i = 0; i < manager.lanes.Length;i++)
		{
			for (int y = 0; y < manager.lanes[i].connectors.Length;y++){
				if (mainData.batchLCSettings.setDirection)
				{
					manager.lanes[i].connectors[y].direction = mainData.batchLCSettings.defaultDirection;
				}
				if (mainData.batchLCSettings.setForcedStop)
				{
					manager.lanes[i].connectors[y].forcedStop = mainData.batchLCSettings.defaultForcedStop;
				}
				if (mainData.batchLCSettings.setPassPriority)
				{
					manager.lanes[i].connectors[y].priority = mainData.batchLCSettings.defaultPassPriority;
				}
				if (mainData.batchLCSettings.setVehicleTypeConnector)
				{
					manager.lanes[i].connectors[y].vehicleType = mainData.batchLCSettings.defaultVehicleType;
				}
			}
		}
	}

	void ApplyBatchSettingsToSelectedConnectors()
	{
		Undo.RecordObject(manager,"Apply Connectors Batch Settings");
		foreach(TSLaneConnector connector in selectedConnectors)
		{
			if (mainData.batchLCSettings.setDirection)
			{
				connector.direction = mainData.batchLCSettings.defaultDirection;
			}
			if (mainData.batchLCSettings.setForcedStop)
			{
				connector.forcedStop = mainData.batchLCSettings.defaultForcedStop;
			}
			if (mainData.batchLCSettings.setPassPriority)
			{
				connector.priority = mainData.batchLCSettings.defaultPassPriority;
			}
			if (mainData.batchLCSettings.setVehicleTypeConnector)
			{
				connector.vehicleType = mainData.batchLCSettings.defaultVehicleType;
			}
		}
	}



	void Settings()
	{
		GUI.changed = false;
		manager.visualLinesWidth = EditorGUILayout.Slider("Lines width",manager.visualLinesWidth,0.1f,15f);
		manager.resolution = EditorGUILayout.Slider("Resolution lane",manager.resolution,0.1f,5f);
		manager.laneCurveSpeedMultiplier = EditorGUILayout.Slider("Lane curve speed multiplier",manager.laneCurveSpeedMultiplier,0.01f,5f);
		manager.resolutionConnectors = EditorGUILayout.Slider("Resolution connector",manager.resolutionConnectors,0.1f,4f);
		manager.connectorsCurveSpeedMultiplier = EditorGUILayout.Slider("Connector curve speed multiplier",manager.connectorsCurveSpeedMultiplier,0.01f,5f);
		bool guiChange = GUI.changed;
		manager.scaleFactor = EditorGUILayout.Slider(new GUIContent("Scale factor","This is the scale factor for all the visual on scene editor toold"),manager.scaleFactor,0.01f,5f);
		GUI.changed = guiChange;
		if (GUILayout.Button("Reprocess lane points"))
		{
			RefreshLanes(true);
			ProcessJunctions();
			manager.junctionsProcessed = true;
			EditorUtility.SetDirty(manager);
			SceneView.RepaintAll();
			GUI.changed = false;
		}
		if (GUILayout.Button("Reprocess connectors points"))
		{
			RefreshConnectors(true);
			ProcessJunctions();
			manager.junctionsProcessed = true;
			EditorUtility.SetDirty(manager);
			SceneView.RepaintAll();
			GUI.changed = false;
		}
		if (GUI.changed){
			manager.junctionsProcessed = false;
			EditorUtility.SetDirty(manager);
			SceneView.RepaintAll();
		}

		GUILayout.BeginVertical("Default vehicle type",GUI.skin.box);
		GUILayout.Space(15);
		
		GetEnumBools(manager.defaultVehicleType);
		
		for (int i =0; i < vehicleTypesNames.Length;i++){
			vehicleTypesSelected[i] = EditorGUILayout.Toggle(vehicleTypesNames[i],vehicleTypesSelected[i]);
		}
		
		SetEnumBools(ref manager.defaultVehicleType);

		if (GUILayout.Button("Add Selected as preset"))
		{
			manager.vehicleTypePresets.Add(new TSMainManager.VehicleTypePresets());
			manager.vehicleTypePresets[manager.vehicleTypePresets.Count-1].vehicleType =  manager.defaultVehicleType;
			GetVehicleTypePresetNames(manager.vehicleTypePresets.Count-1);
		}

		for (int i = 0; i < manager.vehicleTypePresets.Count;i++)
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("-",GUILayout.Width(20)))
			{
				manager.vehicleTypePresets.RemoveAt(i);
				break;
			}
			manager.vehicleTypePresets[i].vehicleType = (TSLaneInfo.VehicleType)EditorGUILayout.EnumMaskField("Vehicle preset#" + i,manager.vehicleTypePresets[i].vehicleType);
			if (GUI.changed)
				GetVehicleTypePresetNames(i);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();


		EditorGUI.BeginChangeCheck();
		mainData.enableUndoForCleaingRoadData = EditorGUILayout.Toggle("Enable undo for clearing Road Data?",mainData.enableUndoForCleaingRoadData);
		mainData.enableUndoForLanesDeletion = EditorGUILayout.Toggle("Enable undo for deleting lanes?",mainData.enableUndoForLanesDeletion);
		mainData.allowDeadEndLanes = EditorGUILayout.Toggle("Allow Dead End Lanes?",mainData.allowDeadEndLanes);
		mainData.calculateNerbyPointsForPlayerFinding = EditorGUILayout.Toggle("Calculate Nerby Points For Player Finding?",mainData.calculateNerbyPointsForPlayerFinding);
		mainData.nearbyPointsRadius = EditorGUILayout.FloatField("Nearby points radius",mainData.nearbyPointsRadius);
		mainData.maxLaneCachedPerFrame = EditorGUILayout.FloatField("Max lanes cached per frame",mainData.maxLaneCachedPerFrame);
		if (GUI.changed || EditorGUI.EndChangeCheck())
			EditorUtility.SetDirty(mainData);

	}

	void GetVehicleTypePresetNames(int y)
	{
		GetEnumBools(manager.vehicleTypePresets[y].vehicleType);
		manager.vehicleTypePresets[y].name = "";
		if (manager.vehicleTypePresets[y].vehicleType == (TSLaneInfo.VehicleType)(-1))
			manager.vehicleTypePresets[y].name = "Everything";
		else if ((int)(manager.vehicleTypePresets[y].vehicleType) == 0)
			manager.vehicleTypePresets[y].name = "Nothing";
		else{
			for (int i = 0; i < vehicleTypesSelected.Length;i++){
				if (vehicleTypesSelected[i])
					manager.vehicleTypePresets[y].name += vehicleTypesNames[i] + " "; 
			}
		}
	}


		private Vector3[] points;
		bool dontDoAnything = false;




	void MultipleLanesSelection()
	{

		selectedLanes.Clear();
		if (mainData.useSphereSelection)
		{
			for (int i = 0; i < manager.lanes.Length;i++){
				if (Vector3.Distance(mainData.MultipleSelectionOrigin,manager.lanes[i].conectorA)< Mathf.Abs(mainData.MultipleSelectionRadius) &&
				    Vector3.Distance(mainData.MultipleSelectionOrigin,manager.lanes[i].conectorB)< Mathf.Abs(mainData.MultipleSelectionRadius) &&
				    Vector3.Distance(mainData.MultipleSelectionOrigin,manager.lanes[i].points[manager.lanes[i].points.Length/2].point)< Mathf.Abs(mainData.MultipleSelectionRadius))
				{
					selectedLanes.Add(manager.lanes[i]);
				}
			}
		}else{
			for (int i = 0; i < manager.lanes.Length;i++){
				if (mainData.MultipleSelectionBounds.Contains(manager.lanes[i].conectorA) &&
				    mainData.MultipleSelectionBounds.Contains(manager.lanes[i].conectorB)&&
				    mainData.MultipleSelectionBounds.Contains(manager.lanes[i].points[manager.lanes[i].points.Length/2].point))
				{
					selectedLanes.Add(manager.lanes[i]);
				}
			}
		}
	}

	void MultipleConnectorsSelection()
	{
		selectedConnectors.Clear();
		if (mainData.useSphereSelection)
		{
			for (int i = 0; i < manager.lanes.Length;i++){
				for (int y = 0; y < manager.lanes[i].connectors.Length;y++){
					if (Vector3.Distance(mainData.MultipleSelectionOrigin,manager.lanes[i].connectors[y].conectorA)< Mathf.Abs(mainData.MultipleSelectionRadius) &&
					    Vector3.Distance(mainData.MultipleSelectionOrigin,manager.lanes[i].connectors[y].conectorB)< Mathf.Abs(mainData.MultipleSelectionRadius) &&
					    Vector3.Distance(mainData.MultipleSelectionOrigin,manager.lanes[i].connectors[y].points[manager.lanes[i].connectors[y].points.Length/2].point)< Mathf.Abs(mainData.MultipleSelectionRadius))
					{
						selectedConnectors.Add(manager.lanes[i].connectors[y]);
					}
				}
			}
		}else{
			for (int i = 0; i < manager.lanes.Length;i++){
				for (int y = 0; y < manager.lanes[i].connectors.Length;y++){
					if (mainData.MultipleSelectionBounds.Contains(manager.lanes[i].connectors[y].conectorA) &&
					    mainData.MultipleSelectionBounds.Contains(manager.lanes[i].connectors[y].conectorB)&&
					    mainData.MultipleSelectionBounds.Contains(manager.lanes[i].connectors[y].points[manager.lanes[i].connectors[y].points.Length/2].point))
					{
						selectedConnectors.Add(manager.lanes[i].connectors[y]);
					}
				}
			}
		}
	}




	void UpdateMultiLaneConnectorSelection(int controlID)
	{
		bool selectConnector = manager.connectionsMenuSelection == 2 && manager.menuSelection ==1;
		bool selectLanes =manager.laneMenuSelection == 3 && manager.menuSelection ==0;
		if (selectLanes|| selectConnector)
		{
			DrawSelectionRectangle(selectConnector,selectLanes);

			if (Event.current.button == 0 ){
				switch(Event.current.GetTypeForControl(controlID))
				{
				case EventType.MouseDown:
				case EventType.MouseDrag:
					if (Event.current.shift){
						GUIUtility.hotControl = controlID;

						Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
						RaycastHit hit = new RaycastHit();
						bool rayHit =Physics.Raycast(ray, out hit);
						if (rayHit)
						{
							mainData.MultipleSelectionBounds.center = mainData.MultipleSelectionOrigin = hit.point;
						}else{
							mainData.MultipleSelectionBounds.center = mainData.MultipleSelectionOrigin = ray.origin + ray.direction * 10f;
						}
						 
						SceneView.RepaintAll();
					}
					break;
				case EventType.MouseUp:
					GUIUtility.hotControl = controlID;

					if (selectConnector)
						MultipleConnectorsSelection();
					if (selectLanes)
						MultipleLanesSelection();
					SceneView.RepaintAll();
					GUIUtility.hotControl = 0;
					break;
				}
			}

		}
	}


	Color cubeColor = new Color(1,0,0,0.15f);
	Color outlineColor = new Color(1,1,1,1);
	void DrawSelectionRectangle(bool selectConnector, bool selectLanes){

		if (!mainData.useSphereSelection){

			Handles.DrawSolidRectangleWithOutline(new Vector3[]{
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.min.z),
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.min.z),
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.min.z),
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.min.z),
			},cubeColor,outlineColor);

			Handles.DrawSolidRectangleWithOutline(new Vector3[]{
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.min.z),
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.max.z),
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.max.z),
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.min.z),
			},cubeColor,outlineColor);

			Handles.DrawSolidRectangleWithOutline(new Vector3[]{
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.min.z),
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.min.z),
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.max.z),
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.max.z),
			},cubeColor,outlineColor);


			Handles.DrawSolidRectangleWithOutline(new Vector3[]{
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.max.z),
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.min.z),
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.min.z),
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.max.z),
			},cubeColor,outlineColor);


			Handles.DrawSolidRectangleWithOutline(new Vector3[]{
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.max.z),
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.max.z),
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.max.z),
				new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.max.z),
			},cubeColor,outlineColor);

			Handles.DrawSolidRectangleWithOutline(new Vector3[]{
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.max.z),
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.max.z),
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.min.z),
				new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.min.z)
			},cubeColor,outlineColor);

			Vector3 size = mainData.MultipleSelectionBounds.min;
			Vector3 sizex = Handles.Slider2D(new Vector3(mainData.MultipleSelectionBounds.min.x,mainData.MultipleSelectionBounds.center.y,mainData.MultipleSelectionBounds.center.z),Vector3.right,Vector3.right, Vector3.up,HandleUtility.GetHandleSize(mainData.MultipleSelectionBounds.center)*0.05f,Handles.DotCap,0);
			Vector3 sizey = Handles.Slider2D(new Vector3(mainData.MultipleSelectionBounds.center.x,mainData.MultipleSelectionBounds.min.y,mainData.MultipleSelectionBounds.center.z),Vector3.right,Vector3.up, Vector3.right,HandleUtility.GetHandleSize(mainData.MultipleSelectionBounds.center)*0.05f,Handles.DotCap,0);
			Vector3 sizez = Handles.Slider2D(new Vector3(mainData.MultipleSelectionBounds.center.x,mainData.MultipleSelectionBounds.center.y,mainData.MultipleSelectionBounds.min.z),Vector3.right,Vector3.forward, Vector3.right,HandleUtility.GetHandleSize(mainData.MultipleSelectionBounds.center)*0.05f,Handles.DotCap,0);
			mainData.MultipleSelectionBounds.min = new Vector3( Mathf.Clamp(sizex.x,float.MinValue,mainData.MultipleSelectionBounds.max.x),Mathf.Clamp(sizey.y,float.MinValue,mainData.MultipleSelectionBounds.max.y),Mathf.Clamp(sizez.z,float.MinValue,mainData.MultipleSelectionBounds.max.z));

			Vector3 size1 = mainData.MultipleSelectionBounds.max;
			Vector3 sizex1 = Handles.Slider2D(new Vector3(mainData.MultipleSelectionBounds.max.x,mainData.MultipleSelectionBounds.center.y,mainData.MultipleSelectionBounds.center.z),Vector3.right,Vector3.right, Vector3.up,HandleUtility.GetHandleSize(mainData.MultipleSelectionBounds.center)*0.05f,Handles.DotCap,0);
			Vector3 sizey1 = Handles.Slider2D(new Vector3(mainData.MultipleSelectionBounds.center.x,mainData.MultipleSelectionBounds.max.y,mainData.MultipleSelectionBounds.center.z),Vector3.right,Vector3.up, Vector3.right,HandleUtility.GetHandleSize(mainData.MultipleSelectionBounds.center)*0.05f,Handles.DotCap,0);
			Vector3 sizez1 = Handles.Slider2D(new Vector3(mainData.MultipleSelectionBounds.center.x,mainData.MultipleSelectionBounds.center.y,mainData.MultipleSelectionBounds.max.z),Vector3.right,Vector3.forward, Vector3.right,HandleUtility.GetHandleSize(mainData.MultipleSelectionBounds.center)*0.05f,Handles.DotCap,0);
			mainData.MultipleSelectionBounds.max = new Vector3( sizex1.x,sizey1.y,sizez1.z);
			if (size != mainData.MultipleSelectionBounds.min|| size1 != mainData.MultipleSelectionBounds.max)
			{
				if (selectConnector)
					MultipleConnectorsSelection();
				if (selectLanes)
					MultipleLanesSelection();
			}
		}
		else{
			Color handleColor = Handles.color;
			Handles.color = cubeColor;
			Handles.SphereCap(0,mainData.MultipleSelectionOrigin,Quaternion.identity,mainData.MultipleSelectionRadius*2f);
			Handles.color = handleColor;
			float radius = mainData.MultipleSelectionRadius;
			mainData.MultipleSelectionRadius = Handles.RadiusHandle(Quaternion.identity,mainData.MultipleSelectionOrigin,mainData.MultipleSelectionRadius);
			if (radius != mainData.MultipleSelectionRadius)
			{
				if (selectConnector)
					MultipleConnectorsSelection();
				if (selectLanes)
					MultipleLanesSelection();
			}
		}

	}


	int lanesCounter =0;
	int currentLanesCounter=0;
	int lastLaneCount = 0;
	void CheckLanes()
	{
		int i=0;
		if (lanesCounter>=manager.lanes.Length)lanesCounter=0;
		for(;  lanesCounter < manager.lanes.Length; lanesCounter++){
			i = lanesCounter;
			if (currentLanesCounter > mainData.maxLaneCachedPerFrame)
			{
				currentLanesCounter=0;
				break;
			}
			Bounds	bounds = new Bounds(manager.lanes[i].conectorA,Vector3.one);
			bounds.Encapsulate(manager.lanes[i].conectorB);
			int midPointIndex =manager.lanes[i].points.Length/2;
			if (manager.lanes[i].points.Length!=0)
				bounds.Encapsulate(manager.lanes[i].points[midPointIndex].point);
			for (int ii =0; ii < manager.lanes[i].connectors.Length;ii++)
			{
				bounds.Encapsulate(manager.lanes[i].connectors[ii].conectorB); 
			}
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(SceneView.GetAllSceneCameras()[0]);
			
			if (GeometryUtility.TestPlanesAABB(planes, bounds)){
				tempLanes.Add(i);
			}
			else{
				tempLanes.Remove(i);
			}
			currentLanesCounter++;
		}
		if (lastLaneCount != tempLanes.Count)
		{
			lastLaneCount = tempLanes.Count;
			SceneView.RepaintAll();
		}
	}


	//OnSceneGUI start
    void OnSceneGUI() 
	{

		if (Tools.current == Tool.View || Tools.viewTool == ViewTool.Orbit || Tools.viewTool == ViewTool.FPS) dontDoAnything = true;
		else dontDoAnything = false;
		
		int controlID = GUIUtility.GetControlID(FocusType.Passive);

		UpdateMultiLaneConnectorSelection(controlID);





		bool editPoints1 = false;
		bool editConnectorPoints = false;
		
		Ray ray = HandleUtility.GUIPointToWorldRay((Event.current.mousePosition));

		RaycastHit hit = new RaycastHit();
		bool rayHit =Physics.Raycast(ray, out hit);
		
		if (manager.menuSelection == 0 && manager.laneMenuSelection == 2 && Event.current.button == 0  && !dontDoAnything)
		{

			editPoints1 = true;
			EditPoints(editPoints1, controlID, rayHit, hit);
		}
		
		Handles.color = Color.green;
		bool focused = false;
		bool selected = false;
		bool selectedConnector = false;
		bool connectorA = false;
		bool connectorB = false;
		int selectedLaneForConnector = -1;
		float minDist = float.MaxValue;

		if (manager.lanes == null) return;
//		for(int i = 0; i < manager.lanes.Length; i++){
		foreach(int lane in tempLanes){
			int i = lane;
			if (i >= manager.lanes.Length)continue;
			if ((i == currentLaneSelected || (manager.laneMenuSelection == 3 && selectedLanes.Contains(manager.lanes[i]))) && manager.menuSelection ==0) Handles.color = Color.yellow;
			focused = false;
			selected = false;
			bool draw = false;
			//Lane Drawing and selection
			if(manager.lanes[i].middlePoints.Count !=0){
				points =  new Vector3[manager.lanes[i].points.Length];
				int w = 0;

				Bounds	bounds = new Bounds(manager.lanes[i].conectorA,Vector3.one);
				bounds.Encapsulate(manager.lanes[i].conectorB);
//				if (w == manager.lanes[i].points.Length/2)
				bounds.Encapsulate(manager.lanes[i].points[manager.lanes[i].points.Length/2].point);
				for (int ii =0; ii < manager.lanes[i].connectors.Length;ii++)
				{
					bounds.Encapsulate(manager.lanes[i].connectors[ii].conectorB); 
				}
				Plane[] planes = GeometryUtility.CalculateFrustumPlanes(SceneView.GetAllSceneCameras()[0]);

				if (GeometryUtility.TestPlanesAABB(planes, bounds))
					draw = true;
				//				draw = true;
				if (!draw)continue;

				for (w = 0; w < manager.lanes[i].points.Length;w++)
				{
					points[w] = manager.lanes[i].points[w].point;
					float dist = (manager.lanes[i].points[w].point - hit.point).magnitude;
					
					bool shouldSelect = false;
					Vector2 screenP1 = HandleUtility.WorldToGUIPoint(w==0? manager.lanes[i].conectorA:manager.lanes[i].points[w-1].point);
					Vector2 screenP2 = HandleUtility.WorldToGUIPoint(manager.lanes[i].points[w].point);
					Vector2 p1 = (screenP1-screenP2);
					Vector2 p2= (screenP1- Event.current.mousePosition);
					float dot = Vector2.Dot(p1.normalized,p2.normalized);
					bool isLastGood = (w == manager.lanes[i].points.Length-1?(p2.sqrMagnitude < p1.sqrMagnitude):(p2.sqrMagnitude < p1.sqrMagnitude*2));
					if (dot > 0.95f && isLastGood)
						shouldSelect = true;
					
					
					if (rayHit && shouldSelect || (rayHit  && dist  <= 1* manager.scaleFactor && dist < minDist)) 
					//if (rayHit && dist  <= 1 && dist < minDist)
					{
						minDist = dist; 
						if ((Event.current.type ==  EventType.mouseDown || Event.current.type ==  EventType.mouseUp) && Event.current.button ==0  && (manager.laneMenuSelection == 0|| (manager.menuSelection == 1 && manager.connectionsMenuSelection == 0)) && !dontDoAnything){
							if (manager.menuSelection == 0)	
								selected = true;
							if (w == 0) connectorA = true;
							if (w == manager.lanes[i].points.Length-1) connectorB = true;
							GUIUtility.hotControl = controlID;
						}
						if (addConnector1)
							selectedLaneForConnector = i;
						if (manager.menuSelection == 0 && !removeLanePoint && manager.laneMenuSelection ==0){
							focused = true;
							Handles.color = Color.blue;
							SceneView.RepaintAll();
						}
					}


				}

				Handles.DrawAAPolyLine(manager.visualLinesWidth, points);

				if (manager.lanes[i].points.Length > 3){
					Vector3[] p = new Vector3[4];
					int index = w/2;
					Quaternion tempDir = Quaternion.LookRotation((points[index+1 < manager.lanes[i].points.Length? index+1:index]-points[index]));
					p[0] = points[index] + tempDir * Vector3.right * manager.lanes[i].laneWidth ;
					p[1]= points[index] + tempDir * Vector3.forward * 5*manager.scaleFactor ;
					p[2] = points[index] + tempDir * -Vector3.right  * manager.lanes[i].laneWidth ;
					p[3] = points[index] ;
					if (manager.lanes[i].totalDistance != 0){

						Handles.DrawSolidRectangleWithOutline(p, Handles.color,Handles.color);
						Handles.Label(p[1],"Lane " + i,EditorStyles.whiteLargeLabel);
						if (manager.menuSelection ==0 && manager.laneMenuSelection == 1){
							Handles.color = Color.blue;
							//Right
							bool isRight = false;
							bool isLeft = false;
							if (rayHit && (p[0] - hit.point).magnitude <= 1f * manager.scaleFactor){
								Handles.color = Color.yellow;
								isRight = true;
								isLeft = false;
							}
							Handles.DrawSolidDisc(p[0], Vector3.up, 0.5f  * (manager.scaleFactor/2f));
							Handles.color = Color.blue;
							//Left
							if (rayHit && (p[2] - hit.point).magnitude <= 1f* manager.scaleFactor){
								Handles.color = Color.red;
								isLeft = true;
								isRight = false;
							}
							Handles.DrawSolidDisc(p[2], Vector3.up, 0.5f * (manager.scaleFactor/2f));
							Handles.color = Color.green;
							if ((Event.current.type == EventType.mouseDown || Event.current.type == EventType.MouseDown) && (isRight || isLeft) && Event.current.button == 0&& (addLaneLink || removeLaneLink) && !dontDoAnything)
							{
								GUIUtility.hotControl = controlID;
								if (addLaneLink && !linkLane1Set)
								{
									linkLane1Set = true;
									linkLane1 = i;
									linkLane1Right = isRight;
									if (isRight)
										laneLinkPos = p[0];
									else laneLinkPos = p[2];
								}
								if (removeLaneLink)
								{
									Undo.RecordObject(manager,"Remove Lane Link");
									if (isRight){
										if (manager.lanes[i].laneLinkRight!=-1){
											if (manager.lanes[manager.lanes[i].laneLinkRight].laneLinkLeft ==i)
												manager.lanes[manager.lanes[i].laneLinkRight].laneLinkLeft = -1;
											else if (manager.lanes[manager.lanes[i].laneLinkRight].laneLinkRight ==i)
												manager.lanes[manager.lanes[i].laneLinkRight].laneLinkRight = -1;
										}
										manager.lanes[i].laneLinkRight = -1;
									}
									if (isLeft){
										if (manager.lanes[i].laneLinkLeft !=-1){
											if (manager.lanes[manager.lanes[i].laneLinkLeft].laneLinkLeft ==i)
												manager.lanes[manager.lanes[i].laneLinkLeft].laneLinkLeft = -1;
											else if (manager.lanes[manager.lanes[i].laneLinkLeft].laneLinkRight ==i)
												manager.lanes[manager.lanes[i].laneLinkLeft].laneLinkRight = -1;

										}
										manager.lanes[i].laneLinkLeft = -1;
									}
								}
								GUIUtility.hotControl = 0;
								Event.current.Use();
							}
							if ((Event.current.type == EventType.mouseUp || Event.current.type == EventType.MouseUp) && (isRight || isLeft)&& Event.current.button == 0&& addLaneLink && !dontDoAnything)
							{
								Undo.RecordObject(manager,"Add Lane Link");
								GUIUtility.hotControl = controlID;
								if (addLaneLink && !linkLane2Set)
								{
									linkLane2Set = true;
									linkLane2 = i;
									linkLane2Right = isRight;
								}
								if (linkLane1Right && !linkLane2Right && linkLane1Set && linkLane2Set ){
									if (manager.lanes[linkLane1] != manager.lanes[linkLane2]){
										manager.lanes[linkLane1].laneLinkRight = linkLane2;
										manager.lanes[linkLane2].laneLinkLeft = linkLane1;
									}
									linkLane1 = -1;
									linkLane2 = -1;
									linkLane1Set = false;
									linkLane2Set = false;
								}else if (!linkLane1Right && linkLane2Right && linkLane1Set && linkLane2Set ){
									if (manager.lanes[linkLane1] != manager.lanes[linkLane2]){
										manager.lanes[linkLane1].laneLinkLeft = linkLane2;
										manager.lanes[linkLane2].laneLinkRight = linkLane1;
									}
									linkLane1 = -1;
									linkLane2 = -1;
									linkLane1Set = false;
									linkLane2Set = false;
								}else if (!linkLane1Right && !linkLane2Right && linkLane1Set && linkLane2Set ){
									if (manager.lanes[linkLane1] != manager.lanes[linkLane2]){
										manager.lanes[linkLane1].laneLinkLeft = linkLane2;
										manager.lanes[linkLane2].laneLinkLeft = linkLane1;
									}
									linkLane1 = -1;
									linkLane2 = -1;
									linkLane1Set = false;
									linkLane2Set = false;
								}else if (linkLane1Right && linkLane2Right && linkLane1Set && linkLane2Set ){
									if (manager.lanes[linkLane1] != manager.lanes[linkLane2]){
										manager.lanes[linkLane1].laneLinkRight = linkLane2;
										manager.lanes[linkLane2].laneLinkRight = linkLane1;
									}
									linkLane1 = -1;
									linkLane2 = -1;
									linkLane1Set = false;
									linkLane2Set = false;
								}
								
								GUIUtility.hotControl = 0;
								Event.current.Use();
							}
							
							if (linkLane1Set && !linkLane2Set)
							{
								Handles.DrawLine(laneLinkPos,hit.point);
							}
							
							if (manager.lanes[i].laneLinkLeft != -1)
							{

								Vector3 p1 = Vector3.zero;
								int index1 = (manager.lanes[manager.lanes[i].laneLinkLeft].points.Length) /2;
								Quaternion tempDir1 = Quaternion.LookRotation((manager.lanes[manager.lanes[i].laneLinkLeft].points[index1+1 < manager.lanes[manager.lanes[i].laneLinkLeft].points.Length? index1+1:index1].point-manager.lanes[manager.lanes[i].laneLinkLeft].points[index1].point));

								if (manager.lanes[manager.lanes[i].laneLinkLeft].laneLinkRight == i)
									p1 = manager.lanes[manager.lanes[i].laneLinkLeft].points[index1].point + tempDir1 * Vector3.right  * manager.lanes[manager.lanes[i].laneLinkLeft].laneWidth ;
								else if (manager.lanes[manager.lanes[i].laneLinkLeft].laneLinkLeft == i)
									p1 = manager.lanes[manager.lanes[i].laneLinkLeft].points[index1].point + tempDir1 * -Vector3.right  * manager.lanes[manager.lanes[i].laneLinkLeft].laneWidth ;
								Handles.DrawLine(p[2],p1);
							}
							if (manager.lanes[i].laneLinkRight != -1)
							{
								Vector3 p1 = Vector3.zero;
								int index1 = (manager.lanes[manager.lanes[i].laneLinkRight].points.Length) /2;
								Quaternion tempDir1 = Quaternion.LookRotation((manager.lanes[manager.lanes[i].laneLinkRight].points[index1+1 < manager.lanes[manager.lanes[i].laneLinkRight].points.Length? index1+1:index1].point-manager.lanes[manager.lanes[i].laneLinkRight].points[index1].point));

								if (manager.lanes[manager.lanes[i].laneLinkRight].laneLinkLeft == i)
									p1 = manager.lanes[manager.lanes[i].laneLinkRight].points[index1].point + tempDir1 * -Vector3.right * manager.lanes[manager.lanes[i].laneLinkRight].laneWidth ;
								else if (manager.lanes[manager.lanes[i].laneLinkRight].laneLinkRight == i)
									p1 = manager.lanes[manager.lanes[i].laneLinkRight].points[index1].point + tempDir1 * Vector3.right * manager.lanes[manager.lanes[i].laneLinkRight].laneWidth ;
								Handles.DrawLine(p[0],p1);
							}
							
//							SceneView.RepaintAll();
						}
					}
					
				}
			}
			Handles.color = Color.grey;
			//Connectors drawing and selection
			float connectoMinDist = float.MaxValue;
			for (int y = 0; y < manager.lanes[i].connectors.Length; y++)
			{
				if(manager.lanes[i].connectors[y].middlePoints.Count !=0){
					if (manager.lanes[i].connectors[y].conectorA != manager.lanes[i].conectorB)
					{
						manager.lanes[i].connectors[y].conectorA = manager.lanes[i].conectorB;
						editConnectorPoints = true;
						EditPoints(editConnectorPoints, controlID, rayHit, hit,ref manager.lanes[i].connectors[y].middlePoints,ref manager.lanes[i].connectors[y].conectorA, ref manager.lanes[i].connectors[y].conectorB, ref manager.lanes[i].connectors[y].points, ref manager.lanes[i].connectors[y].totalDistance, true, true);
						SceneView.RepaintAll();
					}
					if (manager.lanes[i].connectors[y].conectorB != manager.lanes[manager.lanes[i].connectors[y].nextLane].conectorA)
					{
						manager.lanes[i].connectors[y].conectorB = manager.lanes[manager.lanes[i].connectors[y].nextLane].conectorA;
						editConnectorPoints = true;
						EditPoints(editConnectorPoints, controlID, rayHit, hit,ref manager.lanes[i].connectors[y].middlePoints,ref manager.lanes[i].connectors[y].conectorA, ref manager.lanes[i].connectors[y].conectorB, ref manager.lanes[i].connectors[y].points, ref manager.lanes[i].connectors[y].totalDistance, true, true);
						SceneView.RepaintAll();
					}
					points =  new Vector3[manager.lanes[i].connectors[y].points.Length];
					int w = 0;
					Handles.color = Color.magenta;
					for (w = 0; w < manager.lanes[i].connectors[y].points.Length;w++)
					{
						points[w] = manager.lanes[i].connectors[y].points[w].point;
						float dist = (manager.lanes[i].connectors[y].points[w].point - hit.point).magnitude;
						
						bool shouldSelect = false;
						Vector2 screenP1 = HandleUtility.WorldToGUIPoint(w==0? manager.lanes[i].connectors[y].conectorA:manager.lanes[i].connectors[y].points[w-1].point);
						Vector2 screenP2 = HandleUtility.WorldToGUIPoint(manager.lanes[i].connectors[y].points[w].point);
						Vector2 p1 = (screenP1-screenP2);
						Vector2 p2= (screenP1- Event.current.mousePosition);
						float dot = Vector2.Dot(p1.normalized,p2.normalized);
						bool isLastGood = (w == manager.lanes[i].connectors[y].points.Length-1?(p2.sqrMagnitude < p1.sqrMagnitude):(p2.sqrMagnitude < p1.sqrMagnitude*2));
						if (dot > 0.95f && isLastGood)
							shouldSelect = true;
					
					
						if (rayHit && shouldSelect || (rayHit && dist <= 1 && dist < connectoMinDist)) 
						//if (rayHit && dist <= 1 && dist < connectoMinDist)
						{
							connectoMinDist = dist;
							if (manager.menuSelection == 1 && !addConnector1)// && (manager.connectionsMenuSelection != 2 || removeConnector))
							{
								bool entered = false;
								if ((Event.current.type ==  EventType.mouseDown || Event.current.type ==  EventType.mouseUp) && Event.current.button ==0  && !dontDoAnything){
									entered = true;
									GUIUtility.hotControl = controlID;
									if (manager.connectionsMenuSelection == 0){
										
										currentConnectorSelected = y;
										selectedConnector = true;
										selectedLaneForConnector = i;
										currentLaneSelected = i;

									}
								}
								if (entered && !selectedConnector)
									GUIUtility.hotControl = 0;
								Handles.color = Color.blue;
								SceneView.RepaintAll();
							}
						}
						if (((currentConnectorSelected == y && currentLaneSelected == i )
						     || (manager.connectionsMenuSelection == 2 
						 && selectedConnectors.Contains(manager.lanes[i].connectors[y])))
						    && manager.menuSelection ==1)
								Handles.color = Color.yellow;
					}


					Handles.DrawAAPolyLine(manager.visualLinesWidth, points);
					Handles.color = Color.green;
				}
			}
			Handles.color = Color.green;
			if (newConnector!=null && newConnector.points !=null && newConnector.points.Length != 0)
			{
				points =  new Vector3[newConnector.points.Length];
				for (int w = 0; w < newConnector.points.Length;w++)
					points[w] = newConnector.points[w].point;
				Handles.DrawAAPolyLine(manager.visualLinesWidth, points);
			}
			//If add connectors is active, draw spheres on the lanes connectors A and B
			if (addConnector1){
				if (newConnector != null ){
					Handles.color = Color.green;
					if (newConnector != null && newConnector.conectorB == manager.lanes[i].conectorA || ( (newConnector.conectorB - manager.lanes[i].conectorA).magnitude < 3 * manager.scaleFactor))
						Handles.color = Color.white;
					Handles.SphereCap(0,manager.lanes[i].conectorA,Quaternion.identity,2 * manager.scaleFactor);
					Handles.color = Color.blue;
					if (newConnector != null && newConnector.conectorA == manager.lanes[i].conectorB || ((newConnector.conectorA - manager.lanes[i].conectorB).magnitude < 3 * manager.scaleFactor))
						Handles.color = Color.white;
					Handles.SphereCap(0,manager.lanes[i].conectorB,Quaternion.identity,2 * manager.scaleFactor);
					Handles.color = Color.green;
				}
			}
			
			//Check if a lane is focused or selected
			if (focused)Handles.color = Color.green;
			if (selected && manager.menuSelection ==0) {currentLaneSelected = i;
				Repaint();
				if (removeLane){ 
					DeleteLanesCheck();
					GUIUtility.hotControl = 0;
					break;
				}
				GUIUtility.hotControl = 0;
				if (manager.menuSelection != 1 && manager.connectionsMenuSelection !=0) Event.current.Use();
				SceneView.RepaintAll();
			}
			
			
			
			if (Handles.color == Color.yellow) Handles.color = Color.green;
		}
	
		if (selectedConnector && manager.menuSelection ==1) {
			Repaint();
				if (removeConnector){
				Undo.RecordObject(manager,"Delete Connector");
				RemoveConnector(currentLaneSelected,currentConnectorSelected);
					Event.current.Use();GUIUtility.hotControl = 0;return;
				}
				GUIUtility.hotControl = 0;
				if (manager.menuSelection == 1 && manager.connectionsMenuSelection ==0 && !addConnector1) Event.current.Use();
				SceneView.RepaintAll();
			}
		
		if (manager.menuSelection == 1 && manager.connectionsMenuSelection == 1 && Event.current.button == 0)
		{
			editConnectorPoints = true;
			editPoints1 = false;
			if (currentLaneSelected >=0 && currentLaneSelected < manager.lanes.Length && manager.lanes.Length>0)
				if(currentConnectorSelected >=0 && currentConnectorSelected < manager.lanes[currentLaneSelected].connectors.Length && manager.lanes[currentLaneSelected].connectors.Length >0)
				EditPoints(editConnectorPoints, controlID, rayHit, hit,ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].middlePoints,ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].conectorA, ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].conectorB, ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].points, ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].totalDistance, true, false);
		}
		
		AddLanesAndConnectors(rayHit,hit, addLane1, addConnector1, controlID, connectorA, connectorB, selectedLaneForConnector);
	}//OnSceneGUI end

	void RemoveConnector(int selectedLane,int selectedConnector)
	{
		manager.lanes[selectedLane].connectors = manager.lanes[selectedLane].connectors.Remove(manager.lanes[selectedLane].connectors[selectedConnector]);
		manager.junctionsProcessed = false;
	}

	void RemoveLane(int selectedLane)
	{
		if (selectedLane == -1)return;
		if (onLaneDeleted != null)onLaneDeleted(selectedLane);
		for (int q = 0; q < manager.lanes.Length;q++)
		{
			for (int u = 0; u < manager.lanes[q].connectors.Length;u++)
			{
				if (manager.lanes[q].connectors[u].nextLane == selectedLane)
					manager.lanes[q].connectors = manager.lanes[q].connectors.Remove(manager.lanes[q].connectors[u]);
			}
			for (int u = 0; u < manager.lanes[q].connectors.Length;u++)
			{
				if (manager.lanes[q].connectors[u].nextLane > selectedLane)
					manager.lanes[q].connectors[u].nextLane--;
			}
			CheckLaneLinking(q,selectedLane);

		}
		manager.lanes = manager.lanes.Remove(manager.lanes[selectedLane]);
		tempLanes.Remove(selectedLane);
		manager.junctionsProcessed = false;

	}

	void CheckLaneLinking(int laneToCheck, int removedLane)
	{
		if (manager.lanes[laneToCheck].laneLinkLeft == removedLane)
		{
			manager.lanes[laneToCheck].laneLinkLeft = -1;
		}
		if (manager.lanes[laneToCheck].laneLinkLeft > removedLane)
		{
			manager.lanes[laneToCheck].laneLinkLeft--;
		}
		if (manager.lanes[laneToCheck].laneLinkRight == removedLane)
		{
			manager.lanes[laneToCheck].laneLinkRight = -1;
		}
		if (manager.lanes[laneToCheck].laneLinkRight > removedLane)
		{
			manager.lanes[laneToCheck].laneLinkRight--;
		}
	}
	
	public void ReserveNearConnectorPoints(TSPoints points,TSLaneConnector connector)
	{
		for (int pointIndex = 0; pointIndex < points.otherConnectorsPoints.Length; pointIndex++)
		{
			if (manager.lanes[points.otherConnectorsPoints[pointIndex].lane].connectors[points.otherConnectorsPoints[pointIndex].connector].points[points.otherConnectorsPoints[pointIndex].pointIndex].reservationID != 10000)
				manager.lanes[points.otherConnectorsPoints[pointIndex].lane].connectors[points.otherConnectorsPoints[pointIndex].connector].points[points.otherConnectorsPoints[pointIndex].pointIndex].reservationID = 10000;
			else manager.lanes[points.otherConnectorsPoints[pointIndex].lane].connectors[points.otherConnectorsPoints[pointIndex].connector].points[points.otherConnectorsPoints[pointIndex].pointIndex].reservationID = 0;
		}
	}
	
	
	
	
	void DrawFunc(int iD, Vector3 pos, Quaternion rot, float size)
	{
		Handles.DrawAAPolyLine(manager.visualLinesWidth, points);
	}
	
	void EditPoints(bool editPoints1, int controlID, bool rayHit, RaycastHit hit)
	{
		if (editPoints1 && currentLaneSelected != -1 && currentLaneSelected < manager.lanes.Length)
		EditPoints(editPoints1, controlID, rayHit, hit,ref manager.lanes[currentLaneSelected].middlePoints, 
			ref manager.lanes[currentLaneSelected].conectorA, ref manager.lanes[currentLaneSelected].conectorB, 
			ref manager.lanes[currentLaneSelected].points, ref manager.lanes[currentLaneSelected].totalDistance, false, false);
	}
	
	void EditPoints(bool editPoints1, int controlID, bool rayHit, RaycastHit hit,ref List<Vector3> middlePoints,ref Vector3 conectorA,ref Vector3 conectorB, ref TSPoints[] points, ref float totalDistance, bool isConnector, bool forcedUpdate)
	{
		bool changedPosition = false;
		if (forcedUpdate) changedPosition = true;
		if (editPoints1)
		{
			if (mainData.enableUndoForLanesDeletion)
				Undo.RecordObject(manager,"Edit points");
			Vector3 lastPos = Vector3.zero;
			for(int i = 0; i < middlePoints.Count;i++){
				if ( (manager.menuSelection == 0 && !removeLanePoint) || (manager.menuSelection == 1 && !removeConnectorPoint)){
					lastPos = middlePoints[i];
					middlePoints[i] = Handles.PositionHandle (middlePoints[i],Quaternion.identity);
					if (lastPos != middlePoints[i])changedPosition = true;
				}
				Handles.color = Color.red;
				Handles.SphereCap(0,middlePoints[i],Quaternion.identity,2 * manager.scaleFactor);
				
			}
			if (!isConnector){
				lastPos = conectorA;
				conectorA = Handles.PositionHandle (conectorA,Quaternion.identity);
				if (lastPos != conectorA)changedPosition = true;
				lastPos = conectorB;
				conectorB = Handles.PositionHandle (conectorB,Quaternion.identity);
				if (lastPos != conectorB)changedPosition = true;
				Handles.color = Color.green;
				Handles.SphereCap(0,conectorA,Quaternion.identity,2 * manager.scaleFactor);
				Handles.SphereCap(0,conectorB,Quaternion.identity,2 * manager.scaleFactor);
			}
			if (changedPosition)
			{

				manager.junctionsProcessed = false;
				RefreshPoints(conectorA,conectorB,middlePoints,ref points, ref totalDistance,isConnector);
			}
			if (Event.current.type == EventType.mouseDown && changedPosition == false && rayHit && ((manager.menuSelection == 0 && manager.laneMenuSelection == 2 && !removeLanePoint) || (manager.menuSelection == 1 && manager.connectionsMenuSelection == 1 && !removeConnectorPoint) )  && !dontDoAnything)
			{
				GUIUtility.hotControl = controlID;
				int insertPosition = 0;
				float hitDistance = (conectorA - hit.point).magnitude;
				int nearPoint = 0;
				float maxDist = float.MaxValue;
				for (insertPosition = 0; insertPosition < middlePoints.Count;insertPosition++)
				{
					float currentDist = (hit.point - middlePoints[insertPosition]).magnitude;
					if (currentDist < maxDist)
					{
						maxDist = currentDist;
						nearPoint = insertPosition;
					}
				}
				float hitConnectorBDist = (conectorB - hit.point).magnitude;
				if (hitConnectorBDist < maxDist)nearPoint = middlePoints.Count-1;
				bool insertBeforeConnectorA = false;
				bool insertAfterConnectorB = false;
				if (nearPoint == 0)
				{
					//If this is near the first middle point, we  need to compare both points distances to
					//connector A to see which one is near and insert the point acordingly
					float currentDist = (conectorA - middlePoints[nearPoint]).magnitude;

					insertBeforeConnectorA = CheckBehindPoint(conectorA,middlePoints[nearPoint],hit.point);

					if (hitDistance < currentDist)
					{
						nearPoint = 0;

					}
					else nearPoint = 1;

					if (insertBeforeConnectorA){
						nearPoint =0;
					}
					if (middlePoints.Count == 1){
						insertAfterConnectorB = CheckBehindPoint(conectorB,middlePoints[0],hit.point);
					}
				}else{
					float currentDist = (middlePoints[nearPoint-1] - middlePoints[nearPoint]).magnitude;
					hitDistance = (middlePoints[nearPoint-1] - hit.point).magnitude;

					if (nearPoint == middlePoints.Count-1)
					{
						insertAfterConnectorB = CheckBehindPoint(conectorB,middlePoints[nearPoint],hit.point);
						if (insertAfterConnectorB){
							nearPoint = middlePoints.Count;
							hitDistance = currentDist -1;
						}
					}

					if (hitDistance > currentDist)
						nearPoint++;
				}
				if (Event.current.shift)
				{
					insertBeforeConnectorA = false;
					insertAfterConnectorB = true;
					nearPoint = middlePoints.Count;
				}
				if (insertBeforeConnectorA)
				{
					middlePoints.Insert(nearPoint, conectorA);
					conectorA = hit.point;
				}else if (insertAfterConnectorB)
				{
					middlePoints.Insert(nearPoint, conectorB);
					conectorB = hit.point;
				}else{
					middlePoints.Insert(nearPoint, hit.point);
				}
				Event.current.Use();
				GUIUtility.hotControl = 0;

				RefreshPoints(conectorA,conectorB,middlePoints,ref points, ref totalDistance,isConnector);
				manager.junctionsProcessed = false;
			}
			
			if (Event.current.type ==  EventType.mouseDown && Event.current.button ==0 && !dontDoAnything){
				float minDist = float.MaxValue;
				int pointIndex = -1;
				for (int w = 0; w < middlePoints.Count;w++){
					if ((manager.menuSelection == 0 && manager.laneMenuSelection == 2 && removeLanePoint)||(manager.menuSelection == 1 && manager.connectionsMenuSelection == 1 && removeConnectorPoint)){
						float dist = (middlePoints[w] - hit.point).magnitude;
						if (rayHit && dist < minDist && dist <= 1 && middlePoints.Count > 1)
						{
							minDist = dist;
							pointIndex = w;
						}
					}
				}
				if (pointIndex !=-1)
					middlePoints.Remove(middlePoints[pointIndex]);
				Event.current.Use();
				GUIUtility.hotControl = 0;

				RefreshPoints(conectorA,conectorB,middlePoints,ref points, ref totalDistance,isConnector);
				manager.junctionsProcessed = false;
			}

		}
	}


	bool CheckBehindPoint(Vector3 point1, Vector3 point2, Vector3 hitPoint)
	{
		GameObject tempObject = new GameObject();
		tempObject.transform.position = point1;
		tempObject.transform.rotation = Quaternion.LookRotation(point1 - point2);
		float behind = tempObject.transform.InverseTransformPoint(hitPoint).z;
		DestroyImmediate(tempObject);
		if (behind > 0){
			return true;
		}
		return false;
	}

	/// <summary>
	/// Refreshs the lanes.
	/// </summary>
	/// <param name="showProgressBar">If set to <c>true</c> show progress bar.</param>
	void RefreshLanes(bool showProgressBar)
	{
		
		float progress = 0;
		if (showProgressBar)
			EditorUtility.DisplayProgressBar("Processing lanes points","Starting....",progress);
		for (int i = 0; i < manager.lanes.Length; i++)
		{
			RefreshPoints(manager.lanes[i].conectorA,
			              manager.lanes[i].conectorB,
			              manager.lanes[i].middlePoints,
			              ref manager.lanes[i].points,
			              ref manager.lanes[i].totalDistance,
			              false);
			if (showProgressBar){
				progress = i/manager.lanes.Length;
				EditorUtility.DisplayProgressBar("Processing lanes points","Lane" + i +"/"+manager.lanes.Length ,progress);
				
			}
		}

		EditorUtility.ClearProgressBar();
		ProcessJunctions();
	}


	void RefreshConnectors(bool showProgressBar)
	{
		
		float progress = 0;
		if (showProgressBar)
			EditorUtility.DisplayProgressBar("Processing connectors points","Starting....",progress);
		for (int i = 0; i < manager.lanes.Length; i++)
		{
			for (int w = 0; w < manager.lanes[i].connectors.Length; w++)
			{
				RefreshPoints(manager.lanes[i].connectors[w].conectorA,
				              manager.lanes[i].connectors[w].conectorB,
				              manager.lanes[i].connectors[w].middlePoints,
				              ref manager.lanes[i].connectors[w].points,
				              ref manager.lanes[i].connectors[w].totalDistance,
				              true);
				if (showProgressBar){
					progress = i/manager.lanes.Length;
					EditorUtility.DisplayProgressBar("Processing connectors points","Lane:" + i +"/"+manager.lanes.Length + " Connector:"+ w +"/"+manager.lanes[i].connectors.Length,progress);
					
				}
			}
		}
		EditorUtility.ClearProgressBar();
		ProcessJunctions();
	}



	/// <summary>
	/// Refreshs the points.
	/// </summary>
	/// <param name="conectorA">Conector a.</param>
	/// <param name="conectorB">Conector b.</param>
	/// <param name="middlePoints">Middle points.</param>
	/// <param name="points">Points.</param>
	/// <param name="totalDistance">Total distance.</param>
	void RefreshPoints(Vector3 conectorA,Vector3 conectorB, List<Vector3> middlePoints,ref TSPoints[] points, ref float totalDistance, bool isConnector)
	{
		Vector3[] pts = new Vector3[4 + middlePoints.Count];
		pts[0] = conectorA;
		pts[1] = conectorA;
		int r = 2;
		for (r =2; r < (2+middlePoints.Count) ;r++)
			pts[r] = middlePoints[r-2];
		
		pts[r] = conectorB;
		pts[r+1] = conectorB;
		points = new TSPoints[0];

		createPoints(isConnector?manager.resolutionConnectors:manager.resolution,pts,ref points,ref totalDistance);
	}


	void AddLanesAndConnectors(bool rayHit, RaycastHit hit, bool addLane1, bool addConnector1, int controlID, bool connectorA, bool connectorB, int lane)
	{
		if (Event.current.button == 0 && !dontDoAnything && ((addLane1 && manager.menuSelection == 0 && manager.laneMenuSelection == 0) || (addConnector1 && manager.menuSelection == 1 && manager.connectionsMenuSelection == 0)))
		{

			switch(Event.current.GetTypeForControl(controlID))
			{
			case EventType.MouseDown:

				GUIUtility.hotControl = controlID;
		        if (rayHit) {
					if (addLane1){
						Undo.RecordObject(manager,"Add new Lane"); 
						addLane = true;
						AddLane(hit.point, false);
					}
					if (addConnector1 && manager.lanes.Length >0)
					{


						addConnector = true;
						AddConnector(lane,connectorA, connectorB, false, hit.point);
					}
				}
				Event.current.Use();
				break;
				
			case EventType.MouseUp:
				GUIUtility.hotControl = 0;
		        if (rayHit) {
					if (addLane){
						addLane = false;
						AddLane(hit.point, true);
						manager.junctionsProcessed = false;
					}
					if (addConnector && manager.lanes.Length >0)
					{

						addConnector = false;
						AddConnector(lane,connectorA, connectorB, false, hit.point);

					}
				}
				Event.current.Use();
				break;
				
			case EventType.mouseDrag:
				if (rayHit) {
					if (addLane){
						addLane = false;
						AddLane(hit.point,false);
						addLane = true;
						SceneView.RepaintAll();
					}
					if (addConnector && manager.lanes.Length >0)
					{

						addConnector = false;
						AddConnector(lane,connectorA, connectorB, true, hit.point);
						addConnector = true;
						SceneView.RepaintAll();
					}
				}
				Event.current.Use();
				break;
				
			}
		}
	}
	
	
	void AddLane(Vector3 position, bool finished)
	{
		if (addLane)
		{
			TSLaneInfo newLane = new TSLaneInfo();
			newLane.conectorA = newLane.conectorB = position;
			newLane.connectors = new TSLaneConnector[0];
			newLane.points = new TSPoints[0];
			newLane.vehicleType = manager.defaultVehicleType;
			manager.lanes = manager.lanes.Add(newLane);
		}
		else
		{
			manager.lanes[manager.lanes.Length-1].conectorB = position;
			manager.lanes[manager.lanes.Length-1].middlePoints = new List<Vector3>();
			manager.lanes[manager.lanes.Length-1].middlePoints.Add((manager.lanes[manager.lanes.Length-1].conectorA + manager.lanes[manager.lanes.Length-1].conectorB )/2f);
			Vector3[] pts = new Vector3[5];
			pts[0] = manager.lanes[manager.lanes.Length-1].conectorA;
			pts[1] = manager.lanes[manager.lanes.Length-1].conectorA;
			pts[2] = manager.lanes[manager.lanes.Length-1].middlePoints[0];
			pts[3] = manager.lanes[manager.lanes.Length-1].conectorB;
			pts[4] = manager.lanes[manager.lanes.Length-1].conectorB;
			manager.lanes[manager.lanes.Length-1].points = new TSPoints[0];// List<TSPoints>();
			createPoints(manager.resolution,pts,ref manager.lanes[manager.lanes.Length-1].points,ref manager.lanes[manager.lanes.Length-1].totalDistance);
			currentLaneSelected = manager.lanes.Length-1;
			if (finished && manager.lanes[currentLaneSelected].points.Length <3){
				RemoveLane(currentLaneSelected);
				currentLaneSelected = manager.lanes.Length-1;
			}
			
		}
	}
	
	private bool addedConnectorA = false;
	private bool addedConnectorB = false;
	private int laneFrom = -1;
	private int laneTo = -1;
	
	void AddConnector(int lane, bool connectorA, bool connectorB, bool dragging, Vector3 position)
	{
		if (addConnector)
		{
			addedConnectorA = false;
			addedConnectorB = false;
			if (connectorA)
			{
				addedConnectorB = true;
				newConnector.conectorA = newConnector.conectorB = manager.lanes[lane].conectorA;
				newConnector.nextLane = lane;
				laneTo = lane;

			}else if (connectorB){
				addedConnectorA = true;
				laneFrom = lane;
				newConnector.conectorA = newConnector.conectorB = manager.lanes[lane].conectorB;
			}
		}
		else
		{
			bool finished = false;
			float sign = 0;
			float angle = 0;
			if (!dragging){
				if (connectorA && !addedConnectorB)
				{
					addedConnectorB = true;
					newConnector.nextLane = lane;
					laneTo = lane;
					newConnector.conectorB = manager.lanes[lane].conectorA;
				}
				if (connectorB && !addedConnectorA)
				{
					addedConnectorA = true;
					laneFrom = lane;
					newConnector.conectorA = manager.lanes[lane].conectorB;
				}
				if (addedConnectorA && addedConnectorB){
					angle = Vector3.Angle(manager.lanes[laneTo].conectorA - manager.lanes[laneTo].points[1].point,manager.lanes[laneFrom].points[manager.lanes[laneFrom].points.Length-2].point-manager.lanes[laneFrom ].conectorB );
					Vector3 referenceRight= Vector3.Cross(Vector3.up, manager.lanes[laneFrom].points[manager.lanes[laneFrom].points.Length-2].point-manager.lanes[laneFrom ].conectorB );
					sign = Mathf.Sign(Vector3.Dot(manager.lanes[laneTo].conectorA - manager.lanes[laneTo].points[1].point, referenceRight));// >= 0.0f) ? 1.0f: -1.0f;
					angle *= sign;
					if (angle > 30) newConnector.direction = TSLaneConnector.Direction.Right;
					else if (angle < -30) newConnector.direction = TSLaneConnector.Direction.Left;
					else newConnector.direction = TSLaneConnector.Direction.Straight;
					finished = true;
				}
			}else
			{
				if (!addedConnectorA){
					newConnector.conectorA = position;
				}
				if (!addedConnectorB){
					newConnector.conectorB = position;
				}
				
			}
			newConnector.middlePoints = new List<Vector3>();
			newConnector.middlePoints.Add(((newConnector.conectorA + newConnector.conectorB )/2f));
			
			if (finished){
				Undo.RecordObject(manager,"Add new Connector");
				manager.junctionsProcessed = false;
				if (Mathf.Abs(angle) > 20){
					float multiplier = (Mathf.Abs(angle)/90f)*0.35f;
					Quaternion tempDir = Quaternion.LookRotation(manager.lanes[laneTo].conectorA-manager.lanes[laneTo].points[1].point);
					Quaternion tempDir1 = Quaternion.LookRotation(manager.lanes[laneFrom ].points[manager.lanes[laneFrom ].points.Length-2].point-manager.lanes[laneFrom].conectorB);
					
					GameObject tempConnectorA = new GameObject();
					tempConnectorA.transform.position = newConnector.conectorA;
					tempConnectorA.transform.rotation = tempDir1;
					Vector3 tempDistance = tempConnectorA.transform.InverseTransformPoint(newConnector.conectorB);

					DestroyImmediate(tempConnectorA);
					newConnector.middlePoints[0] += (tempDir *  Vector3.forward * Mathf.Abs(tempDistance.x)*multiplier) + (tempDir1 * -Vector3.forward * Mathf.Abs(tempDistance.z)*multiplier);
				}
			}
			Vector3[] pts = new Vector3[5];
			pts[0] = newConnector.conectorA;
			pts[1] = newConnector.conectorA;
			pts[2] = newConnector.middlePoints[0];
			pts[3] = newConnector.conectorB;
			pts[4] = newConnector.conectorB;
			newConnector.points = new TSPoints[0];
			createPoints(manager.resolution,pts,ref newConnector.points, ref newConnector.totalDistance);
			if (addedConnectorA && addedConnectorB) {
				manager.lanes[laneFrom].connectors = manager.lanes[laneFrom].connectors.Add(newConnector);
				
			}
			if (finished)newConnector = new TSLaneConnector();
		}
	}
	
	
	// For interpolations
	Vector3 Interp(float t, Vector3[] pts) {
		//		float t = currPt/100;
		int numSections = pts.Length - 3;
		
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
		
		float u = t * (float) numSections - (float) currPt;
		
		Vector3 a = pts[currPt];
		Vector3 b = pts[currPt + 1 ];
		Vector3 c = pts[currPt + 2];
		Vector3 d = pts[currPt + 3];
		
		
		return .5f * (
			(-a + 3f * b - 3f * c + d) * (u * u * u)+ (2f * a - 5f * b + 4f * c - d) * (u * u)
			+ (-a + c) * u
			+ 2f * b);
	}



	// For interpolations
	Vector3 InterpEvenly(float t, Vector3[] pts, int currPt) {

		float u = t;
		
		Vector3 a = pts[currPt];
		Vector3 b = pts[currPt + 1 ];
		Vector3 c = pts[currPt + 2];
		Vector3 d = pts[currPt + 3];
		return .5f * (
			(-a + 3f * b - 3f * c + d) * (u * u * u)+ (2f * a - 5f * b + 4f * c - d) * (u * u)
			+ (-a + c) * u
			+ 2f * b);
	}


	Vector3 cubic_interpolate( Vector3 y0, Vector3 y1, Vector3 y2, Vector3 y3, float mu ) {
		Vector3 a0, a1, a2, a3;
		float mu2;
		mu2 = mu*mu;
		a0 = y3 - y2 - y0 + y1; //p
		a1 = y0 - y1 - a0;
		a2 = y2 - y0;
		a3 = y1;
		return ( a0*mu*mu2 + a1*mu2 + a2*mu + a3 );
	}


	
	Vector2 GetMainGameViewSize()
	{
	    System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
	    System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
	    System.Object Res = GetSizeOfMainGameView.Invoke(null,null);
    	return (Vector2)Res;
	}	
	
	void createPoints(float numSegments, Vector3[] pts,ref TSPoints[] points)
	{
		float totalDistance = 0;
		createPoints(numSegments, pts,ref points, ref totalDistance);
	}
	
	void createPoints1(int numSegments, Vector3[] pts,ref TSPoints[] points, ref float totalDistance)
	{
		int countPT=0;
		totalDistance = 0;
		for(int i=0; i<numSegments; i++) {
			
			
			float t = i / (float)numSegments;   
			Vector3 val = Interp(t,pts);
			
			TSPoints point = new TSPoints();
			point.point = val;
			
			RaycastHit hit;
			if(Physics.Raycast(point.point + Vector3.up * 5 , -Vector3.up, out hit))
			{
				point.point = hit.point;
			}
			if (i >0)
				point.distanceToNextPoint =  (points[i-1].point - point.point).magnitude;
			else point.distanceToNextPoint = 0;
			totalDistance += point.distanceToNextPoint;
			points = points.Add(point);
			countPT++;
		}
		TSPoints pointLast = new TSPoints();
		pointLast.point = pts[pts.Length-1];
		pointLast.distanceToNextPoint =  (points[points.Length-1].point - pointLast.point).magnitude;
		totalDistance +=pointLast.distanceToNextPoint;
		RaycastHit hit1;
		if(Physics.Raycast(pointLast.point + Vector3.up * 5 , -Vector3.up, out hit1))
		{
			pointLast.point = hit1.point;
		}
		points = points.Add(pointLast);
		
	}



	void createPoints(float numSegments, Vector3[] pts,ref TSPoints[] points, ref float totalDistance)
	{
		int countPT=0;
		totalDistance = 0;
		for(int i=0; i<pts.Length-3; i++) {
			
			float tcurr = 0;

			float t;   
			if (i ==0)
				t = (numSegments/Vector3.Distance(pts[i],pts[i+2]));   
			else if (i == pts.Length-4) t = (numSegments/Vector3.Distance(pts[i+1],pts[i+2]));   
			else  t = (numSegments/Vector3.Distance(pts[i+1],pts[i+2]));
			while (tcurr <= 1)
			{
				if (1-tcurr< t/2f )break;
				TSPoints point = new TSPoints();
				point.point = InterpEvenly(tcurr,pts,i);
				
				RaycastHit hit;
				if(Physics.Raycast(point.point + Vector3.up * 5 , -Vector3.up, out hit))
				{
					point.point = hit.point;
				}
				if (countPT >0)
					point.distanceToNextPoint =  (points[countPT-1].point - point.point).magnitude;
				else point.distanceToNextPoint = 0;
				totalDistance += point.distanceToNextPoint;
				points = points.Add(point);
				tcurr +=t;
				countPT++;
			}
		}
		TSPoints pointLast = new TSPoints();
		pointLast.point = pts[pts.Length-1];
		if (points.Length-1 >=0)
		pointLast.distanceToNextPoint =  (points[points.Length-1].point - pointLast.point).magnitude;
		totalDistance +=pointLast.distanceToNextPoint;
		RaycastHit hit1;
		if(Physics.Raycast(pointLast.point + Vector3.up * 5 , -Vector3.up, out hit1))
		{
			pointLast.point = hit1.point;
		}
		points = points.Add(pointLast);
		
	}




	
	//Tool methods
	int getNearestWayppoint(TSPoints[] waypoint, Vector3 point)
	{
		return getNearestWayppoint( waypoint, point, float.MaxValue);
	}
	int getNearestWayppoint(TSPoints[] waypoint, Vector3 point, float minsidedist)
	{
		int o =0;
		int i =0;
		bool entered = false;
		foreach (TSPoints waypointEval in waypoint)
		{
			float fdist = Vector3.Distance(waypointEval.point,point);// waypointEval.InverseTransformPoint(myTransform.position).magnitude;
			if ( fdist < minsidedist)	
			{				
				entered = true;
				minsidedist = fdist;				
				o=i;
			}
			i++;
		}
		if (o==0 && !entered) o = -1;
		return o;
	}	


	public void Save(string path)
	{
		var serializer = new XmlSerializer(typeof(TSLaneInfo[]));
		using(StreamWriter stream = new StreamWriter( new FileStream(path, FileMode.Create), System.Text.Encoding.UTF8 ))
		{
			serializer.Serialize(stream, manager.lanes);
		}
	}
	
	public TSLaneInfo[] Load(string path)
	{
		var serializer = new XmlSerializer(typeof(TSLaneInfo[]));
		using(StreamReader stream = new StreamReader( new FileStream(path, FileMode.Open),System.Text.Encoding.UTF8))
		{
			return serializer.Deserialize(stream) as TSLaneInfo[];
		}
	}

	public string GetiTSDirectory ()
	{  
		if (Directory.Exists(Application.dataPath +Path.DirectorySeparatorChar + "iTS"+ Path.DirectorySeparatorChar + "Traffic System"  + Path.DirectorySeparatorChar + "Required" ))
			return "Assets" +Path.DirectorySeparatorChar + "iTS"+ Path.DirectorySeparatorChar + "Traffic System"  + Path.DirectorySeparatorChar + "Required"+ Path.DirectorySeparatorChar ;
		Stack<string> stack = new Stack<string>();
		// Add the root directory to the stack
		stack.Push(Application.dataPath); 
		// While we have directories to process...
		//		Debug.Log ("Pre-Seacrhing all cars on the specified directories!");
		while (stack.Count > 0) {
			// Grab a directory off the stack
			string currentDir = stack.Pop();
			{
				foreach (string dir in Directory.GetDirectories(currentDir)) {
					if (dir.EndsWith("iTS")) {
						if (Directory.Exists(dir + Path.DirectorySeparatorChar+ "Traffic System" + Path.DirectorySeparatorChar + "Required"+Path.DirectorySeparatorChar))
						{
							return dir.Replace(Application.dataPath,"Assets") + Path.DirectorySeparatorChar+ "Traffic System" + Path.DirectorySeparatorChar + "Required"+Path.DirectorySeparatorChar;
						}
					}
					// Add directories at the current level into the stack
					stack.Push(dir);
				}
				
			}
		}
		return "Not found!";
	}

} //End of Main Class



public static class Extensions {
	//=========================================================================
	// Removes all instances of [itemToRemove] from array [original]
	// Returns the new array, without modifying [original] directly
	// .Net2.0-compatible
	public static T[] Remove<T> (this T[] original, T itemToRemove) {  
		int numIdx = System.Array.IndexOf(original, itemToRemove);
		if (numIdx == -1) return original;
		List<T> tmp = new List<T>(original);
		tmp.RemoveAt(numIdx);
		return tmp.ToArray();
	}

	public static T[] Add<T> (this T[] original, T itemToAdd) {  
		System.Array.Resize(ref original, original.Length+1);
		original[original.Length - 1] = itemToAdd;
		return original;
	}
}


//namespace System.Enum.Extensions {
	
	public static class EnumerationExtensions {
		
		public static bool Has<T>(this System.Enum type, T value) {
			try {

			return (((((int)(object)type) & (1<<(int)(object)value))) > 0);//(int)(object)value);
			} 
			catch {
				return false;
			}
		}
		
		public static bool Is<T>(this System.Enum type, T value) {
			try {
				return (int)(object)type == (int)(object)value;
			}
			catch {
				return false;
			}    
		}
		
		
		public static T Add<T>(this System.Enum type, T value) {
			try {
			return (T)(object)(( ((int)(object)type)|(1<<(int)(object)value)));
			}
			catch(System.Exception ex) {
				throw new System.ArgumentException(
					string.Format(
					"Could not append value from enumerated type '{0}'.",
					typeof(T).Name
					), ex);
			}    
		}
		
		
		public static T Remove<T>(this System.Enum type, T value) {
			try {
			return (T)(object)((((int)(object)type) & (~(1<<(int)(object)value))));
			}
			catch (System.Exception ex) {
				throw new System.ArgumentException(
					string.Format(
					"Could not remove value from enumerated type '{0}'.",
					typeof(T).Name
					), ex);
			}  
		}
		
		/// <summary>
		/// Get all available Resources directory paths within the current project.
		/// </summary>


}


