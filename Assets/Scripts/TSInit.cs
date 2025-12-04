using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jp.hashilus
{
    public class TSInit : MonoBehaviour
    {
        public GameObject iTSManager;
        void Start()
        {
            if (ConfigManager.Instance == null)
            {
                Debug.LogError("タイトル画面からやり直してください");
                return;
            }
            // Config Managerから値を反映
            iTSManager.SetActive(ConfigManager.Instance.tsData.useTrafficSystem);
            TSTrafficSpawner.mainInstance.Amount = ConfigManager.Instance.tsData.spawnAmount;
        }

    }
}