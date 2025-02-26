using UnityEngine;

[CreateAssetMenu(fileName = "Instrument", menuName = "Scriptable Objects/Instrument")]
public class Instrument : ScriptableObject
{
    public string instrumentName;
    public AudioClip[] instrumentSamples;
}
