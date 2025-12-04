using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.WeatherMaker;
using UnityEngine.UI;
using jp.hashilus;

public class WeatherInit : MonoBehaviour
{
    [SerializeField]
    private Toggle rain;        // 雨
    [SerializeField]
    private Toggle snow;        // 雪
    [SerializeField]
    private Toggle hail;        // ひょう
    [SerializeField]
    private Toggle sleet;       // みぞれ
    [SerializeField]
    private Toggle lighting;    // 雷
    [SerializeField]
    private Toggle wind;        // 風
    [SerializeField]
    private Toggle fog;         // 霧
    [SerializeField]
    private Dropdown cloud;     // 曇り
    [SerializeField]
    private Slider timeOfDay;   // 時刻(単位は秒。例：午前9時00分 = 9 * 60(分) * 60(秒) + 0分 * 60(秒) = 32400

    private void Start()
    {
        // タイトル画面から遷移してきた時だけ天気情報を更新する
        if (ConfigManager.Instance != null)
        {
            rain.isOn = ConfigManager.Instance.weatherData.rain;
            snow.isOn = ConfigManager.Instance.weatherData.snow;
            hail.isOn = ConfigManager.Instance.weatherData.hail;
            sleet.isOn = ConfigManager.Instance.weatherData.sleet;
            lighting.isOn = ConfigManager.Instance.weatherData.lighting;
            wind.isOn = ConfigManager.Instance.weatherData.wind;
            fog.isOn = ConfigManager.Instance.weatherData.fog;
            cloud.value = (int)ConfigManager.Instance.weatherData.cloud;
            timeOfDay.value = ConfigManager.Instance.weatherData.timeOfDay * 3600;
        } else
        {
            Debug.Log("Weather does not change.");
        }
    }
}
