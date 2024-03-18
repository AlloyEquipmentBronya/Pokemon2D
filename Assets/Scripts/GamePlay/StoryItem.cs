using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryItem : MonoBehaviour, IPlayeTrigger
{
    [SerializeField] Dialog dialog;

    public void OnPlayerTriggered(PlayerControl player)
    {
        player.Character.Animator.IsMoving = false;
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
    }
    public bool TriggerRepeatedly => false;
}
