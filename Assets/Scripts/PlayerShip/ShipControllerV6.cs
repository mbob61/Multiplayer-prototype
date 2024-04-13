using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipControllerV6 : NetworkBehaviour
{
    private TeamMaterialAssigner teamMaterialAssigner;

    [Header("GameObject References")]
    //[SerializeField] private Transform firePoint;
    //[SerializeField] private GameObject networkedBullet;
    //[SerializeField] private GameObject nonNetworkedBullet;
    //[SerializeField] private GameObject bodyGraphic;
    //[SerializeField] private GameObject turret;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Renderer bodyGraphic;
    [SerializeField] private ShipMovementComponent shipMovementComponent;

    [Header("Planet Conversion")]
    [SerializeField] private int teamID = 1;

    private void Awake()
    {
        teamMaterialAssigner = FindObjectOfType<TeamMaterialAssigner>();
        bodyGraphic.materials[0].color = teamMaterialAssigner.GetMaterialForTeamWithID(teamID).color;
    }

    private void Update()
    {
        if (!IsOwner) return;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
    }

    //public void FireBullet(InputAction.CallbackContext context)
    //{
    //    if (requireNetwork)
    //    {
    //        if (!IsOwner) return;
    //    }

    //    if (context.performed)
    //    {
    //        if (!clientAuthorititiveMovement)
    //        {
    //            CreateBulletServerRpc();
    //        }
    //        else
    //        {
    //            CreateBullet();
    //        }
    //    }
    //}

    //[ServerRpc]
    //private void CreateBulletServerRpc()
    //{
    //    Debug.Log($"ServerRPc - {OwnerClientId}");
    //    GameObject spawnedBullet = spawnBulletAndApplyForce(networkedBullet);
    //    spawnedBullet.GetComponent<NetworkObject>().Spawn(true);
    //}

    //private void CreateBullet()
    //{
    //    spawnBulletAndApplyForce(nonNetworkedBullet);
    //}

    //private GameObject spawnBulletAndApplyForce(GameObject bullet)
    //{
    //    GameObject spawnedBullet = Instantiate(bullet, firePoint.position, turret.transform.rotation);
    //    spawnedBullet.GetComponent<Rigidbody>().AddForce(spawnedBullet.transform.forward * 2.0f, ForceMode.Impulse);
    //    return spawnedBullet;
    //}

    public int GetTeamID()
    {
        return teamID;
    }
}
