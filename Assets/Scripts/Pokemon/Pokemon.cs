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


    //PokemonBase ���͵�����
   public PokemonBase Base { get { return _base; } }
   public int Level {
        get { return _level; }
        set { _level = value; }
    }

    /// <summary>
    /// ��õľ���ֵ
    /// </summary>
    public int Exp { get; set; }

    //����HP
    public int HP { get; set; }
    //���������� �ļ������� 
    public List<Move> Moves { get; set; }

    public Move CurrentMove { get; set;}

    /// <summary>
    /// ��ֵ�Լ�������  �洢��ֵ�仯
    /// </summary>
    public Dictionary<Stat,int> Stats { get; private set; }
    /// <summary>
    /// ��ֵ�Լ��ϵ����� �洢��������/�ȼ�
    /// </summary>
    public Dictionary<Stat,int> StatBoosts { get; private set; }
    /// <summary>
    /// �������Դ洢 ����״̬��Ϣ
    /// </summary>
    public Queue<string> StatusChanges { get; private set; }
    /// <summary>
    /// ״̬���� ����
    /// </summary>
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }

    /// <summary>
    /// ���ȶ�״̬/ս��������ʧ
    /// </summary>
    public Condition VolatileStatus { get; private set; }
    /// <summary>
    /// ս������״̬��ʧ ʱ��/�غ�
    /// </summary>
    public int VolatileStatusTime { get; set; }

    /// <summary>
    /// ί�з����¼�  ���Ƶ�״̬�仯
    /// </summary>
    public event System.Action OnStatusChanged;
    /// <summary>
    /// ί�з����¼�  ���Ƶ�Ѫ���仯
    /// </summary>
    public event System.Action OnHPChanged;

    public void Init()
    {
     
        //����һ������������ű����εĳ�ʼ����
        Moves = new List<Move>();
        //����������ʱ��Ӽ���
        foreach (var move in Base.LearnableMoves)
        {
            if(move.Level<=Level)
            {
                Moves.Add(new Move(move.MoveBase));
            }
            //�����������
            if(Moves.Count>=PokemonBase.MaxNumOfMoves)
            {
                break;
            }
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;


        StatusChanges = new Queue<string>();
        //��ʼ�����ȼ�Ϊ0
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;

    }

    /// <summary>
    /// �ù��캯������ ����
    /// </summary>
    /// <param name="saveData"></param>
    public Pokemon(PokemonSaveData saveData)
    {
        //�������� 
        _base = PokemonDB.GetObjectByName(saveData.name);

        HP = saveData.hp;
        Level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusId != null)
            Status = ConditionDB.Conditions[saveData.statusId.Value];
        else
            Status = null;

        //ͨ�����캯����������
        Moves = saveData.moves.Select(s => new Move(s)).ToList();


        CalculateStats();

        StatusChanges = new Queue<string>();
        //��ʼ�����ȼ�Ϊ0
        ResetStatBoost();
        VolatileStatus = null;
    }

    /// <summary>
    /// ���Ҫ����ı����ε�����
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
    /// ����ͳ��
    /// </summary>
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.����, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.����, Mathf.FloorToInt((Base.Defence * Level) / 100f) + 5);
        Stats.Add(Stat.�ع�, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.�ط�, Mathf.FloorToInt((Base.SpDefence * Level) / 100f) + 5);
        Stats.Add(Stat.�ٶ�, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        int oldMaxHp = MaxHp;

        MaxHp= Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10+Level;
        if (oldMaxHp != 0)
            HP += MaxHp - oldMaxHp;
    }

    /// <summary>
    /// ״̬����
    /// </summary>
    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.����,0},
            {Stat.����,0},
            {Stat.�ع�,0 },
            {Stat.�ط�,0 },
            {Stat.�ٶ�,0 },
            {Stat.������,0 },
            {Stat.������,0 }
        };
    }

    /// <summary>
    /// ��øı�����ֵ
    /// </summary>
    /// <param name="stat">����</param>
    /// <returns></returns>
    int GetStat(Stat stat)
    {
        //��ֵ�仯ֵ
        int statVal = Stats[stat];

        //�ȼ��仯ֵ
        int boost = StatBoosts[stat];
        //�ȼ������ı��ʱ仯
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);


        return statVal;
    }


    /// <summary>
    /// ��List��ֵ�浽 �ֵ伯����(����Ч���ȼ�������)
    /// </summary>
    /// <param name="statBoosts">list ���ͺ͵ȼ�</param>
    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            //״̬���
            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}��{stat}������");
            else
                StatusChanges.Enqueue($"{Base.Name}��{stat}������");

           StatBoosts[stat]= Mathf.Clamp(StatBoosts[stat] + boost,-6,6);

            Debug.Log($"{stat}�仯��{StatBoosts[stat]}");
        }
    }

    /// <summary>
    /// ����ǲ���������
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
    /// ����ʱ��ÿ��������ļ���
    /// </summary>
    /// <returns></returns>
    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == _level).FirstOrDefault();
    }


    /// <summary>
    /// ѧϰ����
    /// </summary>
    /// <param name="moveToLearn">ѧϰ�ļ���</param>
    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
            return;
        Moves.Add(new Move(moveToLearn));
    }

    /// <summary>
    /// �Ƿ���ڼ���
    /// </summary>
    /// <param name="moveToCheck"></param>
    /// <returns></returns>
    public bool HasMove(MoveBase moveToCheck)
    {
        return Moves.Count(m => m.Base == moveToCheck) > 0;
        
    }
    /// <summary>
    /// ��ⱦ���ν��� �ȼ�
    /// </summary>
    /// <returns></returns>
    public Evolution CheckForEvolution()
    {
        //�����б����ҵ���Ӧ�ȼ��Ľ�������
      return  Base.Evolutions.FirstOrDefault(e => e.ReqquiredLevel <= Level);
    }
    /// <summary>
    /// ��ⱦ���ν��� ��Ʒ
    /// </summary>
    /// <param name="item">������Ʒ</param>
    /// <returns></returns>
    public Evolution CheckForEvolution(ItemBase item)
    {
        //�����б����ҵ���Ӧ ������Ʒ
        return Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);
    }
    /// <summary>
    /// �����ı�
    /// </summary>
    /// <param name="evolution"></param>
    public void Evolve(Evolution evolution)
    {
        // �ı䱦���ε�����
        _base = evolution.EvolvesInto;
        //���¼������е�����
        CalculateStats();
    }
    /// <summary>
    /// ����HP-�쳣״̬
    /// </summary>
    public void Heal()
    {
        HP = MaxHp;
        OnHPChanged?.Invoke();
        CureStatus();
    }


    public int Attack
    {
        get { return GetStat(Stat.����); }
    }
    public int Defense
    {
        get { return GetStat(Stat.����); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.�ع�); }
    }
    public int SpDefence
    {
        get { return GetStat(Stat.�ط�); }
    }
    public int Speed
    {
        get { return GetStat(Stat.�ٶ�); }
    }
    public int MaxHp
    {
        get;
        private set;
    }

    public DamageDetails TakeDamage(Move move,Pokemon attacker)
    {
        //��ʼ����ֵ
        float critical= 1f;
        if(Random.value<=6.25f)
        {
            critical = 2f;
        }

        //������������������
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            //������������Ϊ damageDetails ������������˳�ʼֵ��
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };
        //�жϼ������� ���ع������� ��/��
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
    /// ����HP
    /// </summary>
    /// <param name="amount"></param>
    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
    }
    /// <summary>
    /// HP����
    /// </summary>
    /// <param name="damage"></param>
    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    /// <summary>
    /// ���  ״̬ 
    /// </summary>
    /// <param name="conditionID">����</param>
    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null)return;
        Status= ConditionDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}{Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    /// <summary>
    /// ���� ����쳣״̬
    /// </summary>
   public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    /// <summary>
    /// ���ȶ�״̬ ����
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
    /// ���ȶ�״̬  ����/���
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
    /// ������ �غϽ��� �ܵ��˺�  
    /// </summary>
    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }
    /// <summary>
    /// �غϿ�ʼ�ж� �쳣״̬ �޷��ж�
    /// </summary>
    /// <returns></returns>
    public bool OnBeforeMove()
    {
        //canPerformMove �Ƿ� ����ִ���ƶ��ж�
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
    /// ս����������Ч��(������)״̬�ȼ� / ���ȶ�״̬(��Եȵ�)
    /// </summary>
    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }

}
//���ڱ��� ���� ��Ϣ
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


