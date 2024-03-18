using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//ö�� ս��ϵͳ�ĸ���״̬
public enum BattleState { Start,ActionSelection,MoveSelection,RunningTurn,Busy,PartyScreen,Bag,AboutToUse,MoveForget,BattleOver}
/// <summary>
/// �����Ϊ
/// </summary>
public enum BattleAction { Move,SwitchPokemon,UseItem,Run}
public class BattelSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;

    [SerializeField] MoveSelectonUI moveSelectonUI;
    [SerializeField] InventoryUI inventoryUI;

    [Header("����")]
    //����
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    /// <summary>
    /// ��ǰս�� ״̬
    /// </summary>
    BattleState state;
    //��ǰѡ����Ϊ|����
    int currentAction;
    int currentMove;

    bool aboutToUseChoice=true;

    //����ս���Ƿ�ʤ����ʧ��  �¼���������
    public event Action<bool> OnBattleOver;

    PokemonParty playerParty;
    Pokemon wildPokemon;
    PokemonParty trainerParty;

    bool isTrainerBattle = true;
    PlayerControl player;
    TrainerController trainer;
    
    /// <summary>
    /// ���ܴ���
    /// </summary>
    int escapeAttempts;
    MoveBase moveToLearn;


    /// <summary>
    /// ����Ұ��������ս��
    /// </summary>
    /// <param name="playerParty"></param>
    /// <param name="wildPokemon"></param>
    public void StartBattle(PokemonParty playerParty,Pokemon wildPokemon)
    {
        isTrainerBattle = false;
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerControl>();
        // isTrainerBattle = false;

        AudioManager.i.PlayMusic(wildBattleMusic);

        StartCoroutine(SetupBattle());
    }

    /// <summary>
    /// ������ѵ���ҶԾ�
    /// </summary>
    /// <param name="playParty">���</param>
    /// <param name="pokemonParty">ѵ����</param>
    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerControl>();
        trainer = trainerParty.GetComponent<TrainerController>();

        AudioManager.i.PlayMusic(trainerBattleMusic);

        StartCoroutine(SetupBattle());
    }

    //ս����Ϊ��Э��
    public IEnumerator SetupBattle()
    {
        //��ʼ����ʾս��UI
        playerUnit.Clear();
        enemyUnit.Clear();

        if(!isTrainerBattle)
        {
            //Ұ�������ζ�ս
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

            //�� yield return ������һ��Э�� �ȴ�Э�̽���
            yield return dialogBox.TypeDialog("Ұ����" + enemyUnit.Pokemon.Base.Name + "������!");
        }
        else
        {
            //ѵ���ҶԾ�

            //����ս���� ��ʾ��� ����ʾ������
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name}��Ҫս��");

            //ѵ�����ɳ���һֻ������
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var trainPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(trainPokemon);
            yield return dialogBox.TypeDialog(trainer.Name+"�ɳ�" + enemyUnit.Pokemon.Base.Name + "��");

            //����ɳ�������
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog("ȥ�ɣ�"+playerUnit.Pokemon.Base.Name+"��");
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        //���ܴ���
        escapeAttempts = 0;

        partyScreen.Init();
        //�ж�ѡ��
        ActionSelection();
    }

 


    /// <summary>
    /// ����ս������
    /// </summary>
    /// <param name="won"></param>
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        //�ö�������б����ε���״̬��ʼ������
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        //֪ͨ��Ϸ����������ս�� �¼�
        playerUnit.Hub.ClearData();
        OnBattleOver(won);
    }


    //ս�������԰�
    void ActionSelection()
    {
       state=BattleState.ActionSelection;
        dialogBox.SetDialog("Ҫ��ô��?");
        dialogBox.EnableActonSelector(true);
    }

    /// <summary>
    /// �򿪱���
    /// </summary>
    void OpenBag()
    {
        //����״̬
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActonSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    //�򿪱����ζ���
    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;

        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    /// <summary>
    /// ��Ҫ�л�������
    /// </summary>
    /// <param name="newPokemon">�з��л��ı�����</param>
    /// <returns></returns>
    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name}���ɳ�{newPokemon.Base.Name}��" +
            $"�Ƿ�Ҫ�л���������");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    /// <summary>
    /// ѡ��������UI��ʾ
    /// </summary>
    /// <param name="pokemon"></param>
    /// <param name="newMove"></param>
    /// <returns></returns>
    IEnumerator ChooseMoveToForget(Pokemon pokemon,MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"��ѡ��Ҫ�����ļ���");
        moveSelectonUI.gameObject.SetActive(true);
        moveSelectonUI.SetMoveDate(pokemon.Moves.Select(x=>x.Base).ToList(), newMove);

        //���¼��ܸ�ֵ��moveTolearn �Ա����
        moveToLearn = newMove;

        state = BattleState.MoveForget;
    }


    /// <summary>
    /// �غ� �ж� 
    /// </summary>
    /// <param name="playerAction">�����Ϊ</param>
    /// <returns></returns>
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;
        //ѡ��ս��
        if(playerAction==BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority= playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;
            //�ж��ж����ȼ�
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;
            //��һ�غ�
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            //Hp����0 �غϼ��� 
            if (secondPokemon.HP > 0)
            {
                //�ڶ��غ�
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else 
        {
            //ѡ�񽻻�������
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }

            //ѡ�����
            else if(playerAction==BattleAction.UseItem)
            {
                dialogBox.EnableActonSelector(false);
                //yield return ThrowPokeball();
            }
            //����
            else if(playerAction==BattleAction.Run)
            {
                dialogBox.EnableActonSelector(false);
                yield return TryToEscape();
            }

            //�����������˻غ�
            var enemyMove=enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }
        //ս��û���� �ص�ѡ���ж���
        if (state != BattleState.BattleOver)
            ActionSelection();
    }


    /// <summary>
    /// ִ��ս���ж�
    /// </summary>
    /// <param name="sourceUnit">�ж���</param>
    /// <param name="targetUnit">�з�</param>
    /// <param name="move">�ж�������</param>
    /// <returns></returns>
    IEnumerator RunMove(BattleUnit sourceUnit,BattleUnit targetUnit,Move move)
    {
        //ս����ʼǰ����ж����Ƿ����ƶ� �����ƶ��޷��ж�
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if(!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hub.WaitForHPUpdate();//�������û�и���HP 
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        //pp-1
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}ʹ����{move.Base.Name}");

        //�жϼ����Ƿ�����
        if (CheckIfMoveHit(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            //�������� -��Ч
            sourceUnit.PlayAttackAnimation();
            AudioManager.i.PlaySfx(move.Base.Sound);

            yield return new WaitForSeconds(1f);

            //�ܻ����� -��Ч
            targetUnit.PlayHitAnimation();
            AudioManager.i.PlaySfx(AudioId.Hit);

            //�Ƿ���״̬�仯����
            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon,move.Base.Target);

            }
            else
            {
                //������HP�Ƿ���� ���˳� 
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                //Ѫ���仯 �ı�UI
                yield return targetUnit.Hub.WaitForHPUpdate();
                yield return ShowDamgeDetails(damageDetails);
            }

            //�ڶ���״̬��Ч����
            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Change)
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon,targetUnit.Pokemon,secondary.Target);
                }
            }
            if (targetUnit.Pokemon.HP <= 0)
            {
               yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}����δ����!");
        }


            //�غϽ����� �ж��˺�
              //sourceUnit.Pokemon.OnAfterTurn();  ��ս�������� ���γ����˺��ж� bug
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hub.UpdateHPAsync();

            if (sourceUnit.Pokemon.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}������");
                sourceUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(sourceUnit);
            }

    }

    /// <summary>
    /// �ж� ����Ӱ��
    /// </summary>
    /// <param name="effects">Ч��</param>
    /// <param name="source">������</param>
    /// <param name="target">����Ŀ��</param>
    /// <param name="moveTarget">��������Ŀ��</param>
    /// <returns></returns>
    IEnumerator RunMoveEffects(MoveEffects effects ,Pokemon source,Pokemon target,MoveTarget moveTarget)
    {
        
        //״̬ ������
        if (effects.Boosts != null)
        {
            //����Ŀ���Ƿ�Ϊ�Լ�
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        //״̬ �쳣��
        if (effects.Status != ConditionID.��)
        {
            target.SetStatus(effects.Status);
        }

        //���ȶ�״̬ ���ҵ�/�쳣��
        if (effects.VolatileStatus != ConditionID.��)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }


        //��ʾ������״̬
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    /// <summary>
    /// �ж�������� �غ� 
    /// </summary>
    /// <param name="sourceUnit"></param>
    /// <returns></returns>
    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        //ս������ ��ִ��Э��
        if (state == BattleState.BattleOver) yield break;
        //�� ״̬ state=BattleState.RunningTurnʱ ��ִ������Ĵ���
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        //ȼ�� �����ж��ڻغϽ������ж�
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        yield return sourceUnit.Hub.WaitForHPUpdate();

        if (sourceUnit.Pokemon.HP <= 0)
        {
           StartCoroutine(HandlePokemonFainted(sourceUnit));

            //�� ״̬ state=BattleState.RunningTurnʱ ��ִ������Ĵ���
            yield return new WaitUntil(() => state == BattleState.RunningTurn);

        }
    }
    /// <summary>
    /// �����ж�
    /// </summary>
    /// <param name="move">����</param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    bool CheckIfMoveHit(Move move,Pokemon source,Pokemon target)
    {
        //����
        if (move.Base.AlwaysHit)
            return true;

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.������];
        int evasion = target.StatBoosts[Stat.������];

        var boostValues = new float[] { 1f, 4/3f, 5/3f, 2f, 7/3f, 8/3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[-evasion ];
        else
            moveAccuracy *= boostValues[evasion ];

        return UnityEngine.Random.Range(1, 101) <=moveAccuracy;
    }

    /// <summary>
    /// ״̬���� ��ʾ�ڶԻ�����
    /// </summary>
    /// <param name="pokemon"></param>
    /// <returns></returns>
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while(pokemon.StatusChanges.Count>0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }



    /// <summary>
    /// ��������  �ĵ��º���Ϊ
    /// </summary>
    /// <param name="faintedUnit"></param>
    /// <returns></returns>
    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {

        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name}������");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        //ս��ʤ��  ��������
        if(!faintedUnit.IsPlayerUnit)
        {
            bool battleWon = true;
            if (isTrainerBattle)
                //Ϊ��˵��û�н����ı�������
                battleWon = trainerParty.GetHealthyPokemon() == null;
            if (battleWon)
                AudioManager.i.PlayMusic(battleVictoryMusic);


            //��þ���
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;

            //��ѵ���һ�ö�
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus / 7);
            playerUnit.Pokemon.Exp += expYield;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}�����{expGain}����ֵ");

            //���þ�����
           yield return playerUnit.Hub.SetExpSmooth();
            //�������
            while(playerUnit.Pokemon.CheckForLevelUp())
            {
                //������Ƶȼ�
                playerUnit.Hub.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}�ȼ�������{playerUnit.Pokemon.Level}��");

                //ѧϰ����
               var learnMove=  playerUnit.Pokemon.GetLearnableMoveAtCurrLevel();

                if(learnMove!=null)
                {
                    if(playerUnit.Pokemon.Moves.Count<PokemonBase.MaxNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(learnMove.MoveBase);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}ѧ����{learnMove.MoveBase.Name}");
                        //�������ü���UI����ʾ
                        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}��Ҫѧϰ{learnMove.MoveBase.Name}");
                        yield return dialogBox.TypeDialog($"���Ǽ����Ѿ�����");
                        //ѡ������һ�����ܣ�����µļ���
                        yield return ChooseMoveToForget(playerUnit.Pokemon, learnMove.MoveBase);

                       //ֱ��״̬��Ϊ ��������״̬������ִ�� ��һ�еĴ���
                        yield return new WaitUntil(() => state != BattleState.MoveForget);

                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hub.SetExpSmooth(true);
            }
        }


        CheckForBattleOver(faintedUnit);
    }



    /// <summary>
    /// �������ε���ʱ����Ϊ ���ս������
    /// </summary>
    /// <param name="faintedUnit">���µı�����</param>
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if(faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
                BattleOver(false);
        }
        else
        {
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                   StartCoroutine(AboutToUse(nextPokemon));
                else
                    BattleOver(true);
            }
        }
    }


    IEnumerator ShowDamgeDetails(DamageDetails damageDetails)
    {
        if(damageDetails.Critical>1f)
        {
            yield return dialogBox.TypeDialog("��ɱ���!");
        }
        if(damageDetails.TypeEffectiveness>1f)
        {
            yield return dialogBox.TypeDialog("ʮ��Ч��!");
        }
        else if(damageDetails.TypeEffectiveness <1f)
        {
            yield return dialogBox.TypeDialog("Ч������");
        }
    }

    
    /// <summary>
    ///ս���е���Ϊ�߼� ����/Լ��
    /// </summary>
    public  void HandleUpdate()
    {
        if(state==BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if(state==BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if(state==BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if(state==BattleState.Bag)
        {
            Action onBack = () =>
              {
                  inventoryUI.gameObject.SetActive(false);
                  state = BattleState.ActionSelection;
              };
            //�¼�onItemUsed �в����ж���Ϊ
            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
              {
                  StartCoroutine(OnItemUsed(usedItem));
              };

            inventoryUI.HandleUpdate(onBack,onItemUsed);
        }
        else if(state==BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if(state==BattleState.MoveForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
              {
                  moveSelectonUI.gameObject.SetActive(false);
                  //ѡ�������µļ���
                  if(moveIndex==PokemonBase.MaxNumOfMoves)
                  {
                      //��ѧϰ�¼���
                      StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}û��ѧϰ{moveToLearn.Name}"));
                  }
                  else
                  {
                      //�������еļ��ܣ�ѧϰ�µļ���
                      var selectedMove = playerUnit.Pokemon.Moves[moveIndex];
                      StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}����{selectedMove.Base.Name}ѧϰ��{moveToLearn.Name}"));

                      //��ֵ�滻 ���� ���� ѧϰ
                      playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                  }
                  moveToLearn = null;
                  state = BattleState.RunningTurn;
              };


            moveSelectonUI.HandleMoveSelection(onMoveSelected);
        }

    }

    /// <summary>
    /// �����û�ѡ��ѡ�� ��Ϊ
    /// </summary>
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;
        //�޶�currenAction�ķ�Χ
        currentAction = Mathf.Clamp(currentAction, 0, 3);

        //BattleDialogBox���� UpdateActionSelection�������Ϊ��ѡ��ʱ������
        dialogBox.UpdateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            //ѡ��ս��
            if(currentAction==0)
            {
                MoveSelection();
            }
            else if(currentAction==1)
            {
                //����
                //��һ����
                //StartCoroutine(RunTurns(BattleAction.UseItem));
                OpenBag();
            }
            else if (currentAction == 2)
            {
                //������
                //��¼֮ǰ��״̬
                //prevState = state; д����������
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    //�����û�ѡ�� ѡ�� ����
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count-1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        //ȷ������ ����ʩ�ż���
        if(Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        //����ѡ�񷵻� �����Ϊ
        else if(Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {

        Action onSelected = () =>
          {
              //��ö����е�ǰѡ�еı�����
              var selectMember = partyScreen.SelectedMember;
              if (selectMember.HP <= 0)
              {
                  partyScreen.SetMessageText("�������޷�ս��");
                  return;
              }
              if (selectMember == playerUnit.Pokemon)
              {
                  partyScreen.SetMessageText("��ǰ����Ϊѡ�еı�����");
                  return;
              }

              partyScreen.gameObject.SetActive(false);

              //��������ս���غ�ʱ���������� ��ִ��RunTurns
              if (partyScreen.CalledFrom == BattleState.ActionSelection)
              {
                  //prevState = null;
                  StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
              }
              else//�������ε� ѡ���л�
              {
                  state = BattleState.Busy;
                  bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                  StartCoroutine(SwitchPokemon(selectMember, isTrainerAboutToUse));
              }
              partyScreen.CalledFrom = null;
          };


        Action onBack = () =>
          {
              if (playerUnit.Pokemon.HP < 0)
              {
                  partyScreen.SetMessageText("��ѡ�񱦿���");
                  return;
              }

              partyScreen.gameObject.SetActive(false);

              //����Ƿ���ս��һ������ ��X״̬Ϊnull
              if (partyScreen.CalledFrom == BattleState.AboutToUse)
              {
                  //prevState = null;
                  StartCoroutine(SendNextTrainerPokemon());
              }
              else
                  ActionSelection();

              partyScreen.CalledFrom = null;
          };
        

        partyScreen.HandleUpdate(onSelected,onBack);

        
        
    }

    /// <summary>
    /// �Ƿ��л�������
    /// </summary>
    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdaChoiceBox(aboutToUseChoice);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if(aboutToUseChoice==true)
            {
                //����Ҵ򿪶����л������ν���ʱ
                //ѵ�����ɳ�������
                //prevState = BattleState.AboutToUse; д����������
                OpenPartyScreen();
            }
            else
            {
                //û�� ��
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }


    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="newPokemon">Ҫ�л��ı�����</param>
    /// <returns></returns>
    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse=false)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"�����ɣ�{playerUnit.Pokemon.Base.Name}");
            //�л�����
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);

        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog("ȥ�� " + newPokemon.Base.Name + "��");


        //������ڵз������ι���? �л������ε�ʱ�� ��/��
        if (isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerPokemon());
        else
            state = BattleState.RunningTurn;

        
    }

    /// <summary>
    /// �ɳ���һ�����ı�����
    /// </summary>
    /// <returns></returns>
    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name}�ɳ�{ nextPokemon.Base.Name}");

        state = BattleState.RunningTurn;
    }

    /// <summary>
    /// ִ���ж���Ʒʹ��/������Ϊ �ȴ���������Ϊ���
    /// </summary>
    /// <param name="usedItem"></param>
    /// <returns></returns>
    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if(usedItem is PokeballItem)
        {
            yield return ThrowPokeball((PokeballItem)usedItem);
        }
        StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    /// <summary>
    /// ������Ϊ
    /// </summary>
    /// <returns></returns>
    IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        state = BattleState.Busy;
        //���ܲ���ѵ���ҵı�����
        if(isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"�㲻�ܶ�ѵ���ҵı�����ʹ�þ�����");
            state = BattleState.RunningTurn;
            yield break;
        }

      yield return dialogBox.TypeDialog($"{player.Name}�ӳ�{pokeballItem.Name}!");
       var pokeballObj= Instantiate(pokeballSprite, playerUnit.transform.position-new Vector3(2,0), Quaternion.identity);
        var pokeball = pokeballObj.gameObject.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.Icon;

        //����
        //ֱ��������ɲ�ִ����һ��
       yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();

        //���뾫���򶯻�
       yield return enemyUnit.PlayCaptureAnimation();
        //����
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1.5f, 0.5f).WaitForCompletion();

       
        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon,pokeballItem);

        //ҡ��
        for (int i = 0; i < Mathf.Min(shakeCount,3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }
        if(shakeCount==4)
        {
            //����ɹ�
            yield return dialogBox.TypeDialog($"����{enemyUnit.Pokemon.Base.Name}�ɹ���!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}��������Ķ���");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            //����ʧ��
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
           yield return enemyUnit.PlayBreakOutAnimation();

            if(shakeCount<2)
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}����ʧ����!");
            else
                yield return dialogBox.TypeDialog($"����һ��ͳɹ���!");

            Destroy(pokeball);

            state = BattleState.RunningTurn;

        }
    }


    /// <summary>
    /// ���㲶���Ƿ�ɹ�
    /// </summary>
    /// <param name="pokemon">������</param>
    /// <returns>ҡ�δ��� ��������Ĵ���ɹ�����</returns>
    int TryToCatchPokemon(Pokemon pokemon,PokeballItem pokeballItem)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate *
            pokeballItem.CatchRateModfier * ConditionDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);
        //����255ʱ
        if (a >= 255)
            //ҡ������ �ɹ�����
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while(shakeCount<4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;
            ++shakeCount;
        }

        return shakeCount;
    }



    /// <summary>
    /// ����
    /// </summary>
    /// <returns></returns>
    IEnumerator TryToEscape()
    {
        if(isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("�㲻����ѵ���ҵ�ս��������");
            state = BattleState.RunningTurn;
            yield break;
        }
        ++escapeAttempts;
        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if(enemySpeed<playerSpeed)
        {
            yield return dialogBox.TypeDialog($"�ɹ�������");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if(UnityEngine.Random.Range(0,256)<f)
            {
                yield return dialogBox.TypeDialog($"�ɹ�������");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"����ʧ����");
                //�ص��ж� ״̬��
                state = BattleState.RunningTurn;
            }
        }
    }
}
