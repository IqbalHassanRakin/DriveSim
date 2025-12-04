using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jp.hashilus
{
    public class Navigator : MonoBehaviour
    {
        public enum Direction
        {
            forward,
            turn_left,
            turn_right,
        };


        [SerializeField, TooltipAttribute("上向きの矢印をセットしてください。左右を指定するとZ軸で回転します")]
        private GameObject arrow;
        [SerializeField, TooltipAttribute("前進(forward)、右折(turn right)、左折(turn left)")]
        private Direction direction;
        [SerializeField, TooltipAttribute("プレイヤーのタグ")]
        private string player = "Player";

        private Vector3[] rotations =
        {
            Vector3.zero,            // 前進(回転しない)
            new Vector3(0, 0,  90f), // 左折(左回転)
            new Vector3(0, 0, -90f), // 右折(右回転)
        };

        private void Start()
        {
            arrow.transform.Rotate(rotations[(int)direction]);
            arrow.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == player)
            {
                arrow.SetActive(true);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == player)
            {
                arrow.SetActive(false);
            }
        }
    }
}