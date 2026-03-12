using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    [SerializeField] private OpenRouterChat _aiScript;
    [SerializeField] private HeartController _scriptHeart;

    public int _buttonID;
    public int _stoneID;

    public Image[] _allButtonsImage;
    public Image[] _subButtonsImage;
    public Color[] _buttonColors;

    // Fragmento detectado
    private int _detectedFragmentIndex = -1;
    private int _detectedFragmentNumber = -1;

    void Start()
    {

    }

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

        DetectHeartPiece(); // SOLO detectar
    }

    // ------------------------------------------------
    // DETECTAR FRAGMENTO (NO ELIMINA)
    // ------------------------------------------------
    void DetectHeartPiece()
    {
        int index = _buttonID * 4 + _stoneID;

        if (index < 0 || index >= 16)
            return;

        int fragmentNumber = System.Array.IndexOf(_scriptHeart._piecesPose, index);

        // SI NO HAY FRAGMENTO AQUI
        if (fragmentNumber == -1)
            return;

        // SI YA FUE RECOGIDO, NO HACER NADA
        if (_scriptHeart._heartAssets[fragmentNumber]._pieceGot)
            return;

        // SI YA ESTA DETECTADO Y PENDIENTE, NO REPETIR
        if (_detectedFragmentNumber == fragmentNumber)
            return;

        _detectedFragmentIndex = index;
        _detectedFragmentNumber = fragmentNumber;

        _aiScript._pieceObtained = fragmentNumber;

        Debug.Log("ESTAS SOBRE EL FRAGMENTO #" + (fragmentNumber + 1));

        _aiScript._onHeartFragment = true;
    }
    // ------------------------------------------------
    // RECOGER FRAGMENTO (DESPUES DEL DIALOGO)
    // ------------------------------------------------
    public void CollectHeartPiece()
    {
        if (_detectedFragmentIndex == -1)
            return;

        Debug.Log("RECOGIENDO FRAGMENTO #" + _detectedFragmentNumber);

        _scriptHeart._heartAssets[_detectedFragmentIndex]._pieceGot = true;

        _detectedFragmentIndex = -1;
        _detectedFragmentNumber = -1;
    }
    // ------------------------------------------------
    // SABER CUAL FRAGMENTO ES
    // ------------------------------------------------
    public int GetFragmentNumber()
    {
        return _detectedFragmentNumber;
    }

    // ------------------------------------------------
    // SABER SI HAY UNO DETECTADO
    // ------------------------------------------------
    public bool HasFragmentDetected()
    {
        return _detectedFragmentIndex != -1;
    }
}