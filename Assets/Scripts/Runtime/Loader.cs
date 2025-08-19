using UnityEngine;
using UnityEngine.SceneManagement;
using VD.Rulebound.CS;

public class Loader : MonoBehaviour
{
    private void Awake()
    {
        CSInterpreter.GlobalFlags.Clear();
        StateManager.Load();

        Flag flag;

        if ((flag = CSInterpreter.GetFlag("c_selfrule")) != null && flag.Value) {
            SceneManager.LoadScene("IntroScene");
            return;
        }
        
        SceneManager.LoadScene("MainMenuScene");
    }
}
