using UnityEngine;

namespace jp.hashilus
{
    public class StartBake : MonoBehaviour
    {
        public GameObject[] bakes;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                foreach(var bake in bakes)
                {
                    bake.GetComponent<Animator>().enabled = true;
                    bake.GetComponent<AudioSource>().Play();
                }
            }
        }
    }
}