using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BambooStem : MonoBehaviour
{
    public Vector2 MinMaxStemLength;
    public Transform Stem;
    public Transform LeavesContainer;
    public Transform BambooLeafPrefab;
    public int TotalLeaves = 8;
    public Vector2 LeafSpawnXMinMax;
    public Vector2 LeafSpawnAngleMinMax;

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponentInChildren<LineRenderer>();
    }

    public void Init()
    {
        Stem.localScale = new Vector3(Random.Range(MinMaxStemLength.x, MinMaxStemLength.y), 1, 1);

        for (int i = 0; i < TotalLeaves; i++)
        {
            var t = Instantiate(BambooLeafPrefab, LeavesContainer);

            t.transform.localPosition = NewLeafPosition();
        }
        
        foreach (Transform child in LeavesContainer)
        {
            var eulerRotation = Vector3.zero;
            var randomDirection = Random.value >= 0.5;
            var randomAngle = Random.Range(LeafSpawnAngleMinMax.x, LeafSpawnAngleMinMax.y);
            eulerRotation.z = randomDirection ? randomAngle : -randomAngle;
            
            child.localRotation = Quaternion.Euler(eulerRotation);
        }
    }

    private Vector2 NewLeafPosition()
    {
        var r = Random.Range(0, lr.positionCount - 2);
        var p1 = lr.GetPosition(r);
        var p2 = lr.GetPosition(r + 1);

        var x = Random.Range(p1.x, p2.x);
        var y = Utils.GetYFromX(p1, p2, x);

        return new Vector2(x, y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + transform.right * LeafSpawnXMinMax.x, Vector3.one * .1f);
        Gizmos.DrawWireCube(transform.position + transform.right * LeafSpawnXMinMax.y * Stem.localScale.x, Vector3.one * .1f);
    }
}
