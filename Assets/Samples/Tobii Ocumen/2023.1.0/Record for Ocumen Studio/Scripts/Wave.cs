using UnityEngine;

public class Wave : MonoBehaviour
{
    
#pragma warning disable 649 // Field is never assigned
    [SerializeField] private float frequency;
    [SerializeField] private float forceMultiplier;
#pragma warning restore 649
    
    private Rigidbody _rigidbody;
    private float _positionInfluence;
    private float _nextWaveTime;
    
    private void OnEnable()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _positionInfluence = transform.localPosition.x + transform.localPosition.z;
    }

    private void FixedUpdate()
    {
        if (Time.time + (_positionInfluence * 0.1f) > _nextWaveTime) {
            _nextWaveTime += frequency;
            _rigidbody.AddRelativeForce(Vector3.up * forceMultiplier);
        }
    }
}
