using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    Transform _Target;

    private void Awake()
    {
        _Target = Camera.main.transform;
    }

    private void Update()
    {
        if (_Target != null)
        {
            Vector3 pos = transform.position;
            pos.x = _Target.position.x;
            pos.y = _Target.position.y;
            transform.position = pos;
        }
    }
}
