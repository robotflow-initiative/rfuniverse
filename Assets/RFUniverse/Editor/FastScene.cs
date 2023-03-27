using UnityEditor;
using UnityEditor.SceneManagement;

public class FastScene : Editor
{
    [MenuItem("RFUniverse/Scene/Edit")]
    static void LoadEdit()
    {
        EditorSceneManager.OpenScene("Assets/EditMode/Edit.unity");
    }
    [MenuItem("RFUniverse/Scene/Empty")]
    static void LoadEmpty()
    {
        EditorSceneManager.OpenScene("Assets/RFUniverse/Empty.unity");
    }
}
