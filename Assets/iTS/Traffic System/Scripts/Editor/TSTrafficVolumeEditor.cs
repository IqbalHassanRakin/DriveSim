using UnityEngine;
using System.Collections;
using UnityEditor;


[CustomEditor(typeof(TSTrafficVolume))]
public class TSTrafficVolumeEditor : Editor {

	public override void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox("Hold shift and drag the collider handles to edit the collider size on the scene view", MessageType.Info);
		DrawDefaultInspector();
	}
}
