using UnityEngine;
using TMPro;

public class TMPWaveAndFall : MonoBehaviour
{
    public TMP_Text tmp;

    [Header("Wave Settings")]
    public float amplitude = 5f;
    public float frequency = 2f;

    [Header("Fall Settings")]
    public float delayBeforeFall = 2f;
    public float fallSpeed = 40f;
    public float fadeSpeed = 1.5f;
    public float letterDelay = 0.1f;

    private TMP_TextInfo textInfo;
    private Vector3[][] originalVertices;

    private float[] fallOffsets;
    private float[] alphaValues;

    private int charCount;
    private float startTime;

    void Awake()
    {
        if (tmp == null)
            tmp = GetComponent<TMP_Text>();
    }

    void Start()
    {
        Initialize();
    }

    public void RestartAnimation()
    {
        Initialize();
    }

    void Initialize()
    {
        tmp.ForceMeshUpdate();
        textInfo = tmp.textInfo;

        charCount = textInfo.characterCount;

        originalVertices = new Vector3[textInfo.meshInfo.Length][];

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVertices[i] = textInfo.meshInfo[i].vertices.Clone() as Vector3[];
        }

        fallOffsets = new float[charCount];
        alphaValues = new float[charCount];

        for (int i = 0; i < charCount; i++)
            alphaValues[i] = 1f;

        startTime = Time.time;
    }

    void Update()
    {
        if (tmp == null)
            return;

        tmp.ForceMeshUpdate();
        textInfo = tmp.textInfo;

        if (textInfo.characterCount != charCount)
        {
            Initialize();
            return;
        }

        float time = Time.time - startTime;

        for (int i = 0; i < charCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

            Vector3[] sourceVertices = originalVertices[materialIndex];

            // Restaurar posición base primero (MUY IMPORTANTE)
            for (int j = 0; j < 4; j++)
                vertices[vertexIndex + j] = sourceVertices[vertexIndex + j];

            // ---------- WAVE ----------
            float waveOffset = Mathf.Sin(Time.time * frequency + i * 0.5f) * amplitude;

            // ---------- FALL ----------
            float fallOffset = 0f;

            if (time > delayBeforeFall + i * letterDelay)
            {
                fallOffsets[i] += fallSpeed * Time.deltaTime;
                alphaValues[i] -= fadeSpeed * Time.deltaTime;

                fallOffset = fallOffsets[i];

                byte alpha = (byte)(Mathf.Clamp01(alphaValues[i]) * 255);

                for (int j = 0; j < 4; j++)
                    colors[vertexIndex + j].a = alpha;
            }

            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j].y += waveOffset;
                vertices[vertexIndex + j].y -= fallOffset;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
            tmp.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}