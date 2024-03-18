using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection,PartySelection,MoveToForget,Busy}

public class InventoryUI : MonoBehaviour
{
    //�Ӷ��� ��Ʒ UI�ϼ�
    [SerializeField] GameObject itemList;
    //Ϊ UI��ֵ����Ʒ��
    [SerializeField] ItemSlotUI itemSlotUI;

    //��������
    [SerializeField] Text categoryText;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectonUI moveSelectonUI;

    Action<ItemBase> onItemUsed;

    int selectedItem = 0;
    int selectedCategory = 0;

    MoveBase moveToLearn;

    const int itemsInViewport = 8;

    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    InventoryUIState state;
    RectTransform itemListRect;

    private void Awake()
    {
        //�����Ҷ����µ�  ��Ʒ/���������
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        
        UpdateItemList();
        inventory.onUpdated += UpdateItemList;
    }

    /// <summary>
    /// ������ƷUI����
    /// </summary>
    void UpdateItemList()
    {
        //��������Ӷ���
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        //�����Ҷ����µ���Ʒ����
        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {

            //�ڣ����࣬���ࣩ ��������������
          var slotUIObj=  Instantiate(itemSlotUI, itemList.transform);
            //ΪUi��ֵ
            slotUIObj.SetData(itemSlot);

            //�����ɵ�ÿһ����뵽������  ���ڸ���ѡ��Ч��
            slotUIList.Add(slotUIObj);
        }
        //��ʱ����ѡ�е���ɫ
          UpdateItemSelection();
    }


    /// <summary>
    /// ����/���  �����߼�����  
    /// </summary>
    /// <param name="onBack">����</param>
    /// <param name="onItemUsed">ѡ����Ʒִ�е��߼� ���򡢳��� ʹ��</param>
    public void HandleUpdate(Action onBack,Action<ItemBase> onItemUsed=null)
    {
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelecton = selectedItem;
            int prevCategory = selectedCategory;

            if (Input.GetKeyDown(KeyCode.DownArrow))
                selectedItem++;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                selectedItem--;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                selectedCategory++;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                selectedCategory--;

            //ѭ�� �л�
            if (selectedCategory > Inventory.ItemCategories.Count - 1)
                selectedCategory = 0;
            else if (selectedCategory < 0)
                selectedCategory = Inventory.ItemCategories.Count - 1;

            //selectedCategory = Mathf.Clamp(selectedCategory, 0, Inventory.ItemCategories.Count - 1);
            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            //�±�䶯ʱ �����б� ���л�ҳ��
            if (prevCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            }

            else if (prevSelecton != selectedItem)
            {
                UpdateItemSelection();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                //�򿪶���
                // OpenPartyScreen();
                //ʹ����Ʒ
                StartCoroutine(ItemSelected());
            }
            else if (Input.GetKeyDown(KeyCode.X))
                onBack?.Invoke();
        }
        //�������л��������ζ������ʱ
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelection = () =>
            {
                //ʹ����Ʒ Э��
                StartCoroutine(UseItem());
            };
            Action onBackPartyScreen = () =>
              {
                  ClosePartyScreen();
              };

