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
            //���������� ȡ�������¼�
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
            { ConditionID.�ж�,psnColor },
            { ConditionID.����,slpColor },
            { ConditionID.����,frzColor },
            { ConditionID.����,brnColor },
            { ConditionID.���,parColor },
        };

        SetStatusText();

        //��״̬�ı�ʱ ͬʱִ�� SetStatusText�ı�״̬
        _pokemon.OnStatusChanged += SetStatusText;
        //�������ε�HP�仯ʱ ���� ���� UI����ʾ
        _pokemon.OnHPChanged += UpdateHP;


    }

    /// <summary>
    /// UI����ʾ״̬���� ������ɫ
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
    /// ��ʼ���ȼ�����
    /// </summary>
    public void SetLevel()
    {
        levelText.text = "Lvl " + _pokemon.Level;
    }



    /// <summary>
    /// ��ʼ��Exp��
    /// </summary>
    public void SetExp()
    {
        //ֻ����Ҳ���exp
        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
        
    }



    /// <summary>
    /// ƽ��������
    /// </summary>
    /// <param name="reset">��������</param>
    /// <returns></returns>
    public IEnumerator SetExpSmooth(bool reset=false)
    {
        //ֻ����Ҳ���exp
        if (expBar == null) yield break;
        
        //����ʱ����
        if(reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
      yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();


    }


    /// <summary>
    /// ������ʾ�� Scale����ֵ
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
    /// ��װ Э�̺�ĸ���HP����
    /// </summary>
    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    //Ѫ���仯ʱ ����Ϊ�����ε�Ѫ��������
   public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }

    /// <summary>
    /// �ж� HP����  
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating==false);
    }

    /// <summary>
    /// ���ս���еĶ��� ���¼�
    /// </summary>
    public void ClearData()
    {
        if (_pokemon != null)
        {
            //���������� ȡ�������¼�
            _pokemon.OnStatusChanged -= SetStatusText;
            _pokemon.OnHPChanged -= UpdateHP;
        }
    }

}
