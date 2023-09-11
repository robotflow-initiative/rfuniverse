using UnityEditor;
using UnityEditor.SceneManagement;

namespace RFUniverse
{
    public class FastScene : Editor
    {
        [MenuItem("RFUniverse/Scene/Edit")]
        static void LoadEdit()
        {
            EditorSceneManager.OpenScene("Assets/EditMode/Runtime/Edit.unity");
        }
        [MenuItem("RFUniverse/Scene/Empty")]
        static void LoadEmpty()
        {
            EditorSceneManager.OpenScene("Assets/RFUniverse/Runtime/Empty.unity");
        }
    }
}
