using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MusicBlock
{
    public string type; // "note" ou "chord"
    public string value; // Nom de la note ou de l'accord
    public string duration; // Durée de la note ou de l'accord sous forme de texte (ex : "croche", "blanche")
}

[System.Serializable]
public class Partition
{
    public List<MusicBlock> blocks;
}

public enum TrackMode
{
    SingleTrack,   // Mode où seule une piste est jouée (accords)
    DoubleTrack    // Mode où deux pistes sont jouées (accords sur la première, notes sur la deuxième)
}


public enum PlayMode
{
    Partition,   // Mode de lecture de la partition
    RandomPlaying // Mode de lecture aléatoire
}

public enum Mode
{
    Ionien,  // Majeur
    Dorien,
    Phrygien,
    Lydien,
    Mixolydien,
    Aeolien,  // Mineur naturel
    Locrien
}

public enum Tonalite
{
    C,  // Do
    CSharp,  // DoSharp
    D,  // Ré
    DSharp,  // RéSharp
    E,  // Mi
    F,  // Fa
    FSharp,  // FaSharp
    G,  // Sol
    GSharp,  // SolSharp
    A,  // La
    ASharp,  // LaSharp
    B   // Si
}

public class MusicPlayer : MonoBehaviour
{
    [TextArea(20, 50)]
    public string jsonPartition;
    public AudioClip[] notes; // Contient les 12 notes de base (une octave)
    public float tempo = 120f;
    public PlayMode playMode = PlayMode.Partition; // Mode de lecture
    private Dictionary<string, int> noteMap;
    public Mode currentMode = Mode.Ionien;  // Mode choisi
    public Tonalite currentTonalite = Tonalite.C;  // Tonalité choisie
    public TrackMode trackMode = TrackMode.SingleTrack; // Mode de lecture des pistes

    private Dictionary<Mode, List<int>> modeIntervals = new Dictionary<Mode, List<int>>();
    private Dictionary<Tonalite, List<string>> tonaliteAccidentals = new Dictionary<Tonalite, List<string>>();

