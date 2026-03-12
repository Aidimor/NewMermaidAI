using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{

    public int[] _piecesPose;
    [System.Serializable]
    public class HeartAssets
    {
        public Image _piece;
        public bool _pieceGot;
    }
    public HeartAssets[] _heartAssets;
    public Animator _heartGotAnimator;
    public bool _heartCompleted;

    // Start is called before the first frame update
    void Start()
    {
        GenerateRandomPieces();
    }

    void GenerateRandomPieces()
    {
        for (int i = 0; i < _piecesPose.Length; i++)
        {
            int randomNumber;

            do
            {
                randomNumber = Random.Range(0, 16); // 0–15
            }
            while (System.Array.IndexOf(_piecesPose, randomNumber) != -1);

            _piecesPose[i] = randomNumber;
        }
    }


}
