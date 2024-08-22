using UnityEngine;

public class DetectObjects : MonoBehaviour
{
    [Header("Object Detection Ray")]
    [SerializeField] float _braceEnableTime;
    [SerializeField] float _forwardRayPos = 0.45f;
    [SerializeField] float _forwardRayLength = 0.65f;
    [SerializeField] float _downRayLength = 1.0f;
    [SerializeField] LayerMask _objectLayerMask;

    [Header("Brace Detection Ray")]
    [SerializeField] float _braceRayPos = 1.85f;
    [SerializeField] float _braceRayLength = 0.36f;
    [SerializeField] float _braceRayDownLength = 1.1f;
    [SerializeField] LayerMask _braceLayerMask;

    bool _isForwardRayHit = false;
    bool _isBraceRayHit = false;
    RaycastHit _forwardRayData;
    RaycastHit _downRayData;
    RaycastHit _braceRayData;
    RaycastHit _braceRayDownData;

    private void FixedUpdate()
    {
        ConstructRay();
        ConstructBraceRay();
    }
    
    void ConstructBraceRay()
    {
        // Debug
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + _braceRayPos, transform.position.z),
        transform.forward * _braceRayLength, Color.red);

        _isBraceRayHit = Physics.Raycast(new Vector3(transform.position.x, transform.position.y + _braceRayPos, transform.position.z),
            transform.forward, out _braceRayData, _braceRayLength, _braceLayerMask);

        if (_isBraceRayHit)
        {
            Physics.Raycast(_braceRayData.point + Vector3.up, Vector3.down,
                out _braceRayDownData, _braceRayDownLength, _braceLayerMask);

            Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + _braceRayPos, transform.position.z),
        transform.forward * _braceRayLength, Color.blue);

            Debug.DrawRay(_braceRayData.point + Vector3.up, Vector3.down * _braceRayDownLength, Color.blue);
        }
    }

    public void TurnOffBraceRay()
    {
        _braceRayLength = 0.0f;
        Invoke(nameof(EnableBraceRay), _braceEnableTime);
    }

    void EnableBraceRay()
    {
        _braceRayLength = 0.36f;
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
            Physics.Raycast(_forwardRayData.point + Vector3.up, Vector3.down,
                out _downRayData, _downRayLength, _objectLayerMask);

            // this ray will give the point position at the top of the obstacles for our foot/hand Target matching
            Debug.DrawRay(_forwardRayData.point + Vector3.up, Vector3.down * _downRayLength, Color.blue);
        }
    }

    // Properties
    public RaycastHit DownRayData => _downRayData;
    public RaycastHit ForwardRayData => _forwardRayData;
    public RaycastHit BraceRayData => _braceRayData;
    public RaycastHit BraceRayDownData => _braceRayDownData;
}