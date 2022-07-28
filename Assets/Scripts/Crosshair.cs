using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class Crosshair : MonoBehaviour
{
    public GameObject mLeftSolid, mLeftLight, mRightSolid, mRightLight;
    public SpriteRenderer lineEndDot, lineStartDot;
    Sprite arrow;
    LineRenderer dragLine;
    Vector2 dragDist;
    Vector2 mousePos;
    Animator animator;
    bool mouseDown = false;
    Player player;
    const float energyScale = 5;
    public static float energy, secondaryEnergy;
    public GameObject splatPrefab;
    bool overUI;

    void Start()
    {
        animator = GetComponent<Animator>();
        dragLine = GetComponent<LineRenderer>();
        player = FindObjectOfType<Player>();
        Cursor.visible = false;
        arrow = lineEndDot.sprite;
        lineEndDot.enabled = false;
        lineStartDot.enabled = false;
        dragLine.enabled = false;
        dragLine.positionCount = 2;
        energy = 1;
    }

    public void OverUI(bool t)
    {
        overUI = t;
        animator.SetBool("pointer", t || SceneManager.paused);
    }

    public void Reset()
    {
        energy = 1;
        if (!overUI)
        {
            animator.SetBool("pointer", false);
        }
    }

    void Update()
    {
        Vector2 tempPos = Input.mousePosition;
        if (mouseDown)
        {
            dragDist += (tempPos - mousePos);
        }
        mousePos = tempPos;
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(mousePos);

        if (SceneManager.paused)
        {
            return;
        }

        secondaryEnergy = Mathf.Min(secondaryEnergy + Time.deltaTime / 2, 1);
        mLeftLight.transform.localRotation = Quaternion.RotateTowards(mLeftLight.transform.localRotation, Quaternion.AngleAxis((energy - 1) * 115, Vector3.forward), Time.deltaTime * 100);

        Time.timeScale = Mathf.Lerp(Time.timeScale, mouseDown ? .1f : 1, Time.deltaTime / Time.timeScale * 2);
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        if (Input.GetMouseButtonDown(0) && !overUI && energy > 0.0001f)
        {
            lineEndDot.enabled = true;
            lineStartDot.enabled = true;
            mouseDown = true;
            dragLine.enabled = true;
        }

        Vector3 lineStart = (Vector2)Camera.main.ScreenToWorldPoint(mousePos - dragDist);
        float maxLength = Mathf.Min(2.5f, energy * energyScale);
        Vector3 lineEnd = lineStart + Vector3.ClampMagnitude((transform.position - lineStart), maxLength);
        float lineLength = (lineEnd - lineStart).magnitude;
        mLeftSolid.transform.localRotation = Quaternion.AngleAxis((energy - lineLength / energyScale - 1) * 115, Vector3.forward);
        if (Input.GetMouseButtonUp(0))
        {
            lineEndDot.enabled = false;
            lineStartDot.enabled = false;
            mouseDown = false;
            dragLine.enabled = false;
            dragDist = Vector2.zero;
            if (player.alive)
            {
                player.Sling((lineEnd - lineStart).normalized * lineLength / 2);
                energy -= lineLength / energyScale;
            }
        }

        dragLine.SetPosition(0, lineStart);
        dragLine.SetPosition(1, lineEnd);
        lineEndDot.transform.position = lineEnd;
        lineStartDot.transform.position = lineStart;
        if (lineLength > .25f && (transform.position - lineStart).sqrMagnitude > Mathf.Pow(maxLength + 0.4f, 2))
        {
            lineEndDot.sprite = arrow;
        }
        else
        {
            lineEndDot.sprite = lineStartDot.sprite;
        }
        lineEndDot.transform.rotation = Quaternion.FromToRotation(Vector2.right, lineEnd - lineStart);

        if (Input.GetMouseButtonDown(1) && player.alive)
        {
            //Splat();
        }
    }

    void Splat()
    {
        int r = Random.Range(6, 10);
        for (int i = 0; i < r; i++)
        {
            Vector2 dir = (transform.position - player.transform.position).normalized;
            Quaternion rot = Quaternion.FromToRotation(Vector2.up, dir);
            float ran = Random.value * 1.6f - .8f;
            float dist = ran * ran * ran;
            Vector3 pos = player.transform.position + Quaternion.AngleAxis(90, Vector3.forward) * dir * dist;
            GameObject newSplat = Instantiate(splatPrefab, pos, rot);
            newSplat.transform.localScale *= Random.value + .2f;
            newSplat.GetComponent<Rigidbody2D>().velocity = dir * 20;
        }
    }
}

