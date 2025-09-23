using UnityEngine;

public class ImageOscillator : MonoBehaviour
{
    [Header("Oscillation Settings")]
    public Vector2 direction = new Vector2(1f, 0f); // (1,0)=Horizontal, (0,1)=Vertical, (1,1)=Diagonal
    public float amplitude = 10f;   // How far it moves (pixels)
    public float frequency = 1f;    // Speed of oscillation

    private RectTransform rectTransform;
    private Vector3 startPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float wave = Mathf.Sin(Time.time * frequency) * amplitude;
        rectTransform.anchoredPosition = startPos + (Vector3)(direction.normalized * wave);
    }
}