            //����PartyScreen ��ѡ�з���
            partyScreen.HandleUpdate(onSelection, onBackPartyScreen);
        }
        else if (state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (int moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelected(moveIndex));
            };

            moveSelectonUI.HandleMoveSelection(onMoveSelected);
        }
    }
   /// <summary>
   /// ���� ѡ������ ִ�� 
   /// </summary>
    IEnumerator ItemSelected()
    {
        //ȷ��Э������ʱ�������������鷢��
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);
        //�� �̵��е���Ϊ
        if(GameControlller.Instance.State==GameState.Shop)
        {
            onItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;
        }
            //ս��״̬
        if(GameControlller.Instance.State==GameState.Battle)
        {
            //��ս����
            if(!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"�����Ʒ������ս����ʹ��");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            //����ս����
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"�����Ʒ���ڲ���ʹ��");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }

        //ѡ�е��Ǿ�����ʱֱ��ʹ����Ʒ
        //�����ָ���Ʒ�ȵȴ򿪶���ʹ��
        if (selectedCategory == (int)ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
            //���Ӽ���ѧϰ�б�� �����ζ�������� ��ʾ ѧϰ�����ı�
            if (item is TmItem)
                partyScreen.ShowIfTmIsUsable(item as TmItem);

        }
    }


    /// <summary>
    /// ʹ����Ʒ
    /// </summary>
    /// <returns></returns>
    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandleTmItems();

       var item=inventory.GetItem(selectedItem, selectedCategory);
        var pokemon= partyScreen.SelectedMember;

        //���������Ʒ
        if (item is EvolutionItem)
        {
            //��ⱦ���ν��� -������Ʒ
            var evolution= pokemon.CheckForEvolution(item);
            if (evolution != null)
            {
                yield return EvolutionManger.i.Evolve(pokemon, evolution);
            }
            else
            {
                yield return DialogManager.Instance.ShowDialogText("��Ʒû��Ч��");
                ClosePartyScreen();
                yield break;
            }
        }

       var usedItem= inventory.UseItem(selectedItem, partyScreen.SelectedMember,selectedCategory);
        if(usedItem!=null)
        {
            if((usedItem is RecoverItem))
            yield return DialogManager.Instance.ShowDialogText($"���ʹ����{usedItem.Name}");
            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            if(selectedCategory==(int)ItemCategory.Items)
            yield return DialogManager.Instance.ShowDialogText("��Ʒû��Ч��");
        }
        ClosePartyScreen();
    }

    /// <summary>
    /// ����ѧϰ������
    /// </summary>
    /// <returns></returns>
    IEnumerator HandleTmItems()
    {
        var tmItem = inventory.GetItem(selectedItem, selectedCategory) as TmItem;
        if (tmItem == null)
            yield break;

        var pokemon= partyScreen.SelectedMember;

        //����ӵ���ж�
        if(pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}�Ѿ�ѧ����{tmItem.Move.Name}�ļ���");
            yield break;
        }
        //����ѧϰ�ļ����ж�
        if (!tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}����ѧϰ{tmItem.Move.Name}�ļ���");
            yield break;
        }

        if(pokemon.Moves.Count<PokemonBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}ѧ����{tmItem.Move.Name}");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}����ѧϰ{tmItem.Move.Name}");
            yield return DialogManager.Instance.ShowDialogText($"���Ǽ�����������{PokemonBase.MaxNumOfMoves}");
            yield return ChooseMoveToForget(pokemon, tmItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
        }

    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText($"��ѡ��Ҫ�����ļ���",true,false);
        moveSelectonUI.gameObject.SetActive(true);
        moveSelectonUI.SetMoveDate(pokemon.Moves.Select(x => x.Base).ToList(), newMove);

        //���¼��ܸ�ֵ��moveTolearn �Ա����
        moveToLearn = newMove;
        state = InventoryUIState.MoveToForget;
    }

    /// <summary>
    /// ����ѡ��Ч�� ��ɫ �ı� ����
    /// </summary>
    public void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);
        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (selectedItem == i)
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }

        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();
    }

    /// <summary>
    /// ����
    /// </summary>
    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        //���ݵ�ǰѡ������Ŀ��selectedItem������ͼ�пɼ���Ŀ������һ�루itemsInViewport/2���������Ҫ������λ�ã�scrollPos��
        float scrollPos = Mathf.Clamp(selectedItem-itemsInViewport/2,0,selectedItem) * slotUIList[0].Height;
        //�ı���Ʒ���� �� �߶�ֵ  //�л���Ч��
        itemListRect.localPosition = new Vector3(itemListRect.localPosition.x, scrollPos);


        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    /// <summary>
    /// �л�ҳ��ʱ����
    /// </summary>
    void ResetSelection()
    {
        selectedItem = 0;

        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }


    /// <summary>
    /// �򿪶������
    /// </summary>
    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        //�����ʾ���ı�
        partyScreen.ClaerMemberSlotMessage();
        partyScreen.gameObject.SetActive(false);
    }

    /// <summary>
    /// Э�� ���Ƽ�������/�滻�߼� 
    /// </summary>
    /// <param name="moveIndex"></param>
    /// <returns></returns>
    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        var pokemon = partyScreen.SelectedMember;

        DialogManager.Instance.CloseDialog();
        moveSelectonUI.gameObject.SetActive(false);
        //ѡ�������µļ���
        if (moveIndex == PokemonBase.MaxNumOfMoves)
        {
            //��ѧϰ�¼���
            yield return (DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}û��ѧϰ{moveToLearn.Name}"));
        }
        else
        {
            //�������еļ��ܣ�ѧϰ�µļ���
            var selectedMove = pokemon.Moves[moveIndex];
            yield return (DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}����{selectedMove.Base.Name}ѧϰ��{moveToLearn.Name}"));

            //��ֵ�滻 ���� ���� ѧϰ
            pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }
        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }
}
