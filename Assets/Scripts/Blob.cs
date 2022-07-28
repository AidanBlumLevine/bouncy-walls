using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob : MonoBehaviour
{
    public Vector2[] controlPoints;
    LineRenderer lr;
    public Material mat, lineMat;
    public Color lineColor, matColor;
    Player sphere;
    Rigidbody2D rb;
    MeshFilter meshFilter;
    public float thickness = 1f; //how quickly the player slows down
    public float damping = .8f; //basically friction
    public float homeAttraction = 1f; //how strongly it is pulled towards home
    public float sphereToLineTransfer = 1f; //how much of balls velocity is transfered to each point it hits
    public float lineToSphereTransfer = .5f; //how much of lines velocity is transfered to the ball on hit
    public float neighborTransfer = 1f; //how much of velocity is added to neighbors
    public float squishMulti = .5f; //hom much points are attracted to eachother
    public bool inverse = false;
    public float maxFillDist = 1000;
    List<Point> line = new List<Point>();

    List<Vector3> meshVerts = new List<Vector3>();
    List<int> lineToVert = new List<int>();
    public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    PolygonCollider2D edge;

    void Start()
    {
        edge = gameObject.AddComponent<PolygonCollider2D>();
        sphere = FindObjectOfType<Player>();
        rb = sphere.GetComponent<Rigidbody2D>();

        lr = gameObject.AddComponent<LineRenderer>();
        lr.loop = true;
        lr.startWidth = .08f;
        lr.endWidth = .08f;
        lr.material = lineMat;
        lr.endColor = lineColor;
        lr.startColor = lineColor;
        lr.useWorldSpace = false;

        List<Vector2> extendedPoints = new List<Vector2>();
        for (int i = 0; i < controlPoints.Length; i++)
        {
            extendedPoints.Add(controlPoints[i]);
            Vector2 dir = controlPoints[(i + 1) % controlPoints.Length] - controlPoints[i];
            float dist = dir.magnitude;
            dir /= dist;
            if (dist >= 2)
            {
                if (dist == 2)
                {
                    extendedPoints.Add(controlPoints[i] + dir * dist / 2);
                }
                else
                {
                    extendedPoints.Add(controlPoints[i] + dir);
                    extendedPoints.Add(controlPoints[(i + 1) % controlPoints.Length] - dir);
                }
            }
        }
        controlPoints = extendedPoints.ToArray();

        Vector2 last = avg(controlPoints[controlPoints.Length - 1], controlPoints[0]);
        for (int i = 0; i < controlPoints.Length; i++)
        {
            Vector2 next = controlPoints[(i + 1) % controlPoints.Length];
            Vector2 nextPoint = avg(next, controlPoints[i]);
            float t = 0;
            while (t < 1)
            {
                t += .0001f;
                Vector2 b = Bezier(last, controlPoints[i], nextPoint, t);
                if (line.Count == 0)
                {
                    if ((last - b).sqrMagnitude > .01f)
                    {
                        line.Add(new Point(b));
                    }
                }
                else if ((line[line.Count - 1].point - b).sqrMagnitude > .01f)
                {
                    line.Add(new Point(b));
                }
            }
            last = line[line.Count - 1].point;
        }
        for (int i = 0; i < line.Count; i++)
        {
            Point next = line[(i + 1) % line.Count];
            if ((line[i].point - next.point).sqrMagnitude > .02f)
            {
                line.Insert((i + 1) % line.Count, new Point((line[i].point + next.point) / 2));
                i++;
            }
        }

        //Debug.DrawRay(controlPoints[controlPoints.Length - 1] + (Vector2)transform.position, Vector3.left * .5f, Color.red, 100);
        //OLD WAY OF GENERATING LINE
        // for (int i = 0; i < controlPoints.Length; i++)
        // {
        //     Vector2 next = controlPoints[(i + 1) % controlPoints.Length];
        //     Vector2 nextPoint = avg(next, controlPoints[i]);
        //     float distScale = Mathf.Max(Mathf.Abs(last.x - nextPoint.x), Mathf.Abs(last.y - nextPoint.y));
        //     for (float t = .05f / distScale; t < 1; t += .1f / distScale)
        //     {
        //         line.Add(new Point(Bezier(last, controlPoints[i], nextPoint, t)));
        //     }
        //     last = line[line.Count - 1].point;
        // }
        for (int i = 0; i < line.Count; i++)
        {
            line[i].SetNeighbors(line[mod(i - 1, line.Count)], line[(i + 1) % line.Count]);
        }

        mat.color = matColor;
        gameObject.AddComponent<MeshRenderer>().material = mat;
        meshFilter = gameObject.AddComponent<MeshFilter>();
        CalculateMesh();
    }

    void Update()
    {
        UpdateVerts();
        lr.positionCount = line.Count;
        lr.SetPositions(Point.toV3(line));
    }

    void FixedUpdate()
    {
        if (SceneManager.paused)
        {
            return;
        }
        foreach (Point p in line)
        {
            p.velocity *= damping;
            //p.velocity = Vector2.ClampMagnitude(p.velocity, 10);
            p.velocity -= .05f * homeAttraction * (p.point - p.home);
            p.before.velocity += p.velocity * neighborTransfer / 100;
            p.after.velocity += p.velocity * neighborTransfer / 100;
            p.point += p.velocity;

            float bDistCurrent = (p.before.point - p.point).magnitude;
            float aDistCurrent = (p.after.point - p.point).magnitude;

            float faB = (p.bDist - bDistCurrent) / bDistCurrent * squishMulti;
            if (bDistCurrent < p.bDist) { faB *= .1f; }
            p.velocity -= faB * (p.before.point - p.point);
            p.before.velocity += faB * (p.before.point - p.point);

            float faA = (p.aDist - aDistCurrent) / aDistCurrent * squishMulti;
            if (aDistCurrent < p.aDist) { faA *= .1f; }
            p.velocity -= faA * (p.after.point - p.point);
            p.after.velocity += faA * (p.after.point - p.point);
        }

        if (!sphere.alive)
        {
            return;
        }

        float mag = rb.velocity.magnitude;
        Vector2 force = Vector2.zero;
        for (int i = 0; i < line.Count; i++)
        {
            Vector2 push = line[i].point - (Vector2)(sphere.transform.position - transform.position); //Account for moving gameobject here
            if (push.sqrMagnitude < .25)
            {
                float pushMag = push.magnitude;
                Vector2 pushNormalized = push / pushMag;
                float velMag = rb.velocity.magnitude;
                Vector2 velNormalized = rb.velocity / velMag;
                Vector2 normal = line[i].normal() * (inverse ? -1 : 1);
                sphere.Touch((Mathf.Max(.05f, Vector2.Dot(velNormalized, pushNormalized))) * velMag, normal);

                Vector2 pForceAligned = (Vector2)Vector3.Project(line[i].velocity, push);
                Vector2 rbForceAligned = (Vector2)Vector3.Project(rb.velocity, push);
                if (Vector2.Dot(rb.velocity, push) >= 0) //player is pushing point
                {
                    line[i].Push(rbForceAligned / 50 * sphereToLineTransfer);
                    rb.velocity -= rbForceAligned * thickness / 5;

                    foreach (Point o in line)
                    {
                        Vector2 PtoO = o.point - line[i].point;
                        if (PtoO.sqrMagnitude > .25f)
                        {
                            float scale = .1f * Mathf.Min(1, 2 / PtoO.magnitude);
                            float distFromLine = Vector3.Cross(PtoO, pushNormalized).magnitude;
                            float dir = Vector2.Dot(push, PtoO);
                            if (distFromLine < 1 && dir > 0)
                            {
                                o.Push(rbForceAligned / 50 * scale * sphereToLineTransfer);
                            }
                        }
                    }
                }
                if (Vector2.Dot(push, line[i].velocity) < 0) //point is pushing player
                {
                    float scale = Mathf.Min(1, (line[i].point - line[i].home).magnitude * 3); //push harder when streched further
                    Vector2 unscaledForce = (Vector2)Vector3.Project(pForceAligned, normal);
                    rb.velocity += unscaledForce * 50 * lineToSphereTransfer * scale; //push along normal
                }
                float p = push.magnitude - .5f; //minus player radius
                line[i].point -= pushNormalized * p; //jump out of point
            }
        }
    }

    void CalculateMesh()
    {
        Vector3[] verts = Point.toV3(line);
        // Triangulator tr = new Triangulator(verts);
        // int[] indices = tr.Triangulate();

        List<int> indices = new List<int>();
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 nextVert = verts[(i + 1) % verts.Length];

            meshVerts.Add(verts[i]);
            lineToVert.Add(meshVerts.Count - 1);

            Vector3 newVertDir = line[i].normal() * (inverse ? 1 : -1);

            float depth = float.MaxValue;
            float closest = float.MaxValue;
            Vector2 other = Vector2.zero;
            foreach (Vector3 o in verts)
            {
                Vector2 PtoO = o - (verts[i] + nextVert) * 0.5f;
                if (PtoO.sqrMagnitude > .9f)
                {
                    float dir = Vector2.Dot(newVertDir, PtoO);
                    if (dir > 0)
                    {
                        float distFromLine = Vector3.Cross(PtoO, newVertDir).magnitude;
                        float dist = PtoO.magnitude;

                        if (distFromLine < .25 && dist < depth && dist < maxFillDist)
                        {
                            other = o;
                            depth = dist;
                            closest = distFromLine;
                        }
                    }
                }
            }
            if (depth == float.MaxValue)
            {
                depth = 3;
            }

            meshVerts.Add((verts[i] + nextVert) * 0.5f + newVertDir * depth / 2);
            //point inward
            indices.Add(meshVerts.Count - 2);
            indices.Add(meshVerts.Count - 1);
            indices.Add(meshVerts.Count);
            //point outward
            indices.Add(meshVerts.Count - 1);
            indices.Add(meshVerts.Count);
            indices.Add(meshVerts.Count + 1);

        }

        for (int i = 0; i < indices.Count; i++)
        {
            if (indices[i] >= meshVerts.Count)
            {
                indices[i] %= meshVerts.Count;
            }
        }

        Mesh msh = new Mesh();
        msh.vertices = meshVerts.ToArray();
        msh.triangles = indices.ToArray();
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        meshFilter.mesh = msh;
    }

    public void SetChild(Splat s)
    {
        float smallest = float.MaxValue;
        Point point = null;
        foreach (Point p in line)
        {
            float d = (p.point - (Vector2)s.transform.position).sqrMagnitude;
            if (d < smallest)
            {
                point = p;
                smallest = d;
            }
        }
        s.SetPoint(point);
    }

    void UpdateVerts()
    {
        Vector2[] edgePoints = new Vector2[line.Count];
        for (int i = 0; i < line.Count; i++)
        {
            meshVerts[lineToVert[i]] = line[i].point;
            edgePoints[i] = line[i].point;
        }
        meshFilter.mesh.vertices = meshVerts.ToArray();
        edge.points = edgePoints;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            for (int i = 0; i < controlPoints.Length; i++)
            {
                Gizmos.DrawSphere(controlPoints[i] + (Vector2)transform.position, .05f);
                Gizmos.DrawLine(controlPoints[i] + (Vector2)transform.position, controlPoints[(i + 1) % controlPoints.Length] + (Vector2)transform.position);
            }
        }
    }

    Vector2 avg(Vector2 a, Vector2 b)
    {
        return (a + b) / 2;
    }

    Vector3 Bezier(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        return (1 - t) * (1 - t) * start + 2 * t * (1 - t) * control + t * t * end;
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}
