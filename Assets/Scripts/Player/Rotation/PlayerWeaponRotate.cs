using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponRotate : MonoBehaviour, IPlayerUpdateBehaviour
{
	[Header("회전 설정")]
	public float maxRotationSpeed = 360f; 
	public float minRotationSpeed = 50f; 
	public float accelerationRate = 5f; 
	public float decelerationRate = 3f; 
	public float rotationSmoothness = 0.15f; 

	[Header("감지 설정")]
	public float checkInterval = 0.1f;
	public float sampleDuration = 3f;
	public float movementThreshold = 0.001f;

	[Header("디버그")]
	[SerializeField] private string _currentDirection = "Stopped";
	[SerializeField] private float _currentSpeed = 0f;
	[SerializeField] private float _targetAngle = 0f;
	[SerializeField] private float _accumulatedAngle = 0f;
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
		_targetAngle = transform.localRotation.eulerAngles.y;
		_accumulatedAngle = _targetAngle;
		
		_currentSpeed = 0f;
		
		_samples.Clear();
		_sampleTimes.Clear();
		_previousDelta = Vector2.zero;
		_timer = 0f;
		
		_currentDirection = "Stopped";
		_clockwiseCount = 0;
		_counterClockwiseCount = 0;
		_totalSamples = 0;
	}


	public void OnPlayerUpdate()
	{
		if (_input == null) return;

		Vector2 currentDelta = _input.weaponAxis;

		_timer += Time.deltaTime;

		if (_timer >= checkInterval)
		{
			_timer = 0f;
			CollectSample(currentDelta);
		}

		RemoveOldSamples();
		AnalyzeSamples();
		ApplyRotation(currentDelta);
		_previousDelta = currentDelta;
	}

	void CollectSample(Vector2 currentDelta)
	{
		float cross = _previousDelta.x * currentDelta.y - _previousDelta.y * currentDelta.x;

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

		if (_samples.Count > 0)
		{
			float latestSample = _samples[_samples.Count - 1];

			if (Mathf.Abs(latestSample) <= movementThreshold)
			{
				_currentDirection = "Stopped";
				
				_samples.Clear();
				_sampleTimes.Clear();
				
				_clockwiseCount = 0;
				_counterClockwiseCount = 0;
				_totalSamples = 0;
				return;
			}
		}

		if (_samples.Count == 0)
		{
			_currentDirection = "Stopped";
			return;
		}

		foreach (float sample in _samples)
		{
			if (Mathf.Abs(sample) > movementThreshold)
			{
				if (sample > 0)
				{
					_counterClockwiseCount++; 
				}
				else
				{
					_clockwiseCount++; 
				}
			}
		}

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
		float inputMagnitude = currentDelta.magnitude;
		float normalizedInput = Mathf.Clamp01(inputMagnitude * 10f); // 민감도 조정

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
			_currentSpeed = Mathf.Lerp(_currentSpeed, normalizedInput, accelerationRate * Time.deltaTime);
		}

		float rotationDirection = 0f;
		if (_currentDirection == "Clockwise")
		{
			rotationDirection = 1f; 
		}
		else if (_currentDirection == "CounterClockwise")
		{
			rotationDirection = -1f;
		}

		float actualRotationSpeed = _currentSpeed * maxRotationSpeed;
		
		if (actualRotationSpeed >= minRotationSpeed && rotationDirection != 0f)
		{
			float rotationAmount = rotationDirection * actualRotationSpeed * Time.deltaTime;
			_targetAngle += rotationAmount;
		}

		_accumulatedAngle = Mathf.Lerp(_accumulatedAngle, _targetAngle, rotationSmoothness);
		
		transform.localRotation = Quaternion.Euler(0f, _accumulatedAngle, 0f);
	}

	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 400, 30), "Dir: " + _currentDirection);
		GUI.Label(new Rect(10, 40, 400, 30), "Speed: " + _currentSpeed.ToString("F2") + " (" + (_currentSpeed * maxRotationSpeed).ToString("F0") + " Angle/s)");
		GUI.Label(new Rect(10, 70, 400, 30), "TargetSpeed: " + _targetAngle.ToString("F1") + " / CurrentAngle: " + _accumulatedAngle.ToString("F1"));
		GUI.Label(new Rect(10, 100, 400, 30), "Sample: " + _totalSamples + " (Clockwise:" + _clockwiseCount + " CounterClockwise:" + _counterClockwiseCount + ")");
	}
}