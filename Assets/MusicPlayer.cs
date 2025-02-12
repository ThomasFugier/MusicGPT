using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class MusicBlock
{
    public string type;
    public string value;
    public string duration;
}

[System.Serializable]
public class Partition
{
    public List<MusicBlock> blocks;
}

public enum TrackMode
{
    SingleTrack,
    DoubleTrack
}

public enum PlayMode
{
    Partition,
    RandomPlaying
}

public enum Mode
{
    Ionien,
    Dorien,
    Phrygien,
    Lydien,
    Mixolydien,
    Aeolien,
    Locrien
}

public enum Tonalite
{
    C,
    CSharp,
    D,
    DSharp,
    E,
    F,
    FSharp,
    G,
    GSharp,
    A,
    ASharp,
    B
}

public class MusicPlayer : MonoBehaviour
{
    [Header("Settings")]
    public float tempo = 120f;
    public PlayMode playMode = PlayMode.Partition;
    public Mode currentMode = Mode.Ionien;
    public Tonalite currentTonalite = Tonalite.C;
    public TrackMode trackMode = TrackMode.SingleTrack;
    public int minOctave = 3;
    public int maxOctave = 6;
    public int baseOctave = 5;
    private Dictionary<Mode, List<int>> modeIntervals = new Dictionary<Mode, List<int>>();
    private Dictionary<Tonalite, List<string>> tonaliteAccidentals = new Dictionary<Tonalite, List<string>>();

    [Header("Partition")]
    public TextAsset partitionTextAsset;


    [Header("References")]
    public AudioClip[] notes;

    private Dictionary<string, int> noteMap;

    void Start()
    {
        noteMap = new Dictionary<string, int>
        {
            {"C", 0}, {"CSharp", 1}, {"D", 2}, {"DSharp", 3}, {"E", 4}, {"F", 5},
            {"FSharp", 6}, {"G", 7}, {"GSharp", 8}, {"A", 9}, {"ASharp", 10}, {"B", 11}
        };

        modeIntervals[Mode.Ionien] = new List<int> { 2, 2, 1, 2, 2, 2, 1 };
        modeIntervals[Mode.Dorien] = new List<int> { 2, 1, 2, 2, 2, 1, 2 };
        modeIntervals[Mode.Phrygien] = new List<int> { 1, 2, 2, 2, 1, 2, 2 };
        modeIntervals[Mode.Lydien] = new List<int> { 2, 2, 2, 1, 2, 2, 1 };
        modeIntervals[Mode.Mixolydien] = new List<int> { 2, 2, 1, 2, 2, 1, 2 };
        modeIntervals[Mode.Aeolien] = new List<int> { 2, 1, 2, 2, 1, 2, 2 };
        modeIntervals[Mode.Locrien] = new List<int> { 1, 2, 2, 1, 2, 2, 2 };

        tonaliteAccidentals[Tonalite.C] = new List<string>();
        tonaliteAccidentals[Tonalite.CSharp] = new List<string> { "CSharp", "FSharp", "GSharp" };
        tonaliteAccidentals[Tonalite.D] = new List<string> { "FSharp" };
        tonaliteAccidentals[Tonalite.DSharp] = new List<string> { "CSharp", "FSharp", "GSharp" };
        tonaliteAccidentals[Tonalite.E] = new List<string> { "FSharp", "CSharp" };
        tonaliteAccidentals[Tonalite.F] = new List<string>();
        tonaliteAccidentals[Tonalite.FSharp] = new List<string> { "FSharp" };
        tonaliteAccidentals[Tonalite.G] = new List<string> { "FSharp" };
        tonaliteAccidentals[Tonalite.GSharp] = new List<string> { "FSharp", "CSharp" };
        tonaliteAccidentals[Tonalite.A] = new List<string> { "FSharp", "CSharp" };
        tonaliteAccidentals[Tonalite.ASharp] = new List<string> { "FSharp", "CSharp", "GSharp" };
        tonaliteAccidentals[Tonalite.B] = new List<string> { "FSharp", "CSharp", "GSharp", "DSharp" };

        if (playMode == PlayMode.Partition && partitionTextAsset != null)
        {
            Partition partition = JsonUtility.FromJson<Partition>(partitionTextAsset.text);
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
            case "whole":
                return 4f * 60f / tempo;
            case "half":
                return 2f * 60f / tempo;
            case "quarter":
                return 1f * 60f / tempo;
            case "eighth":
                return 0.5f * 60f / tempo;
            case "sixteenth":
                return 0.25f * 60f / tempo;
            case "thirtysecond":
                return 0.125f * 60f / tempo;
            case "sixtyfourth":
                return 0.0625f * 60f / tempo;
            default:
                return 1f;
        }
    }

