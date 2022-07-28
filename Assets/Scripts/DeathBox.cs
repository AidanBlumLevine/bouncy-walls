using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBox : MonoBehaviour
{
    Player player;
    void Start()
    {
        player = FindObjectOfType<Player>();
        BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        box.isTrigger = true;
        box.size = renderer.size;
    }

    void OnTriggerEnter2D(Collider2D col){
        if(col.gameObject == player.gameObject){
            player.Kill();
        }
    }
}
