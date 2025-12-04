using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jp.hashilus
{
    public class Situation02 : MonoBehaviour
    {
        public CarController carController;
        public float keepDistance;  // 車間距離
        private float _keepDistance;
        // 自動車制御メインスクリプト
        private RCC_CarControllerV3 controller;

        private Transform playerCar;
        private bool isBreak = false;

        // Use this for initialization
        void Start()
        {
            controller = GetComponent<RCC_CarControllerV3>();
            playerCar = carController.gameObject.transform;
            _keepDistance = keepDistance * keepDistance;
        }

        // Update is called once per frame
        void Update()
        {
            // ブレーキングポイントに到達したら強制ブレーキ
            if (isBreak == false)
            {
                InputsCustom();
            }
            else
            {
                controller.gasInput = 0.0f;
                controller.brakeInput = 1.0f;
            }
        }

        void InputsCustom()
        {
            if (carController.gear == GearMode.DGear)
            {
                Vector3 diff = transform.position - playerCar.position;
                //Debug.Log("diff.seqMagnitude=" + diff.sqrMagnitude);
                if (diff.sqrMagnitude < _keepDistance)
                {
                    controller.gasInput = Mathf.Clamp01(Input.GetAxis(carController.acceleratorInput));
                }
                else
                {
                    controller.gasInput = 0.0f;
                }
                if (controller.speed > 1.0f)
                {
                    controller.brakeInput = Mathf.Clamp01(Input.GetAxis(carController.breakInput));
                }
                else
                {
                    controller.brakeInput = 0;
                }
            }
            if (carController.gear == GearMode.RGear)
            {
                if (controller.speed > 1.0f)
                {
                    controller.gasInput = Mathf.Clamp01(Input.GetAxis(carController.breakInput));
                }
                else
                {
                    controller.gasInput = 0;
                }
                controller.brakeInput = Mathf.Clamp01(Input.GetAxis(carController.acceleratorInput));
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "BreakingPoint")
            {
                isBreak = true;
            }
        }
    }
}