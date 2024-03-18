using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move      //������
{
    public MoveBase Base { get; set; }
    public int PP { get; set; }

    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP;

    }

    /// <summary>
    /// ���ؼ�������
    /// </summary>
    /// <param name="saveData"></param>
    public Move(MoveSaveData saveData)
    {
        //Bug ���Ǹ�Base��ֵ ������Сʱ
       Base= MoveDB.GetObjectByName(saveData.name);
        PP = saveData.pp;
    }

    /// <summary>
    /// ����ļ�������
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
    /// ����PP
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