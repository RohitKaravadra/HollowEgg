using UnityEngine;

[System.Serializable]
public class PlayerAttack
{
    [SerializeField][Range(1, 100f)] int _AttackRate = 1;
    [SerializeField] Vector2 _AttackRange;
    [SerializeField] Vector2 _Offset;
    [SerializeField] LayerMask _HittableLayers;

    private float _LastAttackTime;
    public bool CanAttack => Time.time - _LastAttackTime >= 1f / _AttackRate;

    public void OnDrawGizmos(Vector2 pos, Vector2 dir)
    {
        Vector2 offset = dir.y != 0 ? new Vector2(_Offset.y, _Offset.x) : _Offset;
        Vector2 size = dir.y != 0 ? new Vector2(_AttackRange.y, _AttackRange.x) : _AttackRange;
        Vector2 point = pos + (offset * dir);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(point, size);
    }

    private bool CheckHit(Vector2 pos, Vector2 dir)
    {
        // Adjust capsule orientation and offset
        Vector2 offset = dir.y != 0 ? new Vector2(_Offset.y, _Offset.x) : _Offset;
        Vector2 size = dir.y != 0 ? new Vector2(_AttackRange.y, _AttackRange.x) : _AttackRange;
        Vector2 point = pos + (offset * dir);

        // Perform overlap check
        var hit = Physics2D.OverlapBox(point, size, 0, _HittableLayers);

        if (hit.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(1); // << typo? should be Damage?
            return true;
        }

        return false;
    }


    public bool Attack(Vector2 pos, Vector2 dir)
    {
        _LastAttackTime = Time.time;
        return CheckHit(pos, dir);
    }
}
