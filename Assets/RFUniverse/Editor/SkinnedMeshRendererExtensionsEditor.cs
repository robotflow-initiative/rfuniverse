using UnityEngine;
using UnityEditor;

public class SkinnedMeshRendererExtensionsEditor : MonoBehaviour
{
    [MenuItem("CONTEXT/SkinnedMeshRenderer/Bake Mesh")]
    static void BakeMesh(MenuCommand menuCommand)
    {
        // 获取当前选中的SkinnedMeshRenderer组件
        SkinnedMeshRenderer skinnedMeshRenderer = menuCommand.context as SkinnedMeshRenderer;

        // 安全检查
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer not found.");
            return;
        }

        // 创建一个新的Mesh实例来存储baked mesh
        Mesh bakedMesh = new Mesh();

        // 把当前的pose bake到mesh中
        skinnedMeshRenderer.BakeMesh(bakedMesh);

        // 保存baked mesh到项目中
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Baked Mesh",
            skinnedMeshRenderer.gameObject.name + "_Baked",
            "asset",
            "Please enter a file name to save the baked mesh to");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(bakedMesh, path);
            AssetDatabase.SaveAssets();
        }
    }
}
