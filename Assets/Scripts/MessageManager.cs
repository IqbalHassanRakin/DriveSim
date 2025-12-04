using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jp.hashilus
{
    public class MessageManager : MonoBehaviour
    {
        private bool isEmergency;

        private void OnEnable()
        {
            isEmergency = false;
        }

        public void FinishedCentering()
        {
            if (!isEmergency)
            {
                gameObject.SetActive(false);
            }
        }

        public void Emergency()
        {
            isEmergency = true;
        }
    }
}
