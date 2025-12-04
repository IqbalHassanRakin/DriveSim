using UnityEngine;

namespace jp.hashilus
{
    public class StartCars : MonoBehaviour
    {
        public GameObject[] cars;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                foreach(var car in cars)
                {
                    car.GetComponent<Situation03>().isStart = true;
                }
            }
        }
    }
}