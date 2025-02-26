using UnityEngine;

[CreateAssetMenu(fileName = "Instrument", menuName = "Scriptable Objects/Instrument")]
public class Instrument : ScriptableObject
{
    public float volume = 1;
    public string instrumentName;
    public AudioClip[] instrumentSamples;
}
