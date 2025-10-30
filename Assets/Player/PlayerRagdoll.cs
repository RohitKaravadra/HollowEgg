using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{
    [SerializeField] SpriteRenderer _Visuals;
    [SerializeField] Vector2 _ForceOnEnable = Vector2.up * 5f;

    Rigidbody2D _Rb;
    public bool Enabled
    {
        get => gameObject.activeSelf;
        set
        {
            gameObject.SetActive(value);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            if (_Visuals)
                _Visuals.enabled = !value;
            if (_Rb || TryGetComponent(out _Rb))
            {
                _Rb.linearVelocity = _ForceOnEnable;
                _Rb.angularVelocity = Random.Range(-200f, 200f);
            }
        }
    }
}
