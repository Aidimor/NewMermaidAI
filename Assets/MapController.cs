using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    [SerializeField] private OpenRouterChat _aiScript;
    public int _buttonID;
    public int _stoneID;
    public Image[] _allButtonsImage;
    public Image[] _subButtonsImage;
    public Color[] _buttonColors;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonMapVoid(int id)
    {
        _buttonID = id;
        _aiScript.onPlace = id;
        for (int i = 0; i < _allButtonsImage.Length; i++)
        {
            _allButtonsImage[i].color = _buttonColors[0];
        }
        _allButtonsImage[_buttonID].color = _buttonColors[1];
    }

    public void ButtonStoneVoid(int id)
    {
        _stoneID = id;
        _aiScript._onStone = id;
        for (int i = 0; i < _subButtonsImage.Length; i++)
        {
            _subButtonsImage[i].color = _buttonColors[0];
        }
        _subButtonsImage[_stoneID].color = _buttonColors[1];
    }
}
