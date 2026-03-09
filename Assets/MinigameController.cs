using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MinigameController : MonoBehaviour
{
    [SerializeField] private SpriteMermaidController _scriptMermaid;
    [SerializeField] private OpenRouterChat _aiChat;
    public GameObject _parent;
    public bool _minigameOn;

    [System.Serializable]
    public class GameAssets
    {
        public GameObject _player;
        public Image _basket;
        public Sprite[] _allBasketSprites;
        public Vector2[] _playerPos;
        public GameObject _parent;

        public int _basketID;
        public int _pos;
        public bool _pressed;
        public float _playerSpeed;
        public TextMeshProUGUI _quantityText;
    }

    public GameAssets _gameAssets;

    public float _instantiateTimer;

    [System.Serializable]
    public class ObjectsPool
    {
        public int _totalPool;
        public List<GameObject> _allPrefabs = new List<GameObject>();
        public GameObject[] _prefabs;
    }

    public ObjectsPool _objectsPool;

    public int _totalCatched;

    RectTransform playerRect;

    public static MinigameController instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        playerRect = _gameAssets._player.GetComponent<RectTransform>();

        _instantiateTimer = Random.Range(1f, 2f);

        CreatePool();
    }

    void Update()
    {
        if (_aiChat._onGame)
        {
            StartCoroutine(GameStartsNumerator());
        }

        if (!_minigameOn) return;

        _instantiateTimer -= Time.deltaTime;

        PrefabsVoid();
        BasketControl();
        _gameAssets._basket.GetComponent<RectTransform>().localScale = Vector2.Lerp(_gameAssets._basket.GetComponent<RectTransform>().localScale, new Vector2(1.4f, 1.4f), 4 * Time.deltaTime);

      
    }

    void CreatePool()
    {
        for (int i = 0; i < _objectsPool._totalPool; i++)
        {
            GameObject obj = Instantiate(
                _objectsPool._prefabs[0],
                transform.position,
                transform.rotation,
                _gameAssets._parent.transform
            );

            obj.SetActive(false);

            _objectsPool._allPrefabs.Add(obj);
        }
    }

    GameObject GetPoolObject()
    {
        for (int i = 0; i < _objectsPool._allPrefabs.Count; i++)
        {
            if (!_objectsPool._allPrefabs[i].activeInHierarchy)
            {
                return _objectsPool._allPrefabs[i];
            }
        }

        return null;
    }

    void PrefabsVoid()
    {
        if (_instantiateTimer > 0) return;

        GameObject prefab = GetPoolObject();

        if (prefab == null)
        {
            _instantiateTimer = Random.Range(1f, 2f);
            return;
        }

        GamePrefab gp = prefab.GetComponent<GamePrefab>();

        gp._sprite[0].SetActive(false);
        gp._sprite[1].SetActive(false);

        int randomLane = Random.Range(0, 3);
        int randomType = Random.Range(0, 100);

        int ID = (randomType >= 60) ? 0 : 1;

        prefab.SetActive(true);

        gp._id = ID;
        gp._sprite[ID].SetActive(true);

        RectTransform rect = prefab.GetComponent<RectTransform>();

        rect.localScale = new Vector2(3, 3);
        rect.anchoredPosition = new Vector2(_gameAssets._playerPos[randomLane].x, 720f);

        gp._fallSpeed = Random.Range(2f, 5f);

        _instantiateTimer = Random.Range(1f, 2f);
    }

    public void BasketControl()
    {
        Vector2 targetPos = new Vector2(
            _gameAssets._playerPos[_gameAssets._pos].x,
            playerRect.anchoredPosition.y
        );

        playerRect.anchoredPosition = Vector2.MoveTowards(
            playerRect.anchoredPosition,
            targetPos,
            _gameAssets._playerSpeed * Time.deltaTime
        );

        _gameAssets._basket.sprite = _gameAssets._allBasketSprites[_gameAssets._basketID];

        float horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal < 0 && _gameAssets._pos > 0 && !_gameAssets._pressed)
        {
            _gameAssets._pos--;
            _gameAssets._pressed = true;
        }

        if (horizontal > 0 && _gameAssets._pos < 2 && !_gameAssets._pressed)
        {
            _gameAssets._pos++;
            _gameAssets._pressed = true;
        }

        if (horizontal == 0)
        {
            _gameAssets._pressed = false;
        }
    }

    public void CatchedVoid()
    {
        _totalCatched++;
        _gameAssets._quantityText.text = _totalCatched.ToString("f0");
    }

    public IEnumerator GameStartsNumerator()
    {
        _aiChat._onGame = false;
        _scriptMermaid._bubbleParticle.Play();
        yield return new WaitForSeconds(0.75f);
        _scriptMermaid._mainAnimator.gameObject.SetActive(false);
        _aiChat.inputField.gameObject.SetActive(false);
        _parent.SetActive(true);
    }

}