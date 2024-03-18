using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarinerFove : MonoBehaviour, IPlayeTrigger
{
    /// <summary>
    /// ���� ѵ�����ӽ� ����ս��
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerTriggered(PlayerControl player)
    {
        player.Character.Animator.IsMoving = false;
        GameControlller.Instance.OnEnterTrainerView(GetComponentInParent<TrainerController>());
    }
    public bool TriggerRepeatedly => false;
}
