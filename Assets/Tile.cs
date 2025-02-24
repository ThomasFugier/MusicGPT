using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Octave thisOctave;
    public Button button;
    public Tonalite tonalite;

    private float startTime;

    public void OnPointerDown(PointerEventData eventData)
    {
        thisOctave.keyboard.PlayTone(tonalite, thisOctave.octaveIndex);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
}
