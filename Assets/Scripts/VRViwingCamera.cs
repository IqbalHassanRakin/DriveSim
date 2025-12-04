using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jp.hashilus
{
    public class VRViwingCamera : MonoBehaviour
    {

        public GameObject camerarig;

        // Use this for initialization
        void Start()
        {
            if (ConfigManager.Instance == null)
            {
                Debug.LogError("タイトル画面からやり直してください");
                return;
            }
            camerarig.SetActive(!ConfigManager.Instance.isPlayerCamera);
        }
    }
}
