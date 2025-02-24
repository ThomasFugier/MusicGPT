using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Octave thisOctave;
    public Button button;
    public Tonalite tonalite;
    public TMPro.TextMeshProUGUI text;
    public Color highlightColor;
    public UnityEngine.UI.Image highlight;

    private float startTime;
    public bool isInScale = false;

    public void Update()
    {
        if(isInScale)
        {
            highlight.color = Color.Lerp(highlight.color, highlightColor, Time.deltaTime * 3);
        }

        else
        {
            highlight.color = Color.Lerp(highlight.color, Color.clear, Time.deltaTime * 3);
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
        thisOctave.keyboard.PlayTone(tonalite, thisOctave.octaveIndex);
        Debug.Log("ICI");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
}
