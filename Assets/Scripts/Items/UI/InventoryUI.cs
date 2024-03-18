using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection,PartySelection,MoveToForget,Busy}

public class InventoryUI : MonoBehaviour
{
    //子对象 物品 UI合集
    [SerializeField] GameObject itemList;
    //为 UI赋值的物品槽
    [SerializeField] ItemSlotUI itemSlotUI;

    //类型描述
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
        //获得玩家对象下的  物品/库存对象组件
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        
        UpdateItemList();
        inventory.onUpdated += UpdateItemList;
    }

    /// <summary>
    /// 更新物品UI数据
    /// </summary>
    void UpdateItemList()
    {
        //清除所有子对象
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        //获得玩家对象下的物品属性
        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {

            //在（子类，父类） 父类下生成子类
          var slotUIObj=  Instantiate(itemSlotUI, itemList.transform);
            //为Ui赋值
            slotUIObj.SetData(itemSlot);

            //将生成的每一项加入到集合中  用于更新选中效果
            slotUIList.Add(slotUIObj);
        }
        //打开时更新选中的颜色
          UpdateItemSelection();
    }


    /// <summary>
    /// 背包/库存  更新逻辑方法  
    /// </summary>
    /// <param name="onBack">返回</param>
    /// <param name="onItemUsed">选中物品执行的逻辑 扔球、出售 使用</param>
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

            //循环 切换
            if (selectedCategory > Inventory.ItemCategories.Count - 1)
                selectedCategory = 0;
            else if (selectedCategory < 0)
                selectedCategory = Inventory.ItemCategories.Count - 1;

            //selectedCategory = Mathf.Clamp(selectedCategory, 0, Inventory.ItemCategories.Count - 1);
            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            //下标变动时 更新列表 （切换页面
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
                //打开队伍
                // OpenPartyScreen();
                //使用物品
                StartCoroutine(ItemSelected());
            }
            else if (Input.GetKeyDown(KeyCode.X))
                onBack?.Invoke();
        }
        //当界面切换到宝可梦队伍界面时
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelection = () =>
            {
                //使用物品 协程
                StartCoroutine(UseItem());
            };
            Action onBackPartyScreen = () =>
              {
                  ClosePartyScreen();
              };

            //调用PartyScreen 的选中方法
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
   /// 区分 选中类型 执行 
   /// </summary>
    IEnumerator ItemSelected()
    {
        //确保协程运行时不会有其他事情发生
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);
        //在 商店中的行为
        if(GameControlller.Instance.State==GameState.Shop)
        {
            onItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;
        }
            //战斗状态
        if(GameControlller.Instance.State==GameState.Battle)
        {
            //在战斗中
            if(!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"这个物品不能在战斗中使用");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            //不在战斗中
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"这个物品现在不能使用");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }

        //选中的是精灵球时直接使用物品
        //其他恢复物品等等打开队伍使用
        if (selectedCategory == (int)ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
            //当从技能学习列表打开 宝可梦队伍界面是 显示 学习技能文本
            if (item is TmItem)
                partyScreen.ShowIfTmIsUsable(item as TmItem);

        }
    }


    /// <summary>
    /// 使用物品
    /// </summary>
    /// <returns></returns>
    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandleTmItems();

       var item=inventory.GetItem(selectedItem, selectedCategory);
        var pokemon= partyScreen.SelectedMember;

        //处理进化物品
        if (item is EvolutionItem)
        {
            //检测宝可梦进化 -进化物品
            var evolution= pokemon.CheckForEvolution(item);
            if (evolution != null)
            {
                yield return EvolutionManger.i.Evolve(pokemon, evolution);
            }
            else
            {
                yield return DialogManager.Instance.ShowDialogText("物品没有效果");
                ClosePartyScreen();
                yield break;
            }
        }

       var usedItem= inventory.UseItem(selectedItem, partyScreen.SelectedMember,selectedCategory);
        if(usedItem!=null)
        {
            if((usedItem is RecoverItem))
            yield return DialogManager.Instance.ShowDialogText($"玩家使用了{usedItem.Name}");
            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            if(selectedCategory==(int)ItemCategory.Items)
            yield return DialogManager.Instance.ShowDialogText("物品没有效果");
        }
        ClosePartyScreen();
    }

    /// <summary>
    /// 技能学习处理函数
    /// </summary>
    /// <returns></returns>
    IEnumerator HandleTmItems()
    {
        var tmItem = inventory.GetItem(selectedItem, selectedCategory) as TmItem;
        if (tmItem == null)
            yield break;

        var pokemon= partyScreen.SelectedMember;

        //技能拥有判定
        if(pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}已经学会了{tmItem.Move.Name}的技能");
            yield break;
        }
        //不能学习的技能判定
        if (!tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}不能学习{tmItem.Move.Name}的技能");
            yield break;
        }

        if(pokemon.Moves.Count<PokemonBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}学会了{tmItem.Move.Name}");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}尝试学习{tmItem.Move.Name}");
            yield return DialogManager.Instance.ShowDialogText($"但是技能数超过了{PokemonBase.MaxNumOfMoves}");
            yield return ChooseMoveToForget(pokemon, tmItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
        }

    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText($"请选择要忘掉的技能",true,false);
        moveSelectonUI.gameObject.SetActive(true);
        moveSelectonUI.SetMoveDate(pokemon.Moves.Select(x => x.Base).ToList(), newMove);

        //将新技能赋值给moveTolearn 以便调用
        moveToLearn = newMove;
        state = InventoryUIState.MoveToForget;
    }

    /// <summary>
    /// 更新选中效果 颜色 文本 滑动
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
    /// 滚动
    /// </summary>
    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        //根据当前选定的项目（selectedItem）和视图中可见项目数量的一半（itemsInViewport/2），计算出要滚动的位置（scrollPos）
        float scrollPos = Mathf.Clamp(selectedItem-itemsInViewport/2,0,selectedItem) * slotUIList[0].Height;
        //改变物品集合 的 高度值  //切换的效果
        itemListRect.localPosition = new Vector3(itemListRect.localPosition.x, scrollPos);


        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    /// <summary>
    /// 切换页面时重置
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
    /// 打开队伍界面
    /// </summary>
    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        //清空显示的文本
        partyScreen.ClaerMemberSlotMessage();
        partyScreen.gameObject.SetActive(false);
    }

    /// <summary>
    /// 协程 控制技能遗忘/替换逻辑 
    /// </summary>
    /// <param name="moveIndex"></param>
    /// <returns></returns>
    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        var pokemon = partyScreen.SelectedMember;

        DialogManager.Instance.CloseDialog();
        moveSelectonUI.gameObject.SetActive(false);
        //选择遗忘新的技能
        if (moveIndex == PokemonBase.MaxNumOfMoves)
        {
            //不学习新技能
            yield return (DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}没有学习{moveToLearn.Name}"));
        }
        else
        {
            //忘记已有的技能，学习新的技能
            var selectedMove = pokemon.Moves[moveIndex];
            yield return (DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}忘记{selectedMove.Base.Name}学习了{moveToLearn.Name}"));

            //赋值替换 技能 忘记 学习
            pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }
        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }
}
