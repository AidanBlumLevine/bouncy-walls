using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyOrb : MonoBehaviour
{
    public float energy = .25f;

    public void Collect()
    {
        Crosshair.energy = Mathf.Min(1, Crosshair.energy + energy);
    }
}
