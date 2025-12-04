using UnityEngine;

namespace jp.hashilus
{
    [System.Serializable]
    public class WeatherData
    {
        // 雲の種類
        public enum CloudType
        {
            NoClouds = 0,
            LightClouds,
            MediumClouds,
            HeavyClouds,
            StormClouds,
        }
        // 天候
        public bool rain;           // 雨
        public bool snow;           // 雪
        public bool hail;           // ひょう
        public bool sleet;          // みぞれ
        public bool lighting;       // 雷
        public bool wind;           // 風
        public bool fog;            // 霧
        public CloudType cloud;     // 曇り

        // 時間帯
        public int timeOfDay;     // 時刻(単位は時間:0～23まで有効)
    }
}