using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    //�Ƿ�����ҵĵ�Ԫ ������
    [SerializeField] bool isPlayerUnit;  
   
    [SerializeField] BattleHub hub;

    [SerializeField] GameObject expBar;

    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }
    /// <summary>
    /// ս���仯Ѫ��
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
        //��������ͼƬ���óɱ��� ��������
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

        //��ԭ��С
        transform.localScale = new Vector3(1, 1, 1);
        //ս����ָ��з������ε���ɫ
        image.color = orginalColor;
        PlayEnterAnimation();

    }
    public void Clear()
    {
        hub.gameObject.SetActive(false);
    }

    //�볡����
    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
            image.transform.localPosition = new Vector3(-500f, orginalPos.y);
        else
            image.transform.localPosition = new Vector3(500f, orginalPos.y);

        image.transform.DOLocalMoveX(orginalPos.x, 1f);
    }

    /// <summary>
    /// ��������
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
    /// ���뾫���򶯻�
    /// </summary>
    /// <returns></returns>
   public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        //͸��
        sequence.Append(image.DOFade(0,0.5f));
        //�ƶ�Y
        sequence.Join(image.transform.DOLocalMoveY(orginalPos.y + 50f, 0.5f));
        //��С
        sequence.Join(image.transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));

        yield return sequence.WaitForCompletion();
    }

    /// <summary>
    /// ����������
    /// </summary>
    /// <returns></returns>
    public IEnumerator PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        //͸��
        sequence.Append(image.DOFade(1, 0.5f));
        //�ƶ�Y
        sequence.Join(image.transform.DOLocalMoveY(orginalPos.y, 0.5f));
        //��С
        sequence.Join(image.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));

        yield return sequence.WaitForCompletion();
    }

}
