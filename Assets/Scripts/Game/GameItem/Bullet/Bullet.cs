﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Bullet : MonoBehaviour
{
    bool isDestory;
    float damage;
    float playerKnockback;
    float fallingDistance = -0.06f;//每秒自然下落的距离

    Rigidbody2D myRigidbody;
    Collider2D myCollider;
    Animator animator;

    Player player;
    BulletPools bulletPool;

    //回收到对象池的时间
    WaitForSeconds WaitSeconds = new WaitForSeconds(2f);

    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        player = GameManager.Instance.player;
        bulletPool = player.bulletPool;
    }

    void Update()
    {
        //模拟子弹自然下落
        if (!isDestory) { transform.Translate(0, fallingDistance * Time.deltaTime, 0, Space.World); }
    }

    /// <summary>
    /// 初始化,赋予伤害,击退力,射程
    /// </summary>
    public void Initialization()
    {
        isDestory = false;
        damage = player.Damage;
        playerKnockback = player.Knockback;
        //子弹一段时间后触发自动销毁
        Invoke("AutoDestroy", player.Range * 0.03f);
    }

    /// <summary>
    /// 自动销毁
    /// </summary>
    void AutoDestroy()
    {
        if (isDestory) { return; }
        //子弹快速下落一小段时间后销毁
        myRigidbody.gravityScale = 1.7f;
        Invoke("Destroy", 0.13f);
    }

    /// <summary>
    /// 销毁
    /// </summary>
    void Destroy()
    {
        //关闭重力，停止移动，关闭碰撞体，播放消失动画,返回对象池
        isDestory = true;
        myRigidbody.gravityScale = 0;
        myRigidbody.velocity = Vector2.zero;
        myCollider.enabled = false;
        animator.Play("Destroy");
        StartCoroutine(GoBackToPool());
    }

    /// <summary>
    /// 回收到对象池
    /// </summary>
    /// <returns></returns>
    IEnumerator GoBackToPool()
    {
        yield return WaitSeconds;
        transform.position = Vector2.zero;
        myCollider.enabled = true;
        animator.Play("Idle");
        animator.Update(0);
        bulletPool.Back(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //接触后消除眼泪，不触发其他方法
        if (CommonUnit.TagCheck(collision.gameObject, player.TagThatDefaultByBullet) || CommonUnit.ComponentCheck(collision.gameObject, player.TypeThatDefaultByBullet))
        {
            Destroy();
        }

        if (CommonUnit.TagCheck(collision.gameObject, new string[] { }))
        {

        }
        //接触后消除眼泪并触发对象的受击方法
        else if (CommonUnit.ComponentCheck(collision.gameObject, player.TypeThatCanBeAttackedByBullet))
        {
            Vector3 force = Vector3.Normalize(collision.transform.position - transform.position) * playerKnockback;
            IAttackable iAttackable = collision.GetComponent<IAttackable>();
            iAttackable.BeAttacked(damage, force);
            if (player.penetrating == false)
            {
                Destroy();
            }
        }
        //接触后消除眼泪并触发对象的销毁方法
        else if (CommonUnit.ComponentCheck(collision.gameObject, player.TypeThatCanBeDestroyedByBullet))
        {
            IDestructible destructible = collision.GetComponent<IDestructible>();
            destructible.DestorySelf();
            if (player.penetrating == false)
            {
                Destroy();
            }
        }
        //其他的接触后无反应
    }
}
