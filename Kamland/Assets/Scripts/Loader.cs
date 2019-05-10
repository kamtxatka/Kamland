using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    [SerializeField] GameObject gameManager;

    void Awake()
    {
        if (GameManager.instance == null)
            Instantiate(gameManager);
    }
}
