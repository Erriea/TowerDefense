using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProceduralMusic : MonoBehaviour
{
   [Header("Musical Settings")]
    [SerializeField] private int rootMidiNote = 62;
    [SerializeField] private int bpm = 85;

    [Header("Audio Settings")]
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private float volume = 0.15f;

    [Header("Instrument Settings")]
    [SerializeField] private float harpDecay = 3f;
    [SerializeField] private float ambientVolume = 0.25f;
    [SerializeField] private float bassVolume = 0.2f;

    private AudioSource audioSource;

    // D minor scale
    private readonly int[] scale =
    {
        0, 2, 3, 5, 7, 8, 10
    };

    // Dm, Bb, F, A
    private readonly int[][] chords =
    {
        new int[] { 0, 3, 7 },
        new int[] { 8, 0, 3 },
        new int[] { 3, 7, 10 },
        new int[] { 7, 11, 2 }
    };

    private int[] currentPhrase;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        GeneratePhrase();
        GenerateAudio();
    }

    private void GeneratePhrase()
    {
        currentPhrase = new int[16];

        for (int bar = 0; bar < 4; bar++)
        {
            int[] chord = chords[bar];

            for (int beat = 0; beat < 4; beat++)
            {
                int index = bar * 4 + beat;

                if (Random.value < 0.7f)
                {
                    int chordTone =
                        chord[Random.Range(0, chord.Length)];

                    currentPhrase[index] =
                        rootMidiNote + chordTone + 12;
                }
                else
                {
                    int scaleTone =
                        scale[Random.Range(0, scale.Length)];

                    currentPhrase[index] =
                        rootMidiNote + scaleTone + 12;
                }
            }
        }

        // Resolve the melody
        currentPhrase[15] = rootMidiNote + 12;
    }

    private void GenerateAudio()
    {
        float secondsPerBeat = 60f / bpm;

        int samplesPerBeat =
            Mathf.RoundToInt(sampleRate * secondsPerBeat);

        int totalSamples = samplesPerBeat * 16;

        float[] audioData = new float[totalSamples];

        for (int beat = 0; beat < 16; beat++)
        {
            int midiNote = currentPhrase[beat];

            float frequency = GetNoteFrequency(midiNote);

            int startSample = beat * samplesPerBeat;

            int noteLength =
                Mathf.RoundToInt(samplesPerBeat * 0.85f);

            for (int i = 0; i < noteLength; i++)
            {
                int sampleIndex = startSample + i;

                float time = (float)i / sampleRate;

                // Harp-like plucked sound
                float harp = GenerateHarp(
                    frequency, time, harpDecay
                );

                // Ambient magical layer
                float ambient = GenerateAmbient(
                    frequency / 2f, time
                );

                audioData[sampleIndex] =
                    harp * volume +
                    ambient * ambientVolume;
            }
        }

        AudioClip clip = AudioClip.Create(
            "FantasySoundtrack",
            totalSamples,
            1,
            sampleRate,
            false
        );

        clip.SetData(audioData, 0);

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.volume = 1f;
        audioSource.spatialBlend = 0f;

        audioSource.Play();

        Debug.Log("Fantasy soundtrack generated!");
    }

    private float GenerateHarp(
        float frequency,
        float time,
        float decay)
    {
        // Fundamental frequency
        float fundamental = Mathf.Sin(
            2f * Mathf.PI * frequency * time
        );

        // Harmonics make the sound brighter
        float harmonic2 = Mathf.Sin(
            2f * Mathf.PI * frequency * 2f * time
        ) * 0.5f;

        float harmonic3 = Mathf.Sin(
            2f * Mathf.PI * frequency * 3f * time
        ) * 0.25f;

        // Exponential decay
        float envelope = Mathf.Exp(-decay * time);

        return (fundamental + harmonic2 + harmonic3)
               * envelope;
    }

    private float GenerateAmbient(
        float frequency,
        float time)
    {
        float wave1 = Mathf.Sin(
            2f * Mathf.PI * frequency * time
        );

        float wave2 = Mathf.Sin(
            2f * Mathf.PI * frequency * 1.5f * time
        ) * 0.3f;

        return (wave1 + wave2) * 0.5f;
    }

    private float GetNoteFrequency(int midiNote)
    {
        return 440f * Mathf.Pow(
            2f,
            (midiNote - 69) / 12f
        );
    }
    
    // [Header("Music Settings")]
    // [SerializeField] private int bpm = 100;
    // [SerializeField] private int beats = 16;
    // [SerializeField] private int sampleRate = 44100;
    //
    // [Header("Sound Settings")]
    // [SerializeField] private float volume = 0.2f;
    //
    // private AudioSource audioSource;
    //
    // private readonly float[] notes =
    // {
    //     //the Hz value corresponds to the note
    //     261.63f, //C4
    //     293.66f, //D4
    //     329.63f, //E4
    //     392.00f, //G4
    //     440.00f  //A4
    // };
    //
    // private void Start()
    // {
    //     audioSource = GetComponent<AudioSource>();
    //
    //     GenerateMusic();
    // }
    //
    // private void GenerateMusic()
    // {
    //     float secondsPerBeat = 100f / bpm;
    //
    //     int samplesPerBeat =
    //         Mathf.RoundToInt(sampleRate * secondsPerBeat);
    //
    //     int totalSamples = samplesPerBeat * beats;
    //
    //     float[] audioData = new float[totalSamples];
    //
    //     //randomly select notes
    //     System.Random random = new System.Random();
    //
    //     for (int beat = 0; beat < beats; beat++)
    //     {
    //         float frequency = notes[random.Next(notes.Length)];
    //
    //         int startSample = beat * samplesPerBeat;
    //
    //         int noteLength = Mathf.RoundToInt(
    //             samplesPerBeat * 0.8f
    //         );
    //
    //         for (int i = 0; i < noteLength; i++)
    //         {
    //             int sampleIndex = startSample + i;
    //
    //             float time = (float)i / sampleRate;
    //
    //             //generates sine wave
    //             float wave = Mathf.Sin(
    //                 2f * Mathf.PI * frequency * time
    //             );
    //
    //             //fades the note in and out
    //             float envelope = Mathf.Sin(
    //                 Mathf.PI * i / noteLength
    //             );
    //
    //             audioData[sampleIndex] +=
    //                 wave * envelope * volume;
    //         }
    //     }
    //
    //     //creates the audio clip
    //     AudioClip generatedClip = AudioClip.Create(
    //         "ProceduralSoundtrack",
    //         totalSamples,
    //         1,
    //         sampleRate,
    //         false
    //     );
    //
    //     generatedClip.SetData(audioData, 0);
    //
    //     audioSource.clip = generatedClip;
    //     audioSource.loop = true;
    //     audioSource.Play();
    // }
}
