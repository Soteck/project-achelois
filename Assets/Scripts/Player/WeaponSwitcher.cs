using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    public int selectedWeapon = 0;
    PlayerInputActions inputActions;
    public UnityEngine.Animations.Rigging.Rig rig;

    public void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputActions.Player.Scroll.performed += Scroll;
    }

    // Start is called before the first frame update
    public void Start()
    {
        SelectWeapon();
    }

    // Update is called once per frame
    public void Scroll(InputAction.CallbackContext context)
    {
        Debug.Log(context);
        if (context.phase == InputActionPhase.Performed)
        {
            float scrollValue = context.ReadValue<float>();
            int previousSelectedWeapong = selectedWeapon;
            if (scrollValue > 0f)
            {
                if (selectedWeapon >= transform.childCount - 1)
                {
                    selectedWeapon = 0;
                }
                else
                {
                    ++selectedWeapon;
                }
            }
            if (scrollValue < 0f)
            {
                if (selectedWeapon <= 0)
                {
                    selectedWeapon = transform.childCount - 1;
                }
                else
                {
                    --selectedWeapon;
                }
            }

            /*if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                selectedWeapon = 0;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
            {
                selectedWeapon = 1;
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3)
            {
                selectedWeapon = 2;
            }

            if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount >= 4)
            {
                selectedWeapon = 3;
            }*/

            if (previousSelectedWeapong != selectedWeapon)
            {
                SelectWeapon();
            }
        }
    }

    private void SelectWeapon()
    {
        int i = 0;
        rig.weight = 0.0f;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
                rig.weight = 1.0f;
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            ++i;
        }
    }
}
