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
    /// ��ʼ����
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
    /// ѡ��  ��������
    /// </summary>
    /// <returns></returns>
    IEnumerator StartMemuState()
    {
        state = ShopState.Memu;


        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("����Ҫʲô��",
            choices: new List<string>() { "��", "��", "����" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);


        if (selectedChoice == 0)
        {
            //��

            yield return GameControlller.Instance.MoveCamera(shopCameraOffset);
            walletUI.Show();
            shopUI.Show(merchant.AvailableItems,(item)=>StartCoroutine(BuyItem(item)),
              ()=> StartCoroutine(OnBackFromBuying()));

            state = ShopState.Buying;

        }
        else if (selectedChoice == 1)
        {
            //��
            state = ShopState.Selling;
            inventoryUI.gameObject.SetActive(true);
        }
        else if (selectedChoice == 2)
        {
            OnFinish.Invoke();
            //�˳�
        }
    }
    /// <summary>
    /// �������
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
    /// ���� �Ľ��� �ص�����ѡ�����
    /// </summary>
    void OnBackFromSelling()
    {
        inventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMemuState());
    }
    /// <summary>
    /// ����
    /// </summary>
    /// <param name="item">������Ʒ</param>
    /// <returns></returns>
    IEnumerator SellItem(ItemBase item)
    {
        //�����߼�
        state = ShopState.Busy;

        if(!item.IsSellable)
        {
            yield return DialogManager.Instance.ShowDialogText("�㲻��������!");
            state = ShopState.Selling;
            yield break;
        }
        //��ʾǮ��
        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price / 2);
        int countToSell = 1;

        //��ñ�����Ʒ������
        //ʵ�ֶ���
        int itemCount = inventory.GetItemCount(item);
        if(itemCount>1)
        {
            yield return DialogManager.Instance.ShowDialogText($"����Ҫ��������?",
                waitForinput:false,autoClose:false);
            //ͨ���¼�Ϊ�� ѡ�е���ĿΪ coutTosell��ֵ
           yield return countSelectorUI.ShowSelector(itemCount, sellingPrice,
                (selectedCount) => countToSell = selectedCount);
        }

        //���� 
        sellingPrice = countToSell * sellingPrice;

        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($"����Ʒ���ۼ۸�Ϊ{sellingPrice}�Ƿ�Ҫ������",
            choices: new List<string>() { "��", "��",},
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if(selectedChoice==0)
        {
            //��
            inventory.RemoveItem(item,countToSell);
            //TODO����Ǯ��ӵ���ҵ�Ǯ����
            Wallet.i.AddMoney(sellingPrice);
            yield return DialogManager.Instance.ShowDialogText($"ʧȥ{item.Name} ���{sellingPrice}!");
        }

        //������ �ر�Ǯ��
        walletUI.Close();

        state = ShopState.Selling;
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="item">ѡ�������Ʒ</param>
    /// <returns></returns>
    IEnumerator BuyItem(ItemBase item)
    {
        state = ShopState.Busy;

        yield return DialogManager.Instance.ShowDialogText("����Ҫ�������?",
            waitForinput: false, autoClose: false);
        int countToBuy = 1;
        yield return countSelectorUI.ShowSelector(100, item.Price,
            (selectedCount) => countToBuy = selectedCount);
        DialogManager.Instance.CloseDialog();

        float totalPrice = item.Price * countToBuy;


        if(Wallet.i.HasMoney(totalPrice))
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"���Ứ��{totalPrice}",
                choices: new List<string>() { "��", "��", },
                onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

            //ѡ����
            if(selectedChoice==0)
            {
                //��
                inventory.AddItem(item, countToBuy);
                Wallet.i.takeMoney(totalPrice);
                yield return DialogManager.Instance.ShowDialogText("��л���Ĺ���");
            }
            
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText("û���㹻��Ǯȥ����!");
        }

        state = ShopState.Buying;
    }
    /// <summary>
    /// �ӹ�����淵��
    /// </summary>
    IEnumerator OnBackFromBuying()
    {
        yield return GameControlller.Instance.MoveCamera(-shopCameraOffset);

        shopUI.Close();
        walletUI.Close();
        StartCoroutine(StartMemuState());

    }

}
