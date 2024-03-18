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
    /// 获得选中的宝可梦 的对象
    /// </summary>
    public Pokemon SelectedMember => pokemons[selection];

    /// <summary>
    /// 可以从不同的状态调用宝可梦队伍屏幕，如 ActionSelection、RunningTurn、AboutTousel
    /// </summary>
    public BattleState? CalledFrom { get; set; }

    //memberSlots 数组中就存储了所有子物体中的 PartyMemberUI 组件，可以在后续的代码中使用
    public void Init()
    {
        //获得所有子对象
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        party = PokemonParty.GetPlayerParty();

        SetPartyDate();

        party.OnUpdated += SetPartyDate;
    }

    //将宝可梦们 加入到宝可梦队伍UI数组中
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

        messageText.text = "选择宝可梦";
    }

    /// <summary>
    /// 更新队伍选择行为
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




        //当玩家选中队伍中的宝可梦时 
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
    /// 控制显示宝可梦 能否学习技能文本
    /// </summary>
    /// <param name="tmItem"></param>
    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            string message = tmItem.CanBeTaught(pokemons[i]) ? "能学习" : "不能学习";
            memberSlots[i].SetMessage(message);
        }
    }

    /// <summary>
    /// 清除文本
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
