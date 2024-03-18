using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Text messageText;

    Pokemon _pokemon;

    //public Text MessageText => messageText;

    public void Init(Pokemon pokemon)
    {
        _pokemon = pokemon;
        UpdateData();
        //��ʼ��Ϊ��
        SetMessage("");
        _pokemon.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lvl " + _pokemon.Level;
        hpBar.SetHP((float)_pokemon.HP / _pokemon.MaxHp);
    }




    //�ı���ɫ
    public void SetSelected(bool seleted)
    {
        if (seleted)
            nameText.color = GlobalSettings.i.HighlightedColor;
        else
            nameText.color = Color.black;
    }

    /// <summary>
    /// ��ʾ �ܷ�ѧϰ�����ı�
    /// </summary>
    /// <param name="message"></param>
    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
