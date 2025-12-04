using UnityEngine;

namespace jp.hashilus
{
    public class StartBicycle : MonoBehaviour
    {
        public GameObject[] bicycles;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                foreach(var bicycle in bicycles)
                {
                    bicycle.GetComponent<Animator>().enabled = true;
                }
            }
        }
    }
}