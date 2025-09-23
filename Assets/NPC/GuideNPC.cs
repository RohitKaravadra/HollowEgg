using UnityEngine;

public class GuideNPC : MonoBehaviour
{
    SpriteRenderer _SpriteRenderer;
    Transform _Target;
    bool HasTargte => _Target != null;

    private void Start()
    {
        _Target = EnemySharedData._PlayerTransform;
        _SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (HasTargte && _SpriteRenderer != null)
        {
            float dir = _Target.position.x - transform.position.x;
            if (Mathf.Abs(dir) < 0.2f) return;
            _SpriteRenderer.flipX = dir < 0;
        }
    }
}
