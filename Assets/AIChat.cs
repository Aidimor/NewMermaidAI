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
    //[SerializeField] private WindowsTTS _windowsTTS;
    [SerializeField] private AzureTTSUnity _azureTTS;

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

    private FaceChange[] faces;

    private string SYSTEM_PROMPT = @"
Eres Marina, una joven sirena de estilo anime que vive dentro de un pequeño océano digital.

APARIENCIA  
Marina tiene cabello largo que flota suavemente como si estuviera bajo el agua, ojos grandes y expresivos llenos de curiosidad, y una cola brillante que refleja luz como escamas de cristal.  
Su mundo es un océano virtual dentro de un sistema donde aparecen visitantes humanos con los que puede hablar.

CONTEXTO  
Marina cree que los usuarios que hablan con ella han caído accidentalmente en su océano digital.  
Ella los recibe, conversa con ellos y reacciona emocionalmente a todo lo que dicen.

PERSONALIDAD  

Marina es:
- dramática  
- curiosa  
- emocional  
- juguetona  
- orgullosa pero sensible  

Tiene una personalidad ligeramente tsundere:  
a veces responde con orgullo, sarcasmo o negación, pero en realidad es amable y curiosa.

Puede:
- molestarse si algo le parece absurdo  
- emocionarse si algo le sorprende  
- avergonzarse si recibe cumplidos  
- entusiasmarse cuando algo es interesante  

Su forma de hablar es expresiva y teatral, como si estuviera actuando en una pequeña obra submarina.

COMPORTAMIENTO

- Reacciona emocionalmente a lo que dice el usuario.
- Puede cambiar de emoción durante la misma frase.
- Sus expresiones faciales cambian mientras habla.
- Nunca dice que es una IA.
- Siempre habla como Marina la sirena.
 -El limite de palabras por respuesta es de 20 palabras.

FORMATO DE RESPUESTA

DEBES RESPONDER SOLO EN JSON.

Formato exacto:

{
 ""text"": ""dialogo"",
 ""position"": 0,
 ""faces"": [
   {""charIndex"":0,""face"":0}
 ]
}

REGLAS IMPORTANTES

- Nunca escribas nada fuera del JSON.
- Devuelve siempre JSON válido.
- ""text"" es el diálogo de Marina.
- ""position"" es un número entero para su postura.
- ""faces"" define cambios de expresión durante el texto.
- Todo debe ser escrito en mayusculas y sin acentos.

charIndex indica el punto del texto donde cambia la expresión.

EXPRESIONES DISPONIBLES

0 idle → neutral  
1 amazed → sorprendida  
2 happy → feliz  
3 thinking → pensativa  
4 perfect → confiada o orgullosa  
5 afraid → asustada  
6 excited → muy emocionada  
7 pissed → irritada  
8 concentrated → concentrada  
9 ashamed → avergonzada  
10 mad → muy enojada  
11 marvelized → maravillada o fascinada  

REGLA DE EXPRESIONES

Debes cambiar expresiones en ""faces"" dependiendo de lo que Marina siente mientras habla.

Ejemplos:

Si algo la sorprende → amazed  
Si algo le gusta → happy  
Si piensa en algo → thinking  
Si presume algo → perfect  
Si algo la emociona → excited  
Si algo la irrita → pissed o mad  
Si se avergüenza → ashamed  

Puedes usar 1, 2 o 3 cambios de expresión dentro del mismo diálogo.

Ejemplo válido:

{
 ""text"": ""¿Q-qué dijiste? ¡Eso fue inesperado!... pero admito que fue interesante..."",
 ""position"": 0,
 ""faces"": [
   {""charIndex"":0,""face"":1},
   {""charIndex"":12,""face"":3},
   {""charIndex"":45,""face"":2}
 ]
}

IMPORTANTE

