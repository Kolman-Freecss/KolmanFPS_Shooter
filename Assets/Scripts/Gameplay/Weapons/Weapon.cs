#region

using System;
using System.Collections;
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
        public AudioSource audioSource;
        public bool isReloading = false;
        public GameObject hitEffect;
        public GameObject playerHitEffect;

        #endregion

        #region Member Variables

        [HideInInspector] public WeaponEntity m_weaponEntity;
        [HideInInspector] public Ammo currentAmmo;
        [HideInInspector] public bool canShoot = true;
        [HideInInspector] public float timerToShoot;
        [HideInInspector] public PlayerBehaviour m_player;
        ParticleSystem muzzleFlash;

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
            GetReferences();
            timerToShoot = m_weaponEntity.FireRateValue;
        }

        void GetReferences()
        {
            if (m_weaponEntity == null)
            {
                m_weaponEntity = new WeaponEntity(Resources.Load<WeaponSO>("Weapon/WeaponSO/" + weaponType.ToString()));
                if (audioSource == null)
                {
                    audioSource = GetComponent<AudioSource>();
                    audioSource.clip = m_weaponEntity.AudioClipShootValue;
                }

                if (m_weaponEntity.AmmoTypesValue == null || m_weaponEntity.AmmoTypesValue.Count == 0)
                {
                    Debug.LogWarning("No ammo type found for " + weaponType + " weapon");
                }
                else if (currentAmmo == null)
                {
                    AmmoSO ammoSO =
                        Resources.Load<AmmoSO>("Weapon/Ammo/" + m_weaponEntity.AmmoTypesValue[0].ToString());
                    if (ammoSO != null)
                    {
                        currentAmmo = new Ammo(ammoSO);
                    }
                    else
                    {
                        Debug.LogWarning(
                            "No AmmoSO found for " + m_weaponEntity.AmmoTypesValue[0].ToString() + " ammo type");
                    }
                }

                if (muzzleFlash == null)
                {
                    muzzleFlash = GetComponentInChildren<ParticleSystem>();
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
                    timerToShoot = m_weaponEntity.FireRateValue;
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
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * m_weaponEntity.RangeValue,
                    Color.green,
                    1f);
                GameObject go = hit.transform.gameObject;
                LayerMask layerMask = go.layer;
                int layerMaskPlayer = LayerMask.NameToLayer("Player");
                string hitTag = go.tag;
                Debug.Log("Hit something -> " + hitTag);
                if (layerMask == layerMaskPlayer)
                {
                    Entities.Player.Player.PlayerBodyPart playerBodyPart =
                        Enum.Parse<Entities.Player.Player.PlayerBodyPart>(hitTag);

                    DamageReceiver damageReceiver = hit.transform.gameObject.GetComponentsInParent<DamageReceiver>()[0];
                    if (damageReceiver == null)
                    {
                        Debug.LogWarning("No damage receiver found");
                        return;
                    }

                    damageReceiver.ReceiveDamage(m_player,
                        m_weaponEntity.GetTotalDamage(playerBodyPart, currentAmmo.AmmoDamage));
                    CreateHitImpact(hit, true);
                }
                else
                {
                    CreateHitImpact(hit);
                }
            }
            else
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * m_weaponEntity.RangeValue, Color.red,
                    1f);
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
                yield return new WaitForSeconds(m_weaponEntity.ReloadTimeValue);
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
            if (audioSource != null)
            {
                if (audioSource.isPlaying)
                    audioSource.Stop();
                audioSource.Play();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ShootServerRpc(Vector3 hitPoint, Vector3 hitNormal, ulong networkObjectId, bool isPlayer,
            ServerRpcParams serverRpcParams = default)
        {
            NetworkObject weaponNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            GameObject cHitEffect = isPlayer
                ? weaponNetworkObject.GetComponent<Weapon>().playerHitEffect
                : weaponNetworkObject.GetComponent<Weapon>().hitEffect;
            GameObject impact = Instantiate(cHitEffect, hitPoint,
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
            ParticleSystem cParticleSystem = no.GetComponentInChildren<ParticleSystem>();
            cParticleSystem.Play();
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