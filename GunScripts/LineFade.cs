using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class LineFade : MonoBehaviour
{
    private LineRenderer lr;
    private float duration = 0.5f;

    public void Init(float fadeDuration)
    {
        duration = fadeDuration;
    }

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        Color startColor = lr.startColor;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duration);

            Color c = startColor;
            c.a = alpha;
            lr.startColor = c;
            lr.endColor = c;

            yield return null;
        }

        Destroy(gameObject); // only destroys the line prefab
    }
}