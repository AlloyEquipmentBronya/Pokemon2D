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
    /// ͨ���������� �ж��Ƿ�����ʼ
    /// </summary>
    /// <param name="questName"></param>
    /// <returns></returns>
    public bool IsStarted(string questName)
    {
        var questStatus = quests.FirstOrDefault(q => q.Base.Name == questName)?.Status;
        return questStatus == QuestStatus.Started || questStatus == QuestStatus.Completed;
    }
    /// <summary>
    /// ͨ���������� �ж��Ƿ��������
    /// </summary>
    /// <param name="questName"></param>
    /// <returns></returns>
    public bool IsCompleted(string questName)
    {
        var questStatus = quests.FirstOrDefault(q => q.Base.Name == questName)?.Status;
        return  questStatus == QuestStatus.Completed;
    }

    /// <summary>
    /// ����PlayerController��QuestList���
    /// </summary>
    /// <returns></returns>
    public static QuestList GetQuestList()
    {
        return FindObjectOfType<PlayerControl>().GetComponent<QuestList>();
    }

    public object CaptureState()
    {
        //��List<Quest> ������ת����QuestSavaData
       return quests.Select(q => q.GetSaveData()).ToList();
    }

    public void RestoreState(object state)
    {
        var saveData = state as List<QuestSavaData>;
        if(saveData!=null)
        {
            //��QuestSavaData ��ԭ�� Quest list
            quests = saveData.Select(q => new Quest(q)).ToList();
            onUpdated?.Invoke();
        }
    }
}
