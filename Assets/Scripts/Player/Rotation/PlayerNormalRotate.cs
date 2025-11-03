using UnityEngine;

public class PlayerNormalRotate : MonoBehaviour, IPlayerUpdateBehaviour
{
    public float rotateInterpolation = 0.15f;
    PlayerMovement _movement;

    void Start()
    {
        _movement = GetComponent<PlayerMovement>();
    }

    public void OnPlayerUpdate()
    {
        if( _movement == null ) return;

        Vector3 direction = _movement.MoveDirection;
        if( direction != Vector3.zero )
        {
            Quaternion target = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotateInterpolation);
        }
    }
}
