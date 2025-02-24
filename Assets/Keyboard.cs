using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Keyboard : MonoBehaviour
{
    public MusicPlayer player;
    public List<Octave> octaves = new List<Octave>();

    public void PlayTone(Tonalite tone, int octaveIndex)
    {
        string note = "";

        switch(tone)
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

        note += octaveIndex;

        player.PlayNote(note, 1, null);
    }
}
