using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

[RequireComponent(typeof(AudioSource))]
public class ElevenLabsTTS : MonoBehaviour
{
    [Header("ElevenLabs API")]
    public string apiKey;
    public string voiceId;
    public string model;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Speak(string text, int faceEmotion)
    {
        string prefix = GetEmotionPrefix(faceEmotion);
        StartCoroutine(SpeakCoroutine(prefix + text));
    }

    string GetEmotionPrefix(int face)
    {
        switch (face)
        {
            case 2: return "HAPPY: ";
            case 6: return "EXCITED: ";
            case 5: return "FEARFUL: ";
            case 9: return "SAD: ";
            case 7: return "ANGRY: ";
            default: return "";
        }
    }

    IEnumerator SpeakCoroutine(string text)
    {
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}?output_format=mp3_44100_128";

        var body = new
        {
            text = text,
            model_id = model,
            voice_settings = new
            {
                stability = 0.7,
                similarity_boost = 0.8
            }
        };

        string json = JsonConvert.SerializeObject(body);

        Debug.Log("ELEVEN REQUEST JSON:");
        Debug.Log(json);

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");

        request.uploadHandler = new UploadHandlerRaw(bytes);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("xi-api-key", apiKey.Trim());
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        Debug.Log("ELEVEN RESPONSE CODE: " + request.responseCode);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("ElevenLabs ERROR: " + request.error);
            Debug.LogError("SERVER RESPONSE: " + request.downloadHandler.text);
            yield break;
        }

        byte[] audioBytes = request.downloadHandler.data;

        StartCoroutine(PlayAudio(audioBytes));
    }

    IEnumerator PlayAudio(byte[] audioBytes)
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "tts.mp3");
        System.IO.File.WriteAllBytes(path, audioBytes);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Audio load error: " + www.error);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}