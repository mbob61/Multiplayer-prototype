using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTurretSpawner : MonoBehaviour
{
    [SerializeField] private GameObject turret;
    [SerializeField] private GameObject planetBody;
    [SerializeField] private List<GameObject> rotators;

    private List<GameObject> spawnedTurrets;
    private Vector3 planetPosition;

    private void Start()
    {
        spawnedTurrets = new List<GameObject>();
        planetPosition = transform.parent.transform.position;

    }

    public void SpawnTurrets(int teamID)
    {
        List<TurretSpawnPositionData> turretData = new List<TurretSpawnPositionData>();

        turretData.Add(new TurretSpawnPositionData(new Vector3(0, 0, 0), 0));
        turretData.Add(new TurretSpawnPositionData(new Vector3(180, 0, 0), 0));

        turretData.Add(new TurretSpawnPositionData(new Vector3(90, 0, 0), 1));
        turretData.Add(new TurretSpawnPositionData(new Vector3(270, 0, 0), 1));

        turretData.Add(new TurretSpawnPositionData(new Vector3(0, 0, 90), 2));
        turretData.Add(new TurretSpawnPositionData(new Vector3(0, 0, 270), 2));

        for (int i = 0; i < turretData.Count; i++) { 
            GameObject spawnedTurret = Instantiate(turret, transform.position, Quaternion.Euler(turretData[i].GetRotation()));

            // Set the parent of the turret to a certain rotator (based on the index for it in the TurretSpawnPositionData object)
            spawnedTurret.transform.parent = rotators[turretData[i].GetRotatorIndex()].transform;

            float planetEdge = (planetBody.transform.localScale.x / 2) * transform.parent.transform.localScale.x;
            float halfTurretWidth = (turret.transform.localScale.x / 2);

            spawnedTurret.gameObject.transform.position = planetPosition + (spawnedTurret.transform.up * (planetEdge + halfTurretWidth));
            spawnedTurret.GetComponent<TurretController>().SetTeamToProtect(teamID);
            spawnedTurrets.Add(spawnedTurret);
        }

        foreach (GameObject r in rotators)
        {
            r.GetComponent<TurretParentRotator>().CanRotate(true);
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
    private int rotatorIndex;

    public TurretSpawnPositionData(Vector3 _rotation, int _rotatorIndex)
    {
        rotation = _rotation;
        rotatorIndex = _rotatorIndex;
    }


    public Vector3 GetRotation()
    {
        return rotation;
    }

    public int GetRotatorIndex()
    {
        return rotatorIndex;
    }
}

