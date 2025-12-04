using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jp.hashilus
{
    public class Situation03 : MonoBehaviour
    {
        public CarController carController;

        // 自動車制御メインスクリプト
        private RCC_CarControllerV3 controller;
        private Transform playerCar;

        [HideInInspector] public bool isStart = false;

        // Use this for initialization
        void Start()
        {
            controller = GetComponent<RCC_CarControllerV3>();
            playerCar = carController.gameObject.transform;
        }

        // Update is called once per frame
        void Update()
        {
            // プレイヤーが近寄ってきたら動き出す
            if (isStart == true)
            {
                InputsCustom();
            }
        }

        void InputsCustom()
        {
            if (carController.gear == GearMode.DGear)
            {
                Vector3 diff = transform.position - playerCar.position;
                controller.gasInput = Mathf.Clamp01(Input.GetAxis(carController.acceleratorInput));
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


    }
}