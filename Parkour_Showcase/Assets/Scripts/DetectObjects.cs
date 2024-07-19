using UnityEngine;

public class DetectObjects : MonoBehaviour
{
    [Header("Object Detection Ray")]
    [SerializeField] float _forwardRayPos;
    [SerializeField] float _forwardRayLength;
    [SerializeField] float _downRayLength;
    [SerializeField] LayerMask _objectLayerMask;

    bool _isForwardRayHit = false;
    bool _isDownRayHit = false;
    RaycastHit _forwardRayData;
    RaycastHit _downRayData;

    private void FixedUpdate()
    {
        ConstructRay();
    }

    void ConstructRay()
    {
        _isForwardRayHit = Physics.Raycast(new Vector3(transform.position.x, transform.position.y + _forwardRayPos, transform.position.z),
            transform.forward, out _forwardRayData, _forwardRayLength, _objectLayerMask);
        // Debug
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + _forwardRayPos, transform.position.z),
        transform.forward * _forwardRayLength, Color.red);

        if (_isForwardRayHit)
        {
            _isDownRayHit = Physics.Raycast(_forwardRayData.point + Vector3.up, Vector3.down,
                out _downRayData, _downRayLength, _objectLayerMask);

            // this ray will give the point position at the top of the obstacles for our foot/hand Target matching
            Debug.DrawRay(_forwardRayData.point + Vector3.up, Vector3.down * _downRayLength, Color.blue);
        }
    }

    // Properties
    public bool HasDownRayHit => _isDownRayHit;
    public RaycastHit DownRayData => _downRayData;
    public RaycastHit ForwardRayData => _forwardRayData;
}