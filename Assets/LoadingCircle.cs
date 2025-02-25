using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingCircle : MonoBehaviour
{
    public Image loadingImage;  // L'image à faire tourner
    public float rotationDuration = 1f;  // Durée de la rotation
    public float fadeDuration = 0.5f;  // Durée du fade
    public float minAlpha = 0f;  // Alpha minimum (transparent)
    public float maxAlpha = 1f;  // Alpha maximum (opaque)

    private Tween rotateTween;
    private Tween fadeTween;

    // Fonction d'initialisation qui met l'état à Off
    private void Init()
    {
        // Met l'image à un alpha min et arrête la rotation
        loadingImage.color = new Color(loadingImage.color.r, loadingImage.color.g, loadingImage.color.b, minAlpha);
        if (rotateTween != null) rotateTween.Kill();
    }

    // Fonction pour démarrer le cercle de chargement
    public void On()
    {
        this.transform.rotation = Quaternion.identity;
        // Rotation continue de l'image
        rotateTween = loadingImage.transform.DORotate(new Vector3(0, 0, 360), rotationDuration, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart);

        // Apparition avec un fade-in
        fadeTween = loadingImage.DOFade(maxAlpha, fadeDuration).OnKill(() => fadeTween = null);
    }

    // Fonction pour arrêter le cercle de chargement
    public void Off()
    {
        // Arrêt de la rotation
        if (rotateTween != null)
        {
            rotateTween.Kill();
        }
            

        // Disparition avec un fade-out
        fadeTween = loadingImage.DOFade(minAlpha, fadeDuration).OnKill(() => fadeTween = null);
    }

    private void Start()
    {
        // Initialisation pour mettre l'état à Off dès le départ
        Init();
    }

    private void OnDisable()
    {
        // S'assurer de tuer les tweens si l'objet est désactivé
        rotateTween?.Kill();
        fadeTween?.Kill();
    }
}
