using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    PartyMemberUI[] memberSlots;

    List<Pokemon> pokemons;
    PokemonParty party;

    int selection = 0;

    /// <summary>
    /// ���ѡ�еı����� �Ķ���
    /// </summary>
    public Pokemon SelectedMember => pokemons[selection];

    /// <summary>
    /// ���ԴӲ�ͬ��״̬���ñ����ζ�����Ļ���� ActionSelection��RunningTurn��AboutTousel
    /// </summary>
    public BattleState? CalledFrom { get; set; }

    //memberSlots �����оʹ洢�������������е� PartyMemberUI ����������ں����Ĵ�����ʹ��
    public void Init()
    {
        //��������Ӷ���
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        party = PokemonParty.GetPlayerParty();

        SetPartyDate();

        party.OnUpdated += SetPartyDate;
    }

    //���������� ���뵽�����ζ���UI������
    public void SetPartyDate()
    {
        pokemons = party.Pokemons;
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(pokemons[i]);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        UpdateMemberSelection(selection);

        messageText.text = "ѡ�񱦿���";
    }

    /// <summary>
    /// ���¶���ѡ����Ϊ
    /// </summary>
    /// <param name="onSelected"></param>
    /// <param name="onBack"></param>
    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = selection;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++selection;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --selection;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            selection += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selection -= 2;

        selection = Mathf.Clamp(selection, 0, pokemons.Count - 1);

        if(selection!=prevSelection)
           UpdateMemberSelection(selection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }




        //�����ѡ�ж����еı�����ʱ 
        public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    /// <summary>
    /// ������ʾ������ �ܷ�ѧϰ�����ı�
    /// </summary>
    /// <param name="tmItem"></param>
    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            string message = tmItem.CanBeTaught(pokemons[i]) ? "��ѧϰ" : "����ѧϰ";
            memberSlots[i].SetMessage(message);
        }
    }

    /// <summary>
    /// ����ı�
    /// </summary>
    public void ClaerMemberSlotMessage()
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }


    public void SetMessageText(string message)
    {
        messageText.text = message;
    }

}
