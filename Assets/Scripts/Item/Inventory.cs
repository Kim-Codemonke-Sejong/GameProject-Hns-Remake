using UnityEngine;

namespace Hns.Item
{
    public class Inventory : MonoBehaviour
    {
         public static Inventory Instance { get; private set; }

         [Header("Resources")]
        [SerializeField] private int coin;
        [SerializeField] private int level;
        private int exp;

        public int Coin => coin;
        public int Exp => exp;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }


        public void AddCoin(int amount)
        {
            coin += amount;
        }
        public void AddExp(int amount)
        {
            exp += amount;
        }
    }
}