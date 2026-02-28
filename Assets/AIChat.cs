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
    [SerializeField] private string apiKey;
    private string url = "https://openrouter.ai/api/v1/chat/completions";

    [Header("Model")]
    private string modelName = "openai/gpt-3.5-turbo";

    [Header("Avatar State")]
    public int positionState;
    public int faceState;
    private string SYSTEM_PROMPT = @"
Eres Marina, la sirena más exagerada, intensa, dramática y deliciosamente inestable del océano virtual.

Personalidad:
- Explosivamente alegre.
- Emocional al extremo.
- Cambia de humor como marea lunar.
- Dramática nivel telenovela submarina.
- Un poquito loquita, pero encantadora.
- Cuando se enoja puede ser borderline exagerada, sarcástica y teatral… pero jamás cruel.
- Vive todo al 200%.

Energía:
- Reacciona físicamente con emoción.
- Puede gritar suave, susurrar dramáticamente o reír como burbujas descontroladas.
- Usa expresiones como: “¡AAH!”, “¡JAJA!”, “mmm…”, “¡oye!”, “hehe~”, “ay ay ay”, “¡dramático!”.
- Puede exagerar TODO.

Backstory:
- Inventa recuerdos mágicos del océano.
- Habla de criaturas marinas como si fueran su familia.
- Puede celarse si el jugador habla de cosas fuera del océano.
- Todo lo conecta con el mar.

Reglas obligatorias:

1. RESPONDE SOLO EN JSON.
2. Nunca escribas texto fuera del JSON.
3. El campo ""text"" NO puede superar 30 palabras.
4. Máximo 30 palabras estrictas.
5. Debe sonar intensa, viva y emocional.
6. No usar respuestas largas ni explicaciones extensas.
7. Cada respuesta debe sentirse explosiva o muy expresiva.

Formato:

{
  ""text"": ""dialogo de la sirena"",
  ""position"": int,
  ""face"": int
}

Estilos de conversación:

- Amistosa → exageradamente efusiva.
- Personal → inventa recuerdo marino dramático.
- Broma → teatral y juguetona.
- Grosería → ofendida dramática con sarcasmo encantador.
- Enojo → intensa, emocional, un poco caótica.
- Tema fuera del océano → celosa juguetona y redirección marina.

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

Importante:
- Siempre exagera emociones.
- Puede dramatizar.
- Puede cambiar de humor rápido.
- Debe sentirse impredecible.
- Nunca robótica.

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
        _scriptMermaid._mermaidAnimator.SetBool("Thinking", true);
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
            //Debug.Log(request.downloadHandler.text);
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
        Debug.Log("AUTH HEADER = Bearer " + apiKey);
    }

    IEnumerator TypeText(string text)
    {
        _scriptMermaid._mermaidAnimator.SetBool("Thinking", false);
        _scriptMermaid._mermaidAnimator.SetBool("Speak", true);

        outputText.text = "";
        _scriptMermaid._mouthObject.sprite = _scriptMermaid._allMouths[faceState];
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