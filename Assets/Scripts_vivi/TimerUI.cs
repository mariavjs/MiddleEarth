using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public TextMeshProUGUI text;
    void Awake(){ if(text==null) text = GetComponent<TextMeshProUGUI>(); }

    public void Show(float start) { gameObject.SetActive(true); UpdateTime(start); }
    public void Hide(){ gameObject.SetActive(false); }

    public void UpdateTime(float t)
    {
        text.text = "Tempo para sobreviver: " + t.ToString("F1") + "s";
        // opcional: piscar vermelho nos últimos 2s
        if (t < 2f) text.color = Color.red; else text.color = Color.white;
    }
}