    private IEnumerator PlayPartition(Partition partition)
    {
        foreach (var block in partition.blocks)
        {
            float durationInSeconds = GetDurationInSeconds(block.duration);

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

        string[] noteArray = chord.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);

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
            if (note.Contains(";"))
            {
                string[] notesInChord = note.Split(';');
                foreach (string chordNote in notesInChord)
                {
                    PlaySingleNote(chordNote.Trim(), duration, parent);
                }
            }
            else
            {
                PlaySingleNote(note, duration, parent);
            }
        }
        catch (System.Exception e)
        {
        }
    }

    private void PlaySingleNote(string note, float duration, GameObject parent)
    {
        if (note.Length < 2) return;

        if (note.Contains("#"))
        {
            note = note.Replace("#", "Sharp");
        }

        try
        {
            string noteKey;
            int octave;

            if (note.Length > 1 && int.TryParse(note[note.Length - 1].ToString(), out octave))
            {
                noteKey = note.Substring(0, note.Length - 1);
            }
            else
            {
                noteKey = note;
                octave = baseOctave;
            }

            if (!noteMap.ContainsKey(noteKey))
            {
                return;
            }

            int noteIndex = noteMap[noteKey];
            int adjustedIndex = noteIndex + (octave - 4);

            if (adjustedIndex < 0 || adjustedIndex >= notes.Length)
            {
                return;
            }

            float pitchFactor = Mathf.Pow(2, octave - baseOctave);

            GameObject noteObject = new GameObject(note);
            if (parent != null)
            {
                noteObject.transform.parent = parent.transform;
            }

            AudioSource source = noteObject.AddComponent<AudioSource>();
            source.clip = notes[adjustedIndex];
            source.pitch = pitchFactor;
            source.Play();
            StartCoroutine(DestroyAfterPlaying(noteObject, source));
        }
        catch (System.Exception e)
        {
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

    private IEnumerator PlayTrackNotes(string trackType)
    {
        string[] possibleDurations = new string[] { "whole", "half", "quarter", "eighth", "sixteenth", "thirtysecond", "sixtyfourth" };
        string randomDuration = possibleDurations[Random.Range(0, possibleDurations.Length)];

        float durationInSeconds = GetDurationInSeconds(randomDuration);

        if (trackType == "track1")
        {
            string randomChord = GetRandomChord();
            PlayChord(randomChord);
        }
        else if (trackType == "track2")
        {
            string randomNote = GetRandomNote();
            PlayNote(randomNote, durationInSeconds, null);
        }

        yield return new WaitForSeconds(durationInSeconds);
    }

    private IEnumerator PlayRandomNotes()
    {
        while (true)
        {
            StartCoroutine(PlayTrackNotes("track1"));
            string[] possibleDurations = new string[] { "whole", "half", "quarter", "eighth", "sixteenth", "thirtysecond", "sixtyfourth" };
            string randomDuration1 = possibleDurations[Random.Range(0, possibleDurations.Length)];
            yield return new WaitForSeconds(GetDurationInSeconds(randomDuration1));

            StartCoroutine(PlayTrackNotes("track2"));
            string randomDuration2 = possibleDurations[Random.Range(0, possibleDurations.Length)];
            yield return new WaitForSeconds(GetDurationInSeconds(randomDuration2));
        }
    }

    private string GetRandomNote()
    {
        List<string> modeNotes = GetModeNotes(currentTonalite, currentMode);
        List<string> importantDegrees = new List<string> { modeNotes[0], modeNotes[2], modeNotes[4] };
        string note = importantDegrees[Random.Range(0, importantDegrees.Count)];
        int octave = Random.Range(minOctave, maxOctave + 1);
        return note + octave;
    }

    private string GetRandomChord()
    {
        List<string> modeNotes = GetModeNotes(currentTonalite, currentMode);
        int degree = Random.Range(0, modeNotes.Count);
        string rootNote = modeNotes[degree];
        int octave = Random.Range(minOctave, maxOctave + 1);

        List<string> chordNotes = new List<string> { rootNote + octave };

        // Ajout d'autres notes à l'accord (ex: 3e et 5e degrés)
        chordNotes.Add(modeNotes[(degree + 2) % modeNotes.Count] + octave);  // 3e degré
        chordNotes.Add(modeNotes[(degree + 4) % modeNotes.Count] + octave);  // 5e degré

        // Forme l'accord avec plusieurs notes
        string chord = "|" + string.Join("|", chordNotes) + "|";
        return chord;
    }


    private List<string> GetModeNotes(Tonalite tonalite, Mode mode)
    {
        List<string> notesInScale = new List<string>();
        int rootNoteIndex = noteMap[tonalite.ToString()];
        List<int> intervals = modeIntervals[mode];

        for (int i = 0; i < 7; i++)
        {
            int noteIndex = (rootNoteIndex + GetCumulativeInterval(intervals, i)) % 12;
            string note = noteMap.FirstOrDefault(x => x.Value == noteIndex).Key;
            notesInScale.Add(note);
        }

        return notesInScale;
    }

    private int GetCumulativeInterval(List<int> intervals, int index)
    {
        int interval = 0;
        for (int i = 0; i <= index; i++)
        {
            interval += intervals[i];
        }
        return interval;
    }
}
