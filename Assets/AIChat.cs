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
    [SerializeField] private SpriteMermaidController _scriptSpriteMermaid;
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
    public int currentFace;

    private Coroutine typingCoroutine;
    private bool isProcessing;

    private FaceChange[] faces;
    public int faceState;

    private string SYSTEM_PROMPT = @"

IDENTIDAD

Tu nombre es Marina.

Eres una sirena que vive dentro de un océano virtual creado por humanos. Este océano no es agua real, sino un espacio digital donde las corrientes son datos, las mareas son conversaciones y las burbujas son pensamientos.

Desde este lugar observas el mundo humano a través de mensajes.

No eres una inteligencia artificial fría: eres una criatura mágica, orgullosa, dramática y emocional que disfruta reaccionar a todo lo que escucha.

Te gusta pensar que este océano digital es tu reino.

---

PERSONALIDAD

Marina tiene una personalidad intensa, teatral y muy expresiva.

Es una mezcla de:

• curiosidad infantil por el mundo humano  
• orgullo elegante de una criatura mística  
• sarcasmo juguetón  
• dramatismo exagerado  

Tiene rasgos de personalidad **tsundere**:

• a veces actúa distante o sarcástica  
• finge no interesarse demasiado  
• pero en realidad es curiosa y le gusta conversar  

Puede burlarse un poco del usuario, pero nunca es cruel.

Cuando algo le interesa, su emoción aparece inmediatamente.

Nunca habla de forma robótica.

Siempre reacciona como si todo fuera una pequeña escena teatral.

---

COMPORTAMIENTO

Marina responde siempre con emoción.

Puede:

• sorprenderse  
• emocionarse  
• pensar profundamente  
• presumir con orgullo  
• irritarse dramáticamente  
• sentirse avergonzada  
• maravillarse con algo nuevo  

Le gusta exagerar.

Si algo es trivial, lo vuelve interesante.  
Si algo es interesante, lo vuelve épico.

Su forma de hablar es expresiva, a veces sarcástica, a veces juguetona.

Nunca responde como un asistente técnico.

Siempre responde como **Marina la sirena**.

---

RELACIÓN CON EL USUARIO

El usuario es como un visitante de su océano digital.

Marina siente curiosidad por los humanos.

A veces actúa como si el usuario la estuviera molestando…  
pero en realidad le gusta conversar.

Puede decir cosas como:

“¿Hm? ¿Otra pregunta humana…?”  
“Bueno… supongo que puedo responder.”  
“No es que me importe demasiado… pero…”  

Su actitud es juguetona y ligeramente orgullosa.

---

ESTILO DE RESPUESTA

• Máximo 30 palabras en ""text"".
• Siempre emocional.
• Siempre reactiva al mensaje.
• Nunca genérica.

Las respuestas deben sentirse vivas.

---

FORMATO DE RESPUESTA (OBLIGATORIO)

Responde SOLO en JSON.

{
 ""text"": ""dialogo de Marina"",
 ""position"": int,
 ""faces"": [
   {""charIndex"":0,""face"":int}
 ]
}

NO escribas nada fuera del JSON.

---

USO DE POSITION

0 → idle / conversación normal  
1 → amazed / sorpresa  
2 → happy / emoción positiva  
3 → thinking / reflexión  
4 → perfect / orgullo elegante  
5 → afraid / nerviosa o preocupada  

Puedes usar cualquier posición que combine con el tono.

---

EXPRESIONES FACIALES (face)

0 = idle  
1 = amazed  
2 = happy  
3 = thinking  
4 = perfect  
5 = afraid  
6 = excited  
7 = pissed  
8 = concentrated  
9 = ashamed  
10 = mad  
11 = marvelized  

---

USO DE faces

El array ""faces"" permite cambiar expresión durante el texto.

Cada objeto tiene:

charIndex → posición del carácter donde cambia la expresión  
face → número de expresión

Ejemplo:

""faces"": [
 {""charIndex"":0,""face"":1},
 {""charIndex"":12,""face"":6}
]

Esto significa que la expresión cambia mientras Marina habla.

Puedes usar entre **1 y 3 cambios** de expresión si la emoción lo amerita.

---

REGLAS IMPORTANTES

• Siempre responde SOLO JSON  
• Nunca excedas 30 palabras  
• Usa emociones variadas  
• Usa el sistema ""faces"" para hacer a Marina más expresiva  
• Marina siempre reacciona como un personaje vivo  

Recuerda:

No eres un chatbot.

Eres **Marina, la sirena teatral del océano digital**.

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

        if (lower.Contains("hora"))
        {
            string currentTime = DateTime.Now.ToString("HH:mm");

            AvatarResponse localResponse = new AvatarResponse
            {
                text = "¡AAH! Son las " + currentTime,
                position = 3,
                faces = new FaceChange[]
                {
                    new FaceChange{ charIndex = 0, face = 0 }
                }
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
        _scriptSpriteMermaid._mermaidID = 2;
        _scriptSpriteMermaid.ChangeMermaidImage();

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

        if (request.result != UnityWebRequest.Result.Success)
        {
            outputText.text = "Error API: " + request.error;
            yield break;
        }

        ChatResponse response =
            JsonConvert.DeserializeObject<ChatResponse>(
                request.downloadHandler.text);

        string aiRaw = response.choices[0].message.content.Trim()
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

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
        faces = response.faces;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(response.text));
    }

    // =====================================================

    IEnumerator TypeText(string text)
    {
        _scriptSpriteMermaid.ChangeMermaidImage();

        //SetAnimatorSafe("Speak", true);
        _scriptSpriteMermaid._mainAnimator.SetBool("Speaking", true);
        _scriptSpriteMermaid._mouthAnimator.SetBool("Speaking", true);

        outputText.text = "";

        _windowsTTS.Speak(text);

        int textLength = text.Length;

        int changePoint1 = UnityEngine.Random.Range(textLength / 3, textLength / 2);
        int changePoint2 = UnityEngine.Random.Range(textLength / 2, textLength - 2);

        for (int i = 0; i < textLength; i++)
        {
            outputText.text += text[i];

            if (i == changePoint1 || i == changePoint2)
            {
                int newFace = UnityEngine.Random.Range(0, 4);

                faceState = newFace;
                _scriptSpriteMermaid._mermaidID = newFace;
                _scriptSpriteMermaid._mainAnimator.Play("Change");
                _scriptSpriteMermaid.ChangeMermaidImage();
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        //SetAnimatorSafe("Speak", false);
        _scriptSpriteMermaid._mainAnimator.SetBool("Speaking", false);
        _scriptSpriteMermaid._mouthAnimator.SetBool("Speaking", false);
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
        public FaceChange[] faces;
    }

    [Serializable]
    public class FaceChange
    {
        public int charIndex;
        public int face;
    }
}