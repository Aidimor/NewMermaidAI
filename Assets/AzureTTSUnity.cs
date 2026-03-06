using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

[RequireComponent(typeof(AudioSource))]
public class AzureTTSUnity : MonoBehaviour
{
    [Header("Azure Settings")]
    public string apiKey;
    public string region = "eastus"; // cambia si tu region es otra

    [Header("Voice")]
    public string voiceName = "es-MX-DaliaNeural";

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Llama al TTS con emoción basada en faceState
    /// </summary>
    public void Speak(string text, int faceEmotion)
    {
        StartCoroutine(SpeakCoroutine(text, faceEmotion));
    }

    /// <summary>
    /// Convierte faceState en estilo de voz de Azure
    /// </summary>
    private string GetAzureEmotion(int face)
    {
        switch (face)
        {
            case 2: return "cheerful";      // happy
            case 6: return "excited";       // excited
            case 9: return "sad";           // ashamed
            case 5: return "fearful";       // afraid
            case 7: return "angry";         // pissed
            case 10: return "angry";        // mad
            case 1: return "surprised";     // amazed
            case 3: return "chat";          // thinking
            case 11: return "cheerful";     // marvelized
            default: return "general";      // idle, perfect, concentrated
        }
    }

    IEnumerator SpeakCoroutine(string text, int faceEmotion)
    {
        string url = "https://" + region + ".tts.speech.microsoft.com/cognitiveservices/v1";

        string emotion = GetAzureEmotion(faceEmotion);

        // SSML con estilo de emoción
        string ssml =
        "<speak version='1.0' xml:lang='es-MX' xmlns:mstts='https://www.w3.org/2001/mstts'>" +
        "<voice name='" + voiceName + "'>" +
        "<mstts:express-as style='" + emotion + "'>" +
        text +
        "</mstts:express-as>" +
        "</voice></speak>";

        byte[] body = Encoding.UTF8.GetBytes(ssml);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);

        request.SetRequestHeader("Ocp-Apim-Subscription-Key", apiKey);
        request.SetRequestHeader("Content-Type", "application/ssml+xml");
        request.SetRequestHeader("X-Microsoft-OutputFormat", "riff-24khz-16bit-mono-pcm");
        request.SetRequestHeader("User-Agent", "UnityTTS");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Azure TTS Error: " + request.error);
            Debug.LogError(request.downloadHandler.text);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

        if (clip == null)
        {
            Debug.LogError("AudioClip NULL");
            yield break;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }
}