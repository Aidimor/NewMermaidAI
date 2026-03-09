using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePrefab : MonoBehaviour
{
    public float _fallSpeed;
    public GameObject[] _sprite;
    public int _id;

    void Update()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            transform.position + Vector3.down,
            _fallSpeed * Time.deltaTime
        );

        if (GetComponent<RectTransform>().anchoredPosition.y <= -1000f)
        {
            ResetPrefab();
        }
    }

    void ResetPrefab()
    {
        gameObject.SetActive(false);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 7)
        {
            MinigameController.instance._gameAssets._basket.GetComponent<RectTransform>().localScale = new Vector2(1.5f, 1.5f);
            ResetPrefab();
            MinigameController.instance.CatchedVoid();

            if(MinigameController.instance._totalCatched > 5 && MinigameController.instance._totalCatched < 10)
            {
                MinigameController.instance._gameAssets._basketID = 1;
            }
            else if(MinigameController.instance._totalCatched > 10)
            {
                MinigameController.instance._gameAssets._basketID = 2;
            }
            else if(MinigameController.instance._totalCatched < 5)
            {
                MinigameController.instance._gameAssets._basketID = 0;
            }
        }        
    }
}