using UnityEngine;
using UnityEngine.UI;

namespace jp.hashilus
{
    public class DisplayCarInfo : MonoBehaviour
    {
        string[] gearmessage = {
            "P", // PGear
            "R", // RGear,
            "N", // NGear,
            "D", // DGear,
            "S", // SGear,
            "B", // BGear,
        };
        public Text gearText;
        private CarController carController;
        private RCC_CarControllerV3 controller;

        public Transform rpmArrow;
        public Text rpmText;
        public float minRPMAngle = 0.0F;
        public float maxRPMAngle = 90.0F;
        public float maxRPM = 8000.0F;

        public Transform kmhArrow;
        public Text kmhText;
        public float minKMHAngle = 0.0F;
        public float maxKMHAngle = 90.0F;
        public float maxKMH = 180.0F;

        public Transform stearing;
        public float minStearingAngle = 0.0F;
        public float maxStearingAngle = 90.0F;

        // Use this for initialization
        void OnEnable()
        {
            carController = GetComponent<CarController>();
            controller = GetComponent<RCC_CarControllerV3>();
            carController.gearHandler += UpdateGear;
            carController.collisionHandler += CollisionDetecht;
            DisplayRPM();
            DisplayKMH();
            DisplayStearing();
        }

        void OnDisable()
        {
            carController.gearHandler -= UpdateGear;
            carController.collisionHandler -= CollisionDetecht;
        }

        private void UpdateGear(GearMode gear)
        {
            gearText.text = gearmessage[(int)gear];
            //Debug.LogFormat("Gear={0}({1})" + gearmessage[(int)gear], gear);
        }

        private void CollisionDetecht(Collision collision)
        {
            Debug.Log("CollisionDetecht");
        }

        // Update is called once per frame
        void Update()
        {
            DisplayRPM();
            DisplayKMH();
            DisplayStearing();
        }

        private void DisplayRPM()
        {
            float amount = controller.engineRPM / maxRPM;
            float angle = Mathf.Lerp(minRPMAngle, maxRPMAngle, amount);
            rpmText.text = ((int)controller.engineRPM).ToString("D4");
            rpmArrow.localRotation = Quaternion.Euler(rpmArrow.localRotation.eulerAngles.x, 0, angle);
        }

        private void DisplayKMH()
        {
            float amount = controller.speed * controller.direction / maxKMH;
            float angle = Mathf.Lerp(minKMHAngle, maxKMHAngle, amount);
            kmhText.text = ((int)(controller.speed * controller.direction)).ToString("D3");
            kmhArrow.localRotation = Quaternion.Euler(kmhArrow.localRotation.eulerAngles.x, 0, angle);
        }

        private void DisplayStearing()
        {
            float amount = (controller.steerInput + 1) / 2;
            float angle = Mathf.Lerp(minStearingAngle, maxStearingAngle, amount);
            stearing.localRotation = Quaternion.Euler(stearing.localRotation.eulerAngles.x, 0, angle);
        }
    }
}