using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpriteMermaidController : MonoBehaviour
{
    [SerializeField] private OpenRouterChat _aiScript;

    [System.Serializable]
    public class MermaidSpriteAssets
    {
        public string _reaction;
        public Sprite _openEyes;    
        public Image _openMouth;
        public Image _closedMouth;
    }

    public MermaidSpriteAssets[] _mermaidSpriteAssets;

    [SerializeField] private float _blinkTimer;

    public int _mermaidID;

    public Image _mainSprite;
    public Animator _mainAnimator;
    public Animator _mouthAnimator;

    [System.Serializable]
    public class MapAssets
    {
        public GameObject Parent;
        public Image[] _positions;
    }
    public MapAssets _mapAssets;
    public ParticleSystem _bubbleParticle;

    void Start()
    {
        ChangeMermaidImage();
        _blinkTimer = Random.Range(5f, 10f);
    }

    void Update()
    {
        //HandleBlink();
    }

    void HandleBlink()
    {
        _blinkTimer -= Time.deltaTime;

        if (_blinkTimer <= 0)
        {
            StartCoroutine(BlinkNumerator());
            _blinkTimer = Random.Range(5f, 10f);
        }
    }

    IEnumerator BlinkNumerator()
    {
        //_mainSprite.sprite = _mermaidSpriteAssets[_mermaidID]._closedEyes;

        yield return new WaitForSeconds(0.15f);

        _mainSprite.sprite = _mermaidSpriteAssets[_mermaidID]._openEyes;
    }

    public void ChangeMermaidImage()
    {
        _mermaidID = _aiScript.faceState;

        _mainSprite.sprite = _mermaidSpriteAssets[_mermaidID]._openEyes;

        for (int i = 0; i < _mermaidSpriteAssets.Length; i++)
        {
            _mermaidSpriteAssets[i]._openMouth.gameObject.SetActive(false);
            _mermaidSpriteAssets[i]._closedMouth.gameObject.SetActive(false);
        }

        _mermaidSpriteAssets[_mermaidID]._openMouth.gameObject.SetActive(true);
        _mermaidSpriteAssets[_mermaidID]._closedMouth.gameObject.SetActive(true);
    }
}