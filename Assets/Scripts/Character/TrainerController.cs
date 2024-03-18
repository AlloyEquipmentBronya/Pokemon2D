using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour,Interactable,ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    [SerializeField] AudioClip trainerAppearsClip;


    bool battleLosst = false;

    Character character;

    public string Name
    {
        get
        {
            return name;
        }

    }

    public Sprite Sprite
    {
        get
        {
            return sprite;
        }
       
    }


    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        //�������
        character.LookTowards(initiator.position);

        if (!battleLosst)
        {

            AudioManager.i.PlayMusic(trainerAppearsClip,loop:false);

            //����ս������Ϊ
            yield return DialogManager.Instance.ShowDialog(dialog);
            GameControlller.Instance.StartTrainerBattle(this);
         
        }
        else
        {
            //ս�������Ի�
          yield return DialogManager.Instance.ShowDialog(dialogAfterBattle);
        }
    }

    /// <summary>
    /// ����ս��
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public IEnumerator TriggerTrainerBattle(PlayerControl player)
    {
        AudioManager.i.PlayMusic(trainerAppearsClip,loop:false);

        //��ʾ�Ի���
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        //�������
       var diff= player.transform.position - transform.position;
       var moveVec= diff - diff.normalized;
        //Mathf.Round  ��ȥС��ȡ����
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        //����ս������Ϊ ��ʾ�Ի�
        yield return DialogManager.Instance.ShowDialog(dialog);
        GameControlller.Instance.StartTrainerBattle(this);
       
    }

    /// <summary>
    /// ս��
    /// </summary>
    public void BattleLost()
    {
        battleLosst = true;
        fov.gameObject.SetActive(false);
    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angele = 0f;
        if (dir == FacingDirection.Right)
            angele = 90f;
        else if (dir == FacingDirection.Up)
            angele = 180f;
        else if (dir == FacingDirection.Left)
            angele = 270;

        fov.transform.eulerAngles=new Vector3(0f, 0f, angele);
    }

    public object CaptureState()
    {
        return battleLosst;
    }

    public void RestoreState(object state)
    {
        battleLosst=(bool)state;
        //��ս��ʧ��ʱ ѵ���ҵ���ҰҪʧ��
        if (battleLosst)
            fov.gameObject.SetActive(false);
    }
}
