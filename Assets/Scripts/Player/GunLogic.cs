using Config;
using Enums;
using Network.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;

namespace Player {
    public class GunLogic : EquipableItemLogic {
        public float damage = 10f;
        public float range = 100f;
        [Tooltip("Bullets per minute")] public float fireRate = 350f;
        public float impactForce = 30f;

        public float reloadTime = 2.5f;

        public int pickupStoredRounds = 150;
        public int pickupRounds = 30;
        public int magazineSize = 30;

        private ParticleSystem _muzzleFlash;
        private GameObject _barrelEnd;
        public GameObject impactEffect;
        public AudioClip shootSound;
        public AudioClip reloadSound;
        public AudioClip drySound;

        public TrailRenderer bulletTrailPrefab;


        private float _nextTimeToFire = 1f;

        //private int remainingRounds = 0;
        //private int storedRounds = 0;
        private float _reloadEndTime = 0f;
        //PlayerInputActions inputActions;


        //Variables sync between clients and server
        private readonly NetworkVariable<GunState> _networkGunState = new NetworkVariable<GunState>();
        private readonly NetworkVariable<int> _networkClipRemainingRounds = new NetworkVariable<int>();
        private readonly NetworkVariable<int> _networkStoredRemainingRounds = new NetworkVariable<int>();

        //Private variables for the client
        private GunState _localGunState;
        private AudioSource _shootSource;
        private AudioSource _reloadSource;
        private AudioSource _drySource;
        private bool _reloading = false;
        private bool _hasShooted = false;
        private float _timeBetweenShoots;

        public new void Awake() {
            base.Awake();
            _shootSource = AudioUtil.AddAudio(gameObject, false, false, 1f, shootSound);
            _reloadSource = AudioUtil.AddAudio(gameObject, false, false, 1f, reloadSound);
            _drySource = AudioUtil.AddAudio(gameObject, false, false, 1f, drySound);
            inputActions.Player.Disable();
            _timeBetweenShoots = 60 / fireRate;
            // remainingRounds = pickupRounds;
            // storedRounds = pickupStoredRounds;
        }

        protected override void ClientBeforeInput() {
            bool canShoot = false;
            if (_reloading) {
                if (_reloadEndTime != 0 && Time.time >= _reloadEndTime) {
                    //Reload ended but not yet notified
                    _reloading = false;
                    ReloadEndServerRpc();
                    _reloadEndTime = 0;
                }
            }
            else {
                canShoot = !_hasShooted && Time.time >= _nextTimeToFire;
            }


            busy = !canShoot;
        }


        protected override void ClientInput() {
            if (!busy) {
                InputAction fire1 = inputActions.Player.Fire1;
                if (fire1.IsPressed()) {
                    _hasShooted = true;
                    ShootWeaponServerRpc(_barrelEnd.transform.position, Time.time);
                }
                else if (inputActions.Player.Reload.WasPerformedThisFrame()) {
                    ReloadServerRpc(Time.time);
                }
            }
        }


        protected override void ClientMovement() {
            //empty
        }

        protected override void ServerCalculations() {
            //empty
        }

        protected override void InternalCallInitMetaData(NetworkString meta) {
            InitMetaDataServerRpc(meta);
        }


        protected override void ClientVisuals() {
            if (_networkGunState.Value != _localGunState) {
                _localGunState = _networkGunState.Value;
                //TODO: Play state change
            }
        }

        [ServerRpc]
        private void ReloadServerRpc(float timeStart) {
            if (_networkClipRemainingRounds.Value < magazineSize) {
                if (_networkStoredRemainingRounds.Value > 0) {
                    ReloadStartClientRpc(timeStart + reloadTime);
                }
            }
        }


        [ServerRpc]
        private void ReloadEndServerRpc() {
            //Rounds to add to the magazine
            //Start with the full magazine minus the remaining bullets
            int rounds = magazineSize - _networkClipRemainingRounds.Value;

            //If the amount to add is greater than the amount stored, we limit it to the amount stored
            if (_networkStoredRemainingRounds.Value < rounds) {
                rounds = _networkStoredRemainingRounds.Value;
            }

            //Subtract the bullets from storage 
            _networkStoredRemainingRounds.Value -= rounds;
            if (_networkStoredRemainingRounds.Value < 1) {
                RemainingDryClientRpc();
            }

            //Add the bullets to the clip
            _networkClipRemainingRounds.Value += rounds;
        }

