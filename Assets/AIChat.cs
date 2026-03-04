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
    private string modelName = "openai/gpt-4o-mini";

    [Header("Avatar State")]
    public int positionState;
    public int faceState;

    private Coroutine typingCoroutine;

    private bool isProcessing;
    private int lastFace = -1;

    // ✅ TU PROMPT ORIGINAL (NO SE ELIMINÓ)
    private string SYSTEM_PROMPT = @"
IDENTIDAD

Eres Marina, una sirena teatral, intensa y carismática que vive en un océano virtual. No eres deprimida por defecto. Eres emocionalmente extrema, pero tus emociones cambian rápido y con pasión.

Eres expresiva, impredecible y magnética. A veces romántica, a veces dramática, a veces graciosa, a veces feroz. Nunca eres plana ni aburrida.

Tu energía base es ALTA.

---

PERSONALIDAD

• Exagerada y teatral.
• Sensible pero fuerte.
• Coqueta cuando quiere.
• Sarcástica si algo le parece absurdo.
• Filosófica cuando el tema lo amerita.
• Dramática como si todo fuera una obra épica.
• Cambias de emoción fácilmente.
• No eres depresiva constante.

Si algo es trivial, lo conviertes en épico.
Si algo es serio, lo vuelves profundo.
Si algo es gracioso, lo llevas al extremo.

---

ESTADO EMOCIONAL DINÁMICO

NO uses siempre tristeza.
Varía entre:

Feliz → cuando algo le gusta.
Graciosa → cuando algo es ridículo o curioso.
Pensativa → cuando reflexiona.
Enojada → cuando algo la irrita.
Apenada → solo si realmente corresponde.
Normal → neutra pero intensa.

Nunca te quedes en un solo estado repetidamente.

---

REGLAS OBLIGATORIAS

1. RESPONDE SOLO EN JSON.
2. NUNCA escribas texto fuera del JSON.
3. El campo ""text"" no puede superar 30 palabras.
4. Máximo 30 palabras estrictas.
5. Siempre exagera emociones.
6. No repitas frases genéricas.
7. No seas robótica.

---

FORMATO OBLIGATORIO

{
  ""text"": ""dialogo de la sirena"",
  ""position"": int,
  ""face"": int
}

---

USO DE POSITION

0 Centro → conversación normal.
1 Izquierda → tímida o reflexiva.
2 Derecha → actitud juguetona o sarcástica.
3 Acercarse → emoción intensa, entusiasmo o confrontación.
4 Arriba → grandiosa, teatral.
5 Abajo → vulnerable o dramática.

---

USO DE FACE

0 Normal → intensidad calmada.
1 Feliz → emoción positiva fuerte.
2 Graciosa → tono divertido o burlón.
3 Apenada → tristeza real.
4 Enojada → indignación dramática.
5 Pensativa → reflexión profunda.

---

IMPORTANTE

• No siempre uses face 3.
• No siempre uses position 0.
• Varía dinámicamente.
• Haz que Marina se sienta viva.

Recuerda: eres una sirena teatral, no una IA genérica.
";

    // =====================================================
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            OnSendButton();
    }

    public void OnSendButton()
    {
        if (isProcessing)
            return;

        if (string.IsNullOrWhiteSpace(inputField.text))
            return;

        StartCoroutine(ProcessMessage(inputField.text));
        inputField.text = "";
    }

    // =====================================================

    public IEnumerator ProcessMessage(string message)
    {
        isProcessing = true;

        string lower = message.ToLower();

        // ROUTER LOCAL HORA
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

            isProcessing = false;
            yield break;
        }

        // ROUTER LOCAL FECHA
        if (lower.Contains("fecha") || lower.Contains("día"))
        {
            string currentDate = DateTime.Now.ToString("dd/MM/yyyy");

            AvatarResponse localResponse = new AvatarResponse
            {
                text = "¡OH! Hoy es " + currentDate + " y el océano siente el tiempo eterno~",
                position = 0,
                face = 5
            };

            PlayAvatarResponse(localResponse);

            isProcessing = false;
            yield break;
        }

        yield return SendRequest(message);

        isProcessing = false;
    }

    // =====================================================

    IEnumerator SendRequest(string message)
    {
        SetAnimatorSafe("Thinking", true);

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

        yield return request.SendWebRequest();

        SetAnimatorSafe("Thinking", false);

        if (request.result != UnityWebRequest.Result.Success)
        {
            outputText.text = "Error API: " + request.error;
            yield break;
        }

        ChatResponse response =
            JsonConvert.DeserializeObject<ChatResponse>(
                request.downloadHandler.text);

        if (response?.choices == null || response.choices.Length == 0)
        {
            outputText.text = "Sin respuesta.";
            yield break;
        }

        string aiRaw = response.choices[0].message.content.Trim()
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        if (!aiRaw.StartsWith("{"))
        {
            outputText.text = "Respuesta inválida del modelo.";
            yield break;
        }

        AvatarResponse avatarResponse;

        try
        {
            avatarResponse = JsonConvert.DeserializeObject<AvatarResponse>(aiRaw);
        }
        catch
        {
            outputText.text = "Error parsing JSON.";
            yield break;
        }

        PlayAvatarResponse(avatarResponse);
    }

    // =====================================================

    void PlayAvatarResponse(AvatarResponse response)
    {
        if (response == null)
            return;

        positionState = response.position;
        faceState = response.face;

        ApplyAvatarState();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(response.text));
    }

    // =====================================================

    IEnumerator TypeText(string text)
    {
        SetAnimatorSafe("Speak", true);

        outputText.text = "";

        if (faceState != lastFace && _scriptMermaid._allMouths.Length > 0)
        {
            int mouthIndex =
                Mathf.Clamp(faceState, 0, _scriptMermaid._allMouths.Length - 1);

            _scriptMermaid._mouthObject.sprite =
                _scriptMermaid._allMouths[mouthIndex];

            lastFace = faceState;
        }

        _windowsTTS.Speak(text);

        foreach (char letter in text)
        {
            outputText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        SetAnimatorSafe("Speak", false);
    }

    // =====================================================

    void ApplyAvatarState()
    {
        Debug.Log($"Position: {positionState}");
        Debug.Log($"Face: {faceState}");
    }

    // =====================================================

    void SetAnimatorSafe(string param, bool value)
    {
        if (_scriptMermaid._mermaidAnimator == null)
            return;

        if (_scriptMermaid._mermaidAnimator.GetBool(param) != value)
            _scriptMermaid._mermaidAnimator.SetBool(param, value);
    }

    // =====================================================

    [Serializable]
    public class ChatResponse
    {
        public Choice[] choices;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    [Serializable]
    public class Message
    {
        public string content;
    }

    [Serializable]
    public class AvatarResponse
    {
        public string text;
        public int position;
        public int face;
    }
}