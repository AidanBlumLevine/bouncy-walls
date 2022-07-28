using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappler : MonoBehaviour
{
    bool attached, launching, returning;
    float attachedLength, length;
    Vector2 launchDir;
    List<Vector2> segments = new List<Vector2>();
    LineRenderer lineRenderer;
    Player player;
    public LayerMask attachLayer;
    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        lineRenderer = GetComponent<LineRenderer>();
        segments.Add(transform.position);
    }

    void Update()
    {
        if (!player.alive)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        if (SceneManager.paused)
        {
            return;
        }
        segments[segments.Count - 1] = transform.position;
        if (Input.GetMouseButtonDown(1) && !returning)
        {
            launchDir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
            launching = true;
            segments.Insert(0, transform.position);
        }
        if (launching)
        {
            RaycastHit2D hit; //has it reached the wall
            if (hit = Physics2D.Raycast(segments[0], launchDir, Time.deltaTime * 30, attachLayer))
            {
                segments[0] = hit.point;
                attached = true;
                launching = false;
                attachedLength = 0;
                for (int i = 0; i < segments.Count - 1; i++)
                {
                    Vector2 dir = segments[i + 1] - segments[i];
                    attachedLength += dir.magnitude;
                }
            }//has LOS been broken with the end of the grapple
            else if (hit = Physics2D.Raycast(segments[1], segments[0] - segments[1], (segments[0] - segments[1]).magnitude, attachLayer))
            {
                launching = false;
                returning = true;
                segments[0] = hit.point;
            }
            else if (launching && (segments[0] - segments[segments.Count - 1]).sqrMagnitude > 100)
            {//has it reached max launch dist
                launching = false;
                returning = true;
            }
            else
            {
                segments[0] += launchDir * Time.deltaTime * 30; //otherwise just move
            }

        }
        if (Input.GetMouseButtonUp(1))
        {
            launching = false;
            attached = false;
            returning = true;
        }
        if (returning)
        {
            if (segments.Count > 1)
            {
                segments[0] = Vector2.MoveTowards(segments[0], segments[1], Time.deltaTime * 30);
                if (segments[0] == segments[1])
                {
                    segments.RemoveAt(0);
                }
            }
            else
            {
                returning = false;
            }
        }
        else
        {
            for (int i = 1; i < segments.Count; i++)
            {
                Vector2 dir = segments[i - 1] - segments[i];
                float dm = dir.magnitude;
                RaycastHit2D hit;
                if (hit = Physics2D.Raycast(segments[i], dir, dm - .1f, attachLayer))
                {
                    if (hit.distance > .1f)
                    {
                        segments.Insert(i, hit.point + hit.normal / 100);
                    }
                }
            }

            for (int i = 1; i < segments.Count - 1; i++)
            {
                Vector2 dir = segments[i - 1] - segments[i + 1];
                if (!Physics2D.Raycast(segments[i + 1], dir, dir.magnitude - .1f, attachLayer))
                {
                    segments.RemoveAt(i);
                    i--;
                }
            }

            length = 0;
            for (int i = 1; i < segments.Count - 1; i++)
            {
                Vector2 dir = segments[i - 1] - segments[i];
                length += dir.magnitude;
            }
        }

        lineRenderer.positionCount = segments.Count;
        Vector3[] vLine = new Vector3[segments.Count];
        for (int i = 0; i < segments.Count; i++)
        {
            vLine[i] = segments[i];
        }
        lineRenderer.SetPositions(vLine);
    }

    void FixedUpdate()
    {
        if (attached)
        {
            float dMulti = (segments[segments.Count - 2] - segments[segments.Count - 1]).magnitude - attachedLength + length;
            if (Vector2.Dot(rb.velocity, (segments[segments.Count - 2] - segments[segments.Count - 1])) < 0)
            {
                Vector2 velAlongLine = Vector3.Project(rb.velocity, (segments[segments.Count - 2] - segments[segments.Count - 1]).normalized);
                rb.velocity -= velAlongLine * Mathf.Max(0, dMulti);
            }
            // if (dMulti > 0)
            // {
            //     rb.velocity += lineDir * dMulti * Time.fixedDeltaTime;
            // }
        }
    }

    public void Reset()
    {
        attached = false;
        lineRenderer.positionCount = 0;
        segments.Clear();
        segments.Add(transform.position);
    }
}
