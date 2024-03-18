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


    //Э�� �öԻ�������һ��һ����ʾ���� �������
    /// <summary>
    /// ��ʾ����  һ��һ��
    /// </summary>
    /// <param name="dialog">Ҫ��ʾ���ַ���</param>
    /// <returns></returns>
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        //ToCharArray()   ���ַ���ת�����ַ�����
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        yield return new WaitForSeconds(1f);
    }

    //����DialogTexUI��ʾ
    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }
    //���������Ϊ UI����ʾ
    public void EnableActonSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }
    //������ʾ �����μ��ܵ���ʾ �ͼ�������
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    /// <summary>
    /// ����ս�������л�����ʾ
    /// </summary>
    /// <param name="enabled"></param>
    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    //��actionTexts �����Ϊ��ѡ��ʱ
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
    //�� ���ѡ������
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
   /// �������εļ��ܼ���UI����ʾ
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
