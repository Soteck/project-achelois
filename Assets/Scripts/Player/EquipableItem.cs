using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipableItem : MonoBehaviour {
    public Camera playerCamera;
    public string item_id;
    public Animator animator;
    public bool busy = false;


    [Tooltip("Needed to save weapon pose")]
    public AnimationClip weaponAnimation;
}