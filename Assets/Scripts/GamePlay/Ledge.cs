using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ledge : MonoBehaviour
{
    [SerializeField] int xDir;
    [SerializeField] int yDir;


    private void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    /// <summary>
    /// 判定角色跳跃  能执行跳跃
    /// </summary>
    /// <param name="character"></param>
    /// <param name="moveDir"></param>
    /// <returns></returns>
    public bool TryToJump(Character character,Vector2 moveDir)
    {
        //当方向正确时
        if (moveDir.x == xDir && moveDir.y == yDir)
        {
            StartCoroutine(Jump(character));
            return true;
        }
        return false;
    }
    /// <summary>
    /// 角色跳跃
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    IEnumerator Jump(Character character)
    {
        GameControlller.Instance.PauseGame(true);
        character.Animator.IsJumping = true;

        //获得跳跃的坐标
      var jumpDest= character.transform.position + new Vector3(xDir, yDir) * 2;
        //执行跳跃动画 
      yield return character.transform.transform.DOJump(jumpDest, 0.3f, 1, 0.5f).WaitForCompletion();

        character.Animator.IsJumping = false;
        GameControlller.Instance.PauseGame(false);
    }
}
