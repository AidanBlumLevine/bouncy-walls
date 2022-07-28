using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
public class CameraController : MonoBehaviour
{
    Camera cam;
    Vector2 oldPos, targetPos;
    float oldZoom = 8, targetZoom = 8, t;
    public AnimationCurve posCurve;
    Blur blur, secondBlur;
    public float blurMulti = 1 / 16f;
    public GameObject backdrop, backdrop2;
    public Camera finalRenderCam, secondaryMetaballCam;
    void Start()
    {
        blur = GetComponent<Blur>();
        secondBlur = secondaryMetaballCam.GetComponent<Blur>();
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t < 1)
        {
            transform.position = Vector2.Lerp(oldPos, targetPos, posCurve.Evaluate(t));
            cam.orthographicSize = Mathf.Lerp(oldZoom, targetZoom, posCurve.Evaluate(t));
            blur.blurSpread = blurMulti / cam.orthographicSize;
        }
        else
        {
            transform.position = targetPos;
            cam.orthographicSize = targetZoom;
            oldPos = targetPos;
            oldZoom = targetZoom;
            blur.blurSpread = blurMulti / cam.orthographicSize;
        }
        secondBlur.blurSpread = blur.blurSpread;
        transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        finalRenderCam.orthographicSize = cam.orthographicSize;
        secondaryMetaballCam.orthographicSize = cam.orthographicSize;
        backdrop.transform.localScale = new Vector3(1.77777f * cam.orthographicSize * 2, cam.orthographicSize * 2, 1);
        backdrop2.transform.localScale = backdrop.transform.localScale;
    }

    public void Set(Vector2 pos, float zoom)
    {
        oldPos = transform.position;
        oldZoom = cam.orthographicSize;
        targetPos = pos;
        targetZoom = zoom;
        t = 0;
    }
}