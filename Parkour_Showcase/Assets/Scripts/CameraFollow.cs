using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform _transformToFollow;

    private void LateUpdate()
    {
        transform.rotation = _transformToFollow.rotation;
        if (_transformToFollow.position.y > 0.1f)
        {
            transform.position = new Vector3(_transformToFollow.position.x, _transformToFollow.position.y,
                _transformToFollow.position.z);
        }
        else if(_transformToFollow.position.y < -1)
        {
            transform.position = new Vector3(_transformToFollow.position.x, _transformToFollow.position.y,
                _transformToFollow.position.z);
        }
        else
        {
            transform.position = new Vector3(_transformToFollow.position.x, 0, _transformToFollow.position.z);
        }
    }
}
