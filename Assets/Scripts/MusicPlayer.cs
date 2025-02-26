using System;
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

// Ajout de l'�num�ration pour l'offset d'octave
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

    [Header("Wave Settings")]
    public float baseWavePitch = 1;
    [Range(0.0f, 1.0f)]
    public float baseWaveVolume;

    // Offset d'octave pour la lecture de la partition
    public OctaveShift octaveShift = OctaveShift.None;
    private Dictionary<Mode, List<int>> modeIntervals = new Dictionary<Mode, List<int>>();
    private Dictionary<Tonalite, List<string>> tonaliteAccidentals = new Dictionary<Tonalite, List<string>>();

    [Header("Partition")]
    public TextAsset partitionTextAsset;

    [Space]
    [Header("References")]
    public Instrument[] instruments;
    public Settings settings;

    public Keyboard keyboard;
    public AudioSource baseWave;

    [Space]
    [Header("Infos")]
    public List<Tonalite> notesInScale;
    public bool isPartitionPlaying;

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
            PlayPartition();
        }

        else if (playMode == PlayMode.RandomPlaying)
        {
            StartCoroutine(PlayRandomNotes());
        }

        UnityEngine.Random.InitState(randomSeed);
    }

    public void Update()
    {
        SetNotesInScale();

        // Ajustement du pitch bas� sur la tonalit� choisie
        float pitchAdjustment = GetPitchAdjustmentForTonalite(currentTonalite);
        baseWave.pitch = Mathf.Lerp(baseWave.pitch, baseWavePitch * pitchAdjustment, Time.deltaTime * 5);  // Appliquer le pitch ajust�

        if(isPartitionPlaying == false)
        {
            baseWave.volume = Mathf.Lerp(baseWave.volume, baseWaveVolume, Time.deltaTime * 5);
        }

        else
        {
            baseWave.volume = Mathf.Lerp(baseWave.volume, 0, Time.deltaTime * 5);
        }
       

        SetMode();
        SetTonalite();
    }

    
    public void PlayPartition(string p = "")
    {
        Partition partition = new Partition();

        if (p == "")
        {
            partition = JsonUtility.FromJson<Partition>(partitionTextAsset.text);
    
        }

        else
        {
            partition = JsonUtility.FromJson<Partition>(p);
        }

        StartCoroutine(PlayPartition(partition));
    }

    public void SetMode()
    {
        for(int i = 0 ; i < settings.modesToggles.Count ; i++)
        {
            if(settings.modesToggles[i].isOn)
            {
                currentMode = settings.modesToggles[i].GetComponent<ModeToggle>().mode;
                break;
            }
        }
    }

    public void SetTonalite()
    {
        for(int i = 0; i < settings.tonaliteToggles.Count; i++)
        {
            if (settings.tonaliteToggles[i].isOn)
            {
                currentTonalite = settings.tonaliteToggles[i].GetComponent<TonaliteToggle>().tonalite;
                break;
            }
        }
    }

    public void SetNotesInScale()
    {
        notesInScale = GetNotesInScale(currentTonalite, currentMode);

        for(int i = 0; i < keyboard.octaves.Count; i++)
        {
            for (int j = 0; j < keyboard.octaves[i].tiles.Length; j++)
            {
                bool isInScale = false;

                foreach (Tonalite note in notesInScale)
                {
                    if (keyboard.octaves[i].tiles[j].tonalite == note)
                    {
                        isInScale = true;
                        break;
                    }
                }

                if (settings.highlightScales.isOn)
                {
                    keyboard.octaves[i].tiles[j].canBeHighlighted = true;
                }

                else
                {
                    keyboard.octaves[i].tiles[j].canBeHighlighted = false;
                }

                keyboard.octaves[i].tiles[j].isInScale = isInScale;

                if (settings.lockKeys.isOn)
                {
                    keyboard.octaves[i].tiles[j].canBeLocked = true;
                }
                else
                {
                    keyboard.octaves[i].tiles[j].canBeLocked = false;
                }
            }
        }
    }
   
    private float GetPitchAdjustmentForTonalite(Tonalite tonalite)
    {
        // Cr�er une liste des tonalit�s avec leur position relative � C
        Dictionary<Tonalite, float> tonaliteOffsets = new Dictionary<Tonalite, float>
        {
        {Tonalite.C, 0f},
        {Tonalite.CSharp, 1f / 12f},
        {Tonalite.D, 2f / 12f},
        {Tonalite.DSharp, 3f / 12f},
        {Tonalite.E, 4f / 12f},
        {Tonalite.F, 5f / 12f},
        {Tonalite.FSharp, 6f / 12f},
        {Tonalite.G, 7f / 12f},
        {Tonalite.GSharp, 8f / 12f},
        {Tonalite.A, 9f / 12f},
        {Tonalite.ASharp, 10f / 12f},
        {Tonalite.B, 11f / 12f}
        };

        // Retourne le facteur de transposition pour la tonalit� donn�e
        return Mathf.Pow(2, tonaliteOffsets[tonalite]);
    }

    #region Partition Playing
    private IEnumerator PlayPartition(Partition partition)
    {
        settings.playCompositionButton.interactable = false;
        isPartitionPlaying = true;

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

        settings.playCompositionButton.interactable = true;
        isPartitionPlaying = false;
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
        string note = importantDegrees[UnityEngine.Random.Range(0, importantDegrees.Count)];

        int octave = UnityEngine.Random.Range(minOctave, maxOctave + 1); // Choisir octave de mani�re mod�r�e
        return note + octave;
    }

    private string GetRandomChord(string chordType)
    {
        List<string> modeNotes = GetModeNotes(currentTonalite, currentMode);
        int degree = UnityEngine.Random.Range(0, modeNotes.Count);
        string rootNote = modeNotes[degree];
        int octave = UnityEngine.Random.Range(minOctave, maxOctave + 1);

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
                chordNotes.Add(modeNotes[(degree + 10) % modeNotes.Count] + octave); // 7�me mineur
                break;

            case "maj7":
                chordNotes.Add(modeNotes[(degree + 4) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 7) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 11) % modeNotes.Count] + octave); // 7�me majeure
                break;

            case "m7":
                chordNotes.Add(modeNotes[(degree + 3) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 7) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 10) % modeNotes.Count] + octave); // 7�me mineure
                break;

            case "9":
                chordNotes.Add(modeNotes[(degree + 4) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 7) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 10) % modeNotes.Count] + octave); // 7�me mineure
                chordNotes.Add(modeNotes[(degree + 2) % modeNotes.Count] + octave); // 9�me
                break;

            case "m9":
                chordNotes.Add(modeNotes[(degree + 3) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 7) % modeNotes.Count] + octave);
                chordNotes.Add(modeNotes[(degree + 10) % modeNotes.Count] + octave); // 7�me mineure
                chordNotes.Add(modeNotes[(degree + 2) % modeNotes.Count] + octave); // 9�me
                break;
        }

        // Cr�er l'accord sous forme de cha�ne
        string chord = "|" + string.Join("|", chordNotes) + "|";
        return chord;
    }

    private IEnumerator PlayTrackChord()
    {
        string[] possibleDurations = new string[] { "whole", "half", "quarter", "eighth", "sixteenth", "thirtysecond", "sixtyfourth" }; // Toutes les dur�es possibles pour les accords
        string randomDuration = possibleDurations[UnityEngine.Random.Range(0, possibleDurations.Length)];
        float durationInSeconds = GetDurationInSeconds(randomDuration);

        // Parfois, on ne joue rien (blanc)
        if (UnityEngine.Random.value < 0.2f) // 20% de chance de ne pas jouer l'accord
        {
            yield return new WaitForSeconds(durationInSeconds); // Attendre la dur�e mais ne jouer aucun son
            yield break;
        }

        // Choisir les types d'accords � jouer en fonction du seed
        string[] chordTypes = { "major", "minor", "7", "maj7", "m7", "9", "m9" };
        List<string> selectedChords = new List<string>();
        int maxChords = 4; // Limiter � 4 types d'accords
        int numChords = Mathf.Min(maxChords, chordTypes.Length);

        // S�lectionner des types d'accords de mani�re coh�rente avec le seed
        for (int i = 0; i < numChords; i++)
        {
            int index = (randomSeed + i) % chordTypes.Length;
            if (!selectedChords.Contains(chordTypes[index]))
            {
                selectedChords.Add(chordTypes[index]);
            }
        }

        // Choisir un type d'accord parmi les types s�lectionn�s
        string randomChordType = selectedChords[UnityEngine.Random.Range(0, selectedChords.Count)];

        // G�n�rer et jouer l'accord
        string randomChord = GetRandomChord(randomChordType);
        StartCoroutine(PlayChordRoutine(randomChord, durationInSeconds));

        // Attendre la dur�e de l'accord
        yield return new WaitForSeconds(durationInSeconds);
    }

    private IEnumerator PlayTrackNote()
    {
        string[] possibleDurations = new string[] { "whole", "half", "quarter", "eighth", "sixteenth", "thirtysecond", "sixtyfourth" }; // Toutes les dur�es possibles pour les notes
        string randomDuration = possibleDurations[UnityEngine.Random.Range(0, possibleDurations.Length)];
        float durationInSeconds = GetDurationInSeconds(randomDuration);

        // Parfois, on ne joue rien (blanc)
        if (UnityEngine.Random.value < 0.2f) // 20% de chance de ne pas jouer la note
        {
            yield return new WaitForSeconds(durationInSeconds); // Attendre la dur�e mais ne jouer aucune note
            yield break;
        }

        // Sinon, on g�n�re et joue la note
        string randomNote = GetRandomNote();
        PlayNote(randomNote, durationInSeconds, null);

        // Attendre la dur�e de la note
        yield return new WaitForSeconds(durationInSeconds);
    }

    private IEnumerator PlayRandomNotes()
    {
        yield return new WaitForSeconds(2);

        while (true)
        {
            // Jouer un accord avec une petite pause
            yield return StartCoroutine(PlayTrackNotes("track1"));

            // Choisir une dur�e pour l'accord
            string[] possibleDurations = new string[] {"half", "quarter", "eighth" }; // Dur�es plus coh�rentes
            string randomDuration1 = possibleDurations[UnityEngine.Random.Range(0, possibleDurations.Length)];
            yield return new WaitForSeconds(GetDurationInSeconds(randomDuration1));

            // Jouer une note avec une petite pause
            yield return StartCoroutine(PlayTrackNotes("track2"));

            // Choisir une dur�e pour la note
            string randomDuration2 = possibleDurations[UnityEngine.Random.Range(0, possibleDurations.Length)];
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

    public List<Tonalite> GetNotesInScale(Tonalite baseTonalite, Mode mode)
    {
        if (!noteMap.ContainsKey(baseTonalite.ToString()) || !modeIntervals.ContainsKey(mode))
            return new List<Tonalite>();

        List<Tonalite> scaleNotes = new List<Tonalite>();
        int baseIndex = noteMap[baseTonalite.ToString()];
        scaleNotes.Add(baseTonalite);

        foreach (int interval in modeIntervals[mode])
        {
            baseIndex = (baseIndex + interval) % 12;
            Tonalite nextNote = (Tonalite)Enum.Parse(typeof(Tonalite), noteMap.FirstOrDefault(x => x.Value == baseIndex).Key);
            scaleNotes.Add(nextNote);
        }

        return scaleNotes;
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

            // Appliquer l'offset d'octave si n�cessaire
            if (playMode == PlayMode.Partition)
            {
                octave += (int)octaveShift;
            }

            // V�rifier si la note existe dans le dictionnaire
            if (!noteMap.ContainsKey(noteKey))
            {
                return null;
            }

            // Obtenir l'index de la note (la m�me note peut se trouver � plusieurs octaves)
            int noteIndex = noteMap[noteKey];

            // Calculer le facteur de pitch en fonction de l'octave
            float pitchFactor = Mathf.Pow(2, octave - baseOctave);

            // Cr�er l'objet pour jouer la note
            GameObject noteObject = new GameObject(note + " | " + duration);

            if (parent != null)
            {
                noteObject.transform.parent = parent.transform;
            }

            // Cr�er le composant AudioSource et jouer la note
            AudioSource source = noteObject.AddComponent<AudioSource>();
            source.clip = instruments[0].instrumentSamples[noteIndex];  // Utiliser le m�me fichier audio
            source.pitch = pitchFactor;     // Appliquer le pitch
            source.Play();

            keyboard.PlayVisual(note, duration);

            // D�truire l'objet apr�s avoir jou� la note
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

        for (int i = 0; i < 7; i++) // Notes d�une gamme diatonique classique
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