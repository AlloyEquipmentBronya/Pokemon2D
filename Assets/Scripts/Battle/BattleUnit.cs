using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    //是否是玩家的单元 宝可梦
    [SerializeField] bool isPlayerUnit;  
   
    [SerializeField] BattleHub hub;

    [SerializeField] GameObject expBar;

    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }
    /// <summary>
    /// 战斗变化血条
    /// </summary>
    public BattleHub Hub { get { return hub; } }

    public Pokemon Pokemon { get; set; }


     Image image;
    Vector3 orginalPos;
    Color orginalColor;
    private void Awake()
    {
        image = GetComponent<Image>();
        orginalPos = image.transform.localPosition;
        orginalColor = image.color;
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        //如果是玩家图片设置成背面 否则正面
        if(isPlayerUnit)
        {
            image.sprite = Pokemon.Base.BackSprite;
        }
        else
        {
            image.sprite = Pokemon.Base.FrontSprite;
        }

        hub.gameObject.SetActive(true);
        hub.SetData(pokemon);

        //还原大小
        transform.localScale = new Vector3(1, 1, 1);
        //战斗后恢复敌方宝可梦的颜色
        image.color = orginalColor;
        PlayEnterAnimation();

    }
    public void Clear()
    {
        hub.gameObject.SetActive(false);
    }

    //入场动画
    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
            image.transform.localPosition = new Vector3(-500f, orginalPos.y);
        else
            image.transform.localPosition = new Vector3(500f, orginalPos.y);

        image.transform.DOLocalMoveX(orginalPos.x, 1f);
    }

    /// <summary>
    /// 攻击动画
    /// </summary>
    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
           sequence.Append(image.transform.DOLocalMoveX(orginalPos.x + 50f,0.25f));
        else
            sequence.Append(image.transform.DOLocalMoveX(orginalPos.x -50f, 0.25f));

        sequence.Append(image.transform.DOLocalMoveX(orginalPos.x,0.2f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray,0.1f));
        sequence.Append(image.DOColor(orginalColor, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(orginalPos.y - 150, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }

    /// <summary>
    /// 进入精灵球动画
    /// </summary>
    /// <returns></returns>
   public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        //透明
        sequence.Append(image.DOFade(0,0.5f));
        //移动Y
        sequence.Join(image.transform.DOLocalMoveY(orginalPos.y + 50f, 0.5f));
        //缩小
        sequence.Join(image.transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));

        yield return sequence.WaitForCompletion();
    }

    /// <summary>
    /// 跳出精灵球
    /// </summary>
    /// <returns></returns>
    public IEnumerator PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        //透明
        sequence.Append(image.DOFade(1, 0.5f));
        //移动Y
        sequence.Join(image.transform.DOLocalMoveY(orginalPos.y, 0.5f));
        //缩小
        sequence.Join(image.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));

        yield return sequence.WaitForCompletion();
    }

}
