using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    RandomPlaying,
    Off
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

// Ajout de l'énumération pour l'offset d'octave
public enum OctaveShift
{
    Decrease2 = -2,
    Decrease1 = -1,
    None = 0,
    Increase1 = 1,
    Increase2 = 2
}

public class MusicPlayer : MonoBehaviour
{
    [Header("Settings")]
    public float tempo = 120f;
    public float sustain;
    public PlayMode playMode = PlayMode.Partition;
    public Mode currentMode = Mode.Ionien;
    public Tonalite currentTonalite = Tonalite.C;
    public TrackMode trackMode = TrackMode.SingleTrack;
    public int minOctave = 3;
    public int maxOctave = 6;
    public int baseOctave = 5;
    public int randomSeed;

    // Offset d'octave pour la lecture de la partition
    public OctaveShift octaveShift = OctaveShift.None;
    private Dictionary<Mode, List<int>> modeIntervals = new Dictionary<Mode, List<int>>();
    private Dictionary<Tonalite, List<string>> tonaliteAccidentals = new Dictionary<Tonalite, List<string>>();

    [Header("Partition")]
    public TextAsset partitionTextAsset;

    [Header("References")]
    public AudioClip[] notes;
    public Keyboard keyboard;

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

        UnityEngine.Random.InitState(randomSeed);
    }

    #region Partition Playing
    private IEnumerator PlayPartition(Partition partition)
    {
        yield return new WaitForSeconds(1f);

        foreach (var block in partition.blocks)
        {
            float durationInSeconds = GetDurationInSeconds(block.duration);

            if (block.type == "chord")
            {
                StartCoroutine(PlayChordRoutine(block.value, durationInSeconds));
            }
            else if (block.type == "note")
            {
                StartCoroutine(PlayNoteRoutine(block.value, durationInSeconds));
            }

            yield return new WaitForSeconds(durationInSeconds);
        }
    }



    #endregion

    #region Random Playing
    private IEnumerator PlayTrackNotes(string trackType)
    {
        if (trackType == "track1") // Accord
        {
            yield return StartCoroutine(PlayTrackChord());
        }
        else if (trackType == "track2") // Note solo
        {
            yield return StartCoroutine(PlayTrackNote());
        }
    }


    private string GetRandomNote()
    {
        List<string> modeNotes = GetModeNotes(currentTonalite, currentMode);
        List<string> importantDegrees = new List<string> { modeNotes[0], modeNotes[2], modeNotes[4] }; // Notes importantes
        string note = importantDegrees[Random.Range(0, importantDegrees.Count)];

        int octave = Random.Range(minOctave, maxOctave + 1); // Choisir octave de manière modérée
        return note + octave;
    }

    private string GetRandomChord(string chordType)
    {
        List<string> modeNotes = GetModeNotes(currentTonalite, currentMode);
        int degree = Random.Range(0, modeNotes.Count);
        string rootNote = modeNotes[degree];
        int octave = Random.Range(minOctave, maxOctave + 1);

        List<string> chordNotes = new List<string> { rootNote + octave };

        switch (chordType)
        {
            case "major":
                chordNotes.Add(modeNotes[(degree + 4) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 7) % modeNotes.Count] + octave);
                break;

            case "minor":
                chordNotes.Add(modeNotes[(degree + 3) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 7) % modeNotes.Count] + octave);
                break;

            case "7":
                chordNotes.Add(modeNotes[(degree + 4) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 7) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 10) % modeNotes.Count] + octave); // 7ème mineur
                break;

            case "maj7":
                chordNotes.Add(modeNotes[(degree + 4) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 7) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 11) % modeNotes.Count] + octave); // 7ème majeure
                break;

            case "m7":
                chordNotes.Add(modeNotes[(degree + 3) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 7) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 10) % modeNotes.Count] + octave); // 7ème mineure
                break;

            case "9":
                chordNotes.Add(modeNotes[(degree + 4) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 7) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 10) % modeNotes.Count] + octave); // 7ème mineure
                chordNotes.Add(modeNotes[(degree + 2) % modeNotes.Count] + octave); // 9ème
                break;

            case "m9":
                chordNotes.Add(modeNotes[(degree + 3) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 7) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 10) % modeNotes.Count] + octave); // 7ème mineure
                chordNotes.Add(modeNotes[(degree + 2) % modeNotes.Count] + octave); // 9ème
                break;
        }

        // Créer l'accord sous forme de chaîne
        string chord = "|" + string.Join("|", chordNotes) + "|";
        return chord;
    }

    private IEnumerator PlayTrackChord()
    {
        string[] possibleDurations = new string[] { "whole", "half", "quarter", "eighth", "sixteenth", "thirtysecond", "sixtyfourth" }; // Toutes les durées possibles pour les accords
        string randomDuration = possibleDurations[Random.Range(0, possibleDurations.Length)];
        float durationInSeconds = GetDurationInSeconds(randomDuration);

        // Parfois, on ne joue rien (blanc)
        if (Random.value < 0.2f) // 20% de chance de ne pas jouer l'accord
        {
            yield return new WaitForSeconds(durationInSeconds); // Attendre la durée mais ne jouer aucun son
            yield break;
        }

        // Choisir les types d'accords à jouer en fonction du seed
        string[] chordTypes = { "major", "minor", "7", "maj7", "m7", "9", "m9" };
        List<string> selectedChords = new List<string>();
        int maxChords = 4; // Limiter à 4 types d'accords
        int numChords = Mathf.Min(maxChords, chordTypes.Length);

        // Sélectionner des types d'accords de manière cohérente avec le seed
        for (int i = 0; i < numChords; i++)
        {
            int index = (randomSeed + i) % chordTypes.Length;
            if (!selectedChords.Contains(chordTypes[index]))
            {
                selectedChords.Add(chordTypes[index]);
            }
        }

        // Choisir un type d'accord parmi les types sélectionnés
        string randomChordType = selectedChords[Random.Range(0, selectedChords.Count)];

        // Générer et jouer l'accord
        string randomChord = GetRandomChord(randomChordType);
        StartCoroutine(PlayChordRoutine(randomChord, durationInSeconds));

        // Attendre la durée de l'accord
        yield return new WaitForSeconds(durationInSeconds);
    }

    private IEnumerator PlayTrackNote()
    {
        string[] possibleDurations = new string[] { "whole", "half", "quarter", "eighth", "sixteenth", "thirtysecond", "sixtyfourth" }; // Toutes les durées possibles pour les notes
        string randomDuration = possibleDurations[Random.Range(0, possibleDurations.Length)];
        float durationInSeconds = GetDurationInSeconds(randomDuration);

        // Parfois, on ne joue rien (blanc)
        if (Random.value < 0.2f) // 20% de chance de ne pas jouer la note
        {
            yield return new WaitForSeconds(durationInSeconds); // Attendre la durée mais ne jouer aucune note
            yield break;
        }

        // Sinon, on génère et joue la note
        string randomNote = GetRandomNote();
        PlayNote(randomNote, durationInSeconds, null);

        // Attendre la durée de la note
        yield return new WaitForSeconds(durationInSeconds);
    }

    private IEnumerator PlayRandomNotes()
    {
        while (true)
        {
            // Jouer un accord avec une petite pause
            yield return StartCoroutine(PlayTrackNotes("track1"));

            // Choisir une durée pour l'accord
            string[] possibleDurations = new string[] { "whole", "half", "quarter" }; // Durées plus cohérentes
            string randomDuration1 = possibleDurations[Random.Range(0, possibleDurations.Length)];
            yield return new WaitForSeconds(GetDurationInSeconds(randomDuration1));

            // Jouer une note avec une petite pause
            yield return StartCoroutine(PlayTrackNotes("track2"));

            // Choisir une durée pour la note
            string randomDuration2 = possibleDurations[Random.Range(0, possibleDurations.Length)];
            yield return new WaitForSeconds(GetDurationInSeconds(randomDuration2));
        }
    }
    #endregion

    #region Notes Generation
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
    private IEnumerator PlayChordRoutine(string chord, float duration)
    {
        AudioSource[] sources = PlayChord(chord, duration);
        yield return new WaitForSeconds(duration);

        foreach (var source in sources)
        {
            StartCoroutine(FadeOutNoteVolume(source, sustain));
        }
    }

    private AudioSource[] PlayChord(string chord, float duration)
    {
        GameObject chordObject = new GameObject("Chord: " + chord + " | " + duration);
        string[] noteArray = chord.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
        List<AudioSource> sources = new List<AudioSource>();

        foreach (string note in noteArray)
        {
            AudioSource source = PlaySingleNote(note.Trim(), duration, chordObject);
            if (source != null)
            {
                sources.Add(source);
            }
        }

        return sources.ToArray();
    }

    private IEnumerator PlayNoteRoutine(string note, float duration)
    {
        AudioSource source = PlayNote(note, duration, null);
        yield return new WaitForSeconds(duration);
        StartCoroutine(FadeOutNoteVolume(source, sustain));
    }

    public AudioSource PlayNote(string note, float duration, GameObject parent)
    {

        if (string.IsNullOrEmpty(note)) return null;

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
                return PlaySingleNote(note, duration, parent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in PlayNote: " + e.Message);
        }
        return null;
    }

    public AudioSource PlaySingleNote(string note, float duration, GameObject parent)
    {
        string noteIndexDebug = "";

        if (string.IsNullOrEmpty(note)) return null;

        try
        {
            if (note.Contains("#"))
            {
                note = note.Replace("#", "Sharp");
            }

            string noteKey;
            int octave;

            // Extraire la note et l'octave
            if (note.Length > 1 && int.TryParse(note[note.Length - 1].ToString(), out octave))
            {
                noteKey = note.Substring(0, note.Length - 1);
            }
            else
            {
                noteKey = note;
                octave = baseOctave;
            }

            // Appliquer l'offset d'octave si nécessaire
            if (playMode == PlayMode.Partition)
            {
                octave += (int)octaveShift;
            }

            // Vérifier si la note existe dans le dictionnaire
            if (!noteMap.ContainsKey(noteKey))
            {
                return null;
            }

            // Obtenir l'index de la note (la même note peut se trouver à plusieurs octaves)
            int noteIndex = noteMap[noteKey];

            // Calculer le facteur de pitch en fonction de l'octave
            float pitchFactor = Mathf.Pow(2, octave - baseOctave);

            // Créer l'objet pour jouer la note
            GameObject noteObject = new GameObject(note + " | " + duration);

            if (parent != null)
            {
                noteObject.transform.parent = parent.transform;
            }

            // Créer le composant AudioSource et jouer la note
            AudioSource source = noteObject.AddComponent<AudioSource>();
            source.clip = notes[noteIndex];  // Utiliser le même fichier audio
            source.pitch = pitchFactor;     // Appliquer le pitch
            source.Play();

            // Détruire l'objet après avoir joué la note
            StartCoroutine(DestroyAfterPlaying(noteObject, source));

            return source;
        }

        catch (System.Exception e)
        {
            Debug.LogError("Error in PlaySingleNote: " + noteIndexDebug);
        }

        return null;
    }

    private IEnumerator FadeOutNoteVolume(AudioSource source, float fadeDuration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float time = 0;

        while (time < fadeDuration)
        {
            source.volume = Mathf.Lerp(startVolume, 0, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = 0;
    }

    private IEnumerator DestroyAfterPlaying(GameObject noteObject, AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        Destroy(noteObject);
    }


    private List<string> GetModeNotes(Tonalite tonalite, Mode mode)
    {
        List<string> notesInScale = new List<string>();
        int rootNoteIndex = noteMap[tonalite.ToString()];
        List<int> intervals = modeIntervals[mode];

        for (int i = 0; i < 7; i++) // Notes d’une gamme diatonique classique
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

    #endregion
}