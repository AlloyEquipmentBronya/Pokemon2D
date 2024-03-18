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
        //��ȡ��ǰ��Ϸ��������ķ���������ͨ��ֱ�ӵ�������������Ϸ���������ͽ��в�������
        animator = GetComponent<CharacterAnimator>();

        SetPositionAndSnapToTile(transform.position);
    }

    /// <summary>
    /// ������ҵ�λ�øı�ʱ ���µ���
    /// </summary>
    /// <param name="pos">�ı��λ��</param>
    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

        //�������λ��
        transform.position = pos;
    }

    /// <summary>
    /// �ƶ���Ϊ
    /// </summary>
    /// <param name="moveVec">�ƶ�λ��</param>
    /// <param name="OnMoveOver">�ƶ����������¼�</param>
    /// <returns></returns>
    public IEnumerator Move(Vector2 moveVec,Action OnMoveOver=null)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x,-1f,1f);
        animator.MoveY = Mathf.Clamp(moveVec.y,-1f,1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        //̨�� ��Ծ
        var ledge= CheckForLedge(targetPos);
        if(ledge!=null)
        {
            if (ledge.TryToJump(this, moveVec))
                yield break;
        }

        if (!IsPathClear(targetPos))
            yield break;

      IsMoving = true;

        //�����Һ��ƶ������Ĳ����Ƿ���һ���ǳ�С��ֵ
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            //ֹͣЭ�� ������һ�θ��¼���
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
    /// �ж��Ƿ�����ϰ���
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
        //����ҵ�Ŀ��λ�ü��Բ�η�Χ��û��solidObjectsLayer   ��������
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.I.SolidObjectsLayer | GameLayers.I.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }


    /// <summary>
    /// ���Ŀ���ƶ�λ���ǲ���̨�� ����̨�������
    /// </summary>
    /// <param name="targetPos">�ƶ�Ŀ��λ��</param>
    /// <returns></returns>
    Ledge CheckForLedge(Vector3 targetPos)
    {
       var collider=  Physics2D.OverlapCircle(targetPos, 0.15f, GameLayers.I.LedgeLayer);

        return collider?.GetComponent<Ledge>();
    }



    /// <summary>
    /// ����Ŀ��λ��
    /// </summary>
    /// <param name="targertPos">Ŀ��λ��</param>
    public void LookTowards(Vector3 targertPos)
    {
        var xdiff = Mathf.Floor(targertPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targertPos.y) - Mathf.Floor(transform.position.y);
        //ֻ����x�����y���Ͽ������
        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
        else
            Debug.LogError("���������");
    }

    public CharacterAnimator Animator
    {
        get => animator;
    }

}
