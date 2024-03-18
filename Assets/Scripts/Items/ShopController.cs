using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { Memu,Buying,Selling,Busy}

public class ShopController : MonoBehaviour
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;
    [SerializeField] ShopUI shopUI;
    [SerializeField] Vector2 shopCameraOffset;

    public event Action OnStart;
    public event Action OnFinish;

    ShopState state;

    Merchant merchant;
    public static ShopController i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    Inventory inventory;

    private void Start()
    {
        inventory = Inventory.GetInventory();
    }

    /// <summary>
    /// 开始交易
    /// </summary>
    /// <param name="merchant"></param>
    /// <returns></returns>
    public IEnumerator StarTrading(Merchant merchant)
    {
        this.merchant = merchant;

        OnStart?.Invoke();
        yield return StartMemuState();
    }

    /// <summary>
    /// 选择  交易类型
    /// </summary>
    /// <returns></returns>
    IEnumerator StartMemuState()
    {
        state = ShopState.Memu;


        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("您需要什么呢",
            choices: new List<string>() { "买", "卖", "不了" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);


        if (selectedChoice == 0)
        {
            //买

            yield return GameControlller.Instance.MoveCamera(shopCameraOffset);
            walletUI.Show();
            shopUI.Show(merchant.AvailableItems,(item)=>StartCoroutine(BuyItem(item)),
              ()=> StartCoroutine(OnBackFromBuying()));

            state = ShopState.Buying;

        }
        else if (selectedChoice == 1)
        {
            //卖
            state = ShopState.Selling;
            inventoryUI.gameObject.SetActive(true);
        }
        else if (selectedChoice == 2)
        {
            OnFinish.Invoke();
            //退出
        }
    }
    /// <summary>
    /// 界面控制
    /// </summary>
    public void HandleUpdate()
    {
        if(state==ShopState.Selling)
        {
            inventoryUI.HandleUpdate(OnBackFromSelling,
                (selectedItem)=> StartCoroutine(SellItem(selectedItem)));
        }
        else if(state==ShopState.Buying)
        {
            shopUI.HandleUpdate();
        }
    }
    /// <summary>
    /// 从卖 的界面 回到交易选择界面
    /// </summary>
    void OnBackFromSelling()
    {
        inventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMemuState());
    }
    /// <summary>
    /// 出售
    /// </summary>
    /// <param name="item">出售物品</param>
    /// <returns></returns>
    IEnumerator SellItem(ItemBase item)
    {
        //出售逻辑
        state = ShopState.Busy;

        if(!item.IsSellable)
        {
            yield return DialogManager.Instance.ShowDialogText("你不能卖掉它!");
            state = ShopState.Selling;
            yield break;
        }
        //显示钱包
        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price / 2);
        int countToSell = 1;

        //获得背包物品的数量
        //实现多卖
        int itemCount = inventory.GetItemCount(item);
        if(itemCount>1)
        {
            yield return DialogManager.Instance.ShowDialogText($"你想要卖多少呢?",
                waitForinput:false,autoClose:false);
            //通过事件为将 选中的数目为 coutTosell赋值
           yield return countSelectorUI.ShowSelector(itemCount, sellingPrice,
                (selectedCount) => countToSell = selectedCount);
        }

        //计算 
        sellingPrice = countToSell * sellingPrice;

        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($"该物品出售价格为{sellingPrice}是否要卖掉它",
            choices: new List<string>() { "是", "否",},
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if(selectedChoice==0)
        {
            //是
            inventory.RemoveItem(item,countToSell);
            //TODO：将钱添加到玩家的钱包中
            Wallet.i.AddMoney(sellingPrice);
            yield return DialogManager.Instance.ShowDialogText($"失去{item.Name} 获得{sellingPrice}!");
        }

        //出售玩 关闭钱包
        walletUI.Close();

        state = ShopState.Selling;
    }

    /// <summary>
    /// 购买
    /// </summary>
    /// <param name="item">选择购买的物品</param>
    /// <returns></returns>
    IEnumerator BuyItem(ItemBase item)
    {
        state = ShopState.Busy;

        yield return DialogManager.Instance.ShowDialogText("你想要买多少呢?",
            waitForinput: false, autoClose: false);
        int countToBuy = 1;
        yield return countSelectorUI.ShowSelector(100, item.Price,
            (selectedCount) => countToBuy = selectedCount);
        DialogManager.Instance.CloseDialog();

        float totalPrice = item.Price * countToBuy;


        if(Wallet.i.HasMoney(totalPrice))
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"将会花费{totalPrice}",
                choices: new List<string>() { "是", "否", },
                onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

            //选择购买
            if(selectedChoice==0)
            {
                //是
                inventory.AddItem(item, countToBuy);
                Wallet.i.takeMoney(totalPrice);
                yield return DialogManager.Instance.ShowDialogText("感谢您的购买");
            }
            
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText("没有足够的钱去购买!");
        }

        state = ShopState.Buying;
    }
    /// <summary>
    /// 从购买界面返回
    /// </summary>
    IEnumerator OnBackFromBuying()
    {
        yield return GameControlller.Instance.MoveCamera(-shopCameraOffset);

        shopUI.Close();
        walletUI.Close();
        StartCoroutine(StartMemuState());

    }

}
