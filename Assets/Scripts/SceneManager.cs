using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public Animator resetAnim;
    public static bool paused;
    float resetT = 0;
    Player player;
    void Start()
    {
        player = FindObjectOfType<Player>();
    }

    void Update()
    {
        if (Crosshair.energy < .01f)
        {
            resetT += Time.deltaTime;
        }
        else
        {
            resetT = 0;
        }
        resetAnim.SetBool("visible", paused || resetT > 1.5f || (!player.alive && !player.respawning));

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reset();
        }
    }

    public void Pause()
    {
        paused = !paused;
        if (!paused)
        {
            Time.timeScale = 1;
            Time.fixedDeltaTime = .02f;
        }
        else
        {
            Time.timeScale = 0;
            Time.fixedDeltaTime = 0;
        }
    }

    public void Reset()
    {
        paused = false;
        Time.timeScale = 1;
        Time.fixedDeltaTime = .02f;
        player.Reset();
        foreach (Collectable c in FindObjectsOfType<Collectable>())
        {
            c.Reset();
        }
        FindObjectOfType<Crosshair>().Reset();
        FindObjectOfType<Grappler>().Reset();
    }
}
