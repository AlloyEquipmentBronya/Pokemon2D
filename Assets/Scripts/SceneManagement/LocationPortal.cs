using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ����Ҵ��͵���ͬ��λ�ã������л�������
public class LocationPortal : MonoBehaviour,IPlayeTrigger
{
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
        StartCoroutine(Teleport());
    }

    public bool TriggerRepeatedly => false;

    public void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator Teleport()
    {
        //���ָ���Ĵ��͵���Ϊ�����л���ɾ�������
        GameControlller.Instance.PauseGame(true);
        yield return fader.FaderIn(0.5f);
        //�ڵ�ǰ�����в�������ΪPortal�Ķ��󣬲����ص�һ�����������Ķ��������Ǹö��󲻵��ڵ�ǰ���󣬲�����Ŀ���ţ�destinationPortal���뵱ǰ�����Ŀ������ͬ
        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        //������ҵ����� 

        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);
        yield return fader.FaderOut(0.5f);
        GameControlller.Instance.PauseGame(false);

    }

    public Transform SpawnPoint => spawnPoint;

    public enum DestinationIdentifier
    {
        A, B, C, D, E
    }
}
