using UnityEngine;

public class A1SpriteMotion : MonoBehaviour
{
    private const float MotionSpeedMultiplier = 2.5f;

    public Vector2 driftRange;
    public float cyclesPerSecond = 0.1f;
    public float swayDegrees;
    public float phase;

    private RectTransform rect;
    private Vector2 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            startPosition = rect.anchoredPosition;
            startRotation = rect.localRotation;
        }
    }

    private void Update()
    {
        if (rect == null)
            return;

        var wave = Mathf.Sin(Time.unscaledTime * cyclesPerSecond * MotionSpeedMultiplier * Mathf.PI * 2f + phase);
        rect.anchoredPosition = startPosition + driftRange * wave;
        rect.localRotation = startRotation * Quaternion.Euler(0f, 0f, swayDegrees * wave);
    }
}
