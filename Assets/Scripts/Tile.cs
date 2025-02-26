using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Octave thisOctave;
    public Button button;
    public Tonalite tonalite;
    public TMPro.TextMeshProUGUI text;
    public Color highlightColor;
    public UnityEngine.UI.Image highlight;

    private float startTime;
    public bool isInScale = false;
    public bool canBeHighlighted = false;
    public bool canBeLocked = false;

    private bool isDown;
    public bool isWhiteTile;

    public void Update()
    {
        if(isInScale && canBeHighlighted)
        {
            highlight.color = Color.Lerp(highlight.color, highlightColor, Time.deltaTime * 6);
        }

        else
        {
            highlight.color = Color.Lerp(highlight.color, new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0), Time.deltaTime * 6);
        }

        if(canBeLocked && isInScale == false)
        {
            button.interactable = false;
        }

        else
        {
            button.interactable = true;
        }
    }

    public void Start()
    {
        string note = "";

        switch (tonalite)
        {
            case Tonalite.C:
                note += "C";
                break;

            case Tonalite.D:
                note += "D";
                break;

            case Tonalite.E:
                note += "E";
                break;

            case Tonalite.F:
                note += "F";
                break;

            case Tonalite.G:
                note += "G";
                break;

            case Tonalite.A:
                note += "A";
                break;

            case Tonalite.B:
                note += "B";
                break;

            case Tonalite.CSharp:
                note += "C#";
                break;

            case Tonalite.DSharp:
                note += "D#";
                break;

            case Tonalite.FSharp:
                note += "F#";
                break;

            case Tonalite.GSharp:
                note += "G#";
                break;

            case Tonalite.ASharp:
                note += "A#";
                break;
        }

        text.text = note;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.IsInteractable())
        {
            thisOctave.keyboard.PlayTone(tonalite, thisOctave.octaveIndex);
            PressVisual();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0) && button.IsInteractable())
        {
            thisOctave.keyboard.PlayTone(tonalite, thisOctave.octaveIndex);
            PressVisual();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        button.OnPointerUp(eventData);

        if(isDown)
        {
            ReleaseVisual();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReleaseVisual();
    }

    private void PressVisual()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOScale(Vector3.one * 0.9f, 0.1f);

        if(isWhiteTile)
        {
            button.image.DOColor(new Color(0.8f, 0.8f, 0.8f, 1), 0.1f);
        }

        else
        {
            button.image.DOColor(new Color(0.2f, 0.2f, 0.2f, 1), 0.1f);
        }

        isDown = true;
    }

    private void ReleaseVisual()
    {
        transform.DOKill();
        transform.DOScale(Vector3.one, 0.1f);

        if (isWhiteTile)
        {
            button.image.DOColor(new Color(1, 1, 1, 1), 0.1f);
        }

        else
        {
            button.image.DOColor(new Color(0.1f, 0.1f, 0.1f, 1), 0.1f);
        }

        isDown = false;
    }

    public void PressAndReleaseAfter(float t)
    {
        StartCoroutine(PressAndRelease(t));

    }

    IEnumerator PressAndRelease(float t)
    {
        PressVisual();
        yield return new WaitForSeconds(t);
        ReleaseVisual();
    }
}
