using UnityEngine;

public class FogOffset : MonoBehaviour
{
    [SerializeField] Material _FogMaterial;
    [SerializeField] float _ParallaxFactor = 0.1f;

    Transform _Target;
    bool HasTarget => _Target != null;

    Vector2 _InitPosition;

    private void Awake()
    {
        _InitPosition = transform.position;
        _Target = Camera.main.transform;
    }

    private void Update()
    {
        if (HasTarget)
        {
            Vector2 camPos = _Target.position;
            Vector2 offset = (camPos - _InitPosition) * _ParallaxFactor;
            _FogMaterial.SetVector("_Offset", offset * _ParallaxFactor);
        }
    }
}
