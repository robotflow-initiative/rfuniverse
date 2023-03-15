using UnityEngine;
using UnityEngine.SceneManagement;

namespace RFUniverse
{
    public class FirstMain : MonoBehaviour
    {
        void Awake()
        {
            string[] CommandLineArgs = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < CommandLineArgs.Length; i++)
            {
                if (CommandLineArgs[i].ToLower() == "-edit")
                {
                    SceneManager.LoadScene("Edit");
                    return;
                }
            }
            SceneManager.LoadScene("Empty");
        }
    }
}