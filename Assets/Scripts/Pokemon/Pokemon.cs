using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField]  int _level;


    public Pokemon(PokemonBase pBase,int pLevel)
    {
        _base = pBase;
        _level = pLevel;

        Init();
    }


    //PokemonBase 类型的属性
   public PokemonBase Base { get { return _base; } }
   public int Level {
        get { return _level; }
        set { _level = value; }
    }

    /// <summary>
    /// 获得的经验值
    /// </summary>
    public int Exp { get; set; }

    //控制HP
    public int HP { get; set; }
    //声明技能类 的集合属性 
    public List<Move> Moves { get; set; }

    public Move CurrentMove { get; set;}

    /// <summary>
    /// 键值对集合属性  存储数值变化
    /// </summary>
    public Dictionary<Stat,int> Stats { get; private set; }
    /// <summary>
    /// 键值对集合的属性 存储提升降低/等级
    /// </summary>
    public Dictionary<Stat,int> StatBoosts { get; private set; }
    /// <summary>
    /// 队列属性存储 各种状态消息
    /// </summary>
    public Queue<string> StatusChanges { get; private set; }
    /// <summary>
    /// 状态类型 属性
    /// </summary>
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }

    /// <summary>
    /// 不稳定状态/战斗结束消失
    /// </summary>
    public Condition VolatileStatus { get; private set; }
    /// <summary>
    /// 战斗结束状态消失 时间/回合
    /// </summary>
    public int VolatileStatusTime { get; set; }

    /// <summary>
    /// 委托返回事件  控制的状态变化
    /// </summary>
    public event System.Action OnStatusChanged;
    /// <summary>
    /// 委托返回事件  控制的血量变化
    /// </summary>
    public event System.Action OnHPChanged;

    public void Init()
    {
     
        //创建一个集合用来存放宝可梦的初始技能
        Moves = new List<Move>();
        //宝可梦升级时添加技能
        foreach (var move in Base.LearnableMoves)
        {
            if(move.Level<=Level)
            {
                Moves.Add(new Move(move.MoveBase));
            }
            //最大技能数限制
            if(Moves.Count>=PokemonBase.MaxNumOfMoves)
            {
                break;
            }
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;


        StatusChanges = new Queue<string>();
        //初始提升等级为0
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;

    }

    /// <summary>
    /// 用构造函数加载 数据
    /// </summary>
    /// <param name="saveData"></param>
    public Pokemon(PokemonSaveData saveData)
    {
        //基础数据 
        _base = PokemonDB.GetObjectByName(saveData.name);

        HP = saveData.hp;
        Level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusId != null)
            Status = ConditionDB.Conditions[saveData.statusId.Value];
        else
            Status = null;

        //通过构造函数加载数据
        Moves = saveData.moves.Select(s => new Move(s)).ToList();


        CalculateStats();

        StatusChanges = new Queue<string>();
        //初始提升等级为0
        ResetStatBoost();
        VolatileStatus = null;
    }

    /// <summary>
    /// 获得要保存的宝可梦的数据
    /// </summary>
    /// <returns></returns>
    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList(),
            
        };
        return saveData;
    }



    /// <summary>
    /// 计算统计
    /// </summary>
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.攻击, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.防御, Mathf.FloorToInt((Base.Defence * Level) / 100f) + 5);
        Stats.Add(Stat.特攻, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.特防, Mathf.FloorToInt((Base.SpDefence * Level) / 100f) + 5);
        Stats.Add(Stat.速度, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        int oldMaxHp = MaxHp;

        MaxHp= Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10+Level;
        if (oldMaxHp != 0)
            HP += MaxHp - oldMaxHp;
    }

    /// <summary>
    /// 状态归零
    /// </summary>
    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.攻击,0},
            {Stat.防御,0},
            {Stat.特攻,0 },
            {Stat.特防,0 },
            {Stat.速度,0 },
            {Stat.命中率,0 },
            {Stat.闪避率,0 }
        };
    }

    /// <summary>
    /// 获得改变后的数值
    /// </summary>
    /// <param name="stat">类型</param>
    /// <returns></returns>
    int GetStat(Stat stat)
    {
        //数值变化值
        int statVal = Stats[stat];

        //等级变化值
        int boost = StatBoosts[stat];
        //等级带来的倍率变化
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);


        return statVal;
    }


    /// <summary>
    /// 把List的值存到 字典集合中(赋予效果等级和类型)
    /// </summary>
    /// <param name="statBoosts">list 类型和等级</param>
    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            //状态入队
            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}的{stat}提升了");
            else
                StatusChanges.Enqueue($"{Base.Name}的{stat}降低了");

           StatBoosts[stat]= Mathf.Clamp(StatBoosts[stat] + boost,-6,6);

            Debug.Log($"{stat}变化了{StatBoosts[stat]}");
        }
    }

    /// <summary>
    /// 检查是不是能升级
    /// </summary>
    /// <returns></returns>
    public bool CheckForLevelUp()
    {
        if(Exp>Base.GetExpForLevel(_level+1))
        {
            ++_level;
            CalculateStats();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 升级时获得可以升级的技能
    /// </summary>
    /// <returns></returns>
    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == _level).FirstOrDefault();
    }


    /// <summary>
    /// 学习技能
    /// </summary>
    /// <param name="moveToLearn">学习的技能</param>
    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
            return;
        Moves.Add(new Move(moveToLearn));
    }

    /// <summary>
    /// 是否存在技能
    /// </summary>
    /// <param name="moveToCheck"></param>
    /// <returns></returns>
    public bool HasMove(MoveBase moveToCheck)
    {
        return Moves.Count(m => m.Base == moveToCheck) > 0;
        
    }
    /// <summary>
    /// 检测宝可梦进化 等级
    /// </summary>
    /// <returns></returns>
    public Evolution CheckForEvolution()
    {
        //进化列表中找到对应等级的进化对象
      return  Base.Evolutions.FirstOrDefault(e => e.ReqquiredLevel <= Level);
    }
    /// <summary>
    /// 检测宝可梦进化 物品
    /// </summary>
    /// <param name="item">进化物品</param>
    /// <returns></returns>
    public Evolution CheckForEvolution(ItemBase item)
    {
        //进化列表中找到对应 需求物品
        return Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);
    }
    /// <summary>
    /// 进化改变
    /// </summary>
    /// <param name="evolution"></param>
    public void Evolve(Evolution evolution)
    {
        // 改变宝可梦的种类
        _base = evolution.EvolvesInto;
        //重新计算所有的数据
        CalculateStats();
    }
    /// <summary>
    /// 治疗HP-异常状态
    /// </summary>
    public void Heal()
    {
        HP = MaxHp;
        OnHPChanged?.Invoke();
        CureStatus();
    }


    public int Attack
    {
        get { return GetStat(Stat.攻击); }
    }
    public int Defense
    {
        get { return GetStat(Stat.防御); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.特攻); }
    }
    public int SpDefence
    {
        get { return GetStat(Stat.特防); }
    }
    public int Speed
    {
        get { return GetStat(Stat.速度); }
    }
    public int MaxHp
    {
        get;
        private set;
    }

    public DamageDetails TakeDamage(Move move,Pokemon attacker)
    {
        //初始暴击值
        float critical= 1f;
        if(Random.value<=6.25f)
        {
            critical = 2f;
        }

        //宝可梦有两个种属性
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            //这个代码段则是为 damageDetails 这个变量设置了初始值。
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };
        //判断技能类型 返回攻击类型 物/特
       float attack= (move.Base.Category==MoveCategory.Special) ? attacker.SpAttack:attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefence : Defense;

        float modifiers = Random.Range(0.85f, 1f)*type*critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        DecreaseHP(damage);

        return damageDetails;
    }

    /// <summary>
    /// 增加HP
    /// </summary>
    /// <param name="amount"></param>
    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
    }
    /// <summary>
    /// HP减少
    /// </summary>
    /// <param name="damage"></param>
    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    /// <summary>
    /// 获得  状态 
    /// </summary>
    /// <param name="conditionID">类型</param>
    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null)return;
        Status= ConditionDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}{Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    /// <summary>
    /// 自愈 解除异常状态
    /// </summary>
   public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    /// <summary>
    /// 不稳定状态 设置
    /// </summary>
    /// <param name="conditionID"></param>
    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (VolatileStatus != null) return;
        VolatileStatus = ConditionDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}{VolatileStatus.StartMessage}");
       
    }

    /// <summary>
    /// 不稳定状态  自愈/解除
    /// </summary>
    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Move GetRandomMove()
    {
       var movesWithpp= Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithpp.Count);
        return movesWithpp[r];
    }

    /// <summary>
    /// 宝可梦 回合结束 受到伤害  
    /// </summary>
    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }
    /// <summary>
    /// 回合开始判定 异常状态 无法行动
    /// </summary>
    /// <returns></returns>
    public bool OnBeforeMove()
    {
        //canPerformMove 是否 可以执行移动判断
        bool canPerformMove = true;
        if(Status?.OnBeforeMove!=null)
        {
            if (!Status.OnBeforeMove(this))
                canPerformMove = false;
        }
        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;
        }
        return canPerformMove;
    }
    /// <summary>
    /// 战斗结束重置效果(增幅类)状态等级 / 不稳定状态(麻痹等等)
    /// </summary>
    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }

}
//用于暴击 克制 消息
public class DamageDetails
{
    public bool Fainted { get; set; }

    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}
[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;

}


