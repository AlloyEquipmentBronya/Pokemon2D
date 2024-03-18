using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayeTrigger
{
    
    /// <summary>
    /// 触发野生宝可梦战斗
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerTriggered(PlayerControl player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            {
                player.Character.Animator.IsMoving = false;
                GameControlller.Instance.StartBattle();
            }
        }
    }
    public bool TriggerRepeatedly => true;

}
