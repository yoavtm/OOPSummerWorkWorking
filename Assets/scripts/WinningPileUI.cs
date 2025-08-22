using System.Collections;
using UnityEngine;
using TMPro;

public class WinningPileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text sumText;     // assign your SumText
    [SerializeField] private string prefix = "Sum: ";
    [SerializeField] private bool pulseOnChange = true;
    [SerializeField] private float pulseScale = 1.15f;
    [SerializeField] private float pulseTime = 0.12f;

    private int current;

    public void SetSum(int sum)
    {
        current = sum;
        if (sumText != null)
        {
            sumText.text = $"{prefix}{sum}";
            if (pulseOnChange) StartCoroutine(Pulse());
        }
    }

    public void ResetToZero()
    {
        SetSum(0);
    }

    private IEnumerator Pulse()
    {
        if (sumText == null) yield break;
        var t = sumText.transform;
        Vector3 baseScale = Vector3.one;
        Vector3 upScale = baseScale * Mathf.Max(1f, pulseScale);

        float e = 0f;
        while (e < pulseTime)
        {
            e += Time.deltaTime;
            float a = Mathf.Clamp01(e / pulseTime);
            t.localScale = Vector3.Lerp(baseScale, upScale, a);
            yield return null;
        }

        e = 0f;
        while (e < pulseTime)
        {
            e += Time.deltaTime;
            float a = Mathf.Clamp01(e / pulseTime);
            t.localScale = Vector3.Lerp(upScale, baseScale, a);
            yield return null;
        }

        t.localScale = baseScale;
    }
}
