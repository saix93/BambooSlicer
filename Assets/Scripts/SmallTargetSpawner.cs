using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SpawnerStatus
{
    None,
    Slow,
    Medium,
    Fast,
    Fastest
}

public class SmallTargetSpawner : MonoBehaviour
{
    public List<Vector2> SpawnZones;
    public BambooStick SmallTargetPrefab;
    public BambooStick ScrollPrefab;
    public SpawnerStatus Status = SpawnerStatus.None;
    public Vector2 RandomXForce;
    public Vector2 RandomYForce;
    public Vector2 RandomTorque;
    public AudioClip SpawnSFX;

    [Header("Status data")]
    public float slowTime;
    public float slowProc;
    public float mediumTime;
    public float mediumProc;
    public float fastTime;
    public float fastProc;
    public float fastestTime;
    public float fastestProc;
    public float ScrollSpawnProc = .2f;

    private float currentTimer;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!GameManager._.GameRunning) return;
        
        currentTimer += Time.deltaTime;
        CheckTimer();
    }

    private void CheckTimer()
    {
        switch (Status)
        {
            case SpawnerStatus.None:
                break;
            case SpawnerStatus.Slow:
                if (currentTimer >= slowTime)
                {
                    TryToSpawn(slowProc);
                }
                break;
            case SpawnerStatus.Medium:
                if (currentTimer >= mediumTime)
                {
                    TryToSpawn(mediumProc);
                }
                break;
            case SpawnerStatus.Fast:
                if (currentTimer >= fastTime)
                {
                    TryToSpawn(fastProc);
                }
                break;
            case SpawnerStatus.Fastest:
                if (currentTimer >= fastestTime)
                {
                    TryToSpawn(fastestProc);
                }
                break;
        }
    }

    private void TryToSpawn(float proc)
    {
        currentTimer = 0;

        if (Random.value < proc)
        {
            BambooStick prefab = Random.value < ScrollSpawnProc ? ScrollPrefab : SmallTargetPrefab;
            
            var t = Instantiate(prefab, GetSpawnPosition(), quaternion.identity);
            t.AddRandomForce(RandomXForce, RandomYForce);
            t.AddRandomRotation(RandomTorque);
            
            audioSource.PlayOneShot(SpawnSFX);
        }
    }

    private Vector2 GetSpawnPosition()
    {
        var zone = SpawnZones[Random.Range(0, SpawnZones.Count)];
        var x = Random.Range(zone.x, zone.y);

        return new Vector2(x, transform.position.y);
    }

    public void SetStatus(SpawnerStatus newStatus)
    {
        Status = newStatus;
    }

    private void OnDrawGizmos()
    {
        var tPos = transform.position;
        
        Gizmos.color = Color.red;
        foreach (var zone in SpawnZones)
        {
            Gizmos.DrawWireCube(new Vector2((zone.x + zone.y) / 2, tPos.y), new Vector2(Mathf.Abs(zone.x - zone.y), .2f));
        }
    }
}
