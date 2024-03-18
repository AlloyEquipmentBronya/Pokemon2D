using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Portal : MonoBehaviour, IPlayeTrigger
{
    [SerializeField] int sceneToLoad = -1;
    //��������
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;
    PlayerControl player;

    Fader fader;

    /// <summary>
    /// �����ӿڵ�ʵ�� ���봫����
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
        //���ָ���Ĵ��͵���Ϊ�����л���ɾ�������
        DontDestroyOnLoad(gameObject);

        GameControlller.Instance.PauseGame(true);
        yield return fader.FaderIn(0.5f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        //�ڵ�ǰ�����в�������ΪPortal�Ķ��󣬲����ص�һ�����������Ķ��������Ǹö��󲻵��ڵ�ǰ���󣬲�����Ŀ���ţ�destinationPortal���뵱ǰ�����Ŀ������ͬ
        var destPortal = FindObjectsOfType<Portal>().First(x => x != this&& x.destinationPortal==this.destinationPortal);

        //������ҵ����� 
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);


        yield return fader.FaderOut(0.5f);
        GameControlller.Instance.PauseGame(false);
        //������ Ҫɾ�����͵�  //�����ɾ�� �´ε��û�����������
        Destroy(gameObject);

    }

    public Transform SpawnPoint => spawnPoint;

    public enum DestinationIdentifier
    {
        A,B,C,D,E
    }
}
