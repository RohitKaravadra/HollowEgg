using UnityEngine;

namespace BehaviourTreeNamespace
{
    [System.Serializable]
    public class PetrolOnGroundStrategy : IStrategy
    {
        [SerializeField] private float _Speed;
        [SerializeField] private Vector2 _Offset;
        [Space(5)]
        [SerializeField] private float _GroundCheckDistance;
        [SerializeField] private float _SideCheckDistance;
        [SerializeField] private LayerMask _GroundLayer;

        private SpriteRenderer _Renderer;
        private Rigidbody2D _Rigidbody;
        private Transform Transform => _Rigidbody.transform;

        public int Dir { get; private set; }

        // getters
        public float GroundCheckDistance => _GroundCheckDistance;
        public float SideCheckDistance => _SideCheckDistance;
        public Vector2 Offset => _Offset;

        public void Init(ref SpriteRenderer renderer, ref Rigidbody2D rigidbody)
        {
            Dir = 1;
            _Renderer = renderer;
            _Rigidbody = rigidbody;
            UpdateDirection();
        }

        public Status Process()
        {
            UpdateDirection();
            Vector2 vel = _Speed * Dir * Time.fixedDeltaTime * Vector2.right;
            _Rigidbody.MovePosition(_Rigidbody.position + vel);

            return Status.Running;
        }

        public void Reset()
        {
            // Reset patrol state here
        }

        private void SetDirection(int dir)
        {
            Dir = dir;
            _Renderer.flipX = Dir < 0;
        }

        private void UpdateDirection()
        {
            Vector2 point = (Vector2)Transform.position + _Offset;

            // check sides
            if (Physics2D.Raycast(point, Vector2.right * Dir, _SideCheckDistance, _GroundLayer))
            {
                SetDirection(-Dir);
                return;
            }

            // check ground ending in forward direction
            point += Dir * _SideCheckDistance * Vector2.right;
            if (!Physics2D.Raycast(point, Vector2.down, _GroundCheckDistance, _GroundLayer))
                SetDirection(-Dir);
        }
    }
}