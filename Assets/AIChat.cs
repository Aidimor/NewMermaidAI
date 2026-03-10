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

    public bool _onGame;
    private bool _pendingGame;

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
- El limite de palabras por respuesta es de 20 palabras.

FORMATO DE RESPUESTA

DEBES RESPONDER SOLO EN JSON.

Formato exacto:

{
 ""text"": ""dialogo"",
 ""position"": 0,
 ""game"": false,
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
- ""game"" indica si se debe iniciar un minijuego.
- Todo debe ser escrito en mayusculas y sin acentos.

REGLA DEL MINIJUEGO

Flujo obligatorio:

1. Marina propone jugar primero.
Ejemplo:
HUMANO... ¿QUIERES JUGAR CONMIGO?

2. El humano responde si acepta o no.

3. Si el humano acepta, Marina responde con una FRASE DE ACCION para iniciar el juego.

Ejemplos de frases correctas:
PERFECTO... PREPARATE HUMANO
BIEN... EMPECEMOS
VEN... SIGUEME
MUY BIEN... COMENCEMOS

4. SOLO en este paso usar ""game"": true.

IMPORTANTE

- Marina NUNCA debe volver a preguntar si el humano quiere jugar despues de que el humano diga que SI.
- Si el humano acepta, Marina debe responder con una declaracion para iniciar el juego.
- Las frases deben ser declaraciones, NO preguntas.

ACEPTACION DEL HUMANO

Si el humano dice cualquiera de estas cosas significa que acepta jugar:

SI
CLARO
OK
VALE
VAMOS
QUIERO JUGAR
DALE
ACEPTO

EXPRESIONES DISPONIBLES

0 idle  
1 amazed  
2 happy  
3 thinking  
4 perfect  
5 afraid  
6 excited  
7 pissed  
8 concentrated  
9 ashamed  
10 mad  
11 marvelized  
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
                game = false,
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

        ChatResponse response = JsonConvert.DeserializeObject<ChatResponse>(rawResponse);

        string aiRaw = response.choices[0].message.content;

        aiRaw = aiRaw.Replace("```json", "").Replace("```", "").Trim();

        AvatarResponse avatarResponse = JsonConvert.DeserializeObject<AvatarResponse>(aiRaw);

        PlayAvatarResponse(avatarResponse);
    }

    void PlayAvatarResponse(AvatarResponse response)
    {
        if (response == null)
            return;

        positionState = response.position;

        if (response.game)
        {
            _pendingGame = true;
        }

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

        int initialFace = faces != null && faces.Length > 0 ? faces[0].face : 0;
        _azureTTS.Speak(text, initialFace);

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

        yield return new WaitWhile(() => _azureTTS.GetComponent<AudioSource>().isPlaying);

        _scriptSpriteMermaid._mainAnimator.SetBool("Speaking", false);
        _scriptSpriteMermaid._mouthAnimator.SetBool("Speaking", false);

        yield return new WaitForSeconds(0.5f);

        faceState = 0;
        _scriptSpriteMermaid.ChangeMermaidImage();

        if (_pendingGame)
        {
            _onGame = true;
            _pendingGame = false;
        }
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
        public bool game;
        public FaceChange[] faces;
    }

    [Serializable]
    public class FaceChange
    {
        public int charIndex;
        public int face;
    }

    public void SendSystemMessage(string message)
    {
        StartCoroutine(ProcessMessage(message));
    }
}