    void Start()
    {
        noteMap = new Dictionary<string, int>
        {
            {"C", 0}, {"CSharp", 1}, {"D", 2}, {"DSharp", 3}, {"E", 4}, {"F", 5},
            {"FSharp", 6}, {"G", 7}, {"GSharp", 8}, {"A", 9}, {"ASharp", 10}, {"B", 11}
        };

        // Initialisation des intervalles pour chaque mode
        modeIntervals[Mode.Ionien] = new List<int> { 2, 2, 1, 2, 2, 2, 1 };  // Do majeur (Ionien)
        modeIntervals[Mode.Dorien] = new List<int> { 2, 1, 2, 2, 2, 1, 2 };  // Ré Dorien
        modeIntervals[Mode.Phrygien] = new List<int> { 1, 2, 2, 2, 1, 2, 2 };  // Mi Phrygien
        modeIntervals[Mode.Lydien] = new List<int> { 2, 2, 2, 1, 2, 2, 1 };  // Fa Lydien
        modeIntervals[Mode.Mixolydien] = new List<int> { 2, 2, 1, 2, 2, 1, 2 };  // Sol Mixolydien
        modeIntervals[Mode.Aeolien] = new List<int> { 2, 1, 2, 2, 1, 2, 2 };  // La mineur naturel
        modeIntervals[Mode.Locrien] = new List<int> { 1, 2, 2, 1, 2, 2, 2 };  // Si Locrien

        // Initialisation des altérations pour chaque tonalité
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
                Debug.LogError("Durée non reconnue : " + duration);
                return 1f; // Valeur par défaut
        }
    }

    private IEnumerator PlayPartition(Partition partition)
    {
        foreach (var block in partition.blocks)
        {
            float durationInSeconds = GetDurationInSeconds(block.duration); // Convertir la durée en secondes
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

        if(note.Contains("#"))
        {
            note = note.Replace("#", "Sharp");
        }

        try
        {
            // Identifier la note et l'octave
            string noteKey = note.Length > 2 ? note.Substring(0, 2) : note.Substring(0, 1); // Pour gérer les dièses (par ex. ASharp)
            int octave;

            // Vérifier si l'octave est spécifiée après la note
            if (note.Length > 2 && int.TryParse(note.Substring(note.Length - 1), out octave))
            {
                // Si l'octave est un chiffre à la fin de la note, l'extraire
                octave = int.Parse(note.Substring(note.Length - 1));
                noteKey = note.Substring(0, note.Length - 1);  // La note sans l'octave
            }
            else
            {
                // Si l'octave n'est pas spécifié, affecter une valeur par défaut
                octave = 4; // On suppose l'octave 4 par défaut
            }

            // Calcul de la fréquence de la note
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

    private IEnumerator PlayTrackNotes(string trackType)
    {
        // Générer une durée aléatoire en termes de valeurs musicales possibles
        string[] possibleDurations = new string[] { "ronde", "blanche", "noire", "croche", "doublecroche" };
        string randomDuration = possibleDurations[Random.Range(0, possibleDurations.Length)];

        float durationInSeconds = GetDurationInSeconds(randomDuration); // Convertir la durée en secondes selon le tempo

        if (trackType == "track1")
        {
            // Jouer la première piste (accord)
            string randomChord = GetRandomChord();
            PlayChord(randomChord);
        }
        else if (trackType == "track2")
        {
            // Jouer la deuxième piste (note seule)
            string randomNote = GetRandomNote();
            PlayNote(randomNote, durationInSeconds, null);
        }

        yield return new WaitForSeconds(durationInSeconds); // Attendre la durée correspondante
    }

    private IEnumerator PlayRandomNotes()
    {
        while (true)
        {
            // Appeler la coroutine pour la première piste (accord) avec un temps d'attente spécifique
            StartCoroutine(PlayTrackNotes("track1"));

            // Attendre avec une durée aléatoire pour la première piste avant de jouer la deuxième
            yield return new WaitForSeconds(GetDurationInSeconds("ronde")); // Exemple d'attente pour la première piste, tu peux ajuster la durée ici si nécessaire

            // Appeler la coroutine pour la deuxième piste (note seule) avec un temps d'attente spécifique
            StartCoroutine(PlayTrackNotes("track2"));

            // Attendre avec une durée aléatoire pour la deuxième piste avant de recommencer
            yield return new WaitForSeconds(GetDurationInSeconds("blanche")); // Exemple pour la deuxième piste
        }
    }



    private string GetRandomNote()
    {
        // Récupérer les notes du mode en cours
        List<string> modeNotes = GetModeNotes(currentTonalite, currentMode);

        // Choisir une note à partir de la tonique, de la dominante ou de la médiante
        List<string> importantDegrees = new List<string> { modeNotes[0], modeNotes[2], modeNotes[4] }; // Tonique (I), Médiante (III), Dominante (V)
        string note = importantDegrees[Random.Range(0, importantDegrees.Count)];

        // Choisir un octave entre 3 et 6
        int octave = Random.Range(3, 6);

        return note + octave;  // Retourner la note avec l'octave
    }

    private string GetRandomChord()
    {
        // Récupérer les notes du mode actuel
        List<string> modeNotes = GetModeNotes(currentTonalite, currentMode);

        // Choisir un degré d'accord dans le mode (ex: I, IV, V, etc.)
        int degree = Random.Range(0, modeNotes.Count);

        // Sélectionner la note de base (racine) de l'accord
        string rootNote = modeNotes[degree];
        int octave = Random.Range(3, 6);  // Choisir un octave entre 3 et 6

        // Créer l'accord en fonction du mode
        string chord = "|" + rootNote + octave + "|";

        // Déterminer l'accord à partir du mode :
        // Ionien (majeur) : majeur sur I, IV, V, mineur sur II, III, VI, etc.
        // Dorien : mineur sur I, III, V, VI, majeur sur IV, VII
        // Phrygien : mineur sur I, II, V, VI, majeur sur III, VII
        // Lydien : majeur sur I, II, III, V, mineur sur IV, VI, VII
        // Mixolydien : majeur sur I, IV, V, mineur sur II, III, VI
        // Aeolien (mineur naturel) : mineur sur I, II, III, V, VI, majeur sur IV, VII
        // Locrien : diminué sur I, II, III, IV, mineur sur V, majeur sur VI, VII

        if (currentMode == Mode.Ionien || currentMode == Mode.Lydien || currentMode == Mode.Mixolydien)
        {
            // Les modes majeurs (Ionien, Lydien, Mixolydien) sont souvent majeurs sur I, IV, V
            chord += GetNoteByInterval(rootNote, 4) + octave + "|";  // Tierce majeure
            chord += GetNoteByInterval(rootNote, 7) + octave + "|";  // Quinte
        }
        else if (currentMode == Mode.Dorien || currentMode == Mode.Phrygien || currentMode == Mode.Aeolien)
        {
            // Les modes mineurs (Dorien, Phrygien, Aeolien) sont mineurs sur I, III, V
            chord += GetNoteByInterval(rootNote, 3) + octave + "|";  // Tierce mineure
            chord += GetNoteByInterval(rootNote, 7) + octave + "|";  // Quinte
        }
        else if (currentMode == Mode.Locrien)
        {
            // Le mode Locrien crée un accord diminué (I, III, Vb)
            chord += GetNoteByInterval(rootNote, 3) + octave + "|";  // Tierce mineure
            chord += GetNoteByInterval(rootNote, 6) + octave + "|";  // Quinte diminuée
        }

        return chord;
    }

    private List<string> GetModeNotes(Tonalite tonalite, Mode mode)
    {
        List<string> notesInScale = new List<string>();
        int rootNoteIndex = noteMap[tonalite.ToString()];  // Trouver la position de la note de base dans le dictionnaire

        // Obtenir les intervalles du mode
        List<int> intervals = modeIntervals[mode];

        for (int i = 0; i < 7; i++)
        {
            // Calculer la note en fonction de la position et de l'intervalle
            int noteIndex = (rootNoteIndex + GetCumulativeInterval(intervals, i)) % 12;
            string note = noteMap.FirstOrDefault(x => x.Value == noteIndex).Key;

            // Appliquer les altérations de la tonalité si nécessaire
            if (tonaliteAccidentals[tonalite].Contains(note) && !note.Contains("Sharp"))
            {
                note += "Sharp"; // Ajouter un dièse si l'altération est présente et qu'il n'y en a pas déjà un
            }

            notesInScale.Add(note);  // Ajouter la note à la liste de la gamme
        }

        return notesInScale;  // Retourner les notes du mode
    }


    private int GetCumulativeInterval(List<int> intervals, int index)
    {
        int cumulativeInterval = 0;
        for (int i = 0; i <= index; i++)
        {
            cumulativeInterval += intervals[i];
        }
        return cumulativeInterval;
    }

    private string GetNoteByInterval(string rootNote, int interval)
    {
        int rootNoteIndex = noteMap[rootNote];  // Trouver l'index de la note de base dans le dictionnaire
        int noteIndex = (rootNoteIndex + interval) % 12;  // Appliquer l'intervalle

        // Vérifie si la note générée a besoin d'un dièse
        string note = noteMap.FirstOrDefault(x => x.Value == noteIndex).Key;

        if (note.Contains("#"))
        {
            note = note.Replace("#", "Sharp");  // Remplacer Sharp par un dièse dans la notation
        }

        return note;  // Retourner la note obtenue
    }

}
