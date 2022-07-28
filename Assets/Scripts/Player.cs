using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody2D rb;
    ParticleSystem particle;
    public float maxSpeed = 20;
    float pCooldown = 0;
    Vector2 home;
    [HideInInspector]
    public bool alive = true;
    [HideInInspector]
    public bool respawning = false;
    void Start()
    {
        home = transform.position;
        particle = GetComponent<ParticleSystem>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Reset()
    {
        if (alive)
        {
            Kill();
            respawning = true;
            Invoke("Reset", .01f);
        }
        else
        {
            respawning = false;
            transform.position = home;
            rb.velocity = Vector2.zero;
            GetComponent<MeshRenderer>().enabled = true;
            alive = true;
        }
    }

    public void Sling(Vector2 dir)
    {
        rb.AddForce(dir * 500 / Time.timeScale);
    }

    public void Kill()
    {
        if(!alive){
            return;
        }
        ParticleSystem.Burst b = new ParticleSystem.Burst();
        b.count = 30;
        particle.emission.SetBurst(0, b);
        var shape = particle.shape;
        shape.arc = 360;
        var main = particle.main;
        var curve = main.startSpeed;
        curve.constantMin = 2;
        curve.constantMax = 5;
        main.startColor = Color.blue;
        particle.Play();
        GetComponent<MeshRenderer>().enabled = false;
        alive = false;
    }

    public void Touch(float str, Vector2 dir)
    {
        if (pCooldown > .1f)
        {
            ParticleSystem.Burst b = new ParticleSystem.Burst();
            b.count = str > 5 ? str - 2 : 0;
            particle.emission.SetBurst(0, b);
            var shape = particle.shape;
            shape.arc = 180;
            var main = particle.main;
            main.startColor = new Color(0, .9137255f, .9019608f);
            var curve = main.startSpeed;
            curve.constantMin = str / 8;
            curve.constantMax = str / 2;
            shape.rotation = Quaternion.FromToRotation(Vector2.up, dir).eulerAngles;
            particle.Play();
        }
        pCooldown = 0;
    }

    void FixedUpdate()
    {
        pCooldown += Time.fixedDeltaTime;
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, 20);
    }
}