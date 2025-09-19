using UnityEngine;

[System.Serializable]
public class PlayerAttack
{
    [SerializeField][Range(1, 100f)] int _AttackRate = 1;
    [SerializeField] Vector2 _CapsuleSize;
    [SerializeField] Vector2 _Offset;
    [SerializeField] CapsuleDirection2D _Direction;
    [SerializeField] LayerMask _HittableLayers;
    [SerializeField][Range(0, 100)] int Damange = 1;

    private float _LastAttackTime;
    public bool CanAttack => Time.time - _LastAttackTime >= 1f / _AttackRate;

    public void OnDrawGizmos(Vector2 pos, Vector2 dir)
    {
        _Direction = dir.y != 0 ? CapsuleDirection2D.Vertical : CapsuleDirection2D.Horizontal;
        Vector2 offset = dir.y != 0 ? new Vector2(_Offset.y, _Offset.x) : _Offset;
        Vector2 point = pos + (offset * dir);

        // draw capsule with size and offset
        float radii = _CapsuleSize.x / 2;
        float halfLenght = _CapsuleSize.y / 2 - radii;
        halfLenght = Mathf.Max(0, halfLenght);

        Vector2 topDir = _Direction == CapsuleDirection2D.Horizontal ? Vector2.right : Vector2.up;
        Vector2 bottomDir = _Direction == CapsuleDirection2D.Horizontal ? Vector2.left : Vector2.down;

        Vector2 top = point + (topDir * halfLenght);
        Vector2 bottom = point + (bottomDir * halfLenght);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(top, radii);
        Gizmos.DrawWireSphere(bottom, radii);
    }

    private bool ApplyDamage(Vector2 pos, Vector2 dir)
    {
        Vector2 offset = dir.y != 0 ? new Vector2(_Offset.y, _Offset.x) : _Offset;
        _Direction = dir.y != 0 ? CapsuleDirection2D.Vertical : CapsuleDirection2D.Horizontal;

        Vector2 point = pos + (offset * dir);

        var hits = Physics2D.OverlapCapsuleAll(point, _CapsuleSize, _Direction, 0, _HittableLayers);

        bool isHit = false;
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(Damange);
                isHit = true;
            }
        }

        return isHit;
    }

    public bool Attack(Vector2 pos, Vector2 dir)
    {
        _LastAttackTime = Time.time;
        return ApplyDamage(pos, dir);
    }
}
