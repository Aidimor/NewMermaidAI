using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

[RequireComponent(typeof(AudioSource))]
public class AzureTTSUnity : MonoBehaviour
{
    //API AZURE = DGK46rWbZGjuwSSCpAj5vJIp9rl3qFqeKmi4T2Xi7jjYuHt8Mo60JQQJ99CCACYeBjFXJ3w3AAAYACOGSt9h
    //RESGION AZURE = eastus
    //VOICE AZURE = es-MX-CandelaNeural
    [Header("Azure")]
    public string apiKey;
    public string region = "eastus";

    [Header("Voice")]
    public string voiceName = "es-MX-DaliaNeural";

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Speak(string text, int faceEmotion)
    {
        StartCoroutine(SpeakCoroutine(text, faceEmotion));
    }

    string GetAzureEmotion(int face)
    {
        switch (face)
        {
            case 2: return "cheerful";
            case 6: return "excited";
            case 9: return "sad";
            case 5: return "fearful";
            case 7: return "angry";
            case 10: return "angry";
            case 1: return "surprised";
            case 3: return "chat";
            case 11: return "cheerful";
            default: return "general";
        }
    }

    // agrega pausas naturales
    string HumanizeText(string text)
    {
        text = text.Replace("...", "<break time='400ms'/>");
        text = text.Replace(".", ".<break time='300ms'/>");
        text = text.Replace("!", "!<break time='350ms'/>");
        text = text.Replace("?", "?<break time='350ms'/>");

        return text;
    }

    IEnumerator SpeakCoroutine(string text, int faceEmotion)
    {
        string url = "https://" + region + ".tts.speech.microsoft.com/cognitiveservices/v1";

        string emotion = GetAzureEmotion(faceEmotion);

        text = HumanizeText(text);

        // variaciones humanas
        float rate = Random.Range(0.90f, 1.05f);
        float pitch = Random.Range(-3f, 4f);
        float styleDegree = Random.Range(0.6f, 1.3f);

        string ssml =
        "<speak version='1.0' xml:lang='es-MX' xmlns:mstts='https://www.w3.org/2001/mstts'>" +

        "<voice name='" + voiceName + "'>" +

        "<mstts:express-as style='" + emotion + "' styledegree='" + styleDegree + "'>" +

        "<prosody rate='" + rate + "' pitch='" + pitch + "%'>" +

        text +

        "</prosody>" +

        "</mstts:express-as>" +

        "</voice>" +

        "</speak>";

        byte[] body = Encoding.UTF8.GetBytes(ssml);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

        request.SetRequestHeader("Ocp-Apim-Subscription-Key", apiKey);
        request.SetRequestHeader("Content-Type", "application/ssml+xml");

        request.SetRequestHeader(
            "X-Microsoft-OutputFormat",
            "audio-48khz-192kbitrate-mono-mp3"
        );

        request.SetRequestHeader("User-Agent", "MarinaAI");

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