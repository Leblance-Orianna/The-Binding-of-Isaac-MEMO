﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 游戏过程中生成物体的统一入口
/// </summary>
public class GameItemManager : MonoBehaviour
{
    private Level level;
    private Room currentRoom { get { return level.currentRoom; } }

    private bool isScanningGridGraph = false;

    private void Awake()
    {
        level = GetComponent<Level>();
    }

    /// <summary>
    /// 使用具体位置在当前房间生成游戏物体
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject GenerateGameObjectInCurrentRoom(GameObject prefab, Vector2 position)
    {
        //物体生成后所归属的默认节点
        Transform container = currentRoom.defaultContainer;

        //尝试获取物体类别，不为空便设置到对应节点
        GameItem gameItem = prefab.GetComponent<GameItem>();
        if (gameItem != null)
        {
            switch (gameItem.gameItemType)
            {
                case GameItemType.Prop:
                    container = currentRoom.propContainer;
                    break;
                case GameItemType.Monster:
                    container = currentRoom.monsterContainer;
                    break;
                case GameItemType.Obstacles:
                    container = currentRoom.obstaclesContainer;
                    //配合延迟更新形成消息等待机制
                    if (!isScanningGridGraph && this.gameObject.activeSelf)
                    {
                        isScanningGridGraph = true;
                        StartCoroutine(DeleyScanGridGraph());
                    }
                    break;
                default:
                    break;
            }
        }

        return currentRoom.GenerateGameObjectWithPosition(prefab, position, container);
    }
    /// <summary>
    /// 使用具体位置在当前房间生成游戏物体
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public T GenerateGameObjectInCurrentRoom<T>(T prefab, Vector2 position) where T : MonoBehaviour
    {
        //物体生成后所归属的默认节点
        Transform container = currentRoom.defaultContainer;

        //尝试获取物体类别，不为空便设置到对应节点
        GameItem gameItem = prefab.GetComponent<GameItem>();
        if (gameItem != null)
        {
            switch (gameItem.gameItemType)
            {
                case GameItemType.Prop:
                    container = currentRoom.propContainer;
                    break;
                case GameItemType.Monster:
                    container = currentRoom.monsterContainer;
                    break;
                case GameItemType.Obstacles:
                    container = currentRoom.obstaclesContainer;
                    //配合延迟更新形成消息等待机制
                    if (!isScanningGridGraph && this.gameObject.activeSelf)
                    {
                        isScanningGridGraph = true;
                        StartCoroutine(DeleyScanGridGraph());
                    }
                    break;
                default:
                    break;
            }
        }
        return currentRoom.GenerateGameObjectWithPosition(prefab.gameObject, position, container).GetComponent<T>();
    }

    /// <summary>
    /// 使用具体位置在当前房间生成痕迹
    /// </summary>
    /// <param name="traceSprite"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject GenerateTraceInCurrentRoom(Sprite traceSprite, Vector2 position)
    {
        return currentRoom.GenerateTrace(traceSprite, position);
    }

    public void GenerateTracesInCurrentRoom(List<Sprite> traceSprites, int num, Vector2 position, float maxDirection)
    {
        currentRoom.GenerateTraces(traceSprites, num, position, maxDirection);
    }

    public void DestroyGameObject(GameObject gameObject)
    {
        GameItem gameItem = gameObject.GetComponent<GameItem>();
        if (gameItem != null)
        {
            switch (gameItem.gameItemType)
            {
                case GameItemType.Monster:
                    if (currentRoom.gameObject.activeSelf)
                    {
                        //配合Room类在OnDestroy()内关闭自身，避免关闭游戏时无效调用
                        currentRoom.CheckOpenDoor();
                    }
                    break;
                case GameItemType.Obstacles:
                    //配合延迟机制避免同一帧内触发大量更新
                    if (!isScanningGridGraph && this.gameObject.activeSelf)
                    {
                        isScanningGridGraph = true;
                        StartCoroutine(DeleyScanGridGraph());
                    }
                    break;
                default:
                    break;
            }
        }
    }

    //延迟一帧更新网格
    private IEnumerator DeleyScanGridGraph()
    {
        yield return null;
        AstarPath.active.Scan();
        //while (AstarPath.active.isScanning) { yield return null; }
        isScanningGridGraph = false;
    }
}