using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MusicBlock
{
    public string type; // "note" ou "chord"
    public string value; // Nom de la note ou de l'accord
    public string duration; // Dur�e de la note ou de l'accord sous forme de texte (ex : "croche", "blanche")
}

[System.Serializable]
public class Partition
{
    public List<MusicBlock> blocks;
}

public enum PlayMode
{
    Partition,   // Mode de lecture de la partition
    RandomPlaying // Mode de lecture al�atoire
}

public class MusicPlayer : MonoBehaviour
{
    [TextArea(20, 50)]
    public string jsonPartition;
    public AudioClip[] notes; // Contient les 12 notes de base (une octave)
    public float tempo = 120f;
    public PlayMode playMode = PlayMode.Partition; // Mode de lecture
    private Dictionary<string, int> noteMap;

    void Start()
    {
        noteMap = new Dictionary<string, int>
        {
            {"C", 0}, {"C#", 1}, {"D", 2}, {"D#", 3}, {"E", 4}, {"F", 5},
            {"F#", 6}, {"G", 7}, {"G#", 8}, {"A", 9}, {"A#", 10}, {"B", 11}
        };

        if (playMode == PlayMode.Partition)
        {
            Partition partition = JsonUtility.FromJson<Partition>(jsonPartition);
            StartCoroutine(PlayPartition(partition));
        }
        else if (playMode == PlayMode.RandomPlaying)
        {
            StartCoroutine(PlayRandomNotes());
        }
    }

    private float GetDurationInSeconds(string duration)
    {
        switch (duration.ToLower())
        {
            case "ronde":
                return 4f * 60f / tempo;
            case "blanche":
                return 2f * 60f / tempo;
            case "noire":
                return 1f * 60f / tempo;
            case "croche":
                return 0.5f * 60f / tempo;
            case "doublecroche":
                return 0.25f * 60f / tempo;
            case "triplecroche":
                return 0.125f * 60f / tempo;
            case "quadruplecroche":
                return 0.0625f * 60f / tempo;
            default:
                Debug.LogError("Dur�e non reconnue : " + duration);
                return 1f; // Valeur par d�faut
        }
    }

    private IEnumerator PlayPartition(Partition partition)
    {
        foreach (var block in partition.blocks)
        {
            float durationInSeconds = GetDurationInSeconds(block.duration); // Convertir la dur�e en secondes
            if (block.type == "chord")
            {
                PlayChord(block.value);
            }
            else if (block.type == "note")
            {
                StartCoroutine(PlayNoteRoutine(block.value, durationInSeconds));
            }
            yield return new WaitForSeconds(durationInSeconds);
        }
    }

