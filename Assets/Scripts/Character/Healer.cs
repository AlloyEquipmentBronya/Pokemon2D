using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player,Dialog dialog)
    {
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("您想要治疗宝可梦吗",
           choices: new List<string>() { "是","否"},
            onChoiceSelected: (choiceIndex)=>selectedChoice=choiceIndex);
        //(choiceIndex) => selectedChoice = choiceIndex：
        //一个Lambda表达式作为回调函数，在用户选择某个选项后将选项的索引赋值给selectedChoice变量。

        if(selectedChoice==0)
        {
            //是
            yield return Fader.i.FaderIn(0.5f);

            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal());

            playerParty.PartyUpdated();

            yield return Fader.i.FaderOut(0.5f);
            yield return DialogManager.Instance.ShowDialogText("下次再来哦");
        }
        else if(selectedChoice==1)
        {
            //否
            yield return DialogManager.Instance.ShowDialogText("嗯 如果有需要再来吧");
        }
       
    }
}
