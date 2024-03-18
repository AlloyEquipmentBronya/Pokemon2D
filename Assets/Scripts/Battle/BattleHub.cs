using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHub : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text StatusText;
    [SerializeField] HPBar hpBar;

    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Pokemon _pokemon;
    Dictionary<ConditionID, Color> statusColor;

    public void SetData(Pokemon pokemon)
    {
        if(_pokemon!=null)
        {
            //更换宝可梦 取消订阅事件
            _pokemon.OnStatusChanged -= SetStatusText;
            _pokemon.OnHPChanged -= UpdateHP;
        }

        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP/pokemon.MaxHp);
        SetExp();
        
        statusColor = new Dictionary<ConditionID, Color>()
        {
            { ConditionID.中毒,psnColor },
            { ConditionID.催眠,slpColor },
            { ConditionID.冰冻,frzColor },
            { ConditionID.灼烧,brnColor },
            { ConditionID.麻痹,parColor },
        };

        SetStatusText();

        //当状态改变时 同时执行 SetStatusText文本状态
        _pokemon.OnStatusChanged += SetStatusText;
        //当宝可梦的HP变化时 从新 更新 UI条显示
        _pokemon.OnHPChanged += UpdateHP;


    }

    /// <summary>
    /// UI中显示状态名称 设置颜色
    /// </summary>
    void SetStatusText()
    {
        if(_pokemon.Status==null)
        {
            StatusText.text = "";
        }
        else
        {
            StatusText.text = _pokemon.Status.Id.ToString();
            StatusText.color = statusColor[_pokemon.Status.Id];
        }
    }


    /// <summary>
    /// 初始化等级设置
    /// </summary>
    public void SetLevel()
    {
        levelText.text = "Lvl " + _pokemon.Level;
    }



    /// <summary>
    /// 初始化Exp条
    /// </summary>
    public void SetExp()
    {
        //只有玩家才有exp
        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
        
    }



    /// <summary>
    /// 平滑升级条
    /// </summary>
    /// <param name="reset">升级重置</param>
    /// <returns></returns>
    public IEnumerator SetExpSmooth(bool reset=false)
    {
        //只有玩家才有exp
        if (expBar == null) yield break;
        
        //升级时重置
        if(reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
      yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();


    }


    /// <summary>
    /// 计算显示在 Scale条的值
    /// </summary>
    /// <returns></returns>
    float GetNormalizedExp()
    {
        int currLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
        int nextLevelExp= _pokemon.Base.GetExpForLevel(_pokemon.Level+1);

        float normalizedExp = (float)(_pokemon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    /// <summary>
    /// 封装 协程后的更新HP函数
    /// </summary>
    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    //血量变化时 重新为宝可梦的血条做调整
   public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }

    /// <summary>
    /// 判定 HP更新  
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating==false);
    }

    /// <summary>
    /// 清空战斗中的订阅 的事件
    /// </summary>
    public void ClearData()
    {
        if (_pokemon != null)
        {
            //更换宝可梦 取消订阅事件
            _pokemon.OnStatusChanged -= SetStatusText;
            _pokemon.OnHPChanged -= UpdateHP;
        }
    }

}
