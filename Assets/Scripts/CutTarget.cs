using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CutTarget : MonoBehaviour
{
    public Transform MaxScoreTargets;
    public LayerMask MaxScoreTargetLayer;
    public float AccuracyThreshold = .5f;

    private void Awake()
    {
        foreach (Transform tgt in MaxScoreTargets)
        {
            tgt.gameObject.SetActive(false);
        }
    }

    public void Init()
    {
        var r = Random.Range(0, MaxScoreTargets.childCount);

        MaxScoreTargets.GetChild(r).gameObject.SetActive(true);
    }

    public void UpdateRendererOrder(int order)
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = order;
        var tgts = MaxScoreTargets.GetComponentsInChildren<SpriteRenderer>();

        foreach (var tgt in tgts)
        {
            tgt.sortingOrder = order + 1;
        }
    }

    public Score GetAccuracy(Vector2 from, Vector2 to)
    {
        var hit1 = Physics2D.Raycast(from, (to - from).normalized, (to - from).magnitude, MaxScoreTargetLayer);
        var hit2 = Physics2D.Raycast(to, (from - to).normalized, (from - to).magnitude, MaxScoreTargetLayer);

        if (hit1)
        {
            return Vector2.Distance(hit1.point, hit2.point) >= AccuracyThreshold ? Score.Perfect : Score.Good;
        }

        return Score.Bad;
    }
}
