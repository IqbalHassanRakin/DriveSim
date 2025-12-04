using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace jp.hashilus
{
    [RequireComponent(typeof(AudioSource))]
    public class GlobalControl : MonoBehaviour
    {
        [TooltipAttribute("自動車オブジェクト")]
        public GameObject car;
        private CarController customController;
        private RCC_CarControllerV3 rccController;


        [TooltipAttribute("メッセージ用キャンバス")]
        public GameObject messageCanvas;
        [TooltipAttribute("メッセージテキスト")]
        public Text messageText;

        [TooltipAttribute("実験シーンの初期画面に戻る。")]
        public KeyCode Reload = KeyCode.Backspace;
        [TooltipAttribute("タイトルの実験選択画面に戻る。")]
        public KeyCode Reset = KeyCode.R;
        public string titleSceneName = "Title";
        private string runningMessage = "停止してください。"+ System.Environment.NewLine+"走行中はリセットできません。";

        [TooltipAttribute("コンテンツを中断し、緊急停止シーケンスに遷移。")]
        public KeyCode Emergency = KeyCode.E;
        public AudioClip emergencyStartVoice;
        public AudioClip emergencyEndVoice;
        public int emergencyLayer = 31;
        private AudioSource audioSource;
        [TooltipAttribute("緊急停止時、ハードウェアが確実に停止するまでの時間(秒)")]
        public float secondsOfEmergencyMessage = 30f;
        [TooltipAttribute("街のコライダを指定(緊急停止用)")]
        public GameObject townCollider;

        private string emergencyStartMessage = "緊急停止中";
        private string emergencyStopMessage = "スタッフが参ります。" + System.Environment.NewLine + "そのままお待ちください。";
        public bool isEmegency;
        private float emegencyStopTime;

        [TooltipAttribute("アプリ自体を終了する。")]
        public KeyCode StopApplication = KeyCode.Escape;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            customController = car.GetComponent<CarController>();
            rccController = car.GetComponent<RCC_CarControllerV3>();
        }


        void Update()
        {
            // コンテンツを中断し、緊急停止シーケンスに遷移。
            if (Input.GetKeyDown(Emergency) && isEmegency == false)
            {
                Debug.Log("Emergency");
                // 緊急停止モードに移行
                isEmegency = true;
                // 全てのコントロールを無効にする
                customController.isEmergency = true;
                rccController.canControl = false;
                // 停止時間のカウントダウン
                emegencyStopTime = secondsOfEmergencyMessage;
                // メッセージを表示
                messageCanvas.SetActive(true);
                messageCanvas.SendMessage("Emergency");
                messageText.text = emergencyStartMessage;

                // RigidbodyとColliderを無効化
                // car.GetComponent<Rigidbody>().isKinematic = true;
                //GameObject colliders = car.transform.FindChild("Colliders").gameObject;
                //colliders.SetActive(false);

                // Layerを変更して衝突判定を回避
                GameObject colliders = car.transform.FindChild("Colliders").gameObject;
                colliders.layer = emergencyLayer;
                List<GameObject> list = GetAllChildren.GetAll(colliders);
                foreach (GameObject obj in list)
                {
                    obj.layer = emergencyLayer;
                }
                // 街のコライダを非アクティブ
                if (townCollider != null)
                {
                    townCollider.SetActive(false);
                }

                // 音声の再生
                if (emergencyStartVoice != null && audioSource.isPlaying == false)
                {
                    audioSource.PlayOneShot(emergencyStartVoice);
                }
            }

            // 緊急停止シーケンス中はBSキー及びアプリケーション停止の操作を受け付けない
            if (isEmegency == true)
            {
                // 強制的にブレーキを踏み続ける
                if (rccController.speed > 1.0f)
                {
                    rccController.brakeInput = 1.0f;
                }
                else
                {
                    rccController.brakeInput = 0;
                }
                // カウントダウン
                emegencyStopTime -= Time.deltaTime;
                if (emegencyStopTime < 0.0f)
                {
                    emegencyStopTime = 0.0f;
                    // カウントが0、かつVR内部のスピードが0(誤差を考慮)
                    if (rccController.speed < 1.0f)
                    {
                        // メッセージを表示
                        messageCanvas.SetActive(true);
                        messageText.text = emergencyStopMessage;
                        // 音声の再生
                        if (emergencyStartVoice != null && audioSource.isPlaying == false)
                        {
                            audioSource.PlayOneShot(emergencyEndVoice);
                        }
                    }
                }
                return;
            }

            // タイトルの実験選択画面に戻る。
            if (Input.GetKeyDown(Reset))
            {
                // 走行中は使用できない
                if (rccController.speed > 1.0f)
                {
                    messageCanvas.SetActive(true);
                    messageText.text = runningMessage;

                }
                else
                {
                    // 初期画面に戻る
                    SteamVR_LoadLevel.Begin(titleSceneName);
                }
            }

            // 実験シーンの初期画面に戻る
            if (Input.GetKeyDown(Reload))
            {
                // 走行中は使用できない
                if (rccController.speed > 1.0f)
                {
                    messageCanvas.SetActive(true);
                    messageText.text = runningMessage;

                }
                else
                {
                    SteamVR_LoadLevel.Begin(SceneManager.GetActiveScene().name);
                }
            }

            // アプリ自体を終了する。
            if (Input.GetKeyDown(StopApplication))
            {
                Debug.Log("StopApplication");
                Application.Quit();
            }
        }
    }
}
