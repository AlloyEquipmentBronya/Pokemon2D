using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Portal : MonoBehaviour, IPlayeTrigger
{
    [SerializeField] int sceneToLoad = -1;
    //生成坐标
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;
    PlayerControl player;

    Fader fader;

    /// <summary>
    /// 交互接口的实现 进入传送门
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerTriggered(PlayerControl player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(SwitchScense());
    }

    public bool TriggerRepeatedly => false;

    public void Start()
    {
       fader=  FindObjectOfType<Fader>();
    }

    IEnumerator SwitchScense()
    {
        //解决指定的传送点因为场景切换被删除的情况
        DontDestroyOnLoad(gameObject);

        GameControlller.Instance.PauseGame(true);
        yield return fader.FaderIn(0.5f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        //在当前场景中查找类型为Portal的对象，并返回第一个满足条件的对象。条件是该对象不等于当前对象，并且其目标门（destinationPortal）与当前对象的目标门相同
        var destPortal = FindObjectsOfType<Portal>().First(x => x != this&& x.destinationPortal==this.destinationPortal);

        //设置玩家的坐标 
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);


        yield return fader.FaderOut(0.5f);
        GameControlller.Instance.PauseGame(false);
        //传送完 要删除传送点  //如果不删除 下次调用会重新生成吗？
        Destroy(gameObject);

    }

    public Transform SpawnPoint => spawnPoint;

    public enum DestinationIdentifier
    {
        A,B,C,D,E
    }
}
