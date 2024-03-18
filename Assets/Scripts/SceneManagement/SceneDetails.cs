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
            
            Debug.Log($"玩家进入场景{gameObject.name}，开始加载旁边的区域");
            //加载下一个场景
            LoadScene();
            GameControlller.Instance.SetCurrentScene(this);

            //播放当前加载的背景音乐
            AudioManager.i.PlayMusic(sceneMusic,fade:true);

            //加载旁边连接的区域
            foreach (var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            //清除没有用到的场景
            var prevScene = GameControlller.Instance.PrevScene;

            if(prevScene!=null)
            {
                //获得先前连接的场景对象
                var previoslyLoadeScenes = prevScene.connectedScenes;
                foreach (var scene in previoslyLoadeScenes)
                {
                    //如果先前的场景不是连接的场景对象 且 不是当前场景
                    if (!connectedScenes.Contains(scene) && scene != this)
                        scene.UnLoadScene();
                }
            }
            //如果先前的场景 它不是一个连接的场景  卸载
            if(!connectedScenes.Contains(prevScene)&&prevScene!=null)
                  prevScene.UnLoadScene();
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            var operation= SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            //这可能是为了确保场景只被加载一次，以避免重复加载或其他不必要的操作。
            IsLoaded = true;

            //当场景加载完毕时 还原数据
            operation.completed += (AsyncOperation op) =>
              {
                  savableEntities = GetSavableEntitiesInScene();
                  //调用保存类中的方法  把数据还原给对象
                  SavingSystem.i.RestoreEntityStates(savableEntities);
              };
         
        }
    }
    public void UnLoadScene()
    {
        //如果已经加载了
        if (IsLoaded)
        {
            //调用保存类中的方法 把对象保存
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    /// <summary>
    /// 获得要保存对象的场景数据实体
    /// </summary>
    /// <returns></returns>
   List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currenScene = SceneManager.GetSceneByName(gameObject.name);
        //可保存的实体 savableEntities
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currenScene).ToList();

        return savableEntities;
    }


    public AudioClip SceneMusic => sceneMusic;
}
