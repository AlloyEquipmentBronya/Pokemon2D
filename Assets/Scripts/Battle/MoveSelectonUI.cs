using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectonUI : MonoBehaviour
{
    [SerializeField]List<Text> moveSelectionText;
    [SerializeField] Color highlightedColor;
    int currentSelection = 0;


    /// <summary>
    /// ��ʼ��ѡ������ �������� UI
    /// </summary>
    /// <param name="currentMoves">��ǰ�еļ���</param>
    /// <param name="newMove">��ѧϰ�ļ���</param>
    public void SetMoveDate(List<MoveBase> currentMoves,MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveSelectionText[i].text = currentMoves[i].Name;
        }

        moveSelectionText[currentMoves.Count].text = newMove.Name;
    }

    /// <summary>
    /// ���ܸ�������
    /// </summary>
    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++currentSelection;
        else if(Input.GetKeyDown(KeyCode.UpArrow))
            --currentSelection;

        UpdateMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z))
            onSelected?.Invoke(currentSelection);


    }


    /// <summary>
    /// ѡ����ɫ�仯
    /// </summary>
    /// <param name="selecton"></param>
    public void UpdateMoveSelection(int selecton)
    {
        for (int i = 0; i < PokemonBase.MaxNumOfMoves+1; i++)
        {
            if (i == selecton)
            {
                moveSelectionText[i].color = highlightedColor;
            }
            else
                moveSelectionText[i].color = Color.black;
        }
    }

}
