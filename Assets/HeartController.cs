using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    public List<int> _piecesPose = new List<int>();
    [System.Serializable]
    public class HeartAssets
    {
        public Image _piece;
        public bool _pieceGot;
    }
    public HeartAssets[] _heartAssets;
    public bool _heartCompleted;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
