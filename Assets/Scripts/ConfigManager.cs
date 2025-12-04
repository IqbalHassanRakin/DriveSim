using UnityEngine;

namespace jp.hashilus
{
    public class ConfigManager : SingletonMonoBehaviour<ConfigManager>
    {
        [SerializeField]
        public WeatherData weatherData;
        public TSData tsData;
        public bool _isPlayerCamera;
        public bool isPlayerCamera { get { return _isPlayerCamera; } set { _isPlayerCamera = value; } }

        public void Awake()
        {
            // 二つ以上のインスタンスがあったらDestory
            if (this != Instance)
            {
                Destroy(this);
                return;
            }
            // シーンを跨いでも有効にする
            DontDestroyOnLoad(this.gameObject);
        }

    }
}