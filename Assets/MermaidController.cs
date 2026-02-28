using UnityEngine;
using UnityEngine.UI;

public class MermaidController : MonoBehaviour
{
    public Animator _mermaidAnimator;

    public Image[] _eyesImage;
    public RectTransform[] targets;     // One target per eye

    public float maxDistance;
    public float _eyesSpeed;

    private RectTransform[] eyeRects;
    private Vector2[] initialPositions;

    [SerializeField] private Image _mouthObject;
    [SerializeField] private Sprite[] _allMouths;
    [SerializeField] private float _blinkTimer;

    void Start()
    {
        int count = _eyesImage.Length;

        eyeRects = new RectTransform[count];
        initialPositions = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            eyeRects[i] = _eyesImage[i].GetComponent<RectTransform>();
            initialPositions[i] = eyeRects[i].anchoredPosition;
        }

        _blinkTimer = Random.Range(5f, 10f);
    }

    void Update()
    {
        for (int i = 0; i < eyeRects.Length; i++)
        {
            if (targets[i] != null)
                MoveEye(eyeRects[i], initialPositions[i], targets[i]);
        }

        HandleBlink();
    }

    void MoveEye(RectTransform eye, Vector2 initialPos, RectTransform target)
    {
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            eye.parent as RectTransform,
            target.position,
            null, // if Screen Space Overlay
            out localPoint
        );

        Vector2 offset = localPoint - initialPos;
        offset = Vector2.ClampMagnitude(offset, maxDistance);

        eye.anchoredPosition = Vector2.Lerp(
            eye.anchoredPosition,
            initialPos + offset,
            _eyesSpeed * Time.deltaTime
        );
    }

    void HandleBlink()
    {
        _blinkTimer -= Time.deltaTime;

        if (_blinkTimer <= 0)
        {
            _mermaidAnimator.SetTrigger("Blink");
            _blinkTimer = Random.Range(5f, 10f);
        }
    }
}