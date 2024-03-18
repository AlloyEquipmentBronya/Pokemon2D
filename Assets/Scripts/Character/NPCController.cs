using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState { Idle,Walking,Dialog}
public class NPCController : MonoBehaviour,Interactable,ISavable
{
    [SerializeField] Dialog dialog;

    [Header("����")]
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToComlete;

    [Header("�ƶ�")]
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    NPCState state;
    float idleTimer;
    int currentPattern = 0;
    Quest activeQuest;
    Character character;
    ItemGiver itemGiver;
    PokemonGiver pokemonGiver;
    Healer healer;
    Merchant merchant;

    private void Awake()
    {
        character = GetComponent<Character>();
        //���Դӵ�ǰ���������ӵ�����л�ȡ ItemGiver �����
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
        healer = GetComponent<Healer>();
        merchant = GetComponent<Merchant>();
    }

    /// <summary>
    /// ������Ϊ
    /// </summary>
    /// <param name="initiator">��������</param>
    public IEnumerator Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);
            //��ñ����� ����
            if(questToComlete!=null)
            {
                var quest = new Quest(questToComlete);
                //ֱ��������� �����ƿ���ʧ
                yield return quest.CompleteQuest(initiator);
                questToComlete = null;

                Debug.Log($"{quest.Base.Name} �����");
            }
            //��Ʒ����
            if(itemGiver!=null&& itemGiver.CanBeGiven())
            {
               yield return itemGiver.GiveItem(initiator.GetComponent<PlayerControl>());
            }
            else if (pokemonGiver != null && pokemonGiver.CanBeGiven())
            {
                yield return pokemonGiver.GivePokemon(initiator.GetComponent<PlayerControl>());
            }
            else if(questToStart!=null)
            {
                activeQuest = new Quest(questToStart);
                yield return activeQuest.StartQuest();
                //���ظ�����
                questToStart = null;

                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
            }
            else if(activeQuest!=null)
            {
                if(activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
                else
                {
                    yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialogue);
                }
            }
            else if(healer!=null)
            {
                yield return healer.Heal(initiator, dialog);
            }
            else if(merchant!=null)
            {
                yield return merchant.Trade();
            }
            else
            {
                yield return DialogManager.Instance.ShowDialog(dialog);
            }
            idleTimer = 0f;
            state = NPCState.Idle;
           
        }
       
    }
    private void Update()
    {
        if(state==NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if(idleTimer>timeBetweenPattern)
            {
                idleTimer = 0f;
                if(movementPattern.Count>0)
                StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();

    }
    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

       yield return character.Move(movementPattern[currentPattern]);
        if(transform.position!=oldPos)

        currentPattern = (currentPattern + 1)%movementPattern.Count;

        state = NPCState.Idle;
    }

    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();
        saveData.activeQuest = activeQuest?.GetSaveData();
        if (questToStart != null)
            saveData.questToStart = (new Quest(questToStart)).GetSaveData();
        if (questToComlete != null)
            saveData.questToComplete = (new Quest(questToComlete)).GetSaveData();

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as NPCQuestSaveData;
        
        if(saveData!=null)
        {
            activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;

            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToComlete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;
        }
    }
}
[System.Serializable]
public class NPCQuestSaveData
{
    public QuestSavaData activeQuest;
    public QuestSavaData questToStart;
    public QuestSavaData questToComplete;
}
