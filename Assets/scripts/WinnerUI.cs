// WinnerUI.cs
using UnityEngine;
using TMPro;

public class WinnerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;   // drag a TextMeshPro (UI or world) here
    [SerializeField] private bool hideOnStart = true;

    void Awake()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        if (hideOnStart) gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        if (label) label.text = message;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
