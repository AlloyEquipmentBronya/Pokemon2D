using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] ChoiceBox choiceBox;
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnDialogFinished;


    public bool IsShowing { get; private set; }

    //��������������������
    public static DialogManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// ��ʾ��ʾ�Ի��� �����ı�
    /// </summary>
    /// <param name="text">��ʾ��Ϣ</param>
    /// <param name="waitForinput">�Ƿ���Ҫ��������������ʾ</param>
    /// <param name="autoClose">�Ƿ���Ҫ�رնԻ���</param>
    /// <returns></returns>
    ///
    public IEnumerator ShowDialogText(string text,bool waitForinput=true,bool autoClose=true, 
        List<string> choices = null,Action<int> onChoiceSelected = null)
    {
        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);


        AudioManager.i.PlaySfx(AudioId.UISelect);

        yield return TypeDialog(text);

        if(waitForinput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }
        //ѡ������Ϊ�� �Ҵ���һ��
        if (choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        if (autoClose)
        {
            CloseDialog();
        }
        OnDialogFinished?.Invoke();
    }

    public void CloseDialog()
    {
        dialogBox.SetActive(false);
        IsShowing = false;
    }
    /// <summary>
    /// ��ʾ�Ի� ����Ի� list<string>
    /// </summary>
    /// <param name="dialog"></param>
    /// <param name="choices"></param>
    /// <param name="onChoiceSelected"></param>
    /// <returns></returns>
    public IEnumerator ShowDialog(Dialog dialog,List<string> choices=null,
        Action<int> onChoiceSelected=null )
    {
        yield return new WaitForEndOfFrame();


        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        foreach (var line in dialog.Lines)
        {
            AudioManager.i.PlaySfx(AudioId.UISelect);
            yield return TypeDialog(line);
            yield return new WaitUntil(()=>Input.GetKeyDown(KeyCode.Z));
        }
        //ѡ������Ϊ�� �Ҵ���һ��
        if(choices!=null&&choices.Count>1)
        {
          yield return choiceBox.ShowChoices(choices,onChoiceSelected);
        }

        dialogBox.SetActive(false);
        IsShowing = false;

        OnDialogFinished?.Invoke();
    }
    public void HandleUpdate()
    {
        
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
    }
}