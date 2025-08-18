using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalLoader
{
    private static bool _loaded = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        if (_loaded) return;
        _loaded = true;

        GameObject persistent = Resources.Load<GameObject>("Prefabs/Persistent");
        
        if(persistent != null)
        {
            GameObject obj = Object.Instantiate(persistent);
            Object.DontDestroyOnLoad(obj);

            if (StateManager.Data == null) StateManager.Reset();
        }
    }
}
