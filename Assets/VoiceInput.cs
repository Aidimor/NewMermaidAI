using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class VoiceInput : MonoBehaviour
{
    public OpenRouterChat chatManager;

    [Header("Debug")]
    public string recognizedText;

    private string micName;
    private AudioClip clip;
    private bool isRecording = false;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
            Debug.Log("Mic detectado: " + micName);
        }
        else
        {
            Debug.LogError("No micrófono detectado.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0) && !isRecording)
        {
            clip = Microphone.Start(micName, false, 10, 44100);
            isRecording = true;
            Debug.Log("Grabando...");
        }

        if (Input.GetKeyUp(KeyCode.R) && isRecording)
        {
            StopRecording();
        }
    }

    void StopRecording()
    {
        int position = Microphone.GetPosition(micName);
        Microphone.End(micName);
        isRecording = false;

        if (position <= 0)
        {
            Debug.LogWarning("No se grabó audio.");
            return;
        }

        float[] samples = new float[position * clip.channels];
        clip.GetData(samples, 0);

        // 🔥 NORMALIZAR AUDIO
        float max = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            float abs = Mathf.Abs(samples[i]);
            if (abs > max)
                max = abs;
        }

        if (max > 0)
        {
            float gain = 1f / max;
            for (int i = 0; i < samples.Length; i++)
                samples[i] *= gain;
        }

        AudioClip trimmedClip = AudioClip.Create(
            "trimmed",
            position,
            clip.channels,
            clip.frequency,
            false
        );

        trimmedClip.SetData(samples, 0);
        clip = trimmedClip;

        Debug.Log("Audio normalizado. Enviando a Whisper...");
        //StartCoroutine(SendToLocalWhisper());
    }

    //IEnumerator SendToLocalWhisper()
    //{
    //    byte[] wavData = WavUtility.FromAudioClip(clip);

    //    Debug.Log("Tamaño WAV: " + wavData.Length);

    //    if (wavData.Length < 2000)
    //    {
    //        Debug.LogWarning("Audio demasiado pequeño.");
    //        yield break;
    //    }

    //    WWWForm form = new WWWForm();
    //    form.AddBinaryData("audio", wavData, "audio.wav", "audio/wav");

    //    UnityWebRequest request =
    //        UnityWebRequest.Post("http://127.0.0.1:5000/transcribe", form);

    //    yield return request.SendWebRequest();

    //    if (request.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.LogError("Error local Whisper: " + request.error);
    //        yield break;
    //    }

    //    string json = request.downloadHandler.text;
    //    Debug.Log("JSON recibido: " + json);

    //    WhisperResponse response =
    //        JsonUtility.FromJson<WhisperResponse>(json);

    //    if (response != null && !string.IsNullOrEmpty(response.text))
    //    {
    //        recognizedText = response.text.Trim();
    //        Debug.Log("Texto detectado: " + recognizedText);

    //        //StartCoroutine(chatManager.ProcessMessage(recognizedText));
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Whisper no detectó texto.");
    //    }
    //}

    [System.Serializable]
    public class WhisperResponse
    {
        public string text;
    }
}