﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(RandomGameObjectTable))]
public class RandomGameObject : MonoBehaviour, IRandomGameObject
{
    private void Start()
    {
        //外部调用Generate()方法会销毁本gameobject，便不会执行再start()中的方法，导致双重生成
        Generate();
    }

    public GameObject Generate()
    {
        GameObject go = null;
        GameObject newGameObject = GetComponent<RandomGameObjectTable>().GetGameObject();
        if (newGameObject != null)
        {
            go = GameManager.Instance.level.manager.GenerateGameObjectInCurrentRoom(newGameObject, transform.position);
        }
        Destroy(gameObject);
        return go;
    }
}
