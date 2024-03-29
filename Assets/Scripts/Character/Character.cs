using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    public float moveSpeed;


    public bool IsMoving { get; private set; }

    public float OffsetY { get; private set; } = 0.3f;

    CharacterAnimator animator;

    private void Awake()
    {
        //获取当前游戏对象组件的方法，可以通过直接调用它来访问游戏对象的组件和进行参数调整
        animator = GetComponent<CharacterAnimator>();

        SetPositionAndSnapToTile(transform.position);
    }

    /// <summary>
    /// 设置玩家的位置改变时 重新调整
    /// </summary>
    /// <param name="pos">改变的位置</param>
    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

        //调整后的位置
        transform.position = pos;
    }

    /// <summary>
    /// 移动行为
    /// </summary>
    /// <param name="moveVec">移动位置</param>
    /// <param name="OnMoveOver">移动结束触发事件</param>
    /// <returns></returns>
    public IEnumerator Move(Vector2 moveVec,Action OnMoveOver=null)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x,-1f,1f);
        animator.MoveY = Mathf.Clamp(moveVec.y,-1f,1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        //台阶 跳跃
        var ledge= CheckForLedge(targetPos);
        if(ledge!=null)
        {
            if (ledge.TryToJump(this, moveVec))
                yield break;
        }

        if (!IsPathClear(targetPos))
            yield break;

      IsMoving = true;

        //检测玩家和移动坐标间的差异是否是一个非常小的值
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            //停止协程 并在下一次更新继续
            yield return null;
        }
        transform.position = targetPos;

      IsMoving = false;

        OnMoveOver?.Invoke();
    }


    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }


    /// <summary>
    /// 判定是否存在障碍物
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    private bool IsPathClear(Vector3 targetPos)
    {
        var diff= targetPos - transform.position;
       var dir=  diff.normalized;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1,
             GameLayers.I.SolidObjectsLayer | GameLayers.I.InteractableLayer|GameLayers.I.PlayerLayer)== true)
        {
            return false;
        }
        else
        {
            return true;
        }
      
    }


    private bool IsWalkable(Vector3 targetPos)
    {
        //当玩家的目标位置检测圆形范围内没有solidObjectsLayer   则不能行走
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.I.SolidObjectsLayer | GameLayers.I.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }


    /// <summary>
    /// 检测目标移动位置是不是台阶 返回台阶类组件
    /// </summary>
    /// <param name="targetPos">移动目标位置</param>
    /// <returns></returns>
    Ledge CheckForLedge(Vector3 targetPos)
    {
       var collider=  Physics2D.OverlapCircle(targetPos, 0.15f, GameLayers.I.LedgeLayer);

        return collider?.GetComponent<Ledge>();
    }



    /// <summary>
    /// 朝向目标位置
    /// </summary>
    /// <param name="targertPos">目标位置</param>
    public void LookTowards(Vector3 targertPos)
    {
        var xdiff = Mathf.Floor(targertPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targertPos.y) - Mathf.Floor(transform.position.y);
        //只能在x轴或者y轴上看到玩家
        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
        else
            Debug.LogError("看不到玩家");
    }

    public CharacterAnimator Animator
    {
        get => animator;
    }

}
