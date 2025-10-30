using UnityEngine;

public class FallingRocks : MonoBehaviour
{
    [SerializeField] GameObject _RockPrefab;
    [SerializeField] Vector2 _SpawnArea;
    [SerializeField] float _RangeY = 5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.aquamarine;
        Gizmos.DrawWireCube(transform.position + (Vector3)(_SpawnArea / 2), _SpawnArea);
    }
}
