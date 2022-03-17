using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquipmentHolder : MonoBehaviour {
    public int selectedWeapon = 0;
    public Transform activeWeapon;
    public Transform rightGrip;
    public Transform leftGrip;
    public Camera playerCamera;

    public Transform crossHairTarget;


    private PlayerInputActions inputActions;
    private List<EquipableItem> storedItems = new List<EquipableItem>();
    private EquipableItem activeItem;


    public Animator animator;

    public void Awake() {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputActions.Player.Scroll.performed += Scroll;
        foreach (EquipableItem item in activeWeapon.GetComponentsInChildren<EquipableItem>()) {
            if (!storedItems.Contains(item)) {
                storedItems.Add(InitEquipment(item));
            }
        }
    }

    // Start is called before the first frame update
    public void Start() {
        UnglitchAnimations();
        if (storedItems.Count > 0) {
            Equip(storedItems[0]);
        }

        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.updateMode = AnimatorUpdateMode.Normal;
    }

    public void UnglitchAnimations() {
        //Debug.Log(animator.isActiveAndEnabled);
        //Invoke(nameof(UnglitchAnimations1), 0.005f);
        //Invoke(nameof(UnglitchAnimations2), 0.010f);
    }

    public void UnglitchAnimations1() {
        animator.applyRootMotion = true;
        //animator.updateMode = AnimatorUpdateMode.Normal;
        //animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
    }

    public void UnglitchAnimations2() {
        animator.applyRootMotion = false;
        //animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        //animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    public void Scroll(InputAction.CallbackContext context) {
        //Debug.Log(context);
        if (activeItem != null && !activeItem.busy && context.phase == InputActionPhase.Performed) {
            float scrollValue = context.ReadValue<float>();
            int previousSelectedWeapong = selectedWeapon;
            if (scrollValue > 0f) {
                if (selectedWeapon >= storedItems.Count - 1) {
                    selectedWeapon = 0;
                }
                else {
                    ++selectedWeapon;
                }
            }

            if (scrollValue < 0f) {
                if (selectedWeapon <= 0) {
                    selectedWeapon = storedItems.Count - 1;
                }
                else {
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

            if (previousSelectedWeapong != selectedWeapon) {
                Equip(storedItems[selectedWeapon]);
            }
        }
    }


    public void PickUp(EquipableItem itemPrefab) {
        //TODO: play sound of pickup

        if (!isAlreadyEquipped(itemPrefab)) {
            EquipableItem item = Instantiate(itemPrefab);
            storedItems.Add(InitEquipment(item));
            if (storedItems.Count == 1) {
                Equip(item);
            }

            animator.Rebind();
            animator.Play("equip_" + activeItem.item_id);
        }
    }

    private bool isAlreadyEquipped(EquipableItem itemPrefab) {
        foreach (EquipableItem item in storedItems) {
            if (item.item_id == itemPrefab.item_id) {
                return true;
            }
        }

        return false;
    }

    private EquipableItem InitEquipment(EquipableItem item) {
        item.playerCamera = playerCamera;
        item.animator = animator;
        item.transform.parent = activeWeapon;
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.gameObject.SetActive(false);

        return item;
    }

    private void Equip(EquipableItem item) {
        UnEquip();
        if (item != null) {
            item.gameObject.SetActive(true);
            activeItem = item;
            animator.Play("equip_" + item.item_id);
        }

        UnglitchAnimations();
    }

    private void UnEquip() {
        if (activeItem != null) {
            activeItem.gameObject.SetActive(false);
        }
    }
}