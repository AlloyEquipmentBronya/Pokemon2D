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
    /// �ж���ɫ��Ծ  ��ִ����Ծ
    /// </summary>
    /// <param name="character"></param>
    /// <param name="moveDir"></param>
    /// <returns></returns>
    public bool TryToJump(Character character,Vector2 moveDir)
    {
        //��������ȷʱ
        if (moveDir.x == xDir && moveDir.y == yDir)
        {
            StartCoroutine(Jump(character));
            return true;
        }
        return false;
    }
    /// <summary>
    /// ��ɫ��Ծ
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    IEnumerator Jump(Character character)
    {
        GameControlller.Instance.PauseGame(true);
        character.Animator.IsJumping = true;

        //�����Ծ������
      var jumpDest= character.transform.position + new Vector3(xDir, yDir) * 2;
        //ִ����Ծ���� 
      yield return character.transform.transform.DOJump(jumpDest, 0.3f, 1, 0.5f).WaitForCompletion();

        character.Animator.IsJumping = false;
        GameControlller.Instance.PauseGame(false);
    }
}
