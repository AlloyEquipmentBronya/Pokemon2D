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
        //OnBattleOver���� bool ���� �ж�ս���Ƿ�ʤ�����������Ҷ��ԣ�
        battelSystem.OnBattleOver += EndBattle;
        //��ʼ���������
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

        //�ڲ˵���ѡ��ر�ʱ  �ص�����״̬
        memuController.onBack += () =>
          {
              state = GameState.FeemRoan;
          };
        //�˵�ѡ��
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

            //�������� ����
            AudioManager.i.PlayMusic(CurrentScene.SceneMusic,fade:true);

        };

        ShopController.i.OnStart += () => state = GameState.Shop;
        ShopController.i.OnFinish += () => state = GameState.FeemRoan;
    }


    /// <summary>
    /// ��ͣ ��ҵ���Ϊ
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
        //����battelSystem 
        battelSystem.gameObject.SetActive(true);
        //�ر� �����ͼ�����
        worldCamera.gameObject.SetActive(false);

        //��PokemonParty������ӵ�playerControl������
        var playerPokemon = playerControl.GetComponent<PokemonParty>();

        //�ӵ�ǰ������  ���MapArea������ű�  ��������Ұ�������εķ���
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetWildPokemon();

        //���Ƴ��µı����� ����һ������
        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battelSystem.StartBattle(playerPokemon,wildPokemonCopy);
    }

    TrainerController trainer;
    /// <summary>
    ///ѵ����ս��
    /// </summary>
    /// <param name="trainer"></param>
   public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        //����battelSystem 
        battelSystem.gameObject.SetActive(true);
        //�ر� �����ͼ�����
        worldCamera.gameObject.SetActive(false);
        this.trainer = trainer;
        //��PokemonParty������ӵ�playerControl������
        var playerParty = playerControl.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battelSystem.StartTrainerBattle(playerParty,trainerParty);
    }


    /// <summary>
    /// ����ѵ���ҵ��ӽ� ����ս��
    /// </summary>
    /// <param name="trainer"></param>
    public void OnEnterTrainerView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerControl));
    }


    /// <summary>
    /// ս������ ��Ϸ��Ϊ
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


        //���ڽ��� ��
        bool hasEvolution= playerParty.CheckForEvolution();
        //ִ�н���  
        if (hasEvolution)
            StartCoroutine(playerParty.RunEvolution());
        else
            AudioManager.i.PlayMusic(CurrentScene.SceneMusic,fade:true);

    }

    /// <summary>
    /// ������� ��Ϊ  ������Ȩ����
    /// </summary>
    private void Update()
    {
         if(state==GameState.FeemRoan)
        {
            playerControl.HandleUpdate();
            //�򿪱���
            if(Input.GetKeyDown(KeyCode.Return))
            {
                memuController.OpenMemu();
                state = GameState.Memu;
            }

        }
         //ս��
         else if(state==GameState.Battle)
        {
            battelSystem.HandleUpdate();
        }
         //�Ի�
         else if(state==GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
         //�˵�����
         else if(state==GameState.Memu)
        {
            memuController.HandleUpdate();
        }
         //�������
         else  if(state==GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                //��ʾ��Ϣ֮���
            };
            Action onBack = () =>
             {
                 //�رս���
                 partyScreen.gameObject.SetActive(false);
                 state = GameState.FeemRoan;
             };

            partyScreen.HandleUpdate(onSelected,onBack);
        }
         //��������
         else if(state==GameState.Bag)
        {
            Action onBack = () =>
            {
                //�رս���
               inventoryUI.gameObject.SetActive(false);
                state = GameState.FeemRoan;
            };

            inventoryUI.HandleUpdate(onBack);
        }
         //�̵����
         else if(state==GameState.Shop)
        {
            ShopController.i.HandleUpdate();
        }

        

    }

    /// <summary>
    /// ���õ�ǰ������֮ǰ�ĳ���
    /// </summary>
    /// <param name="currScene">��ǰ�ĳ���</param>
    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    /// <summary>
    /// �˵�ѡ��ִ��
    /// </summary>
    /// <param name="selectedItem"></param>
     void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            //�����ζ���
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            //����
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selectedItem == 2)
        {
            //����
            SavingSystem.i.Save("save01");
            state = GameState.FeemRoan;
        }
        else if (selectedItem == 3)
        {
            //����
            SavingSystem.i.Load("save01");
            state = GameState.FeemRoan;
        }
        //�ص������ж�
    }


    /// <summary>
    /// ������� �ƶ� ��������
    /// </summary>
    /// <param name="moveOffset"></param>
    public IEnumerator MoveCamera(Vector2 moveOffset,bool waitForFadeOut=false)
    {
        yield return Fader.i.FaderIn(0.5f); 


        worldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);

        //�����ַ�ʽ�����������ִ�к���������ߵȴ�����Ч�������ִ�к������롣
        if (waitForFadeOut)
            yield return Fader.i.FaderOut(0.5f);
        else
            StartCoroutine(Fader.i.FaderOut(0.5f));
    }


    public GameState State => state;
}
