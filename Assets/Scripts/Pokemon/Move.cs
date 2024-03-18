using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move      //技能类
{
    public MoveBase Base { get; set; }
    public int PP { get; set; }

    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP;

    }

    /// <summary>
    /// 加载技能数据
    /// </summary>
    /// <param name="saveData"></param>
    public Move(MoveSaveData saveData)
    {
        //Bug 忘记给Base赋值 搞了三小时
       Base= MoveDB.GetObjectByName(saveData.name);
        PP = saveData.pp;
    }

    /// <summary>
    /// 保存的技能数据
    /// </summary>
    /// <returns></returns>
    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData
        {
            name = Base.name,
            pp = PP
        };
        return saveData;
    }

    /// <summary>
    /// 增加PP
    /// </summary>
    /// <param name="amount"></param>
    public void IncreasePP(int amount)
    {
        PP = Mathf.Clamp(PP+amount, 0, Base.PP);
    }

    
}
[System.Serializable]
public class MoveSaveData
{
    public string name;
    public int pp;
}
