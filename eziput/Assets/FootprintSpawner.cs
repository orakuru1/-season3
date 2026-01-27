using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintSpawner : MonoBehaviour
{
    public GameObject footprintPrefab;
    public float distance = 1.5f;

    Vector3 lastPos;
    float distanceSqr;

    void Start()
    {
        lastPos = transform.position;
        distanceSqr = distance * distance;
    }

    void Update()
    {
        Vector3 diff = transform.position - lastPos;

        if (diff.sqrMagnitude >= distanceSqr)
        {
            Quaternion rot =
            transform.rotation
            * Quaternion.Euler(0, 180f, 0)
            * footprintPrefab.transform.rotation;
            
            Instantiate(
                footprintPrefab,
                new Vector3(transform.position.x, 0.3f, transform.position.z),
                rot
            );

            lastPos = transform.position;
        }
    }
}