    private void PlayChord(string chord)
    {
        GameObject chordObject = new GameObject("Chords = " + chord);
        string[] noteArray = chord.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);  // On prend en compte les accords s�par�s par '|'
        foreach (string note in noteArray)
        {
            PlayNote(note.Trim(), 60f / tempo, chordObject);
        }
    }

    private IEnumerator PlayNoteRoutine(string note, float duration)
    {
        PlayNote(note, duration, null);
        yield return new WaitForSeconds(duration);
    }

    private void PlayNote(string note, float duration, GameObject parent)
    {
        if (string.IsNullOrEmpty(note)) return;

        try
        {
            // V�rifier si l'entr�e est un accord (s�par� par un point-virgule)
            if (note.Contains(";"))
            {
                string[] notesInChord = note.Split(';'); // Utiliser ; comme s�parateur pour les notes d'un accord
                foreach (string chordNote in notesInChord)
                {
                    PlaySingleNote(chordNote.Trim(), duration, parent); // Appeler PlaySingleNote pour chaque note dans l'accord
                }
            }
            else
            {
                PlaySingleNote(note, duration, parent); // Si ce n'est pas un accord, jouer la note seule
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors de l'analyse de la note: " + note + " - " + e.Message);
        }
    }

    private void PlaySingleNote(string note, float duration, GameObject parent)
    {
        if (note.Length < 2) return;

        try
        {
            string noteKey = note.Length > 2 ? note.Substring(0, 2) : note.Substring(0, 1);
            int octave = int.Parse(note.Substring(noteKey.Length, 1));
            float pitch = Mathf.Pow(2, (octave - 4) + noteMap[noteKey] / 12f);

            GameObject noteObject = new GameObject(note);
            if (parent != null)
            {
                noteObject.transform.parent = parent.transform;
            }

            AudioSource source = noteObject.AddComponent<AudioSource>();
            int noteIndex = noteMap[noteKey] % notes.Length;
            source.clip = notes[noteIndex] != null ? notes[noteIndex] : FindClosestAvailableNote(noteIndex);
            source.pitch = pitch;
            source.Play();
            StartCoroutine(DestroyAfterPlaying(noteObject, source));
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors de l'analyse de la note: " + note + " - " + e.Message);
        }
    }

    private IEnumerator DestroyAfterPlaying(GameObject noteObject, AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        Destroy(noteObject);
    }

    private AudioClip FindClosestAvailableNote(int index)
    {
        int prevIndex = (index - 1 + notes.Length) % notes.Length;
        int nextIndex = (index + 1) % notes.Length;
        return notes[prevIndex] != null ? notes[prevIndex] : notes[nextIndex];
    }

    // Coroutine pour jouer des notes ou accords al�atoires
    private IEnumerator PlayRandomNotes()
    {
        while (true)
        {
            if (Random.Range(0, 2) == 0) // 50% de chance de jouer une note ou un accord
            {
                string randomNote = GetRandomNote();
                float randomDuration = Random.Range(0.2f, 1.0f); // Dur�e al�atoire entre 0.2 et 1 seconde
                PlayNote(randomNote, randomDuration, null);
            }
            else
            {
                string randomChord = GetRandomChord(); // G�n�rer un accord al�atoire
                float randomDuration = Random.Range(0.5f, 1.5f); // Dur�e al�atoire pour les accords
                PlayChord(randomChord);
            }

            yield return new WaitForSeconds(Random.Range(0.3f, 1.5f)); // Intervalle entre les notes ou accords
        }
    }

    private string GetRandomNote()
    {
        string[] noteKeys = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        return noteKeys[Random.Range(0, noteKeys.Length)] + Random.Range(3, 6); // Note al�atoire avec octave entre 3 et 6
    }

    // G�n�rer un accord al�atoire
    private string GetRandomChord()
    {
        // S�lectionner une note de base pour l'accord
        string rootNote = GetRandomNote().Substring(0, 1); // Prendre la premi�re lettre de la note comme racine
        int octave = Random.Range(3, 6); // S�lectionner une octave entre 3 et 6

        // Cr�er un accord majeur (racine, tierce majeure, quinte)
        string chord = "|" + rootNote + octave + "|"; // Racine
        chord += GetNoteByInterval(rootNote, 4) + octave + "|"; // Tierce majeure
        chord += GetNoteByInterval(rootNote, 7) + octave + "|"; // Quinte

        // 50% chance de cr�er un accord mineur � la place
        if (Random.Range(0, 2) == 0)
        {
            // Cr�er un accord mineur (racine, tierce mineure, quinte)
            chord = "|" + rootNote + octave + "|"; // Racine
            chord += GetNoteByInterval(rootNote, 3) + octave + "|"; // Tierce mineure
            chord += GetNoteByInterval(rootNote, 7) + octave + "|"; // Quinte
        }

        return chord;
    }



    // Obtenir une note par intervalle
    private string GetNoteByInterval(string rootNote, int interval)
    {
        int rootIndex = noteMap[rootNote];
        int targetIndex = (rootIndex + interval) % 12;
        return noteMap.Keys.ToArray()[targetIndex];
    }
}
