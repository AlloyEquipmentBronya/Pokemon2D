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

    //当编辑器被唤醒时调用 类似构造函数
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        //不在移动
        if(!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //设置玩家不能对角移动   当x轴不为零时 y轴为零
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
    /// 移动结束后判断是否有其他行为 对视 触发战斗等等
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
        //当前的碰撞器对象数0 且
        //开始置 空
        if (colliders.Count() == 0 && triggerable != currentlyInTrigger)
            currentlyInTrigger = null;

    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <returns></returns>
    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            //保存 宝可梦对象的数据 名字 当前HP等等
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList(),
        };

        return saveData ;
    }

    /// <summary>
    /// 加载
    /// </summary>
    /// <param name="state"></param>
    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        //恢复/加载 玩家的坐标
        var pos = saveData.position;
        //将保存的坐标赋值给玩家的坐标
        transform.position = new Vector3(pos[0], pos[1]);

        //加载宝可梦的数据
        //通过构造函数 将数据加载
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
