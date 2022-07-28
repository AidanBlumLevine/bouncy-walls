using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ZoomBox : MonoBehaviour
{
    public Vector3 cameraPosition;
    public bool seperateCameraPos;
    public float cameraZoom;
    CameraController cam;
    Player player;

    void Start()
    {
        cam = FindObjectOfType<CameraController>();
        player = FindObjectOfType<Player>();
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (player.alive && col.gameObject == player.gameObject)
        {
            cam.Set(cameraPosition + transform.position, cameraZoom);
        }
    }
}

// [CustomEditor(typeof(ZoomBox)), CanEditMultipleObjects]
// public class ZoomBoxEditor : Editor
// {
//     void OnSceneGUI()
//     {
//         var zb = target as ZoomBox;
//         float cameraZoom = zb.cameraZoom;
//         Vector3 cameraPosition = zb.cameraPosition;
//         if (zb.seperateCameraPos)
//         {
//             cameraPosition = Handles.PositionHandle(cameraPosition + zb.transform.position, Quaternion.identity) - zb.transform.position;
//             zb.cameraPosition = cameraPosition;
//         }
//         cameraZoom = Vector3.Distance(Handles.PositionHandle(cameraPosition + zb.transform.position + Vector3.up * cameraZoom, Quaternion.identity), cameraPosition + zb.transform.position);
//         zb.cameraZoom = cameraZoom;
//         Vector3 c = cameraPosition + zb.transform.position;
//         Vector3 dup = Vector3.up * cameraZoom;
//         Vector3 dul = Vector3.right * cameraZoom * .888f * 2;
//         Handles.DrawLine(c + dup - dul, c + dup + dul);
//         Handles.DrawLine(c + dup + dul, c - dup + dul);
//         Handles.DrawLine(c - dup + dul, c - dup - dul);
//         Handles.DrawLine(c - dup - dul, c + dup - dul);
//     }
// }