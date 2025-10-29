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

    private Vector2 ComputeOffset(Vector2 dir)
    {
        // Adjust capsule orientation and offset
        Vector2 offset = dir.y != 0 ? new Vector2(_Offset.y, _Offset.x) : _Offset;
        offset.x *= dir.x == 0 ? 1 : dir.x;
        offset.y *= dir.y == 0 ? 1 : dir.y;
        return offset;
    }

    private Vector2 ComputeSize(Vector2 dir) => dir.y != 0 ?
        new Vector2(_AttackRange.y, _AttackRange.x) : _AttackRange;


    public void OnDrawGizmos(Vector2 pos, Vector2 dir)
    {
        Vector2 offset = ComputeOffset(dir);
        Vector2 size = ComputeSize(dir);

        Vector2 point = pos + offset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(point, size);
    }

    private bool CheckHit(Vector2 pos, Vector2 dir)
    {
        Vector2 offset = ComputeOffset(dir);
        Vector2 size = ComputeSize(dir);

        Vector2 point = pos + offset;

        // Perform overlap check
        var hits = Physics2D.OverlapBoxAll(point, size, 0, _HittableLayers);

        bool hitSomething = false;
        foreach (var hit in hits)
        {
            if (hit && hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(1); // << typo? should be Damage?
                hitSomething = true;
            }
        }

        return hitSomething;
    }

    public bool Attack(Vector2 pos, Vector2 dir)
    {
        _LastAttackTime = Time.time;
        return CheckHit(pos, dir);
    }
}
