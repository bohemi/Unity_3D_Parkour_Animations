using UnityEngine;

[CreateAssetMenu(menuName = "My Parkours/Parkour Settings")]
public class AnimationSettings : ScriptableObject
{
    [Header("Set Object Height")]
    [SerializeField] string _animatioName;
    [SerializeField] float _minHeight;
    [SerializeField] float _maxHeight;
    [SerializeField] bool _shouldPlayerRotate;
    [SerializeField] Quaternion _rotationDirection;

    public bool CanPerformHeightBasedAction(RaycastHit hitData)
    {
        if (hitData.collider == null)
            return false;

        if (hitData.transform.localScale.y > _maxHeight || hitData.transform.localScale.y < _minHeight)
            return false;

        if (_shouldPlayerRotate)
            _rotationDirection = Quaternion.LookRotation(-hitData.normal);

        return true;
    }

    // Properties
    public bool ShouldPlayerRotate => _shouldPlayerRotate;
    public string AnimationName => _animatioName;
    public Quaternion RotationDirection => _rotationDirection;
}
