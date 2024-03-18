using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest 
{
    public QuestBase Base { get; private set; }

    public QuestStatus Status { get; private set; }

    public Quest(QuestBase _base)
    {
        Base = _base;
    }
    public Quest(QuestSavaData savaData)
    {
        Base = QuestDB.GetObjectByName(savaData.name);
        Status = savaData.status;
    }

    public QuestSavaData GetSaveData()
    {
        var saveData = new QuestSavaData
        {
            name = Base.name,
            status = Status
        };
        return saveData;
    }


    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;

        yield return DialogManager.Instance.ShowDialog(Base.StartDialogue);

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }
    /// <summary>
    /// 任务完成行为
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public IEnumerator CompleteQuest(Transform player)
    {
        Status = QuestStatus.Completed;

        yield return DialogManager.Instance.ShowDialog(Base.CompletedDialogue);
        //当有任务物品时移除物品
        var inventory = Inventory.GetInventory();
        if(Base.RequiredItem!=null)
        {
            inventory.RemoveItem(Base.RequiredItem);
        }
        //如果有奖励 给予玩家物品
        if (Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);
            string playerName = player.GetComponent<PlayerControl>().Name;
            AudioManager.i.PlaySfx(AudioId.ItemObtained, pauseMusic: true);
            yield return DialogManager.Instance.ShowDialogText($"{playerName}获得了{Base.RewardItem.Name}");
        }

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }
   /// <summary>
   /// 是否完成任务
   /// </summary>
   /// <returns></returns>
    public bool CanBeCompleted()
    {
        var inventory = Inventory.GetInventory();
        if(Base.RequiredItem!=null)
        {
            if (!inventory.HasItem(Base.RequiredItem))
                return false;
        }
        return true;
    }

}
[System.Serializable]
public class QuestSavaData
{
    public string name;
    public QuestStatus status;
}

public enum QuestStatus { None,Started,Completed}
