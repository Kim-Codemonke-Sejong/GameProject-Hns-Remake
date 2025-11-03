using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponRotate : MonoBehaviour, IPlayerUpdateBehaviour
{
	[Header("회전 설정")]
	public float maxRotationSpeed = 360f; // 최대 회전 속도 (도/초)
	public float minRotationSpeed = 50f; // 최소 회전 속도 (도/초) - 이 이하면 회전 안함
	public float accelerationRate = 5f; // 가속 속도
	public float decelerationRate = 3f; // 감속 속도
	public float rotationSmoothness = 0.15f; // 회전 보간 (낮을수록 부드러움)

	[Header("감지 설정")]
	public float checkInterval = 0.1f; // 샘플 체크 간격
	public float sampleDuration = 3f; // 샘플 수집 기간 (초)
	public float movementThreshold = 0.001f; // 최소 움직임 임계값 (입력 delta 기준)

	[Header("디버그")]
	[SerializeField] private string _currentDirection = "Stopped";
	[SerializeField] private float _currentSpeed = 0f; // 실제 적용 중인 속도 (0~1)
	[SerializeField] private float _targetAngle = 0f; // 목표 각도
	[SerializeField] private float _accumulatedAngle = 0f; // 현재 각도 (보간됨)
	[SerializeField] private int _clockwiseCount = 0;
	[SerializeField] private int _counterClockwiseCount = 0;
	[SerializeField] private int _totalSamples = 0;

	private List<float> _samples = new List<float>(); // 외적 값 저장
	private List<float> _sampleTimes = new List<float>(); // 샘플 시간 저장
	
	private Vector2 _previousDelta; // 이전 프레임의 입력 delta
	private float _timer = 0f;
	
	private PlayerInputManager _input;

	void Start()
	{
		_input = GetComponent<PlayerInputManager>();
		_previousDelta = Vector2.zero;
		_targetAngle = 0f;
		_accumulatedAngle = 0f;
	}
	void OnEnable()
	{
		// 활성화 시 현재 플레이어의 Y축 회전 각도 저장
		_targetAngle = transform.localRotation.eulerAngles.y;
		_accumulatedAngle = _targetAngle;
		
		// 회전 속도 초기화
		_currentSpeed = 0f;
		
		// 샘플 데이터 초기화
		_samples.Clear();
		_sampleTimes.Clear();
		_previousDelta = Vector2.zero;
		_timer = 0f;
		
		// 디버그 정보 초기화
		_currentDirection = "Stopped";
		_clockwiseCount = 0;
		_counterClockwiseCount = 0;
		_totalSamples = 0;
	}


	public void OnPlayerUpdate()
	{
		if (_input == null) return;

		// 현재 입력 delta
		Vector2 currentDelta = _input.weaponAxis;

		_timer += Time.deltaTime;

		// 샘플 수집
		if (_timer >= checkInterval)
		{
			_timer = 0f;
			CollectSample(currentDelta);
		}

		// 오래된 샘플 제거
		RemoveOldSamples();

		// 방향 분석
		AnalyzeSamples();

		// 회전 적용
		ApplyRotation(currentDelta);

		// 다음 프레임을 위해 저장
		_previousDelta = currentDelta;
	}

	void CollectSample(Vector2 currentDelta)
	{
		// 이전 delta와 현재 delta의 외적 계산
		// 이것은 delta 벡터가 얼마나 회전했는지를 나타냄
		float cross = _previousDelta.x * currentDelta.y - _previousDelta.y * currentDelta.x;

		// 샘플 추가
		_samples.Add(cross);
		_sampleTimes.Add(Time.time);
	}

	void RemoveOldSamples()
	{
		float currentTime = Time.time;
		
		for (int i = _sampleTimes.Count - 1; i >= 0; i--)
		{
			if (currentTime - _sampleTimes[i] > sampleDuration)
			{
				_samples.RemoveAt(i);
				_sampleTimes.RemoveAt(i);
			}
		}
	}

	void AnalyzeSamples()
	{
		_clockwiseCount = 0;
		_counterClockwiseCount = 0;
		_totalSamples = _samples.Count;

		// 최근 샘플이 정지 상태인지 확인
		if (_samples.Count > 0)
		{
			float latestSample = _samples[_samples.Count - 1];
			
			// 입력이 거의 없으면 정지
			if (Mathf.Abs(latestSample) <= movementThreshold)
			{
				_currentDirection = "Stopped";
				
				// 모든 표본 삭제
				_samples.Clear();
				_sampleTimes.Clear();
				
				_clockwiseCount = 0;
				_counterClockwiseCount = 0;
				_totalSamples = 0;
				return;
			}
		}

		// 샘플이 없으면 정지
		if (_samples.Count == 0)
		{
			_currentDirection = "Stopped";
			return;
		}

		// 각 샘플 분석
		foreach (float sample in _samples)
		{
			if (Mathf.Abs(sample) > movementThreshold)
			{
				if (sample > 0)
				{
					_counterClockwiseCount++; // 반시계
				}
				else
				{
					_clockwiseCount++; // 시계
				}
			}
		}

		// 방향 결정 - 최소 5개 이상의 샘플이 있어야 방향 판단
		int minSamplesRequired = 5;
		if (_clockwiseCount > _counterClockwiseCount && _clockwiseCount >= minSamplesRequired)
		{
			_currentDirection = "Clockwise";
		}
		else if (_counterClockwiseCount > _clockwiseCount && _counterClockwiseCount >= minSamplesRequired)
		{
			_currentDirection = "CounterClockwise";
		}
		else
		{
			_currentDirection = "Stopped";
		}
	}

	void ApplyRotation(Vector2 currentDelta)
	{
		// 입력 크기 계산 (0~1로 정규화)
		float inputMagnitude = currentDelta.magnitude;
		float normalizedInput = Mathf.Clamp01(inputMagnitude * 10f); // 민감도 조정

		// 정지 상태면 감속
		if (_currentDirection == "Stopped" || normalizedInput < 0.01f)
		{
			_currentSpeed = Mathf.Lerp(_currentSpeed, 0f, decelerationRate * Time.deltaTime);
			
			if (_currentSpeed < 0.01f)
			{
				_currentSpeed = 0f;
			}
		}
		else
		{
			// 입력이 있으면 가속
			_currentSpeed = Mathf.Lerp(_currentSpeed, normalizedInput, accelerationRate * Time.deltaTime);
		}

		// 회전 방향 결정 (부호 반전으로 방향 수정)
		float rotationDirection = 0f;
		if (_currentDirection == "Clockwise")
		{
			rotationDirection = 1f; // 시계 방향
		}
		else if (_currentDirection == "CounterClockwise")
		{
			rotationDirection = -1f; // 반시계 방향
		}

		// 실제 회전 속도 계산 (최소 속도 이상일 때만)
		float actualRotationSpeed = _currentSpeed * maxRotationSpeed;
		
		if (actualRotationSpeed >= minRotationSpeed && rotationDirection != 0f)
		{
			// 목표 각도 업데이트
			float rotationAmount = rotationDirection * actualRotationSpeed * Time.deltaTime;
			_targetAngle += rotationAmount;
		}

		// 부드러운 보간으로 현재 각도를 목표 각도로 이동
		_accumulatedAngle = Mathf.Lerp(_accumulatedAngle, _targetAngle, rotationSmoothness);
		
		// Y축 기준 회전
		transform.localRotation = Quaternion.Euler(0f, _accumulatedAngle, 0f);
	}

	// 디버그용
	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 400, 30), "Dir: " + _currentDirection);
		GUI.Label(new Rect(10, 40, 400, 30), "Speed: " + _currentSpeed.ToString("F2") + " (" + (_currentSpeed * maxRotationSpeed).ToString("F0") + " Angle/s)");
		GUI.Label(new Rect(10, 70, 400, 30), "TargetSpeed: " + _targetAngle.ToString("F1") + " / CurrentAngle: " + _accumulatedAngle.ToString("F1"));
		GUI.Label(new Rect(10, 100, 400, 30), "Sample: " + _totalSamples + " (Clockwise:" + _clockwiseCount + " CounterClockwise:" + _counterClockwiseCount + ")");
	}
}