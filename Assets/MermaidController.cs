using UnityEngine;
using UnityEngine.UI;

public class MermaidController : MonoBehaviour
{
    [SerializeField] private OpenRouterChat _aiChat;
    public Animator _mermaidAnimator;

    public Image[] _eyesImage;
    public RectTransform[] targets;     // One target per eye

    public float maxDistance;
    public float _eyesSpeed;

    private RectTransform[] eyeRects;
    private Vector2[] initialPositions;

    public Image _mouthObject;
    public Sprite[] _allMouths;
    [SerializeField] private float _blinkTimer;

    [SerializeField] private Vector2[] _mermaidPositions;

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
        MermaidPositionsVoid();
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

    void MermaidPositionsVoid()
    {
        _mermaidAnimator.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_mermaidAnimator.GetComponent<RectTransform>().anchoredPosition, _mermaidPositions[_aiChat.positionState], 3 * Time.deltaTime);
        switch (_aiChat.positionState)
        {
            case 0:
                _mermaidAnimator.GetComponent<RectTransform>().localScale = Vector2.Lerp(_mermaidAnimator.GetComponent<RectTransform>().localScale, new Vector2(1f, 1f), 3 * Time.deltaTime);
                _mermaidAnimator.GetComponent<RectTransform>().rotation = Quaternion.Slerp(_mermaidAnimator.GetComponent<RectTransform>().rotation, Quaternion.Euler(0, 0, 0), 3 * Time.deltaTime);
                break;
            case 1:
                _mermaidAnimator.GetComponent<RectTransform>().localScale = Vector2.Lerp(_mermaidAnimator.GetComponent<RectTransform>().localScale, new Vector2(1f, 1f), 3 * Time.deltaTime);
                _mermaidAnimator.GetComponent<RectTransform>().rotation = Quaternion.Slerp(_mermaidAnimator.GetComponent<RectTransform>().rotation, Quaternion.Euler(0, 0, -90), 3 * Time.deltaTime);
                break;
            case 2:
                _mermaidAnimator.GetComponent<RectTransform>().localScale = Vector2.Lerp(_mermaidAnimator.GetComponent<RectTransform>().localScale, new Vector2(1f, 1f), 3 * Time.deltaTime);
                _mermaidAnimator.GetComponent<RectTransform>().rotation = Quaternion.Slerp(_mermaidAnimator.GetComponent<RectTransform>().rotation, Quaternion.Euler(0, 0, 90), 3 * Time.deltaTime);
                break;
            case 3:
                _mermaidAnimator.GetComponent<RectTransform>().localScale = Vector2.Lerp(_mermaidAnimator.GetComponent<RectTransform>().localScale, new Vector2(1.5f, 1.5f), 3 * Time.deltaTime);
                _mermaidAnimator.GetComponent<RectTransform>().rotation = Quaternion.Slerp(_mermaidAnimator.GetComponent<RectTransform>().rotation, Quaternion.Euler(0, 0, 0), 3 * Time.deltaTime);
                break;
            case 4:
                _mermaidAnimator.GetComponent<RectTransform>().localScale = Vector2.Lerp(_mermaidAnimator.GetComponent<RectTransform>().localScale, new Vector2(1f, 1f), 3 * Time.deltaTime);
                _mermaidAnimator.GetComponent<RectTransform>().rotation = Quaternion.Slerp(_mermaidAnimator.GetComponent<RectTransform>().rotation, Quaternion.Euler(0, 0, 180), 3 * Time.deltaTime);
                break;
            case 5:
                _mermaidAnimator.GetComponent<RectTransform>().localScale = Vector2.Lerp(_mermaidAnimator.GetComponent<RectTransform>().localScale, new Vector2(1f, 1f), 3 * Time.deltaTime);
                _mermaidAnimator.GetComponent<RectTransform>().rotation = Quaternion.Slerp(_mermaidAnimator.GetComponent<RectTransform>().rotation, Quaternion.Euler(0, 0, 0), 3 * Time.deltaTime);
                break;
        }
    }
}