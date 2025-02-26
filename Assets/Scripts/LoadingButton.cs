using UnityEngine;
using DG.Tweening;

public class LoadingButton : MonoBehaviour
{
    public LoadingCircle loadingCircle;
    public GameObject button;

    private Vector3 baseScale;

    public void Start()
    {
        baseScale = button.transform.localScale;
    }

    public void OnClick()
    {
        button.transform.DOPunchScale(transform.localScale * 0.1f, 0.5f).OnComplete(() =>
        {
            button.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                loadingCircle.On();
            });
        });
    }

    public void TurnButtonBack()
    {
        Debug.Log("ICI 2");

        loadingCircle.Off();
        button.transform.DOScale(baseScale, 0.2f).SetEase(Ease.InOutCubic);
    }
}
