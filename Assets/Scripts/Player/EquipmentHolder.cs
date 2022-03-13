using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor.Animations;

public class EquipmentHolder : MonoBehaviour
{
    public int selectedWeapon = 0;
    public Transform activeWeapon;
    public Transform rightGrip;
    public Transform leftGrip;
    public Camera playerCamera;

    public Transform crossHairTarget;

    public UnityEngine.Animations.Rigging.Rig handIk;


    private PlayerInputActions inputActions;
    private List<EquipableItem> storedItems = new List<EquipableItem>();
    private EquipableItem activeItem;


    Animator animator;
    AnimatorOverrideController overrider;
    public void Awake()
    { 
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputActions.Player.Scroll.performed += Scroll;
        foreach(EquipableItem item in activeWeapon.GetComponentsInChildren<EquipableItem>())
        {
            if (!storedItems.Contains(item))
            {
                storedItems.Add(InitEquipment(item));
            }
        }
    }

    // Start is called before the first frame update
    public void Start()
    {
        animator = GetComponent<Animator>();
        overrider = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrider;
        handIk.weight = 0f;
        animator.SetLayerWeight(1, 0.0f);
        if (storedItems.Count > 0)
        {
            Equip(storedItems[0]);
        }
    }

    public void Scroll(InputAction.CallbackContext context)
    {
        Debug.Log(context);
        if (context.phase == InputActionPhase.Performed)
        {
            float scrollValue = context.ReadValue<float>();
            int previousSelectedWeapong = selectedWeapon;
            if (scrollValue > 0f)
            {
                if (selectedWeapon >= storedItems.Count - 1)
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
                    selectedWeapon = storedItems.Count - 1;
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
                Equip(storedItems[selectedWeapon]);
            }
        }
    }


    public void PickUp(EquipableItem itemPrefab)
    {
        //TODO: play sound of pickup

        if (!isAlreadyEquipped(itemPrefab))
        {
            EquipableItem item = Instantiate(itemPrefab);
            storedItems.Add(InitEquipment(item));
            if (storedItems.Count == 1)
            {
                Equip(item);
            }
        }

    }

    private bool isAlreadyEquipped(EquipableItem itemPrefab)
    {
        foreach(EquipableItem item in storedItems)
        {
            if(item.item_id == itemPrefab.item_id)
            {
                return true;
            }
        }
        return false;
    }

    private EquipableItem InitEquipment(EquipableItem item)
    {
        item.playerCamera = playerCamera;
        item.transform.parent = activeWeapon;
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.gameObject.SetActive(false);

        return item;
    }

    private void Equip(EquipableItem item)
    {
        UnEquip();
        if(item != null)
        {
            item.gameObject.SetActive(true);
            activeItem = item;
            handIk.weight = 1.0f;
            animator.SetLayerWeight(1, 1.0f);
            Invoke(nameof(SetAnimationDelayed), 0.001f);
        }

    }

    private void SetAnimationDelayed()
    {

        overrider["anim_no_weapon"] = activeItem.weaponAnimation;
    }

    private void UnEquip()
    {
        if(activeItem != null)
        {
            activeItem.gameObject.SetActive(false);
        }
        handIk.weight = 0f;
        animator.SetLayerWeight(1, 0.0f);
    }


    [ContextMenu("Save weapon pose")]
    void SaveWeaponPose()
    {
        GameObjectRecorder recorder = new GameObjectRecorder(gameObject);
        recorder.BindComponentsOfType<Transform>(activeWeapon.gameObject, false);
        recorder.BindComponentsOfType<Transform>(leftGrip.gameObject, false);
        recorder.BindComponentsOfType<Transform>(rightGrip.gameObject, false);
        recorder.TakeSnapshot(0.0f);
        recorder.SaveToClip(activeItem.weaponAnimation);
        UnityEditor.AssetDatabase.SaveAssets();
    }
}
