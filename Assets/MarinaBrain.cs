using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MarinaBrain : MonoBehaviour
{
    [Header("Memories")]
    public List<string> memories = new List<string>();

    [Header("Emotion")]
    public string currentEmotion = "curious";

    [Header("Relationship")]
    public float trust = 0.3f;
    public float friendship = 0.2f;
    public float curiosity = 0.7f;

    private string internalThought = "";

    [Header("World Knowledge")]
    public Dictionary<string, string> worldKnowledge = new Dictionary<string, string>();

    [Header("Hidden Fragments")]
    // Se seguirá usando solo para pistas base, pero ahora serán dinámicas desde HeartController
    public Dictionary<int, string> hiddenFragments = new Dictionary<int, string>();

    [Header("Heart Controller Reference")]
    public HeartController heartController;

    void Awake()
    {
        InitializeWorldKnowledge();
        InitializeHiddenFragments();
    }

    // -------------------
    // ADD MEMORY
    // -------------------
    public void AddMemory(string memory)
    {
        if (!memories.Contains(memory))
            memories.Add(memory);
    }

    // -------------------
    // BUILD MEMORY
    // -------------------
    public string BuildMemoryContext()
    {
        if (memories.Count == 0)
            return "MARINA AUN NO TIENE RECUERDOS IMPORTANTES.";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("MEMORIAS DE MARINA:");
        foreach (var m in memories)
        {
            sb.AppendLine("- " + m);
        }
        return sb.ToString();
    }

    // -------------------
    // BUILD EMOTION
    // -------------------
    public string BuildEmotionContext()
    {
        return "ESTADO EMOCIONAL DE MARINA: " + currentEmotion;
    }

    // -------------------
    // BUILD RELATIONSHIP
    // -------------------
    public string BuildRelationshipContext()
    {
        return
            "RELACION CON EL HUMANO\n" +
            "CONFIANZA: " + trust.ToString("0.0") + "\n" +
            "AMISTAD: " + friendship.ToString("0.0") + "\n" +
            "CURIOSIDAD: " + curiosity.ToString("0.0");
    }

    // -------------------
    // BUILD THOUGHT
    // -------------------
    public string BuildThoughtContext()
    {
        if (string.IsNullOrEmpty(internalThought))
            return "PENSAMIENTO ACTUAL: OBSERVANDO AL HUMANO.";

        return "PENSAMIENTO ACTUAL: " + internalThought;
    }

    // -------------------
    // INITIALIZE WORLD KNOWLEDGE
    // -------------------
    public void InitializeWorldKnowledge()
    {
        worldKnowledge.Clear();
        worldKnowledge.Add("entrada", "La entrada es amplia y brillante, perfecta para recibir visitantes.");
        worldKnowledge.Add("sala_sirenas", "Aquí viven otras sirenas, cada una con habilidades especiales.");
        worldKnowledge.Add("sala_medusas", "Medusas luminosas flotan suavemente y muestran caminos secretos.");
        worldKnowledge.Add("pasillo", "El pasillo conecta todas las salas, cuidado con los obstáculos.");
        worldKnowledge.Add("sala_arboles", "Los árboles submarinos esconden fragmentos importantes y secretos.");
    }

    // -------------------
    // INITIALIZE HIDDEN FRAGMENTS
    // -------------------
    public void InitializeHiddenFragments()
    {
        hiddenFragments.Clear();
        hiddenFragments.Add(0, "Fíjate bien en el agua alrededor...");
        hiddenFragments.Add(3, "Algo brilla suavemente entre los árboles submarinos...");
        hiddenFragments.Add(7, "Las medusas parecen señalar algo interesante...");
    }

    // -------------------
    // GET GUIDED HINT
    // -------------------
    public string GetGuidedHint(string playerMessage, string placeKey, int playerPlace)
    {
        StringBuilder prompt = new StringBuilder();

        // Pensamiento interno
        internalThought = "Considerando la pregunta del humano y lo que sabe del lugar.";

        // Construye el prompt completo para enviar a la IA
        prompt.AppendLine(BuildMemoryContext());
        prompt.AppendLine(BuildEmotionContext());
        prompt.AppendLine(BuildRelationshipContext());
        prompt.AppendLine(BuildThoughtContext());

        // Añade conocimiento del lugar
        if (!string.IsNullOrEmpty(placeKey) && worldKnowledge.ContainsKey(placeKey))
        {
            prompt.AppendLine("SABER DEL LUGAR: " + worldKnowledge[placeKey]);
        }

        // Añade pistas de fragmentos basadas en HeartController
        prompt.AppendLine(GetFragmentHintFromHeartController(playerPlace));

        // Añade la pregunta del jugador
        prompt.AppendLine("PREGUNTA DEL HUMANO: " + playerMessage);

        // Indicamos que Marina solo responda basándose en la pista real
        prompt.AppendLine("RESPONDE SOLO CON BASE EN LA PISTA REAL Y EL CONOCIMIENTO DEL LUGAR. HABLA COMO UNA AMIGA QUE GUIA, SIN INVENTAR NINGUN LUGAR NI OBJETO. MENCIONA LA SALA Y LO QUE HAY DENTRO.");

        return prompt.ToString();
    }

    // -------------------
    // UPDATE MEMORY AND EMOTION
    // -------------------
    public void UpdateMemoryAndEmotion(string playerMessage)
    {
        AddMemory(playerMessage);

        if (playerMessage.Contains("gracias") || playerMessage.Contains("feliz"))
            currentEmotion = "happy";
        else if (playerMessage.Contains("triste") || playerMessage.Contains("asustado"))
            currentEmotion = "afraid";
        else
            currentEmotion = "curious";

        trust += 0.01f;
        friendship += 0.01f;
        curiosity += 0.01f;

        trust = Mathf.Clamp01(trust);
        friendship = Mathf.Clamp01(friendship);
        curiosity = Mathf.Clamp01(curiosity);
    }

    // -------------------
    // HELPERS
    // -------------------
    public string GetPlaceKey(int placeIndex)
    {
        switch (placeIndex)
        {
            case 0: return "entrada";
            case 1: return "sala_sirenas";
            case 2: return "sala_medusas";
            case 3: return "pasillo";
            case 4: return "sala_arboles";
            default: return "desconocido";
        }
    }

    // -------------------
    // GET FRAGMENT HINT FROM HEARTCONTROLLER
    // -------------------
    public string GetFragmentHintFromHeartController(int playerPlace)
    {
        if (heartController == null)
            return "No puedo ver los fragmentos porque HeartController no está asignado.";

        StringBuilder sb = new StringBuilder();

        string placeKey = GetPlaceKey(playerPlace);
        string placeDescription = worldKnowledge.ContainsKey(placeKey) ? worldKnowledge[placeKey] : "Este lugar parece interesante.";

        // Recorremos los fragmentos
        for (int i = 0; i < heartController._piecesPose.Length; i++)
        {
            int fragmentPos = heartController._piecesPose[i];
            int fragmentPlace = fragmentPos / 4; // dividir por 4 para calcular la "sala"

            if (fragmentPlace == playerPlace)
            {
                if (!heartController._heartAssets[i]._pieceGot)
                {
                    // Construimos la pista **totalmente concreta**
                    sb.AppendLine($"PISTA: En la sala '{placeKey}', {placeDescription} Hay un fragmento de corazón con índice {i}.");
                }
            }
        }

        if (sb.Length == 0)
            sb.AppendLine("No veo ningún fragmento por aquí… sigue buscando.");

        return sb.ToString();
    }
}