using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Mover : NetworkBehaviour
{

    [SerializeField]
    float speed = 10f;
    public Vector3 targetPostion;

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPostion, step);
    }
}
