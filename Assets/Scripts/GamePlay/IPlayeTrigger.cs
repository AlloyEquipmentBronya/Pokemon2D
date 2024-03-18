using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayeTrigger 
{
    void OnPlayerTriggered(PlayerControl player);

    bool TriggerRepeatedly { get; }
}
