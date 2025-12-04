using UnityEngine;
using UnityEngine.UI;

namespace jp.hashilus {
	
	public class ConfigVRViwing : MonoBehaviour {

		private Toggle toggle;

		void Start () {
			toggle = GetComponent<Toggle> ();
			ConfigManager.Instance.isPlayerCamera = toggle.isOn;
		}
	}
}