using TMPro;
using UnityEngine;
using DG.Tweening;

public class InstrumentToggle : MonoBehaviour
{
    public Instrument instrument;
    public UnityEngine.UI.Image icon;
    public TextMeshProUGUI label;
    public Color color;
    public Transform selectionIndexCircle;
    public TextMeshProUGUI selectionIndexText;
    public UnityEngine.UI.Toggle toggle;
    public UnityEngine.UI.Image checkmark;

    private Vector3 baseScale = Vector3.one;

    void Start()
    {
        icon.sprite = instrument.icon;
        label.text = instrument.name;
        checkmark.color = instrument.color;
        selectionIndexCircle.GetComponent<UnityEngine.UI.Image>().color = instrument.color;
    }

    void Update()
    {
        
    }

    public void EnableSelection(int index)
    {
        
        selectionIndexCircle.transform.localScale = Vector3.zero;

        selectionIndexCircle.gameObject.SetActive(true);
        selectionIndexCircle.transform.DOScale(baseScale, 0.25f).SetEase(Ease.OutBounce);
        selectionIndexText.text = index.ToString();
    }

    public void DisableSelection()
    {
        selectionIndexCircle.transform.DOScale(Vector3.zero, 0.1f)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() => selectionIndexCircle.gameObject.SetActive(false));
    }
}