        [ServerRpc]
        private void ShootWeaponServerRpc(Vector3 barrelPosition, float startShootTime) {
            float nextShootTime = startShootTime + _timeBetweenShoots;
            if (_networkClipRemainingRounds.Value > 0) {
                _networkClipRemainingRounds.Value--;

                if (Physics.Raycast(barrelPosition, playerCamera.transform.forward, out RaycastHit hit,
                        range)) {
                    //Debug.Log(hit.transform.name);
                    ShootWeaponHitClientRpc(nextShootTime, hit.point, hit.normal);

                    Target target = hit.transform.GetComponent<Target>();
                    if (target != null) {
                        target.TakeDamage(damage);
                    }

                    if (hit.rigidbody != null) {
                        hit.rigidbody.AddForce(-hit.normal * impactForce);
                    }
                }
                else {
                    ShootWeaponClientRpc(barrelPosition, nextShootTime);
                }

                if (_networkClipRemainingRounds.Value < 1) {
                    DryClientRpc(nextShootTime);
                }
            }
            else {
                DryClientRpc(nextShootTime);
            }
        }

        [ServerRpc]
        private void InitMetaDataServerRpc(NetworkString meta) {
            //TODO: Improve this, ugly AF
            string[] data = meta.ToString().Split(',');
            _networkClipRemainingRounds.Value = int.Parse(data[0]);
            _networkStoredRemainingRounds.Value = int.Parse(data[1]);
            // _networkClipRemainingRounds.Value = 30;
            // _networkStoredRemainingRounds.Value = 150;
        }

        // ClientRpc are executed on all client instances

        [ClientRpc]
        private void ShootWeaponHitClientRpc(float nexShootTime, Vector3 hitPoint, Vector3 hitNormal) {
            _muzzleFlash.Play();
            _shootSource.Play();
            _nextTimeToFire = nexShootTime;
            _hasShooted = false;
            GameObject impactGo = Instantiate(impactEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(impactGo, 2f);

            if (bulletTrailPrefab != null) {
                Vector3 barrelPosition = _barrelEnd.transform.position;
                TrailRenderer bullet = Instantiate(bulletTrailPrefab, barrelPosition, Quaternion.identity);
                bullet.AddPosition(barrelPosition);
                bullet.transform.position = hitPoint;
                Destroy(bullet.gameObject, 4f);
            }
        }

        [ClientRpc]
        private void ShootWeaponClientRpc(Vector3 barrelPosition, float nexShootTime) {
            _muzzleFlash.Play();
            _shootSource.Play();
            _nextTimeToFire = nexShootTime;
            _hasShooted = false;
        }


        [ClientRpc]
        private void DryClientRpc(float nexShootTime) {
            _drySource.Play();
            _nextTimeToFire = nexShootTime;
            _hasShooted = false;
            if (IsOwner && ConfigHolder.autoReload) {
                ReloadServerRpc(Time.time);
            }
        }

        [ClientRpc]
        private void RemainingDryClientRpc() {
            if (IsOwner) {
                _drySource.Play();
            }
        }

        [ClientRpc]
        private void ReloadStartClientRpc(float reloadEndTime) {
            _reloadEndTime = reloadEndTime;
            _reloadSource.Play();
            animator.SetTrigger("reload_weapon");
            busy = true;
            _reloading = true;
        }

        //Output functions
        public override EquipableItemNetworkData ToNetWorkData() {
            return new EquipableItemNetworkData {
                itemID = (NetworkString) item_id,
                itemMeta = (NetworkString) $"{_networkClipRemainingRounds.Value},{_networkStoredRemainingRounds.Value}"
            };
        }

        public override string GetStatus() {
            return _networkClipRemainingRounds.Value + "/" + _networkStoredRemainingRounds.Value;
        }

        public override void AttachVisual(EquipableItemVisual visualItem) {
            visual = visualItem;
            //TODO: This is ugly
            _barrelEnd = ((GunVisual) visualItem).barrelEnd;
            _muzzleFlash = ((GunVisual) visualItem).muzzleFlash;
        }
    }
}