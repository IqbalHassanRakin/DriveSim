using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace jp.hashilus
{
    public class SceneLoad : MonoBehaviour
    {
        public bool isCustomText;

        [SerializeField]
        public WeatherData weatherData;
        public TSData tsData;
        public string sceneName;
		public Toggle playerCamera;

        private Text text;

        private void Start()
        {
            if (isCustomText == true)
            {
                CustomText();
            }
        }

        void CustomText()
        {
            text = transform.FindChild("Text").gameObject.GetComponent<Text>();
            string info= text.text + "\n";

            if (!weatherData.rain && !weatherData.snow && !weatherData.hail && !weatherData.sleet &&
                !weatherData.lighting && !weatherData.wind && !weatherData.fog)
            {
                if (info == "")
                {
                    info = "晴れ";
                }
                else
                {
                    info += " 晴れ";
                }
            }
            if (weatherData.rain)
            {
                if (info == "")
                {
                    info = "雨";
                }
                else
                {
                    info += " 雨";
                }
            }
            if (weatherData.snow)
            {
                if (info == "")
                {
                    info = "雪";
                } else
                {
                    info += " 雪";
                }
            }
            if (weatherData.hail)
            {
                if (info == "")
                {
                    info = "ひょう";
                }
                else
                {
                    info += " ひょう";
                }
            }
            if (weatherData.sleet)
            {
                if (info == "")
                {
                    info = "みぞれ";
                }
                else
                {
                    info += " みぞれ";
                }
            }
            if (weatherData.lighting)
            {
                if (info == "")
                {
                    info = "雷";
                }
                else
                {
                    info += " 雷";
                }
            }
            if (weatherData.wind)
            {
                if (info == "")
                {
                    info = "風";
                }
                else
                {
                    info += " 風";
                }
            }
            if (weatherData.fog)
            {
                if (info == "")
                {
                    info = "霧";
                }
                else
                {
                    info += " 霧";
                }
            }

            info += "\n";

            if (weatherData.cloud == WeatherData.CloudType.NoClouds)
            {
                if (info == "")
                {
                    info = "雲無し";
                }
                else
                {
                    info += " 雲無し";
                }
            }
            if (weatherData.cloud == WeatherData.CloudType.LightClouds)
            {
                if (info == "")
                {
                    info = "少し曇り";
                }
                else
                {
                    info += " 少し曇り";
                }
            }
            if (weatherData.cloud == WeatherData.CloudType.MediumClouds)
            {
                if (info == "")
                {
                    info = "曇り";
                }
                else
                {
                    info += " 曇り";
                }
            }
            if (weatherData.cloud == WeatherData.CloudType.HeavyClouds)
            {
                if (info == "")
                {
                    info = "厚い曇り";
                }
                else
                {
                    info += " 厚い曇り";
                }
            }
            if (weatherData.cloud == WeatherData.CloudType.StormClouds)
            {
                if (info == "")
                {
                    info = "嵐の様な曇り";
                }
                else
                {
                    info += " 嵐の様な曇り";
                }
            }

            info += "\n";
            info += "時刻:" + weatherData.timeOfDay + "時";

            info += "\n";
            if (tsData.useTrafficSystem)
            {
                info += "車の量:" + tsData.spawnAmount;
            } else
            {
                info += "交通制御システム未使用";
            }
            text.text = info;
        }


        public void LoadScene()
        {
            // 天気情報をConfigure Managerにセット
            ConfigManager.Instance.weatherData = weatherData;
            // iTS情報をConfigure Managerにセット
            ConfigManager.Instance.tsData = tsData;
			// カメラ情報をセット
			ConfigManager.Instance.isPlayerCamera = playerCamera.isOn;

            // 指定されたシーンを読み込む
            SteamVR_LoadLevel.Begin(sceneName);
        }
    }
}