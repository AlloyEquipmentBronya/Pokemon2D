using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] QuestBase questToCheck;
    [SerializeField] ObjectActions onStart;
    [SerializeField] ObjectActions onComplete;

    QuestList questList;
    private void Start()
    {
        questList = QuestList.GetQuestList();
        questList.onUpdated += UpDateObjectStatus;

        UpDateObjectStatus();
    }

    private void OnDestroy()
    {
        questList.onUpdated -= UpDateObjectStatus;
    }

    /// <summary>
    /// 任务对象 启用/禁用逻辑
    /// </summary>
    public void UpDateObjectStatus()
    {
        //获得玩家任务列表和 QuestBase questToCheck;的名字进行比较判定
        //遍历当前对象的所以子对象 执行对应的操作
        //开始
        if (onStart!=ObjectActions.DoNothing &&questList.IsStarted(questToCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (onStart == ObjectActions.Enable)
                {
                    child.gameObject.SetActive(true);

                    var savable = child.GetComponent<SavableEntity>();
                    if(savable!=null)
                        SavingSystem.i.RestoreEntity(savable);
                }
                else if (onStart == ObjectActions.Disable)
                    child.gameObject.SetActive(false);
            }
        }
        //完成
        if (onComplete != ObjectActions.DoNothing && questList.IsCompleted(questToCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (onComplete == ObjectActions.Enable)
                {
                    child.gameObject.SetActive(true);

                    var savable = child.GetComponent<SavableEntity>();
                    if (savable != null)
                        SavingSystem.i.RestoreEntity(savable);
                }
                else if (onComplete == ObjectActions.Disable)
                    child.gameObject.SetActive(false);
            }
        }
    }
}

public enum ObjectActions { DoNothing,Enable,Disable}
