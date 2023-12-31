﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Room : MonoBehaviour
{
    #region 房间属性
    [Header("房间属性")]

    [HideInInspector]
    public RoomType roomType;//房间类型

    [HideInInspector]
    public Vector2 coordinate;//坐标

    private RoomLayout roomLayout;//布局文件

    public bool isArrived = false;//是否已到达
    public bool isCleared = false;//是否已清理过

    //房间宽高，像素值/100
    public static float RoomWidth { get { return 4.68f; } }
    public static float RoomHeight { get { return 3.12f; } }
    public static int RoomWidePixels { get { return 468; } }
    public static int RoomHighPixels { get { return 312; } }

    //单位数量和大小
    public static int HorizomtalUnit { get { return 13; } }
    public static int VerticalUnit { get { return 7; } }
    public static float UnitSize { get { return 0.28f; } }

    #endregion

    #region 房间组成
    [Header("房间组成")]

    [SerializeField]
    private Transform fllor;//地板节点

    public List<GameObject> doorList;//门列表
    public int ActiveDoorCount { get { return activeDoorList.Count; } }
    private List<GameObject> activeDoorList = new List<GameObject>();
    private List<GameObject> neighboringDoorList = new List<GameObject>();

    private List<Room> neighboringRoomList = new List<Room>();//相邻的房间
    #endregion

    #region 房间下属节点
    [Header("房间下属节点")]
    public Transform monsterContainer;
    public Transform obstaclesContainer;
    public Transform propContainer;
    public Transform traceContainer;
    public Transform defaultContainer;
    #endregion

    #region 房间外观
    [Header("房间外观")]

    [SerializeField]
    private Sprite bossDoorHole;
    [SerializeField]
    private Sprite bossDoorFrame;
    [SerializeField]
    private Sprite treasureDoorFrame;
    #endregion

    #region 其他
    private Level level;//关卡
    #endregion

    private void Awake()
    {
        level = GameManager.Instance.level;
    }

    /// <summary>
    /// 激活对应方向的门
    /// </summary>
    /// <param name="didirection"></param>
    public void ActivationDoor(Didirection didirection, Room neighboringRoom, GameObject neighboringDoor)
    {
        GameObject door = doorList[(int)didirection];

        for (int i = 0; i < door.transform.childCount; i++)
        {
            door.transform.GetChild(i).gameObject.SetActive(true);
        }
        activeDoorList.Add(door);
        neighboringRoomList.Add(neighboringRoom);
        neighboringDoorList.Add(neighboringDoor);
    }

    /// <summary>
    /// 打开所有已激活的门
    /// </summary>
    public void OpenActivatedDoor()
    {
        for (int i = 0; i < activeDoorList.Count; i++)
        {
            GameObject door = activeDoorList[i];
            door.GetComponent<BoxCollider2D>().enabled = false;
            door.GetComponent<Animator>().Play("Open");
        }
    }

    /// <summary>
    /// 获取布局文件并初始化
    /// </summary>
    /// <param name="type"></param>
    public void Initialize()
    {
        roomLayout = level.pools.GetRoomLayout(roomType);

        for (int i = 0; i < fllor.childCount; i++)
        {
            fllor.GetChild(i).GetComponent<SpriteRenderer>().sprite = roomLayout.floor;
        }

        ChangeDoorOutWard();
    }
    /// <summary>
    /// 改变该房间和相邻房间门的样式
    /// </summary>
    private void ChangeDoorOutWard()
    {
        if (roomType == RoomType.Normal || roomType == RoomType.Start) { return; }

        List<GameObject> doors = new List<GameObject>();
        doors.AddRange(activeDoorList);
        doors.AddRange(neighboringDoorList);

        foreach (var door in doors)
        {
            SpriteRenderer doorFrame = door.transform.GetChild(0).GetComponent<SpriteRenderer>();
            SpriteRenderer doorHole = door.transform.GetChild(1).GetComponent<SpriteRenderer>();
            switch (roomType)
            {
                case RoomType.Boss:
                    doorFrame.sprite = bossDoorFrame;
                    doorHole.sprite = bossDoorHole;
                    break;
                case RoomType.Treasure:
                    doorFrame.sprite = treasureDoorFrame;
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 根据房间布局文件roomLayout 生成内容
    /// </summary>
    public void GenerateRoomContent(float delaySeconds)
    {
        if (roomLayout.tip != null) { GenerateTrace(roomLayout.tip, Vector2.zero); }

        for (int i = 0; i < roomLayout.obstacleList.Count; i++)
        {
            GenerateGameObjectWithCoordinate(roomLayout.obstacleList[i].value1, roomLayout.obstacleList[i].value2, obstaclesContainer);
        }

        for (int i = 0; i < roomLayout.propList.Count; i++)
        {
            GenerateGameObjectWithCoordinate(roomLayout.propList[i].value1, roomLayout.propList[i].value2, propContainer);
        }

        for (int i = 0; i < roomLayout.monsterList.Count; i++)
        {
            GenerateGameObjectWithCoordinate(roomLayout.monsterList[i].value1, roomLayout.monsterList[i].value2, monsterContainer);
        }

        CheckOpenDoor();
    }

    /// <summary>
    /// 检查开门
    /// </summary>
    public void CheckOpenDoor()
    {
        StartCoroutine(DelayCheck());
    }
    /// <summary>
    /// 延迟检查
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayCheck()
    {
        yield return null;
        if (monsterContainer.childCount == 0 && !isCleared)
        {
            OpenActivatedDoor();
            if (roomLayout.isGenerateReward) { GenerateRoomClearingReward(); }
            isCleared = true;
        }
    }
    /// <summary>
    /// 生成清理房间的奖励品
    /// </summary>
    private void GenerateRoomClearingReward()
    {
        GameObject reward = level.pools.GetRoomClearingReward(roomType);
        GenerateGameObjectWithCoordinate(reward, roomLayout.RewardPosition, propContainer);
    }

    /// <summary>
    /// 在房间里生成痕迹
    /// </summary>
    public GameObject GenerateTrace(Sprite traceSprite, Vector2 position)
    {
        GameObject trace = new GameObject(traceSprite.ToString());
        SpriteRenderer sr = trace.AddComponent<SpriteRenderer>();
        sr.sprite = traceSprite;
        sr.sortingOrder = GameDate.RENDERERORDER_TRACES;
        sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        trace.transform.SetParent(traceContainer);
        trace.transform.position = position;
        return trace;
    }

    /// <summary>
    /// 在房间里生成多个痕迹并移动位置
    /// </summary>
    public void GenerateTraces(List<Sprite> traceSprites, int num, Vector2 position, float maxDirection)
    {
        for (int i = 0; i < num; i++)
        {
            Sprite sprite = traceSprites[UnityEngine.Random.Range(0, traceSprites.Count)];
            GameObject trace = GenerateTrace(sprite, position);
            StartCoroutine(Flop(trace, maxDirection));
        }
    }
    IEnumerator Flop(GameObject gameObject, float maxDirection)
    {
        Vector2 start = gameObject.transform.position;
        Vector2 direction = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
        Vector2 distance = direction * UnityEngine.Random.Range(0, maxDirection);
        Vector2 end = start + distance;

        float time = 0;
        float endTime = 0.4f * distance.magnitude;
        while (time < endTime)
        {
            gameObject.transform.position = Vector3.Lerp(start, end, (1f / endTime) * (time += Time.deltaTime));
            yield return null;
        }
    }

    /// <summary>
    /// 使用具体位置在房间里生成单个物体
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <param name="container"></param>
    /// <returns></returns>
    public GameObject GenerateGameObjectWithPosition(GameObject prefab, Vector2 position, Transform container)
    {
        if (prefab == null) { return null; }

        GameObject go = Instantiate(prefab, container);
        go.transform.position = position;
        return go;
    }

    /// <summary>
    /// 使用坐标在房间里生成单个物体
    /// </summary>
    private GameObject GenerateGameObjectWithCoordinate(GameObject prefab, Vector2 coordinate, Transform container)
    {
        if (prefab == null) { return null; }

        GameObject go = Instantiate(prefab, container);
        //Unit数量为 x:13，y:7,每单位大小为UnitSize
        //coordinate范围为 x:1-25，y:1-13,每单位大小为UnitSize / 2
        //以房间中心为原点，左上角为coordinate起始点，进行位置计算
        Vector2 postiton = new Vector2(-(HorizomtalUnit - coordinate.x) * UnitSize / 2, (VerticalUnit - coordinate.y) * UnitSize / 2);
        go.transform.localPosition = postiton;

        return go;
    }


    private void OnDestroy()
    {
        gameObject.SetActive(false);
    }
}
