
using UnityEngine;

public class Consumable : MonoBehaviour
{
    [SerializeField] bool _IsDoubleJump;
    [SerializeField] bool _IsDash;
    [SerializeField] AudioSource _PickupSound;
    [Space(5)]
    [SerializeField] Transform _Orb;

    public bool IsDoubleJump => _IsDoubleJump;
    public bool IsDash => _IsDash;

    private Collider2D _Collider;

    private void Awake() => _Collider = GetComponent<Collider2D>();

    public void OnConsume()
    {
        _Orb.gameObject.SetActive(false);
        _Collider.enabled = false;
        _PickupSound.Play();
    }

    public void OnReset()
    {
        _Orb.gameObject.SetActive(true);
        _Collider.enabled = true;
    }
}
