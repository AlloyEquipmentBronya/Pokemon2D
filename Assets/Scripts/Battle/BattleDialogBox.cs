using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{

    [SerializeField] int lettersPerSecond;

    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    [SerializeField] Text YesText;
    [SerializeField] Text NoText;

    Color highlightedColor;

    private void Start()
    {
        highlightedColor = GlobalSettings.i.HighlightedColor;
    }

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }


    //协程 让对话的文字一字一字显示出来 动画般的
    /// <summary>
    /// 显示文字  一字一字
    /// </summary>
    /// <param name="dialog">要显示的字符串</param>
    /// <returns></returns>
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        //ToCharArray()   把字符串转换成字符数组
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        yield return new WaitForSeconds(1f);
    }

    //控制DialogTexUI显示
    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }
    //控制玩家行为 UI的显示
    public void EnableActonSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }
    //控制显示 宝可梦技能的显示 和技能描述
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    /// <summary>
    /// 控制战斗结束切换的显示
    /// </summary>
    /// <param name="enabled"></param>
    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    //当actionTexts 玩家行为被选中时
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if(i==selectedAction)
            {
                actionTexts[i].color = highlightedColor;
            }
            else
            {
                actionTexts[i].color = Color.black;
            }
        }
    }
    //当 玩家选则技能是
    public void UpdateMoveSelection(int selectedAction,Move move)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i == selectedAction)
            {
               moveTexts[i].color = highlightedColor;
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }
        ppText.text = $"PP{move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if (move.PP == 0)
            ppText.color = Color.red;
        else
            ppText.color = Color.black;
    }

   /// <summary>
   /// 将宝可梦的技能加入UI中显示
   /// </summary>
   /// <param name="moves"></param>
    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if(i<moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = " - ";
            }
        }
    }

    public void UpdaChoiceBox(bool yesSelected)
    {
        if(yesSelected)
        {
            YesText.color = highlightedColor;
            NoText.color = Color.black;
        }
        else
        {
            YesText.color = Color.black;
            NoText.color = highlightedColor;
        }
    }

}
