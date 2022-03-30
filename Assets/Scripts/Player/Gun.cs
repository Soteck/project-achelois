using System.Collections;
using System.Collections.Generic;
using Network.Shared;
using UnityEngine;

public class Gun : EquipableItem {
    public GameObject magazine;

    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public float impactForce = 30f;

    public float reloadTime = 2.5f;

    public bool autoReload = true;

    public int pickupStoredRounds = 150;
    public int piuckupRounds = 30;
    public int magazineSize = 30;

    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip drySound;
    private AudioSource shootSource;
    private AudioSource reloadSource;
    private AudioSource drySource;

    private float nextTimeToFire = 0f;
    private int remainingRounds = 0;
    private int storedRounds = 0;
    private float reloadEndTime = 0f;
    PlayerInputActions inputActions;

    public new void Awake() {
        base.Awake();
        shootSource = AddAudio(false, false, 1f, shootSound);
        reloadSource = AddAudio(false, false, 1f, reloadSound);
        drySource = AddAudio(false, false, 1f, drySound);
        remainingRounds = piuckupRounds;
        storedRounds = pickupStoredRounds;
    }

    private AudioSource AddAudio(bool loop, bool playAwake, float vol, AudioClip clip) {
        AudioSource newAudio = gameObject.AddComponent<AudioSource>();
        newAudio.clip = clip;
        newAudio.loop = loop;
        newAudio.playOnAwake = playAwake;
        newAudio.volume = vol;

        return newAudio;
    }

    protected override void ServerCalculations() {
        //empty
    }

    protected override void ClientBeforeInput() {
        if (reloadEndTime != 0) {
            if (Time.time >= nextTimeToFire) {
                ReloadEnd();
            }
        }
    }
    
    protected override void ClientInput() {
        if (CanShoot()) {
            if (inputActions.Player.Fire1.ReadValue<float>() == 1f) {
                WaitShoot();
                Shoot();
            }
            else if (inputActions.Player.Reload.ReadValue<float>() == 1f) {
                Reload();
            }
        }
    }

    protected override void ClientMovement() {
        
    }

    protected override void ClientVisuals() {
        
    }

    private void Shoot() {
        if (this.remainingRounds > 0) {
            remainingRounds--;
            //Debug.Log(remainingRounds + " " + storedRounds);
            muzzleFlash.Play();
            shootSource.Play();

            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range)) {
                //Debug.Log(hit.transform.name);

                Target target = hit.transform.GetComponent<Target>();
                if (target != null) {
                    target.TakeDamage(damage);
                }

                if (hit.rigidbody != null) {
                    hit.rigidbody.AddForce(-hit.normal * impactForce);
                }

                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }

            if (this.remainingRounds < 1) {
                Dry();
            }
        }
        else {
            Dry();
        }
    }

    private void Dry() {
        drySource.Play();
        if (autoReload) {
            Reload();
        }
    }

    private void Reload() {
        if (remainingRounds < magazineSize) {
            busy = true;
            if (storedRounds > 0) {
                reloadSource.Play();
                animator.SetTrigger("reload_weapon");
                WaitReload();
            }
            else {
                drySource.Play();
                WaitShoot();
            }
        }
    }

    private void ReloadEnd() {
        busy = false;
        int rounds = magazineSize;
        if (rounds > storedRounds) {
            rounds = storedRounds;
        }

        if (remainingRounds > 0) {
            rounds -= remainingRounds;
        }

        storedRounds -= rounds;
        if (storedRounds < 1) {
            drySource.Play();
        }

        remainingRounds += rounds;
        reloadEndTime = 0f;
    }

    private void WaitReload() {
        nextTimeToFire = Time.time + reloadTime;
        reloadEndTime = nextTimeToFire;
    }

    private void WaitShoot() {
        nextTimeToFire = Time.time + 1f / fireRate;
    }

    private bool CanShoot() {
        return !busy && Time.time >= nextTimeToFire;
    }

    public override EquipableItemNetworkData ToNetWorkData() {
        return new EquipableItemNetworkData {
            itemID = item_id,
            itemMeta = $"{remainingRounds},{storedRounds}"
        };
    }
}