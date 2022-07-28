using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splat : MonoBehaviour
{
    Rigidbody2D rb;
    Point p = null;
    Vector2 offset;
    bool ready;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (p != null)
        {
            transform.position = p.point + offset;
        }
    }

    void OnTriggerExit2D(){
        ready = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Blob blob;
        if (ready && (blob = col.gameObject.GetComponent<Blob>()) != null)
        {
            Destroy(rb);
            blob.SetChild(this);
        }
    }

    public void SetPoint(Point p)
    {
        this.p = p;
        offset = transform.position - (Vector3)p.point;
    }
}
