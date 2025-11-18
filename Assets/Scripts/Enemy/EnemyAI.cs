using UnityEngine;
using HnS.Health;

namespace HnSRogue.Enemy
{
    /// <summary>
    /// 적 AI 기본 클래스
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Enemy Settings")]
        public float detectionRange = 10f;
        public float attackRange = 2f;
        public float moveSpeed = 1f;
        public float attackDamage = 10f;
        public float attackCooldown = 2f;

        private float lastAttackTime = 0f;

        // Components
        private Transform player;
        private Rigidbody rb;
        private HealthSystem healthSystem;

        // AI State
        private enum AIState
        {
            Idle,
            Chase,
            Attack
        }
        private AIState currentState = AIState.Idle;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            healthSystem = GetComponent<HealthSystem>();
        }

        private void Start()
        {
            // 플레이어 찾기
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        private void Update()
        {
            if (healthSystem.IsDead())
                return;

            // AI 상태 업데이트
            UpdateAIState();

            // 행동 수행
            PerformAction();
        }

        /// <summary>
        /// AI 상태 업데이트
        /// </summary>
        private void UpdateAIState()
        {
            if (player == null)
            {
                currentState = AIState.Idle;
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                currentState = AIState.Attack;
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = AIState.Chase;
            }
            else
            {
                currentState = AIState.Idle;
            }
        }

        private void PerformAction()
        {
            switch (currentState)
            {
                case AIState.Idle:
                    // 정지
                    break;
                case AIState.Chase:
                    ChasePlayer();
                    break;
                case AIState.Attack:
                    AttackPlayer();
                    break;
            }
        }

        private void ChasePlayer()
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0f;

            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        private void AttackPlayer()
        {
            if (Time.time - lastAttackTime < attackCooldown)
                return;

            if (player == null)
                return;

            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                DamageInfo damageInfo = new DamageInfo(attackDamage, DamageType.Physical, gameObject);
                damageInfo.knockbackDirection = direction;
                damageInfo.knockbackForce = 3f;

                playerHealth.TakeDamage(damageInfo);
            }

            lastAttackTime = Time.time;
            Debug.Log($"[EnemyAI] {gameObject.name} 공격!");
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (healthSystem != null && !healthSystem.IsDead())
            {
                healthSystem.TakeDamage(damageInfo);
            }
        }

        public bool IsDead()
        {
            return healthSystem != null && healthSystem.IsDead();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}