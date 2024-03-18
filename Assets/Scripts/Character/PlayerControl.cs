using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerControl : MonoBehaviour,ISavable
{

    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    const float offsetY = 0.3f;



    private Vector2 input;

    private Character character;

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

    //���༭��������ʱ���� ���ƹ��캯��
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        //�����ƶ�
        if(!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //������Ҳ��ܶԽ��ƶ�   ��x�᲻Ϊ��ʱ y��Ϊ��
            if (input.x != 0) { input.y = 0; }
            //else if (input.y != 0) { input.x = 0; }


            if(input!=Vector2.zero)
            {
              StartCoroutine(character.Move(input,OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
           StartCoroutine(Interact());
    }


    IEnumerator Interact()
    {

       var facingDir= new Vector3(character.Animator.MoveX,character.Animator.MoveY);
        var interactPos= transform.position + facingDir;

        // Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f);

       var collider= Physics2D.OverlapCircle(interactPos, 0.3f,GameLayers.I.InteractableLayer);
        if(collider!=null)
        {
           yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    IPlayeTrigger currentlyInTrigger;
    /// <summary>
    /// �ƶ��������ж��Ƿ���������Ϊ ���� ����ս���ȵ�
    /// </summary>
    private void OnMoveOver()
    {
      var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0,character.OffsetY), 0.2f, GameLayers.I.TriggerableLayers);

        IPlayeTrigger triggerable=null;
        foreach (var collider in colliders)
        {
             triggerable = collider.GetComponent<IPlayeTrigger>();
            if(triggerable!=null)
            {
                if (triggerable == currentlyInTrigger && !triggerable.TriggerRepeatedly)
                    break;

                triggerable.OnPlayerTriggered(this);
                currentlyInTrigger = triggerable;
                break;
            }
        }
        //��ǰ����ײ��������0 ��
        //��ʼ�� ��
        if (colliders.Count() == 0 && triggerable != currentlyInTrigger)
            currentlyInTrigger = null;

    }

    /// <summary>
    /// ����
    /// </summary>
    /// <returns></returns>
    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            //���� �����ζ�������� ���� ��ǰHP�ȵ�
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList(),
        };

        return saveData ;
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="state"></param>
    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        //�ָ�/���� ��ҵ�����
        var pos = saveData.position;
        //����������긳ֵ����ҵ�����
        transform.position = new Vector3(pos[0], pos[1]);

        //���ر����ε�����
        //ͨ�����캯�� �����ݼ���
       GetComponent<PokemonParty>().Pokemons=  saveData.pokemons.Select(s => new Pokemon(s)).ToList();
    }

    public Character Character => character;
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
}
