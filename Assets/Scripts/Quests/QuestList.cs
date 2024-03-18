using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestList : MonoBehaviour,ISavable
{
    List<Quest> quests = new List<Quest>();

    public event Action onUpdated;

    public void AddQuest(Quest quest)
    {
        if (!quests.Contains(quest))
            quests.Add(quest);

        onUpdated?.Invoke();
    }
    /// <summary>
    /// 通过任务名字 判断是否任务开始
    /// </summary>
    /// <param name="questName"></param>
    /// <returns></returns>
    public bool IsStarted(string questName)
    {
        var questStatus = quests.FirstOrDefault(q => q.Base.Name == questName)?.Status;
        return questStatus == QuestStatus.Started || questStatus == QuestStatus.Completed;
    }
    /// <summary>
    /// 通过任务名字 判断是否任务完成
    /// </summary>
    /// <param name="questName"></param>
    /// <returns></returns>
    public bool IsCompleted(string questName)
    {
        var questStatus = quests.FirstOrDefault(q => q.Base.Name == questName)?.Status;
        return  questStatus == QuestStatus.Completed;
    }

    /// <summary>
    /// 返回PlayerController的QuestList组件
    /// </summary>
    /// <returns></returns>
    public static QuestList GetQuestList()
    {
        return FindObjectOfType<PlayerControl>().GetComponent<QuestList>();
    }

    public object CaptureState()
    {
        //将List<Quest> 的数据转换成QuestSavaData
       return quests.Select(q => q.GetSaveData()).ToList();
    }

    public void RestoreState(object state)
    {
        var saveData = state as List<QuestSavaData>;
        if(saveData!=null)
        {
            //将QuestSavaData 还原成 Quest list
            quests = saveData.Select(q => new Quest(q)).ToList();
            onUpdated?.Invoke();
        }
    }
}
