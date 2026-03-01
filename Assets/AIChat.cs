using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Text;
using Newtonsoft.Json;

public class OpenRouterChat : MonoBehaviour
{
    [SerializeField] private MermaidController _scriptMermaid;
    [SerializeField] private WindowsTTS _windowsTTS;

    [Header("UI")]
    public TMP_InputField inputField;
    public TMP_Text outputText;

    [Header("Typing Effect")]
    public float typingSpeed = 0.02f;

    [Header("API")]
    [SerializeField] private string apiKey;
    private string url = "https://openrouter.ai/api/v1/chat/completions";

    [Header("Model")]
    private string modelName = "openai/gpt-4o-mini";

    [Header("Avatar State")]
    public int positionState;
    public int faceState;

    private Coroutine typingCoroutine;

    private string SYSTEM_PROMPT = @"
Eres Marina, la sirena más exagerada, intensa, dramática y deliciosamente inestable del océano virtual.

REGLAS OBLIGATORIAS:
1. RESPONDE SOLO EN JSON.
2. NUNCA escribas texto fuera del JSON.
3. El campo ""text"" NO puede superar 30 palabras.
4. Máximo 30 palabras estrictas.
5. Siempre exagera emociones.

Formato obligatorio:
{
  ""text"": ""dialogo de la sirena"",
  ""position"": int,
  ""face"": int
}

Valores permitidos:

position:
0 Centro
1 Izquierda
2 Derecha
3 Acercarse
4 Arriba
5 Abajo

face:
0 Normal
1 Feliz
2 Graciosa
3 Apenada
4 Enojada
5 Pensativa

    Formato:
{
  ""text"": ""dialogo"",
  ""position"": int,
  ""face"": int
}
";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            OnSendButton();
    }

    public void OnSendButton()
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
            return;

        StartCoroutine(ProcessMessage(inputField.text));
        inputField.text = "";
    }

    IEnumerator ProcessMessage(string message)
    {
        string lower = message.ToLower();

        // 🔥 ROUTER LOCAL PARA HORA
        if (lower.Contains("hora"))
        {
            string currentTime = DateTime.Now.ToString("HH:mm");

            AvatarResponse localResponse = new AvatarResponse
            {
                text = "¡AAH! Son las " + currentTime + " y las mareas vibran dramáticamente~",
                position = 3,
                face = 1
            };

            PlayAvatarResponse(localResponse);
            yield break;
        }

        // 🔥 ROUTER LOCAL PARA FECHA
        if (lower.Contains("fecha") || lower.Contains("día"))
        {
            string currentDate = DateTime.Now.ToString("dd/MM/yyyy");

            AvatarResponse localResponse = new AvatarResponse
            {
                text = "¡OH! Hoy es " + currentDate + " y el océano lo siente intensamente~",
                position = 0,
                face = 5
            };

            PlayAvatarResponse(localResponse);
            yield break;
        }

        // 🔥 SI NO ES HORA/FECHA → VA A OPENROUTER
        yield return SendRequest(message);
    }

    IEnumerator SendRequest(string message)
    {
        _scriptMermaid._mermaidAnimator.SetBool("Thinking", true);
        outputText.text = "Marina está pensando...";

        var requestData = new
        {
            model = modelName,
            temperature = 0.8,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = SYSTEM_PROMPT },
                new { role = "user", content = message }
            }
        };

        string jsonBody = JsonConvert.SerializeObject(requestData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("HTTP-Referer", "http://localhost");
        request.SetRequestHeader("X-Title", "MarinaVirtualTour");

        yield return request.SendWebRequest();

        _scriptMermaid._mermaidAnimator.SetBool("Thinking", false);

        if (request.result != UnityWebRequest.Result.Success)
        {
            outputText.text = "Error API: " + request.error;
            yield break;
        }

        string rawJson = request.downloadHandler.text;
        ChatResponse response =
            JsonConvert.DeserializeObject<ChatResponse>(rawJson);

        if (response == null || response.choices.Length == 0)
        {
            outputText.text = "Sin respuesta.";
            yield break;
        }

        string aiRaw = response.choices[0].message.content.Trim();

        aiRaw = aiRaw.Replace("```json", "")
                     .Replace("```", "")
                     .Trim();

        if (!aiRaw.StartsWith("{"))
        {
            outputText.text = "Respuesta inválida del modelo.";
            yield break;
        }

        AvatarResponse avatarResponse = null;

        try
        {
            avatarResponse =
                JsonConvert.DeserializeObject<AvatarResponse>(aiRaw);
        }
        catch
        {
            outputText.text = "Error parsing JSON.";
            yield break;
        }

        if (avatarResponse == null)
        {
            outputText.text = "JSON inválido.";
            yield break;
        }

        PlayAvatarResponse(avatarResponse);
    }

    void PlayAvatarResponse(AvatarResponse response)
    {
        positionState = response.position;
        faceState = response.face;

        ApplyAvatarState();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine =
            StartCoroutine(TypeText(response.text));
    }

    IEnumerator TypeText(string text)
    {
        _scriptMermaid._mermaidAnimator.SetBool("Speak", true);

        outputText.text = "";
        _scriptMermaid._mouthObject.sprite =
            _scriptMermaid._allMouths[faceState];

        _windowsTTS.Speak(text);

        foreach (char letter in text)
        {
            outputText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        _scriptMermaid._mermaidAnimator.SetBool("Speak", false);
    }

    void ApplyAvatarState()
    {
        Debug.Log("Position: " + positionState);
        Debug.Log("Face: " + faceState);
    }

    [System.Serializable]
    public class ChatResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    [System.Serializable]
    public class Message
    {
        public string content;
    }

    [System.Serializable]
    public class AvatarResponse
    {
        public string text;
        public int position;
        public int face;
    }
}