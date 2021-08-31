using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BambooStick : MonoBehaviour
{
    public Transform SeamsContainer;
    public Transform LeavesContainer;
    public MeshFilter QuadMesh;
    public Collider2D InvalidZoneCollider;
    public float TimeToDisappear = .5f;

    [Header("Small Target")]
    public bool isSmallTarget;
    public CutTarget StartingCutTarget;
    
    private CutTarget cutTarget;
    private Collider2D col2D;
    private Rigidbody2D rb;
    private bool meshCloned;
    private Material baseMaterial;
    private Material seamMaterial;

    private void Awake()
    {
        col2D = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (isSmallTarget)
        {
            StartingCutTarget.Init();
            SetCutTarget(StartingCutTarget);
        }
    }

    public void CutThrough(Vector2 inters1, Vector2 inters2, bool up)
    {
        if (!meshCloned) CloneMesh();

        inters1 = transform.InverseTransformPoint(inters1);
        inters2 = transform.InverseTransformPoint(inters2);
        
        int vert0;
        int vert1;
        var mesh = QuadMesh.mesh;
        var newVerts = mesh.vertices;
        
        if (up)
        {
            vert0 = 0;
            vert1 = 1;
        }
        else
        {
            vert0 = 2;
            vert1 = 3;
        }

        var angle = Vector2.Angle(Vector2.right, inters1 - inters2);

        if (angle < 90)
        {
            newVerts[vert0] = inters2;
            newVerts[vert1] = inters1;
        }
        else
        {
            newVerts[vert0] = inters1;
            newVerts[vert1] = inters2;
        }

        mesh.vertices = newVerts;
        QuadMesh.mesh = mesh;
        RecalculateBounds();

        var v0 = newVerts[vert0];
        var v1 = newVerts[vert1];
        var y = up ? Mathf.Max(v0.y, v1.y) : Mathf.Min(v0.y, v1.y);
        
        RemoveChildren(y, up);
        StartCoroutine(StartTransparency());
    }
    
    private void CloneMesh()
    {
        if (meshCloned) return;
        
        var originalMesh = QuadMesh.sharedMesh;
        var clonedMesh = new Mesh
        {
            name = "bamboo_" + DateTime.Now.ToString("HHmmssffff"),
            vertices = originalMesh.vertices,
            triangles = originalMesh.triangles,
            normals = originalMesh.normals,
            uv = originalMesh.uv
        };

        var newVerts = clonedMesh.vertices;
        for (int i = 0; i < newVerts.Length; i++)
        {
            newVerts[i] = (Vector2)clonedMesh.vertices[i] * QuadMesh.transform.localScale;
        }

        clonedMesh.vertices = newVerts;

        QuadMesh.mesh = clonedMesh;

        QuadMesh.transform.localScale = Vector3.one;
        
        meshCloned = true;
    }

    private void RemoveChildren(float y, bool up)
    {
        foreach (Transform child in SeamsContainer)
        {
            if (up)
            {
                if (child.localPosition.y < y)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                if (child.localPosition.y > y)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        foreach (Transform child in LeavesContainer)
        {
            if (up)
            {
                if (child.localPosition.y < y)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                if (child.localPosition.y > y)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    private IEnumerator StartTransparency()
    {
        var t = 0f;

        while (t < TimeToDisappear)
        {
            t += Time.deltaTime;

            var i = Mathf.InverseLerp(0, TimeToDisappear, t);
            var f = Mathf.Lerp(1, 0, i);

            var c = baseMaterial.color;
            c.a = f;
            baseMaterial.color = c;

            var c2 = seamMaterial.color;
            c2.a = f;
            seamMaterial.color = c2;
            
            yield return null;
        }
        
        Destroy(gameObject);
    }

    public void RecalculateBounds()
    {
        QuadMesh.mesh.RecalculateBounds();
    }

    public void UpdateOrderInLayer(int order)
    {
        var seams = SeamsContainer.GetComponentsInChildren<LineRenderer>();

        foreach (var seam in seams)
        {
            seam.sortingOrder = order + 1;
        }

        var leaves = LeavesContainer.GetComponentsInChildren<LineRenderer>();

        foreach (var leaf in leaves)
        {
            leaf.sortingOrder = order - 1;
        }

        if (cutTarget)
        {
            cutTarget.UpdateRendererOrder(order + 2);
        }
    }

    public void SetCutTarget(CutTarget target)
    {
        cutTarget = target;
    }

    public void SetChildrenRenderers()
    {
        baseMaterial = new Material(QuadMesh.GetComponent<Renderer>().material);
        seamMaterial = new Material(SeamsContainer.GetChild(0).GetChild(0).GetComponent<Renderer>().material);

        QuadMesh.GetComponent<Renderer>().material = baseMaterial;

        foreach (Transform seam in SeamsContainer)
        {
            seam.GetChild(0).GetComponent<Renderer>().material = seamMaterial;
        }
        
        foreach (Transform stem in LeavesContainer)
        {
            var l = stem.GetComponent<BambooStem>();
            l.Stem.GetComponent<Renderer>().material = baseMaterial;

            foreach (Transform leaf in l.LeavesContainer)
            {
                leaf.GetComponent<Renderer>().material = baseMaterial;
            }
        }
    }

    public void DisableCutTarget()
    {
        cutTarget.gameObject.SetActive(false);
    }

    public void DisableCollider()
    {
        InvalidZoneCollider.enabled = false;
        col2D.enabled = false;
    }

    public void AddRandomForce(Vector2 minMaxX, Vector2 minMaxY)
    {
        var direction = new Vector2(Random.Range(minMaxX.x, minMaxX.y), Random.Range(minMaxY.x, minMaxY.y));
        
        rb.AddForce(direction, ForceMode2D.Impulse);
    }

    public void AddRandomForce(Vector2 minMaxX, Vector2 minMaxY, bool right)
    {
        var direction = new Vector2(Random.Range(minMaxX.x, minMaxX.y), Random.Range(minMaxY.x, minMaxY.y));
        if (!right)
        {
            direction.x *= -1;
        }
        
        rb.AddForce(direction, ForceMode2D.Impulse);
    }

    public void AddRandomRotation(Vector2 MinMaxTorque)
    {
        var randomDirection = Random.value >= 0.5;
        var impulse = Random.Range(MinMaxTorque.x, MinMaxTorque.y) * Mathf.Deg2Rad * rb.inertia;
        rb.AddTorque(randomDirection ? impulse : -impulse, ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        if (!QuadMesh.sharedMesh) return;

        var verts = QuadMesh.sharedMesh.vertices;
        var t = transform;
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(t.TransformPoint(verts[0]), .1f);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(t.TransformPoint(verts[1]), .1f);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(t.TransformPoint(verts[2]), .1f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(t.TransformPoint(verts[3]), .1f);
    }
}
