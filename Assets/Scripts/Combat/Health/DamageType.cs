using UnityEngine;
using System;

namespace HnS.Health
{
    public enum DamageType
    {
        Physical,
        Magic,
        Poison,
        Pure
    }


    [Serializable]
    public struct DamageInfo
    {
        public float damageAmount;
        public DamageType damageType;
        public GameObject attacker;
        public Vector3 knockbackDirection;
        public float knockbackForce;
        public bool canCauseStatus;

        public DamageInfo(float damageAmount, DamageType damageType, GameObject attacker = null)
        {
            this.damageAmount = damageAmount;
            this.damageType = damageType;
            this.attacker = attacker;
            this.knockbackDirection = Vector3.zero;
            this.knockbackForce = 0f;
            this.canCauseStatus = true;
        }
    }
}