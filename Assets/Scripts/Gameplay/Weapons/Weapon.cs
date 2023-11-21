#region

using System.Collections;
using System.Collections.Generic;
using Entities.Weapon;
using Entities.Weapon.SO;
using Gameplay.GameplayObjects;
using Gameplay.Player;
using Model;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Gameplay.Weapons
{
    public class Weapon : NetworkBehaviour
    {
        #region Inspector Variables

        public WeaponType weaponType;
        public int damage = 40;
        public float range = 100f; // Range of the weapon
        public float fireRate = 1f; // Cadency
        public float reloadTime = 1f;
        public List<AmmoType> ammoType;
        public ParticleSystem muzzleFlash;
        public AudioSource audioSource;
        public bool isReloading = false;
        public GameObject hitEffect;
        public GameObject playerHitEffect;

        #endregion

        #region Member Variables

        [HideInInspector] public Ammo currentAmmo;
        [HideInInspector] public bool canShoot = true;
        [HideInInspector] public float timerToShoot;
        [HideInInspector] public PlayerBehaviour m_player;

        #endregion

        #region InitData

        private void OnEnable()
        {
            isReloading = false;
            canShoot = true;
        }

        public override void OnNetworkSpawn()
        {
        }

        private void Start()
        {
            timerToShoot = fireRate;
            GetReferences();
        }

        void GetReferences()
        {
            if (ammoType == null || ammoType.Count == 0)
            {
                ammoType = new List<AmmoType>();
                Debug.LogWarning("No ammo type found for " + weaponType + " weapon");
            }
            else if (currentAmmo == null)
            {
                AmmoSO ammoSO = Resources.Load<AmmoSO>("Weapon/Ammo/" + ammoType[0].ToString());
                if (ammoSO != null)
                {
                    currentAmmo = new Ammo(ammoSO);
                }
                else
                {
                    Debug.LogWarning("No AmmoSO found for " + ammoType[0].ToString() + " ammo type");
                }
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    Debug.LogWarning("No audio source found for " + weaponType + " weapon");
                }
            }
        }

        #endregion

        #region Loop

        private void Update()
        {
            if (m_player == null) return;
            if (timerToShoot > 0.0f)
            {
                timerToShoot -= Time.deltaTime;
            }
            else
            {
                canShoot = true;
            }
        }

        #endregion

        #region Logic

        public void AttachToPlayer(PlayerBehaviour player)
        {
            m_player = player;
        }

        public void DetachFromPlayer()
        {
            m_player = null;
        }

        public void Shoot()
        {
            if (canShoot)
            {
                if (currentAmmo != null && currentAmmo.IsAmmoInClip())
                {
                    timerToShoot = fireRate;
                    canShoot = false;
                    currentAmmo.ReduceCurrentAmmo();
                    ShootAudioServerRpc(NetworkObjectId);
                    PlayMuzzleFlash();
                    //TODO: Make projectiles
                    //TEMPORAL
                    /////ShootProjectileServerRpc();
                    ProcessShootRaycast();
                }
                else
                {
                    //TODO: reload and play sound
                    Debug.LogWarning("No ammo");
                }
            }
        }

        public void ProcessShootRaycast()
        {
            RaycastHit hit;
            Transform cameraTransform = m_player.PlayerController.PlayerFpsCamera.transform;
            // ShootServerRpc(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity)) //range
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * range, Color.green,
                    1f);
                string hitTag = hit.transform.gameObject.tag;
                switch (hitTag)
                {
                    case "Player":
                        DamageReceiver damageReceiver = hit.transform.gameObject.GetComponent<DamageReceiver>();
                        if (damageReceiver == null) return;
                        damageReceiver.ReceiveDamage(m_player, GetTotalDamage());
                        CreateHitImpact(hit, true);
                        break;
                    default:
                        CreateHitImpact(hit);
                        break;
                }
            }
            else
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * range, Color.red, 1f);
                Debug.Log("No hit");
            }
        }

        private void CreateHitImpact(RaycastHit hit, bool isPlayer = false)
        {
            if (hitEffect != null)
            {
                ShootServerRpc(hit.point, hit.normal, NetworkObjectId, isPlayer);
            }
            else
            {
                Debug.LogWarning("No hit effect found");
            }
        }

        public void Reload()
        {
            if (!currentAmmo.canReload())
            {
                //TODO: Make Sound
                Debug.LogWarning("No ammo clips");
            }

            if (isReloading) return;
            isReloading = true;
            Debug.Log("Reloading...");
            StartCoroutine(Realoading());

            IEnumerator Realoading()
            {
                //TODO: Make Sound
                yield return new WaitForSeconds(reloadTime);
                currentAmmo.Reload();
                isReloading = false;
            }
        }

        public void PlayMuzzleFlash()
        {
            if (muzzleFlash != null)
            {
                if (muzzleFlash.isPlaying) muzzleFlash.Stop();
                muzzleFlash.Play();
            }
            else
            {
                Debug.LogWarning("No muzzle flash found");
            }
        }

        /// <summary>
        /// Return damage of the weapon plus the damage of the ammo
        /// </summary>
        /// <returns></returns>
        public int GetTotalDamage()
        {
            return damage + currentAmmo.AmmoDamage;
        }

        #endregion

        #region Events

        #endregion

        #region Network Calls/Events

        /// <summary>
        /// Invoke the audio source from the networkObject client that called the server to the rest of the clients 
        /// </summary>
        [ServerRpc]
        public void ShootAudioServerRpc(ulong networkObjectId)
        {
            ShootAudioClientRpc(NetworkManager.Singleton.LocalClientId, networkObjectId);
        }

        [ClientRpc]
        private void ShootAudioClientRpc(ulong clientId, ulong networkObjectId)
        {
            NetworkObject no = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            Weapon sourceWeapon = no.GetComponent<Weapon>();
            if (sourceWeapon.audioSource != null)
            {
                if (sourceWeapon.audioSource.isPlaying) sourceWeapon.audioSource.Stop();
                sourceWeapon.audioSource.Play();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ShootServerRpc(Vector3 hitPoint, Vector3 hitNormal, ulong networkObjectId, bool isPlayer,
            ServerRpcParams serverRpcParams = default)
        {
            NetworkObject weaponNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            GameObject hitEffect = isPlayer
                ? weaponNetworkObject.GetComponent<Weapon>().playerHitEffect
                : weaponNetworkObject.GetComponent<Weapon>().hitEffect;
            GameObject impact = Instantiate(hitEffect, hitPoint,
                Quaternion.LookRotation(hitNormal));
            NetworkObject no = impact.GetComponent<NetworkObject>();
            no.Spawn();
            // TODO: Despawn projectile after some time
            StartCoroutine(DestroyProjectile(no.NetworkObjectId, 2f));
            ShootParticleClientRpc(hitPoint, hitNormal, no.NetworkObjectId);
        }

        private IEnumerator DestroyProjectile(ulong networkObjectId, float timeToDestroy)
        {
            yield return new WaitForSeconds(timeToDestroy);
            DestroyProjectileServerRpc(networkObjectId);
        }

        [ClientRpc]
        void ShootParticleClientRpc(Vector3 hitPoint, Vector3 hitNormal, ulong networkObjectId,
            ClientRpcParams clientRpcParams = default)
        {
            NetworkObject no = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            ParticleSystem particleSystem = no.GetComponentInChildren<ParticleSystem>();
            particleSystem.Play();
        }

        [ServerRpc]
        public void ShootProjectileServerRpc(ServerRpcParams serverRpcParams = default)
        {
            GameObject go = Instantiate(currentAmmo.GetAmmoPrefab(), transform.position,
                Quaternion.identity);
            go.GetComponent<ProjectileController>().parent = this;
            go.GetComponent<Rigidbody>().velocity = go.transform.forward * 15f; //currentAmmo.GetShootForce();
            go.GetComponent<NetworkObject>().Spawn(true);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DestroyProjectileServerRpc(ulong networkObjectId, ServerRpcParams serverRpcParams = default)
        {
            NetworkObject no = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            if (no != null)
            {
                no.Despawn();
                Destroy(no.gameObject);
            }
        }

        #endregion

        #region Debug

        #endregion
    }
}