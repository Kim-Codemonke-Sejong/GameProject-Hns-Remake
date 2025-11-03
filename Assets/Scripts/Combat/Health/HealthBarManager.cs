using UnityEngine;
using UnityEngine.UI;

namespace HnS.Health
{
    public class HealthBarManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Canvas canvas;

        [Header("Position Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 2f, 0f);
        [SerializeField] private bool lookAtCamera = true;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;

            // Canvas 설정 확인
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
            }

            // HealthSystem이 할당되지 않았으면 같은 GameObject에서 찾기
            if (healthSystem == null)
            {
                healthSystem = GetComponentInParent<HealthSystem>();
            }
        }

        private void OnEnable()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += UpdateHealthBar;
            }
        }

        private void OnDisable()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged -= UpdateHealthBar;
            }
        }

        private void Start()
        {
            // 초기 체력바 설정
            if (healthSystem != null && healthSlider != null)
            {
                UpdateHealthBar(healthSystem.CurrentHealth, healthSystem.MaxHealth);
            }
        }

        private void LateUpdate()
        {
            // Canvas 위치를 적의 머리 위로 설정
            if (canvas != null && healthSystem != null)
            {
                canvas.transform.position = healthSystem.transform.position + offset;

                // 카메라를 향하도록 회전
                if (lookAtCamera && mainCamera != null)
                {
                    canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - mainCamera.transform.position);
                }
            }
        }

        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (healthSlider == null)
                return;

            // 0~1 사이의 값으로 정규화
            float normalizedHealth = maxHealth > 0 ? currentHealth / maxHealth : 0f;
            healthSlider.value = Mathf.Clamp01(normalizedHealth);
        }

        // 체력바 표시/숨김
        public void ShowHealthBar(bool show)
        {
            if (canvas != null)
            {
                canvas.gameObject.SetActive(show);
            }
        }

        // 오프셋 런타임 변경
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }
    }
}