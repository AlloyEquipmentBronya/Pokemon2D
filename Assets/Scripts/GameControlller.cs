using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState { FeemRoan,Battle,Dialog,Memu,PartyScreen,Bag,Cutscene,Pause,Evolution,Shop}
public class GameControlller : MonoBehaviour
{

    [SerializeField] PlayerControl playerControl;
    [SerializeField] BattelSystem battelSystem;
    [SerializeField] Camera worldCamera;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
     GameState state;
    GameState prevState;
    GameState stateBeforeEvolution;

    public SceneDetails CurrentScene { get; private set; }

    public SceneDetails PrevScene { get; private set; }

    MemuController memuController;


    public static GameControlller Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        memuController = GetComponent<MemuController>();

        PokemonDB.Init();
        MoveDB.Init();
        ConditionDB.Init();
        ItemDB.Init();
        QuestDB.Init();
    }

    private void Start()
    {
        //OnBattleOver返回 bool 类型 判断战斗是否胜利（相对于玩家而言）
        battelSystem.OnBattleOver += EndBattle;
        //初始化队伍界面
        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
          {
              prevState = state;
              state = GameState.Dialog;
          };
        DialogManager.Instance.OnDialogFinished += () =>
        {
            if (state == GameState.Dialog)
                state = prevState;
        };

        //在菜单中选择关闭时  回到自由状态
        memuController.onBack += () =>
          {
              state = GameState.FeemRoan;
          };
        //菜单选择
        memuController.onMenuSelected += OnMenuSelected;

        EvolutionManger.i.OnStartEvolution += () =>
        {
            stateBeforeEvolution = state;
            state = GameState.Evolution;
         };
        EvolutionManger.i.OnCompleteEvolution += () =>
        {
            partyScreen.SetPartyDate();
            state = stateBeforeEvolution;

            //进化结束 播发
            AudioManager.i.PlayMusic(CurrentScene.SceneMusic,fade:true);

        };

        ShopController.i.OnStart += () => state = GameState.Shop;
        ShopController.i.OnFinish += () => state = GameState.FeemRoan;
    }


    /// <summary>
    /// 暂停 玩家的行为
    /// </summary>
    /// <param name="pause"></param>
    public void PauseGame(bool pause)
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Pause;
        }
        else
        {
            state = prevState;
        }
    }

  public void StartBattle()
    {
        state = GameState.Battle;
        //激活battelSystem 
        battelSystem.gameObject.SetActive(true);
        //关闭 世界地图摄像机
        worldCamera.gameObject.SetActive(false);

        //将PokemonParty组件附加到playerControl对象上
        var playerPokemon = playerControl.GetComponent<PokemonParty>();

        //从当前场景中  获得MapArea组件（脚本  调用生成野生宝可梦的方法
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetWildPokemon();

        //复制出新的宝可梦 创建一个对象
        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battelSystem.StartBattle(playerPokemon,wildPokemonCopy);
    }

    TrainerController trainer;
    /// <summary>
    ///训练家战斗
    /// </summary>
    /// <param name="trainer"></param>
   public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        //激活battelSystem 
        battelSystem.gameObject.SetActive(true);
        //关闭 世界地图摄像机
        worldCamera.gameObject.SetActive(false);
        this.trainer = trainer;
        //将PokemonParty组件附加到playerControl对象上
        var playerParty = playerControl.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battelSystem.StartTrainerBattle(playerParty,trainerParty);
    }


    /// <summary>
    /// 进入训练家的视角 触发战斗
    /// </summary>
    /// <param name="trainer"></param>
    public void OnEnterTrainerView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerControl));
    }


    /// <summary>
    /// 战斗结束 游戏行为
    /// </summary>
    /// <param name="won"></param>
    void EndBattle(bool won)
    {
        if(trainer!=null&&won==true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        partyScreen.SetPartyDate();

        state = GameState.FeemRoan;
        battelSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        var playerParty = playerControl.GetComponent<PokemonParty>();


        //存在进化 ？
        bool hasEvolution= playerParty.CheckForEvolution();
        //执行进化  
        if (hasEvolution)
            StartCoroutine(playerParty.RunEvolution());
        else
            AudioManager.i.PlayMusic(CurrentScene.SceneMusic,fade:true);

    }

    /// <summary>
    /// 玩家输入 行为  控制器权管理
    /// </summary>
    private void Update()
    {
         if(state==GameState.FeemRoan)
        {
            playerControl.HandleUpdate();
            //打开背包
            if(Input.GetKeyDown(KeyCode.Return))
            {
                memuController.OpenMemu();
                state = GameState.Memu;
            }

        }
         //战斗
         else if(state==GameState.Battle)
        {
            battelSystem.HandleUpdate();
        }
         //对话
         else if(state==GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
         //菜单界面
         else if(state==GameState.Memu)
        {
            memuController.HandleUpdate();
        }
         //队伍界面
         else  if(state==GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                //显示信息之类的
            };
            Action onBack = () =>
             {
                 //关闭界面
                 partyScreen.gameObject.SetActive(false);
                 state = GameState.FeemRoan;
             };

            partyScreen.HandleUpdate(onSelected,onBack);
        }
         //背包界面
         else if(state==GameState.Bag)
        {
            Action onBack = () =>
            {
                //关闭界面
               inventoryUI.gameObject.SetActive(false);
                state = GameState.FeemRoan;
            };

            inventoryUI.HandleUpdate(onBack);
        }
         //商店界面
         else if(state==GameState.Shop)
        {
            ShopController.i.HandleUpdate();
        }

        

    }

    /// <summary>
    /// 设置当前场景和之前的场景
    /// </summary>
    /// <param name="currScene">当前的场景</param>
    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    /// <summary>
    /// 菜单选择执行
    /// </summary>
    /// <param name="selectedItem"></param>
     void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            //宝可梦队伍
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            //背包
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selectedItem == 2)
        {
            //保存
            SavingSystem.i.Save("save01");
            state = GameState.FeemRoan;
        }
        else if (selectedItem == 3)
        {
            //加载
            SavingSystem.i.Load("save01");
            state = GameState.FeemRoan;
        }
        //回到自由行动
    }


    /// <summary>
    /// 调整相机 移动 淡化淡出
    /// </summary>
    /// <param name="moveOffset"></param>
    public IEnumerator MoveCamera(Vector2 moveOffset,bool waitForFadeOut=false)
    {
        yield return Fader.i.FaderIn(0.5f); 


        worldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);

        //这两种方式的区别就是先执行后续代码或者等待淡出效果完成再执行后续代码。
        if (waitForFadeOut)
            yield return Fader.i.FaderOut(0.5f);
        else
            StartCoroutine(Fader.i.FaderOut(0.5f));
    }


    public GameState State => state;
}
