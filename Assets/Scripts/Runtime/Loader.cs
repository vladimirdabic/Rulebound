using UnityEngine;
using UnityEngine.SceneManagement;
using VD.Rulebound.CS;

public class Loader : MonoBehaviour
{
    private void Awake()
    {
        CSInterpreter.GlobalFlags.Clear();
        StateManager.Load();

        // TODO: Decide where to next based on previous state (after adding other stuff ofc)
        
        SceneManager.LoadScene("MainMenuScene");
    }
}
