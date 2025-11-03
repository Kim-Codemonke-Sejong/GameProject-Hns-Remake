using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public Vector2 move;
    public Vector2 weaponAxis;
    public bool isWeaponUsing;

    void OnWeaponUse(InputValue value) { isWeaponUsing = value.isPressed; }
    void OnMove(InputValue value) { move = value.Get<Vector2>(); }
    void OnWeaponAxis(InputValue value) { weaponAxis = value.Get<Vector2>(); }
    

}
