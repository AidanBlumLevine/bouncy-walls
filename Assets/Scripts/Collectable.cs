using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
public class Collectable : MonoBehaviour
{
    Player player;
    Vector2 home;
    public UnityEvent collectEvent;
    bool collected;
    void Start()
    {
        home = transform.position;
        player = FindObjectOfType<Player>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!collected && player.alive && col.gameObject == player.gameObject)
        {
            StartCoroutine("Collect");
        }
    }

    public void Reset()
    {
        collected = false;
        transform.position = home;
        GetComponent<Renderer>().enabled = true;
    }

    IEnumerator Collect()
    {
        float dist2 = (transform.position - player.transform.position).sqrMagnitude;
        while (dist2 > .1f)
        {
            if (SceneManager.paused)
            {
                yield return null;
            }
            dist2 = (transform.position - player.transform.position).sqrMagnitude;
            transform.position = Vector2.MoveTowards(transform.position, (Vector2)player.transform.position + player.rb.velocity * Time.fixedDeltaTime, Time.fixedDeltaTime * 6);
            yield return null;
        }
        collected = true;
        collectEvent.Invoke();
        GetComponent<Renderer>().enabled = false;
    }
}
