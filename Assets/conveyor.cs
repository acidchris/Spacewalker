using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class conveyor : MonoBehaviour
{
    private Rigidbody _rigidBody = null;
    public float _speed = 5f;

    
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 pos = _rigidBody.position;
        _rigidBody.position += Vector3.right * _speed * Time.fixedDeltaTime;

        _rigidBody.MovePosition(pos);
    }
}
