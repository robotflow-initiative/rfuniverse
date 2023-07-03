using UnityEngine;
using UnityEngine.SceneManagement;

namespace RFUniverse
{
    public class FirstMain : MonoBehaviour
    {
        void Awake()
        {
            string[] commandLineArgs = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                if (commandLineArgs[i].ToLower() == "-edit")
                {
                    SceneManager.LoadScene("Edit");
                    return;
                }
            }
            SceneManager.LoadScene("Empty");
        }
    }
}