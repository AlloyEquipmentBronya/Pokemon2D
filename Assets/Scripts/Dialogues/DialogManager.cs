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

    //可以在其他类中引用它
    public static DialogManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 显示提示对话框 传入文本
    /// </summary>
    /// <param name="text">提示信息</param>
    /// <param name="waitForinput">是否需要按键触发结束提示</param>
    /// <param name="autoClose">是否需要关闭对话框</param>
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
        //选项数不为空 且大于一个
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
    /// 显示对话 传入对话 list<string>
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
        //选项数不为空 且大于一个
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
    }
}
