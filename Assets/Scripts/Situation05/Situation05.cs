using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jp.hashilus
{
    public class Situation05 : MonoBehaviour
    {
        int counter;
        RaycastHit hit;
        private Animator anim;
        [SerializeField]
        bool isEnableGizmo = false;
        void Start()
        {
            counter = 0;
            anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            counter++;
            if (counter % 10 == 0)
            {
                var radius = transform.lossyScale.x * 0.5f;
                
                var isHit = Physics.SphereCast(transform.position + Vector3.up, radius, transform.forward * 10, out hit, 10.0f);
                if (isHit)
                {
                    anim.speed = 0;
                } else
                {
                    anim.speed = 1.0f;
                }
            }
        }

        void OnDrawGizmos()
        {
            if (isEnableGizmo == false)
                return;

            var radius = transform.lossyScale.x * 0.5f;

            var isHit = Physics.SphereCast(transform.position + Vector3.up, radius, transform.forward * 10, out hit, 10.0f);
            if (isHit)
            {
                Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * hit.distance);
                Gizmos.DrawWireSphere(transform.position + Vector3.up + transform.forward * (hit.distance), radius);
            }
            else
            {
                Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * 100);
            }
        }
    }
}