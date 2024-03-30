using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTurretSpawner : MonoBehaviour
{
    [SerializeField] private GameObject turret;
    [SerializeField] private GameObject planetBody;

    private List<GameObject> spawnedTurrets;
    private Vector3 planetPosition;

    private void Start()
    {
        spawnedTurrets = new List<GameObject>();
        planetPosition = transform.parent.transform.position;

    }

    public void SpawnTurrets(List<TurretSpawnPositionData> spawnData, int teamID)
    {
        foreach (TurretSpawnPositionData data in spawnData){
            GameObject spawnedTurret = Instantiate(turret, transform.position, Quaternion.Euler(data.GetRotation()));
            spawnedTurret.transform.parent = transform.parent.transform;

            float planetEdge = (planetBody.transform.localScale.x / 2) * transform.parent.transform.localScale.x;
            float halfTurretWidth = (turret.transform.localScale.x / 2);


            spawnedTurret.gameObject.transform.position = planetPosition + (spawnedTurret.transform.up * (planetEdge + halfTurretWidth));
            spawnedTurret.GetComponent<TurretController>().SetTeamToProtect(teamID);
            spawnedTurrets.Add(spawnedTurret);
        }
    }

    public void DestroyTurrets()
    {
        if (spawnedTurrets.Count > 0)
        {
            foreach (GameObject t in spawnedTurrets)
            {
                spawnedTurrets.Remove(t);
                Destroy(t);
            }
        }
    }
}

public class TurretSpawnPositionData
{
    private Vector3 rotation;
    private Vector3 position;

    public TurretSpawnPositionData(Vector3 _rotation, Vector3 _position)
    {
        rotation = _rotation;
        position = _position;
    }

    public Vector3 GetPosition()
    {
        return position;
    }

    public Vector3 GetRotation()
    {
        return rotation;
    }
}

