using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player,Dialog dialog)
    {
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("����Ҫ���Ʊ�������",
           choices: new List<string>() { "��","��"},
            onChoiceSelected: (choiceIndex)=>selectedChoice=choiceIndex);
        //(choiceIndex) => selectedChoice = choiceIndex��
        //һ��Lambda���ʽ��Ϊ�ص����������û�ѡ��ĳ��ѡ���ѡ���������ֵ��selectedChoice������

        if(selectedChoice==0)
        {
            //��
            yield return Fader.i.FaderIn(0.5f);

            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal());

            playerParty.PartyUpdated();

            yield return Fader.i.FaderOut(0.5f);
            yield return DialogManager.Instance.ShowDialogText("�´�����Ŷ");
        }
        else if(selectedChoice==1)
        {
            //��
            yield return DialogManager.Instance.ShowDialogText("�� �������Ҫ������");
        }
       
    }
}
