using UnityEngine;
using UnityEngine.VR;

namespace jp.hashilus
{

    public class PositionSettings : MonoBehaviour
    {
        [SerializeField]
        private Transform seatCamera;

        [SerializeField]
        private KeyCode centering = KeyCode.C;
        [SerializeField]
        private string centerButton = "Center";

        // 「体を現実の正面に向けて下さい」のメッセージ
        [SerializeField]
        private KeyCode initMessageKey = KeyCode.A;
        public GameObject HUDMessage;

        private GameObject viveController;
        // 位置を保存用
        private Vector3 startPosition;
        private Vector3 startRotation = Vector3.zero;
        private Quaternion viveControllerRotate = Quaternion.identity;

        private Vector3 offsetPos = Vector3.zero;
        private Vector3 offsetRot;
        private bool isDuringCentering;

        void Start()
        {
            if (seatCamera != null)
            {
                // シートの位置を調整
                transform.position = seatCamera.position;
                transform.rotation = seatCamera.rotation;

                //StoreViveControllerInfo();

                // メッセージを前方3mに移動(あまり遠いと移動に時間がかかるため)
                HUDMessage.transform.position = transform.position + transform.forward * 3.0f;
                HUDMessage.SetActive(false);
            }
        }

        void StoreViveControllerInfo()
        {
            if (viveController == null)
            {
                if (GetComponent<SteamVR_ControllerManager>().left.activeSelf)
                {
                    viveController = GetComponent<SteamVR_ControllerManager>().left;
                }
            }
            if (viveController == null)
            {
                if (GetComponent<SteamVR_ControllerManager>().right.activeSelf)
                {
                    viveController = GetComponent<SteamVR_ControllerManager>().right;
                }
            }
            if (viveController == null)
            {
                return;
            }
            // コントローラーの位置を覚える
            startPosition = viveController.transform.localPosition;
            startRotation = viveController.transform.localRotation.eulerAngles;
            offsetPos = Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {
            // Cキーでカメラの位置をセンタリングする
            if (Input.GetKeyDown(centering) || Input.GetButton(centerButton))
            {
                InputTracking.Recenter();
                //StoreViveControllerInfo();
                // Aキーでメッセージを表示したときだけメッセージを非表示にする
                if (isDuringCentering == true)
                {
                    HUDMessage.SendMessage("FinishedCentering");
                    isDuringCentering = false;
                }
            }

            // Aキーでメッセージを表示する
            if (Input.GetKeyDown(initMessageKey))
            {
                // メッセージを表示にする
                HUDMessage.SetActive(true);
                isDuringCentering = true;
            }

            if (seatCamera != null)
            {
                // 元のコントローラーからの角度と位置の差分を求めて相殺する
                if (viveController != null)
                {
                    // 回転処理は未解決
                    //offsetRot = viveController.transform.localRotation.eulerAngles - startRotation;
                    //offsetRot.x = 0;
                    //offsetRot.z = 0;
                    //transform.rotation = Quaternion.Euler(seatCamera.localRotation.eulerAngles - offsetRot);

                    //offsetPos = viveController.transform.localPosition - startPosition;
                    //offsetPos.y = 0;
                }
                transform.position = seatCamera.position - offsetPos;
                transform.rotation = seatCamera.rotation;
            }
        }
    }
}
