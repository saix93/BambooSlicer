using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public LayerMask SwordLayer;
    public LayerMask InvalidZoneLayer;
    public AudioClip BambooCutSFX;
    public AudioClip SwordSwingSFX;
    
    private LineRenderer lineRenderer;
    private Camera mc;
    private AudioSource audioSource;
    private List<Vector2> colliderPoints = new List<Vector2>();
    private List<Vector2> colliderPointsCopy = new List<Vector2>();
    private float cutTimer;

    private void Awake()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        mc = Camera.main;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!GameManager._.GameRunning)
        {
            lineRenderer.enabled = false;
            colliderPoints.Clear();
            return;
        }
        
        var mousePos = (Vector2)mc.ScreenToWorldPoint(Input.mousePosition);
        
        if (Input.GetMouseButtonDown(0))
        {
            cutTimer = 0;
            lineRenderer.enabled = true;
            AddLinePosition(mousePos, 0);
            colliderPoints.Add(mousePos);
        }

        if (Input.GetMouseButton(0))
        {
            cutTimer += Time.deltaTime;
            AddLinePosition(mousePos, 1);
        }

        if (Input.GetMouseButtonUp(0))
        {
            lineRenderer.enabled = false;
            colliderPoints.Add(mousePos);
            
            colliderPointsCopy = new List<Vector2>(colliderPoints);
            
            if (colliderPoints.Count > 1)
            {
                var from = colliderPoints[0];
                var to = colliderPoints[1];
                var direction = to - from;
                colliderPoints.Clear();

                var hit = Physics2D.Raycast(from, direction.normalized, direction.magnitude, SwordLayer);
                
                if (hit.collider)
                {
                    var cFrom = Physics2D.Raycast(from, Vector2.zero, 0, InvalidZoneLayer);
                    var cTo = Physics2D.Raycast(to, Vector2.zero, 0, InvalidZoneLayer);

                    if (cFrom && cFrom.transform.root == hit.transform.root ||
                        cTo && cTo.transform.root == hit.transform.root)
                    {
                        audioSource.PlayOneShot(SwordSwingSFX);
                        return;
                    }

                    audioSource.PlayOneShot(BambooCutSFX);
                    var bambooStick = hit.collider.GetComponentInParent<BambooStick>();
                    var cutTarget = hit.collider.GetComponent<CutTarget>();
                    var accuracy = cutTarget.GetAccuracy(from, to);
                    var scoreTextPos = mc.WorldToScreenPoint(hit.point);
                    
                    ScoreManager._.AddScore(accuracy, cutTimer, bambooStick.isSmallTarget, scoreTextPos);

                    BambooHit(hit, from, to, bambooStick);
                }
                else
                {
                    audioSource.PlayOneShot(SwordSwingSFX);
                }
            }
        }
    }

    private void BambooHit(RaycastHit2D hit, Vector2 from, Vector2 to, BambooStick stick)
    {
        // Calcula los puntos de intersección
        var direction1 = to - from;
        var direction2 = from - to;
        RaycastHit2D[] res1 = new RaycastHit2D[4];
        RaycastHit2D[] res2 = new RaycastHit2D[4];
        
        Physics2D.RaycastNonAlloc(from, direction1.normalized, res1, direction1.magnitude, SwordLayer);
        Physics2D.RaycastNonAlloc(to, direction2.normalized, res2, direction2.magnitude, SwordLayer);

        res1 = res1.OrderBy(d => d.transform ? ((Vector2)d.transform.position - from).sqrMagnitude : Mathf.Infinity).ToArray();
        var tgt = res1[0];
        
        var inters1 = tgt.point;
        var inters2 = Array.Find(res2, o => o.transform.root == tgt.transform.root).point;

        GameManager._.CutBamboo(inters1, inters2, stick);
    }

    private void AddLinePosition(Vector3 position, int rendererIndex)
    {
        position.z = -2;
        lineRenderer.SetPosition(rendererIndex, position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        List<Vector2> l = new List<Vector2>();

        foreach (var p in colliderPointsCopy)
        {
            Gizmos.DrawSphere(p, .2f);
            l.Add(p);
        }

        for (int i = 0; i < l.Count - 1; i++)
        {
            Gizmos.DrawLine(l[i], l[i + 1]);
        }
    }
}
