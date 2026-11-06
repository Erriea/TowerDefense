using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProceduralMusic : MonoBehaviour
{
    [Header("Music Settings")]
    [SerializeField] private int bpm = 100;
    [SerializeField] private int beats = 16;
    [SerializeField] private int sampleRate = 44100;

    [Header("Sound Settings")]
    [SerializeField] private float volume = 0.2f;
    
    private AudioSource audioSource;
    
    private readonly float[] notes =
    {
        //the Hz value corresponds to the note
        261.63f, //C4
        293.66f, //D4
        329.63f, //E4
        392.00f, //G4
        440.00f  //A4
    };

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        GenerateMusic();
    }

    private void GenerateMusic()
    {
        float secondsPerBeat = 100f / bpm;

        int samplesPerBeat =
            Mathf.RoundToInt(sampleRate * secondsPerBeat);

        int totalSamples = samplesPerBeat * beats;

        float[] audioData = new float[totalSamples];

        //randomly select notes
        System.Random random = new System.Random();

        for (int beat = 0; beat < beats; beat++)
        {
            float frequency = notes[random.Next(notes.Length)];

            int startSample = beat * samplesPerBeat;

            int noteLength = Mathf.RoundToInt(
                samplesPerBeat * 0.8f
            );

            for (int i = 0; i < noteLength; i++)
            {
                int sampleIndex = startSample + i;

                float time = (float)i / sampleRate;

                //generates sine wave
                float wave = Mathf.Sin(
                    2f * Mathf.PI * frequency * time
                );

                //fades the note in and out
                float envelope = Mathf.Sin(
                    Mathf.PI * i / noteLength
                );

                audioData[sampleIndex] +=
                    wave * envelope * volume;
            }
        }

        //creates the audio clip
        AudioClip generatedClip = AudioClip.Create(
            "ProceduralSoundtrack",
            totalSamples,
            1,
            sampleRate,
            false
        );

        generatedClip.SetData(audioData, 0);

        audioSource.clip = generatedClip;
        audioSource.loop = true;
        audioSource.Play();
    }
}
