using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BambooStickSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public BambooStick BambooStickPrefab;
    public BambooStem BambooStemPrefab;
    public Transform BambooSeamPrefab;
    public CutTarget CutTargetPrefab;

    [Header("Data")]
    public int NumberOfSeams;
    public float MinDistanceBetweenSeams;
    public int NumberOfLeavesPerSide;
    public Vector2 LeafSpawnAngleMinMax = new Vector2(.3f, .5f);
    public float leafOffsetY = 4;
    public float SpreadOfLeaves = 2;
    public float SpreadOfSeams = 4;
    public float SpreadOfCutTargets = 3;

    public BambooStick SpawnBambooStick()
    {
        var bambooStick = Instantiate(BambooStickPrefab, transform.position, quaternion.identity);
        
        SpawnSideStems(-GameManager._.xOffset, bambooStick.LeavesContainer);
        SpawnSideStems(GameManager._.xOffset, bambooStick.LeavesContainer);
        SpawnBambooSeams(bambooStick.SeamsContainer);
        bambooStick.SetCutTarget(SpawnCutTarget(bambooStick.transform));

        return bambooStick;
    }

    private void SpawnSideStems(float offset, Transform parent)
    {
        for (int i = 0; i < NumberOfLeavesPerSide; i++)
        {
            var tPos = transform.position;
            
            var minPos = tPos.y - SpreadOfLeaves;
            var maxPos = tPos.y + SpreadOfLeaves;
        
            var position = new Vector3(tPos.x + offset, Random.Range(minPos, maxPos) +  + leafOffsetY, 0);
            var eulerRotation = Vector3.zero;
            var randomAngle = Random.Range(LeafSpawnAngleMinMax.x, LeafSpawnAngleMinMax.y);
            eulerRotation.z = randomAngle;
            eulerRotation.y = offset > 0 ? 0 : 180;

            var stem = Instantiate(BambooStemPrefab, position, Quaternion.Euler(eulerRotation));
            stem.Init();
            stem.transform.SetParent(parent);
        }
    }
    
    private void SpawnBambooSeams(Transform parent)
    {
        List<Vector3> spawnedPositions = new List<Vector3>();

        for (int i = 0; i < NumberOfSeams; i++)
        {
            var pos = GetNewSeamPosition();
            spawnedPositions.Add(pos);

            var seam = Instantiate(BambooSeamPrefab, pos, Quaternion.identity);
            seam.SetParent(parent);
        }
        
        Vector3 GetNewSeamPosition()
        {
            var newPos = GetNewItemPosition(SpreadOfSeams, -2);

            foreach (var pos in spawnedPositions)
            {
                if (Vector3.Distance(newPos, pos) < MinDistanceBetweenSeams)
                {
                    return GetNewSeamPosition();
                }
            }

            return newPos;
        }
    }

    private CutTarget SpawnCutTarget(Transform parent)
    {
        var newPos = GetNewItemPosition(SpreadOfCutTargets, -2);

        var cutTarget = Instantiate(CutTargetPrefab, newPos, Quaternion.identity);
        cutTarget.Init();
        cutTarget.transform.SetParent(parent);

        return cutTarget;
    }

    private Vector3 GetNewItemPosition(float spread, float z)
    {
        var tPos = transform.position;
        
        var minPos = tPos.y - spread;
        var maxPos = tPos.y + spread;
        
        return new Vector3(tPos.x, Random.Range(minPos, maxPos), z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, BambooStickPrefab.GetComponent<BoxCollider2D>().size);

        Gizmos.color = Color.red;
        var markerSize = new Vector3(.8f, .2f, 1);
        var minPos = transform.position - (Vector3.up * SpreadOfLeaves);
        var maxPos = transform.position + (Vector3.up * SpreadOfLeaves);
        minPos.y += leafOffsetY;
        maxPos.y += leafOffsetY;
        Gizmos.DrawWireCube(maxPos, markerSize);
        Gizmos.DrawWireCube(minPos, markerSize);

        Gizmos.color = Color.yellow;
        minPos = transform.position - (Vector3.up * SpreadOfSeams);
        maxPos = transform.position + (Vector3.up * SpreadOfSeams);
        Gizmos.DrawWireCube(maxPos, markerSize);
        Gizmos.DrawWireCube(minPos, markerSize);
        
        Gizmos.color = Color.white;
        minPos = transform.position - (Vector3.up * SpreadOfCutTargets);
        maxPos = transform.position + (Vector3.up * SpreadOfCutTargets);
        Gizmos.DrawWireCube(maxPos, markerSize);
        Gizmos.DrawWireCube(minPos, markerSize);
    }
}
