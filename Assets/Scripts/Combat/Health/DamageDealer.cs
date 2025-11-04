using UnityEngine;
using System.Collections.Generic;

namespace HnS.Health
{
    public class DamageDealer : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private float damageAmount = 10f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private bool canCauseStatus = true;

        [Header("Knockback Settings")]
        [SerializeField] private bool applyKnockback = false;
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private bool useDirectionFromCenter = true;

        [Header("Bounce Settings")]
        [SerializeField] private bool applyBounce = false;
        [SerializeField] private float bounceForce = 10f;
        [SerializeField] private float bounceUpwardModifier = 0.3f; // 위쪽으로 튕기는 정도
        [SerializeField] private bool useBounceDirection = true; // true: 반대 방향, false: 아래쪽으로

        [Header("Hit Settings")]
        [SerializeField] private bool dealDamageOnce = false;
        [SerializeField] private float damageInterval = 0.5f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private string hitboxChildName = "WeaponHitbox";
        [SerializeField] private bool autoFindHitbox = true;

        private HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
        private Dictionary<IDamageable, float> lastHitTimes = new Dictionary<IDamageable, float>();
        private Collider damageCollider;
        private GameObject hitboxObject;

        private void Awake()
        {
            // WeaponHitbox 자식 오브젝트 찾기
            if (autoFindHitbox)
            {
                Debug.Log($"[DamageDealer] '{gameObject.name}'에서 '{hitboxChildName}' 검색 시작...");
                
                hitboxObject = FindHitboxChild(transform, hitboxChildName);
                
                if (hitboxObject != null)
                {
                    Debug.Log($"[DamageDealer] '{hitboxChildName}' 찾음: {hitboxObject.name}");
                    
                    // 이미 hitbox 오브젝트에 DamageDealer가 있는지 확인
                    if (hitboxObject == gameObject)
                    {
                        // 현재 오브젝트가 hitbox인 경우
                        damageCollider = GetComponent<Collider>();
                        if (damageCollider == null)
                        {
                            Debug.LogWarning($"[DamageDealer] Collider가 없습니다. BoxCollider를 추가합니다.");
                            damageCollider = gameObject.AddComponent<BoxCollider>();
                        }
                        damageCollider.isTrigger = true;
                    }
                    else
                    {
                        // hitbox가 다른 오브젝트인 경우, 해당 오브젝트에 설정 적용
                        damageCollider = hitboxObject.GetComponent<Collider>();
                        if (damageCollider == null)
                        {
                            Debug.LogWarning($"[DamageDealer] '{hitboxChildName}'에 Collider가 없습니다. BoxCollider를 추가합니다.");
                            damageCollider = hitboxObject.AddComponent<BoxCollider>();
                        }
                        damageCollider.isTrigger = true;
                        
                        // hitbox 오브젝트에 DamageDealer가 없으면 추가
                        var existingDealer = hitboxObject.GetComponent<DamageDealer>();
                        if (existingDealer == null)
                        {
                            var newDealer = hitboxObject.AddComponent<DamageDealer>();
                            CopySettings(newDealer);
                            Debug.Log($"[DamageDealer] '{hitboxObject.name}'로 컴포넌트 이동 완료");
                            Destroy(this);
                            return;
                        }
                    }
                }
                else
                {
                    Debug.LogError($"[DamageDealer] '{hitboxChildName}' 자식 오브젝트를 찾을 수 없습니다.");
                    Debug.Log($"[DamageDealer] 현재 오브젝트: {gameObject.name}, 자식 수: {transform.childCount}");
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Debug.Log($"[DamageDealer] 자식 [{i}]: {transform.GetChild(i).name}");
                    }
                    
                    // 현재 오브젝트의 Collider 사용
                    damageCollider = GetComponent<Collider>();
                    if (damageCollider == null)
                    {
                        damageCollider = gameObject.AddComponent<BoxCollider>();
                    }
                    damageCollider.isTrigger = true;
                }
            }
            else
            {
                // 자동 찾기 비활성화 시 현재 오브젝트의 Collider 사용
                damageCollider = GetComponent<Collider>();
                if (damageCollider == null)
                {
                    damageCollider = gameObject.AddComponent<BoxCollider>();
                }
                damageCollider.isTrigger = true;
            }
        }

        private GameObject FindHitboxChild(Transform parent, string childName)
        {
            // 직접 자식 검색
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Debug.Log($"[DamageDealer] 검색 중: {child.name} == {childName}?");
                
                if (child.name.Equals(childName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return child.gameObject;
                }
            }

            // 재귀적으로 모든 자식 검색
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                GameObject found = FindHitboxChild(child, childName);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void CopySettings(DamageDealer target)
        {
            target.damageAmount = this.damageAmount;
            target.damageType = this.damageType;
            target.canCauseStatus = this.canCauseStatus;
            target.applyKnockback = this.applyKnockback;
            target.knockbackForce = this.knockbackForce;
            target.useDirectionFromCenter = this.useDirectionFromCenter;
            target.applyBounce = this.applyBounce;
            target.bounceForce = this.bounceForce;
            target.bounceUpwardModifier = this.bounceUpwardModifier;
            target.useBounceDirection = this.useBounceDirection;
            target.dealDamageOnce = this.dealDamageOnce;
            target.damageInterval = this.damageInterval;
            target.targetLayers = this.targetLayers;
            target.hitboxChildName = this.hitboxChildName;
            target.autoFindHitbox = false; // 무한 루프 방지
        }

        private void OnTriggerEnter(Collider other)
        {
            ProcessCollision(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!dealDamageOnce)
            {
                ProcessCollision(other);
            }
        }

        private void ProcessCollision(Collider other)
        {
            // 레이어 체크
            if (targetLayers != 0 && !IsInLayerMask(other.gameObject.layer, targetLayers))
                return;

            // IDamageable 컴포넌트 찾기
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
                damageable = other.GetComponentInParent<IDamageable>();

            if (damageable == null || damageable.IsDead())
                return;

            // 한 번만 데미지를 주는 경우
            if (dealDamageOnce && hitTargets.Contains(damageable))
                return;

            // 데미지 간격 체크
            if (lastHitTimes.ContainsKey(damageable))
            {
                float timeSinceLastHit = Time.time - lastHitTimes[damageable];
                if (timeSinceLastHit < damageInterval)
                    return;
            }

            // 데미지 적용
            DealDamage(damageable, other.gameObject);

            // 튕김 효과 적용
            if (applyBounce)
            {
                ApplyBounceEffect(other.gameObject);
            }

            // 기록 업데이트
            hitTargets.Add(damageable);
            lastHitTimes[damageable] = Time.time;
        }

        private void DealDamage(IDamageable target, GameObject hitObject)
        {
            DamageInfo damageInfo = new DamageInfo(damageAmount, damageType, gameObject);
            damageInfo.canCauseStatus = canCauseStatus;

            // 넉백 설정
            if (applyKnockback)
            {
                Vector3 knockbackDir;
                if (useDirectionFromCenter)
                {
                    // 이 오브젝트의 중심에서 타겟으로의 방향
                    knockbackDir = (hitObject.transform.position - transform.position).normalized;
                }
                else
                {
                    // 이 오브젝트의 forward 방향 사용
                    knockbackDir = transform.forward;
                }

                damageInfo.knockbackDirection = knockbackDir;
                damageInfo.knockbackForce = knockbackForce;
            }


            target.TakeDamage(damageInfo);
        }

        /// <summary>
        /// 타겟 오브젝트에 튕김 효과를 적용합니다.
        /// </summary>
        private void ApplyBounceEffect(GameObject target)
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = target.GetComponentInParent<Rigidbody>();
            }

            if (rb == null)
            {
                Debug.LogWarning($"[DamageDealer] '{target.name}'에 Rigidbody가 없어 튕김 효과를 적용할 수 없습니다.");
                return;
            }

            Vector3 bounceDirection;

            if (useBounceDirection)
            {
                // 충돌 지점에서 반대 방향으로 튕김
                Vector3 directionFromThis = (target.transform.position - transform.position).normalized;
                
                // 위쪽 방향 추가 (더 자연스러운 튕김)
                bounceDirection = directionFromThis + Vector3.up * bounceUpwardModifier;
                bounceDirection.Normalize();
            }
            else
            {
                // 아래쪽으로 튕김 (예: 바닥 트랩)
                bounceDirection = Vector3.down + Vector3.up * bounceUpwardModifier;
                bounceDirection.Normalize();
            }

            // AddForce로 튕김 효과 적용
            rb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);

            Debug.Log($"[DamageDealer] '{target.name}'에 튕김 효과 적용: 방향={bounceDirection}, 힘={bounceForce}");
        }

        private bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return layerMask == (layerMask | (1 << layer));
        }

        // 히트 기록 초기화 (재사용 가능한 발사체 등에 유용)
        public void ResetHitTargets()
        {
            hitTargets.Clear();
            lastHitTimes.Clear();
        }

        // 특정 타겟 히트 기록 제거
        public void RemoveHitTarget(IDamageable target)
        {
            hitTargets.Remove(target);
            lastHitTimes.Remove(target);
        }

        // 런타임에 데미지 설정 변경
        public void SetDamage(float amount)
        {
            damageAmount = amount;
        }

        public void SetDamageType(DamageType type)
        {
            damageType = type;
        }

        public void SetKnockback(float force)
        {
            knockbackForce = force;
            applyKnockback = force > 0f;
        }

        /// <summary>
        /// 튕김 효과 활성화/비활성화
        /// </summary>
        public void SetBounceEnabled(bool enabled)
        {
            applyBounce = enabled;
        }

        /// <summary>
        /// 튕김 힘 설정
        /// </summary>
        public void SetBounceForce(float force)
        {
            bounceForce = force;
        }

        /// <summary>
        /// 튕김 방향 모드 설정 (true: 반대방향, false: 아래방향)
        /// </summary>
        public void SetBounceDirectionMode(bool useOppositeDirection)
        {
            useBounceDirection = useOppositeDirection;
        }

        /// <summary>
        /// 위쪽 튕김 정도 설정 (0~1 권장)
        /// </summary>
        public void SetBounceUpwardModifier(float modifier)
        {
            bounceUpwardModifier = modifier;
        }
    }
}