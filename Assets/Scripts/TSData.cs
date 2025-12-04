using UnityEngine;

namespace jp.hashilus
{
    [System.Serializable]
    public class TSData
    {
        public bool useTrafficSystem = true;    // トラフィックシステムを使うかどうか 
        public int spawnAmount = 10;            // スポーンする車の量
    }
}