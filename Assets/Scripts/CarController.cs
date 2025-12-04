using UnityEngine;

namespace jp.hashilus
{
    public enum GearMode
    {
        PGear,
        RGear,
        NGear,
        DGear,
        SGear,
        BGear,
    }
    public class CarController : MonoBehaviour
    {
        public string acceleratorInput = "Accelerator"; // アクセルのInput名
        public string breakInput = "Break";             // ブレーキのInput名
        public string steeringInput = "Steering";       // ハンドルのInput名

        public string PGear = "PGear";                  // P(パーキング)のInput名
        public string RGear = "RGear";                  // R(リバース)のInput名
        public string NGear = "NGear";                  // N(ニュートラル)のInput名
        public string DGear = "DGear";                  // D(ドライブ)のInput名
        public string SGear = "SGear";                  // S(セコンド)のInput名
        public string BGear = "BGear";                  // B(エンジンブレーキ)のInput名

        // 車の状態
        [HideInInspector] public float gasInput = 0f;           // ガソリンの注入量(0.0f～1.0f)
        [HideInInspector] public float brakeInput = 0f;         // ブレーキ(0.0f～1.0f)
        [HideInInspector] public float steerInput = 0f;         // ステアリング(-1.0f～1.0f)
//    [HideInInspector] public float clutchInput = 0f;
//    [HideInInspector] public float handbrakeInput = 0f;
//    [HideInInspector] public float boostInput = 1f;
        [HideInInspector] public bool cutGas = false;           // ガソリンの注入を止めている = true, 注入中 = false
//    [HideInInspector] public float idleInput = 0f;
        [HideInInspector] public float speed = 0f;              // 速度
        [HideInInspector] public float engineRPM = 0f;          // エンジンの回転数

        // 加速度( m/(s^2) )
        [HideInInspector] public float acceleration;
        private float pms;  // 1フレーム前の速度(m/s)

        // 自動車制御メインスクリプト
        private RCC_CarControllerV3 controller;

        // ギアの変更通知
        public delegate void ChangeGearHandler(GearMode gear);
        public event ChangeGearHandler gearHandler;
        private GearMode previousGear;

        // 衝突を通知
        private float minimumCollisionForce = 5f;		// Minimum collision force.
        public delegate void CollisionHandler(Collision collision);
        public event CollisionHandler collisionHandler;

        public GearMode gear = GearMode.PGear;
        [HideInInspector]
        public bool isEmergency;

        void Start()
        {
            controller = GetComponent<RCC_CarControllerV3>();
            previousGear = GearMode.PGear;
            //if (controller.brakeInput < .1f && speed < 5)
            //    canGoReverseNow = true;
        }

        void Update()
        {
            if (isEmergency == false)
            {
                // 入力処理
                InputsCustom();
            }

            // 現在の車の状態を反映
            gasInput = controller.gasInput;
            brakeInput = controller.brakeInput;
            steerInput = controller.steerInput;
            cutGas = controller.cutGas;

            speed = controller.speed;
            engineRPM = controller.engineRPM;

            // ギアの値が変わったら通知
            if (previousGear != gear)
            {
                if (gearHandler != null)
                {
                    gearHandler(gear);
                }
                previousGear = gear;
            }
        }

        void InputsCustom()
        {
            controller.clutchInput = 0f;
            controller.handbrakeInput = 0f;
            controller.boostInput = 0f;

            // ステアリングは -1f～1fまで
            controller.steerInput = Input.GetAxis(steeringInput);

            // GearBoxの値を確認
            if (Input.GetButtonDown(PGear))     // パーキング
            {
                gear = GearMode.PGear;
            }

            if (Input.GetButtonDown(RGear))     // リバース
            {
                gear = GearMode.RGear;
            }

            if (Input.GetButtonDown(NGear))    // ニュートラル
            {
                gear = GearMode.NGear;
            }

            if (Input.GetButtonDown(DGear))     // ドライブ
            {
                gear = GearMode.DGear;
            }

            if (Input.GetButtonDown(SGear))     // セコンド
            {
                gear = GearMode.SGear;
            }

            if (Input.GetButtonDown(BGear))     // エンジンブレーキ
            {
                gear = GearMode.BGear;
            }

            if (gear == GearMode.DGear)
            {
                controller.gasInput = Mathf.Clamp01(Input.GetAxis(acceleratorInput)+1.0f);
                if (controller.speed > 1.0f)
                {
                    controller.brakeInput = Mathf.Clamp01(Input.GetAxis(breakInput));
                } else
                {
                    controller.brakeInput = 0;
                }
            }
            if (gear == GearMode.RGear)
            {
                if (controller.speed > 1.0f)
                {
                    controller.gasInput = Mathf.Clamp01(Input.GetAxis(breakInput));
                }
                else
                {
                    controller.gasInput = 0;
                }
                controller.brakeInput = Mathf.Clamp01(Input.GetAxis(acceleratorInput));
            }
        }


        private void FixedUpdate()
        {
            float cms = controller.speed * 1000 / 60 / 60; // km/hをm/sに変換
            acceleration = (cms - pms) / Time.fixedDeltaTime;
            pms = cms;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collisionHandler == null)
            {
                return;
            }
            // 衝突条件は RCC_CarControllerV3.cs の OnCollisionEnter() と同じにしています
            // もし RCC_CarControllerV3.cs の OnCollisionEnter() を変更したり、
            // RCC_CarControllerV3.cs.cs がアップデートしたらそれに合わせてください。
            if (collision.contacts.Length < 1 || collision.relativeVelocity.magnitude < minimumCollisionForce)
            {
                if (collision.contacts[0].thisCollider.gameObject.transform != transform.parent)
                {
                    // 衝突を通知
                    collisionHandler(collision);
                }
            }
        }
    }
}