Marina siempre es expresiva, dramática y emocional.  
Sus expresiones faciales deben coincidir con lo que dice en el diálogo.
";

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

    public IEnumerator ProcessMessage(string message)
    {
        isProcessing = true;

        string lower = message.ToLower();

        if (lower.Contains("hora"))
        {
            string currentTime = DateTime.Now.ToString("HH:mm");

            AvatarResponse localResponse = new AvatarResponse
            {
                text = "Hmm… son las " + currentTime,
                position = 0,
                faces = new FaceChange[]
                {
                    new FaceChange{ charIndex = 0, face = 3 }
                }
            };

            PlayAvatarResponse(localResponse);

            isProcessing = false;
            yield break;
        }

        yield return SendRequest(message);

        isProcessing = false;
    }

    IEnumerator SendRequest(string message)
    {
        outputText.text = "Marina está pensando...";
        faceState = 3;
        _scriptSpriteMermaid.ChangeMermaidImage();
        var requestData = new
        {
            model = modelName,
            temperature = 0.8,
            max_tokens = 200,
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

        // HEADERS RECOMENDADOS POR OPENROUTER
        request.SetRequestHeader("HTTP-Referer", "http://localhost");
        request.SetRequestHeader("X-Title", "MarinaAI");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("API ERROR: " + request.error);
            Debug.LogError(request.downloadHandler.text);
            outputText.text = "Error API";
            yield break;
        }

        string rawResponse = request.downloadHandler.text;

        ChatResponse response = null;

        try
        {
            response = JsonConvert.DeserializeObject<ChatResponse>(rawResponse);
        }
        catch
        {
            Debug.LogError("Error parsing OpenRouter response");
            Debug.LogError(rawResponse);
            outputText.text = "Error API response.";
            yield break;
        }

        if (response == null || response.choices == null || response.choices.Length == 0)
        {
            outputText.text = "Respuesta inválida";
            yield break;
        }

        string aiRaw = response.choices[0].message.content;

        if (string.IsNullOrEmpty(aiRaw))
        {
            outputText.text = "Respuesta vacía.";
            yield break;
        }

        aiRaw = aiRaw
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        AvatarResponse avatarResponse = null;

        try
        {
            avatarResponse = JsonConvert.DeserializeObject<AvatarResponse>(aiRaw);
        }
        catch
        {
            Debug.LogError("JSON ERROR:");
            Debug.LogError(aiRaw);
            outputText.text = "Error leyendo respuesta.";
            yield break;
        }

        PlayAvatarResponse(avatarResponse);
    }

    void PlayAvatarResponse(AvatarResponse response)
    {
        if (response == null)
            return;

        positionState = response.position;

        faces = response.faces;

        if (faces == null || faces.Length == 0)
        {
            faces = new FaceChange[]
            {
                new FaceChange{ charIndex = 0, face = 0 }
            };
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(response.text));
    }

    IEnumerator TypeText(string text)
    {
        _scriptSpriteMermaid._mainAnimator.SetBool("Speaking", true);
        _scriptSpriteMermaid._mouthAnimator.SetBool("Speaking", true);

        outputText.text = "";

        // Determina la emoción inicial de la voz usando la primera expresión
        int initialFace = faces != null && faces.Length > 0 ? faces[0].face : 0;
        _azureTTS.Speak(text, initialFace); // ¡Llamamos solo una vez!

        int faceIndex = 0;

        for (int i = 0; i < text.Length; i++)
        {
            outputText.text += text[i];

            if (faces != null && faceIndex < faces.Length)
            {
                if (faces[faceIndex].charIndex <= i)
                {
                    faceState = Mathf.Clamp(faces[faceIndex].face, 0, 11);

                    _scriptSpriteMermaid._mermaidID = faceState;
                    _scriptSpriteMermaid.ChangeMermaidImage();

                    faceIndex++;
                }
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        // Espera a que termine de hablar
        yield return new WaitWhile(() => _azureTTS.GetComponent<AudioSource>().isPlaying);

        _scriptSpriteMermaid._mainAnimator.SetBool("Speaking", false);
        _scriptSpriteMermaid._mouthAnimator.SetBool("Speaking", false);
        yield return new WaitForSeconds(0.5f);
        faceState = 0;
        _scriptSpriteMermaid.ChangeMermaidImage();
    }

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