using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarinerFove : MonoBehaviour, IPlayeTrigger
{
    /// <summary>
    /// 进入 训练家视角 触发战斗
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerTriggered(PlayerControl player)
    {
        player.Character.Animator.IsMoving = false;
        GameControlller.Instance.OnEnterTrainerView(GetComponentInParent<TrainerController>());
    }
    public bool TriggerRepeatedly => false;
}
