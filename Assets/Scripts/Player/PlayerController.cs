using UnityEngine;

public interface IPlayerFixedBehaviour
{
	void OnPlayerFixedUpdate();
}

public interface IPlayerUpdateBehaviour
{
	void OnPlayerUpdate();
}

public class PlayerController : MonoBehaviour
{
	IPlayerUpdateBehaviour[] _updateBehaviours;
	IPlayerFixedBehaviour[] _fixedBehaviours;

	PlayerInputManager _input;
	private PlayerNormalRotate _normalRotate;
	private PlayerWeaponRotate _weaponRotate;

	private GameObject weapon;
	[SerializeField] private bool _isWeaponMode = false;

	void Awake()
	{
		 _input = GetComponent<PlayerInputManager>(); 
		
		_updateBehaviours = GetComponents<IPlayerUpdateBehaviour>();
		_fixedBehaviours = GetComponents<IPlayerFixedBehaviour>();

		_normalRotate = GetComponent<PlayerNormalRotate>();
		_weaponRotate = GetComponent<PlayerWeaponRotate>();

		weapon = GameObject.FindGameObjectWithTag("Weapon");

	}

	void Update()
	{
		TempMouseDetect();
		SetRotationMode();

		if (_normalRotate != null && _normalRotate.enabled)
        _normalRotate.OnPlayerUpdate();
        
    	if (_weaponRotate != null && _weaponRotate.enabled)
        _weaponRotate.OnPlayerUpdate();

		if( _updateBehaviours == null ) return;
		for( int i = 0; i < _updateBehaviours.Length; i++ )
		{
            if ((Object)_updateBehaviours[i] != _normalRotate && (Object)_updateBehaviours[i] != _weaponRotate)
                _updateBehaviours[i].OnPlayerUpdate();
        }
	}

	void FixedUpdate()
	{
		if( _fixedBehaviours == null ) return;
		for( int i = 0; i < _fixedBehaviours.Length; i++ )
			_fixedBehaviours[i].OnPlayerFixedUpdate();
	}


	void TempMouseDetect()
	{
		if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
			_isWeaponMode = true;
		else
			_isWeaponMode = false;
	}
	void SetRotationMode()
	{
		if (_normalRotate != null)
			_normalRotate.enabled = !_isWeaponMode;
			
		if (_weaponRotate != null)
        {
			weapon.SetActive(_isWeaponMode);
			_weaponRotate.enabled = _isWeaponMode;
        }
	}
}
