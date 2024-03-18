using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;
    List<SavableEntity> savableEntities;

    public bool IsLoaded { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            
            Debug.Log($"��ҽ��볡��{gameObject.name}����ʼ�����Աߵ�����");
            //������һ������
            LoadScene();
            GameControlller.Instance.SetCurrentScene(this);

            //���ŵ�ǰ���صı�������
            AudioManager.i.PlayMusic(sceneMusic,fade:true);

            //�����Ա����ӵ�����
            foreach (var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            //���û���õ��ĳ���
            var prevScene = GameControlller.Instance.PrevScene;

            if(prevScene!=null)
            {
                //�����ǰ���ӵĳ�������
                var previoslyLoadeScenes = prevScene.connectedScenes;
                foreach (var scene in previoslyLoadeScenes)
                {
                    //�����ǰ�ĳ����������ӵĳ������� �� ���ǵ�ǰ����
                    if (!connectedScenes.Contains(scene) && scene != this)
                        scene.UnLoadScene();
                }
            }
            //�����ǰ�ĳ��� ������һ�����ӵĳ���  ж��
            if(!connectedScenes.Contains(prevScene)&&prevScene!=null)
                  prevScene.UnLoadScene();
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            var operation= SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            //�������Ϊ��ȷ������ֻ������һ�Σ��Ա����ظ����ػ���������Ҫ�Ĳ�����
            IsLoaded = true;

            //�������������ʱ ��ԭ����
            operation.completed += (AsyncOperation op) =>
              {
                  savableEntities = GetSavableEntitiesInScene();
                  //���ñ������еķ���  �����ݻ�ԭ������
                  SavingSystem.i.RestoreEntityStates(savableEntities);
              };
         
        }
    }
    public void UnLoadScene()
    {
        //����Ѿ�������
        if (IsLoaded)
        {
            //���ñ������еķ��� �Ѷ��󱣴�
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    /// <summary>
    /// ���Ҫ�������ĳ�������ʵ��
    /// </summary>
    /// <returns></returns>
   List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currenScene = SceneManager.GetSceneByName(gameObject.name);
        //�ɱ����ʵ�� savableEntities
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currenScene).ToList();

        return savableEntities;
    }


    public AudioClip SceneMusic => sceneMusic;
}
