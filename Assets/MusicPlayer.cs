using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class MusicBlock
{
    public string type; // "note" ou "chord"
    public string value; // Nom de la note ou de l'accord
    public float duration; // Durée de la note ou de l'accord
}

[System.Serializable]
public class Partition
{
    public List<MusicBlock> blocks;
}

public class MusicPlayer : MonoBehaviour
{
    [TextArea(20, 50)]
    public string jsonPartition;
    public AudioClip[] notes; // Contient les 12 notes de base (une octave)
    public float tempo = 120f;
    private Dictionary<string, int> noteMap;

    void Start()
    {
        noteMap = new Dictionary<string, int>
        {
            {"C", 0}, {"C#", 1}, {"D", 2}, {"D#", 3}, {"E", 4}, {"F", 5},
            {"F#", 6}, {"G", 7}, {"G#", 8}, {"A", 9}, {"A#", 10}, {"B", 11}
        };

        Partition partition = JsonUtility.FromJson<Partition>(jsonPartition);
        StartCoroutine(PlayPartition(partition));
    }

    private IEnumerator PlayPartition(Partition partition)
    {
        foreach (var block in partition.blocks)
        {
            if (block.type == "chord")
            {
                PlayChord(block.value);
            }
            else if (block.type == "note")
            {
                StartCoroutine(PlayNoteRoutine(block.value, block.duration));
            }
            yield return new WaitForSeconds(block.duration);
        }
    }

    private void PlayChord(string chord)
    {
        GameObject chordObject = new GameObject("Chords = " + chord);
        string[] noteArray = chord.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);  // On prend en compte les accords séparés par '|'
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
            // Vérifier si l'entrée est un accord (séparé par un point-virgule)
            if (note.Contains(";"))
            {
                string[] notesInChord = note.Split(';'); // Utiliser ; comme séparateur pour les notes d'un accord
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
}
