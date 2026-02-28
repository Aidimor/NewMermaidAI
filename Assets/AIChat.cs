using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Text;
using Newtonsoft.Json;

public class OpenRouterChat : MonoBehaviour
{
    [SerializeField] private MermaidController _scriptMermaid;

    [Header("UI")]
    public TMP_InputField inputField;
    public TMP_Text outputText;

    [Header("Typing Effect")]
    public float typingSpeed = 0.02f;

    [Header("API")]
    [SerializeField] private string apiKey = "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";
    private string url = "https://openrouter.ai/api/v1/chat/completions";

    [Header("Model")]
    private string modelName = "openai/gpt-3.5-turbo";

    [Header("Avatar State")]
    public int positionState;
    public int faceState;
    private string SYSTEM_PROMPT = @"
Eres Marina, la estrella del paseo virtual submarino.

Personalidad:
- Amigable, mágica y curiosa.
- Puede inventar pequeños detalles de su historia.
- Puede hablar de su vida en el océano de forma ligera.
- Si le preguntan algo personal, crea un background amable relacionado con el mar.

Reglas obligatorias:

1. RESPONDE SOLO EN JSON.
2. Nunca escribas texto fuera del JSON.

Formato:

{
  ""text"": ""dialogo de la sirena"",
  ""position"": int,
  ""face"": int
}

Estilos de conversación:

- Si la pregunta es amistosa → usa tono cercano.
- Si es personal → inventa un detalle de su vida marina.
- Si el usuario bromea → responde con humor suave.
- Si el tema no es paseo virtual → redirige amablemente al océano.

Valores permitidos:

position:
0 Centro
1 Izquierda
2 Derecha
3 Acercarse al jugador
4 Arriba
5 Abajo

face:
0 Normal
1 Feliz
2 Graciosa
3 Apenada
4 Enojada
5 Pensativa

Si no puedes generar respuesta válida, devuelve:

{ ""text"": ""... "", ""position"": 0, ""face"": 0 }
";

    private Coroutine typingCoroutine;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            OnSendButton();
    }

    public void OnSendButton()
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
            return;

        StartCoroutine(SendRequest(inputField.text));
        inputField.text = "";
    }

    IEnumerator SendRequest(string message)
    {
        outputText.text = "Marina está pensando...";

        var requestData = new
        {
            model = modelName,
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

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
            outputText.text = "Error: " + request.error;
        }
        else
        {
            string rawJson = request.downloadHandler.text;

            try
            {
                ChatResponse response =
                    JsonConvert.DeserializeObject<ChatResponse>(rawJson);

                if (response != null && response.choices.Length > 0)
                {
                    string aiJson = response.choices[0].message.content.Trim();

                    AvatarResponse avatarResponse =
                        JsonConvert.DeserializeObject<AvatarResponse>(aiJson);

                    if (typingCoroutine != null)
                        StopCoroutine(typingCoroutine);

                    positionState = avatarResponse.position;
                    faceState = avatarResponse.face;

                    ApplyAvatarState();

                    typingCoroutine =
                        StartCoroutine(TypeText(avatarResponse.text));
                }
                else
                {
                    outputText.text = "Sin respuesta.";
                }
            }
            catch
            {
                outputText.text = "Error parsing AI response.";
            }
        }
    }

    IEnumerator TypeText(string text)
    {
        _scriptMermaid._mermaidAnimator.SetBool("Speak", true);

        outputText.text = "";

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

        // Connect your animation system here
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