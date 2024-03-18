using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//枚举 战斗系统的各种状态
public enum BattleState { Start,ActionSelection,MoveSelection,RunningTurn,Busy,PartyScreen,Bag,AboutToUse,MoveForget,BattleOver}
/// <summary>
/// 玩家行为
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

    [Header("音乐")]
    //声音
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    /// <summary>
    /// 当前战斗 状态
    /// </summary>
    BattleState state;
    //当前选中行为|技能
    int currentAction;
    int currentMove;

    bool aboutToUseChoice=true;

    //区分战斗是否胜利或失败  事件用来触发
    public event Action<bool> OnBattleOver;

    PokemonParty playerParty;
    Pokemon wildPokemon;
    PokemonParty trainerParty;

    bool isTrainerBattle = true;
    PlayerControl player;
    TrainerController trainer;
    
    /// <summary>
    /// 逃跑次数
    /// </summary>
    int escapeAttempts;
    MoveBase moveToLearn;


    /// <summary>
    /// 触发野生宝可梦战斗
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
    /// 触发和训练家对决
    /// </summary>
    /// <param name="playParty">玩家</param>
    /// <param name="pokemonParty">训练家</param>
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

    //战斗行为总协程
    public IEnumerator SetupBattle()
    {
        //开始不显示战斗UI
        playerUnit.Clear();
        enemyUnit.Clear();

        if(!isTrainerBattle)
        {
            //野生宝可梦对战
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

            //用 yield return 启动另一个协程 等待协程结束
            yield return dialogBox.TypeDialog("野生的" + enemyUnit.Pokemon.Base.Name + "出现了!");
        }
        else
        {
            //训练家对决

            //进入战斗先 显示玩家 不显示宝可梦
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name}想要战斗");

            //训练家派出第一只宝可梦
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var trainPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(trainPokemon);
            yield return dialogBox.TypeDialog(trainer.Name+"派出" + enemyUnit.Pokemon.Base.Name + "！");

            //玩家派出宝可梦
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog("去吧！"+playerUnit.Pokemon.Base.Name+"！");
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        //逃跑次数
        escapeAttempts = 0;

        partyScreen.Init();
        //行动选择
        ActionSelection();
    }

 


    /// <summary>
    /// 触发战斗结束
    /// </summary>
    /// <param name="won"></param>
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        //让队伍的所有宝可梦调用状态初始花方法
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        //通知游戏控制器结束战斗 事件
        playerUnit.Hub.ClearData();
        OnBattleOver(won);
    }


    //战斗开场对白
    void ActionSelection()
    {
       state=BattleState.ActionSelection;
        dialogBox.SetDialog("要怎么做?");
        dialogBox.EnableActonSelector(true);
    }

    /// <summary>
    /// 打开背包
    /// </summary>
    void OpenBag()
    {
        //设置状态
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

    //打开宝可梦队伍
    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;

        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    /// <summary>
    /// 将要切换宝可梦
    /// </summary>
    /// <param name="newPokemon">敌方切换的宝可梦</param>
    /// <returns></returns>
    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name}将派出{newPokemon.Base.Name}，" +
            $"是否要切换宝可梦呢");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    /// <summary>
    /// 选择技能遗忘UI显示
    /// </summary>
    /// <param name="pokemon"></param>
    /// <param name="newMove"></param>
    /// <returns></returns>
    IEnumerator ChooseMoveToForget(Pokemon pokemon,MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"请选择要忘掉的技能");
        moveSelectonUI.gameObject.SetActive(true);
        moveSelectonUI.SetMoveDate(pokemon.Moves.Select(x=>x.Base).ToList(), newMove);

        //将新技能赋值给moveTolearn 以便调用
        moveToLearn = newMove;

        state = BattleState.MoveForget;
    }


    /// <summary>
    /// 回合 行动 
    /// </summary>
    /// <param name="playerAction">玩家行为</param>
    /// <returns></returns>
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;
        //选择战斗
        if(playerAction==BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority= playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;
            //判断行动优先级
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;
            //第一回合
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            //Hp大于0 回合继续 
            if (secondPokemon.HP > 0)
            {
                //第二回合
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else 
        {
            //选择交换宝可梦
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }

            //选择道具
            else if(playerAction==BattleAction.UseItem)
            {
                dialogBox.EnableActonSelector(false);
                //yield return ThrowPokeball();
            }
            //逃跑
            else if(playerAction==BattleAction.Run)
            {
                dialogBox.EnableActonSelector(false);
                yield return TryToEscape();
            }

            //交换结束敌人回合
            var enemyMove=enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }
        //战斗没结束 回到选择行动上
        if (state != BattleState.BattleOver)
            ActionSelection();
    }


    /// <summary>
    /// 执行战斗行动
    /// </summary>
    /// <param name="sourceUnit">行动方</param>
    /// <param name="targetUnit">敌方</param>
    /// <param name="move">行动方技能</param>
    /// <returns></returns>
    IEnumerator RunMove(BattleUnit sourceUnit,BattleUnit targetUnit,Move move)
    {
        //战斗开始前检测行动方是否能移动 不能移动无法行动
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if(!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hub.WaitForHPUpdate();//解决混乱没有更新HP 
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        //pp-1
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}使用了{move.Base.Name}");

        //判断技能是否命中
        if (CheckIfMoveHit(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            //攻击动画 -音效
            sourceUnit.PlayAttackAnimation();
            AudioManager.i.PlaySfx(move.Base.Sound);

            yield return new WaitForSeconds(1f);

            //受击动画 -音效
            targetUnit.PlayHitAnimation();
            AudioManager.i.PlaySfx(AudioId.Hit);

            //是否是状态变化技能
            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon,move.Base.Target);

            }
            else
            {
                //宝可梦HP是否归零 是退场 
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                //血量变化 改变UI
                yield return targetUnit.Hub.WaitForHPUpdate();
                yield return ShowDamgeDetails(damageDetails);
            }

            //第二种状态生效条件
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
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}攻击未命中!");
        }


            //回合结束后 判断伤害
              //sourceUnit.Pokemon.OnAfterTurn();  在战斗结束后 两次持续伤害判定 bug
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hub.UpdateHPAsync();

            if (sourceUnit.Pokemon.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}倒下了");
                sourceUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(sourceUnit);
            }

    }

    /// <summary>
    /// 行动 技能影响
    /// </summary>
    /// <param name="effects">效果</param>
    /// <param name="source">攻击方</param>
    /// <param name="target">攻击目标</param>
    /// <param name="moveTarget">技能作用目标</param>
    /// <returns></returns>
    IEnumerator RunMoveEffects(MoveEffects effects ,Pokemon source,Pokemon target,MoveTarget moveTarget)
    {
        
        //状态 增幅类
        if (effects.Boosts != null)
        {
            //作用目标是否为自己
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        //状态 异常类
        if (effects.Status != ConditionID.无)
        {
            target.SetStatus(effects.Status);
        }

        //不稳定状态 混乱等/异常类
        if (effects.VolatileStatus != ConditionID.无)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }


        //显示宝可梦状态
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    /// <summary>
    /// 行动结束后的 回合 
    /// </summary>
    /// <param name="sourceUnit"></param>
    /// <returns></returns>
    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        //战斗结束 不执行协程
        if (state == BattleState.BattleOver) yield break;
        //当 状态 state=BattleState.RunningTurn时 才执行下面的代码
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        //燃烧 或者中毒在回合结束后判定
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        yield return sourceUnit.Hub.WaitForHPUpdate();

        if (sourceUnit.Pokemon.HP <= 0)
        {
           StartCoroutine(HandlePokemonFainted(sourceUnit));

            //当 状态 state=BattleState.RunningTurn时 才执行下面的代码
            yield return new WaitUntil(() => state == BattleState.RunningTurn);

        }
    }
    /// <summary>
    /// 命中判定
    /// </summary>
    /// <param name="move">技能</param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    bool CheckIfMoveHit(Move move,Pokemon source,Pokemon target)
    {
        //必中
        if (move.Base.AlwaysHit)
            return true;

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.命中率];
        int evasion = target.StatBoosts[Stat.闪避率];

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
    /// 状态出队 显示在对话框上
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
    /// 处理宝可梦  的倒下后行为
    /// </summary>
    /// <param name="faintedUnit"></param>
    /// <returns></returns>
    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {

        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name}倒下了");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        //战斗胜利  播放音乐
        if(!faintedUnit.IsPlayerUnit)
        {
            bool battleWon = true;
            if (isTrainerBattle)
                //为空说明没有健康的宝可梦了
                battleWon = trainerParty.GetHealthyPokemon() == null;
            if (battleWon)
                AudioManager.i.PlayMusic(battleVictoryMusic);


            //获得经验
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;

            //是训练家获得多
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus / 7);
            playerUnit.Pokemon.Exp += expYield;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}获得了{expGain}经验值");

            //设置经验条
           yield return playerUnit.Hub.SetExpSmooth();
            //检查升级
            while(playerUnit.Pokemon.CheckForLevelUp())
            {
                //重新设计等级
                playerUnit.Hub.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}等级提升了{playerUnit.Pokemon.Level}级");

                //学习技能
               var learnMove=  playerUnit.Pokemon.GetLearnableMoveAtCurrLevel();

                if(learnMove!=null)
                {
                    if(playerUnit.Pokemon.Moves.Count<PokemonBase.MaxNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(learnMove.MoveBase);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}学会了{learnMove.MoveBase.Name}");
                        //重新设置技能UI的显示
                        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}想要学习{learnMove.MoveBase.Name}");
                        yield return dialogBox.TypeDialog($"但是技能已经满了");
                        //选择忘记一个技能，添加新的技能
                        yield return ChooseMoveToForget(playerUnit.Pokemon, learnMove.MoveBase);

                       //直到状态不为 技能忘记状态结束才执行 下一行的代码
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
    /// 当宝可梦倒下时的行为 检查战斗结束
    /// </summary>
    /// <param name="faintedUnit">倒下的宝可梦</param>
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
            yield return dialogBox.TypeDialog("造成暴击!");
        }
        if(damageDetails.TypeEffectiveness>1f)
        {
            yield return dialogBox.TypeDialog("十分效果!");
        }
        else if(damageDetails.TypeEffectiveness <1f)
        {
            yield return dialogBox.TypeDialog("效果不好");
        }
    }

    
    /// <summary>
    ///战斗中的行为逻辑 函数/约束
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
            //事件onItemUsed 有参来判定行为
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
                  //选择遗忘新的技能
                  if(moveIndex==PokemonBase.MaxNumOfMoves)
                  {
                      //不学习新技能
                      StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}没有学习{moveToLearn.Name}"));
                  }
                  else
                  {
                      //忘记已有的技能，学习新的技能
                      var selectedMove = playerUnit.Pokemon.Moves[moveIndex];
                      StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}忘记{selectedMove.Base.Name}学习了{moveToLearn.Name}"));

                      //赋值替换 技能 忘记 学习
                      playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                  }
                  moveToLearn = null;
                  state = BattleState.RunningTurn;
              };


            moveSelectonUI.HandleMoveSelection(onMoveSelected);
        }

    }

    /// <summary>
    /// 根据用户选则选中 行为
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
        //限定currenAction的范围
        currentAction = Mathf.Clamp(currentAction, 0, 3);

        //BattleDialogBox类下 UpdateActionSelection（玩家行为被选中时）方法
        dialogBox.UpdateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            //选中战斗
            if(currentAction==0)
            {
                MoveSelection();
            }
            else if(currentAction==1)
            {
                //背包
                //仍一个球
                //StartCoroutine(RunTurns(BattleAction.UseItem));
                OpenBag();
            }
            else if (currentAction == 2)
            {
                //宝可梦
                //记录之前的状态
                //prevState = state; 写进方法里了
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    //根据用户选则 选中 技能
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

        //确定技能 调用施放技能
        if(Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        //重新选择返回 玩家行为
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
              //获得队伍中当前选中的宝可梦
              var selectMember = partyScreen.SelectedMember;
              if (selectMember.HP <= 0)
              {
                  partyScreen.SetMessageText("宝可梦无法战斗");
                  return;
              }
              if (selectMember == playerUnit.Pokemon)
              {
                  partyScreen.SetMessageText("当前出场为选中的宝可梦");
                  return;
              }

              partyScreen.gameObject.SetActive(false);

              //如果玩家在战斗回合时交换宝可梦 则执行RunTurns
              if (partyScreen.CalledFrom == BattleState.ActionSelection)
              {
                  //prevState = null;
                  StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
              }
              else//宝可梦晕倒 选择切换
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
                  partyScreen.SetMessageText("请选择宝可梦");
                  return;
              }

              partyScreen.gameObject.SetActive(false);

              //如果是发生战斗一方倒下 按X状态为null
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
    /// 是否切换宝可梦
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
                //当玩家打开队伍切换宝可梦结束时
                //训练家派出宝可梦
                //prevState = BattleState.AboutToUse; 写进方法里了
                OpenPartyScreen();
            }
            else
            {
                //没有 打开
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
    /// 更换宝可梦
    /// </summary>
    /// <param name="newPokemon">要切换的宝可梦</param>
    /// <returns></returns>
    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse=false)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"回来吧！{playerUnit.Pokemon.Base.Name}");
            //切换动画
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);

        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog("去吧 " + newPokemon.Base.Name + "！");


        //当玩家在敌方宝可梦归零? 切换宝可梦的时候 是/否
        if (isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerPokemon());
        else
            state = BattleState.RunningTurn;

        
    }

    /// <summary>
    /// 派出下一健康的宝可梦
    /// </summary>
    /// <returns></returns>
    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name}派出{ nextPokemon.Base.Name}");

        state = BattleState.RunningTurn;
    }

    /// <summary>
    /// 执行判定物品使用/扔球行为 等待至扔球行为完成
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
    /// 扔球行为
    /// </summary>
    /// <returns></returns>
    IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        state = BattleState.Busy;
        //不能捕获训练家的宝可梦
        if(isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"你不能对训练家的宝可梦使用精灵球");
            state = BattleState.RunningTurn;
            yield break;
        }

      yield return dialogBox.TypeDialog($"{player.Name}扔出{pokeballItem.Name}!");
       var pokeballObj= Instantiate(pokeballSprite, playerUnit.transform.position-new Vector3(2,0), Quaternion.identity);
        var pokeball = pokeballObj.gameObject.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.Icon;

        //动画
        //直到动画完成才执行下一个
       yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();

        //进入精灵球动画
       yield return enemyUnit.PlayCaptureAnimation();
        //掉下
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1.5f, 0.5f).WaitForCompletion();

       
        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon,pokeballItem);

        //摇晃
        for (int i = 0; i < Mathf.Min(shakeCount,3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }
        if(shakeCount==4)
        {
            //捕获成功
            yield return dialogBox.TypeDialog($"捕获{enemyUnit.Pokemon.Base.Name}成功了!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}加入了你的队伍");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            //捕获失败
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
           yield return enemyUnit.PlayBreakOutAnimation();

            if(shakeCount<2)
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}捕获失败了!");
            else
                yield return dialogBox.TypeDialog($"还差一点就成功了!");

            Destroy(pokeball);

            state = BattleState.RunningTurn;

        }
    }


    /// <summary>
    /// 计算捕获是否成功
    /// </summary>
    /// <param name="pokemon">宝可梦</param>
    /// <returns>摇晃次数 如果等于四次则成功捕获</returns>
    int TryToCatchPokemon(Pokemon pokemon,PokeballItem pokeballItem)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate *
            pokeballItem.CatchRateModfier * ConditionDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);
        //大于255时
        if (a >= 255)
            //摇晃四下 成功捕获
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
    /// 逃跑
    /// </summary>
    /// <returns></returns>
    IEnumerator TryToEscape()
    {
        if(isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("你不能在训练家的战斗中逃跑");
            state = BattleState.RunningTurn;
            yield break;
        }
        ++escapeAttempts;
        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if(enemySpeed<playerSpeed)
        {
            yield return dialogBox.TypeDialog($"成功逃走了");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if(UnityEngine.Random.Range(0,256)<f)
            {
                yield return dialogBox.TypeDialog($"成功逃走了");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"逃走失败了");
                //回到行动 状态中
                state = BattleState.RunningTurn;
            }
        }
    }
}
