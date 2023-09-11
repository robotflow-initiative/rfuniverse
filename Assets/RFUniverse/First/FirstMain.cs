using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RFUniverse
{
    public class FirstMain : MonoBehaviour
    {
        public static string Version => Application.version.Replace('.','_');
        void Awake()
        {
            //Addressables.InitializeAsync().WaitForCompletion();
            Addressables.CheckForCatalogUpdates().WaitForCompletion();
#if !UNITY_EDITOR && HYBRID_CLR
            HotUpdateAsset hotUpdateAsset = Addressables.LoadAssetAsync<HotUpdateAsset>("HotUpdateAsset").WaitForCompletion();

            foreach (var item in hotUpdateAsset.dlls)
            {
                Assembly.Load(item.bytes);
            }

            //Assembly mainAssembly = Assembly.Load(File.ReadAllBytes($"{Application.streamingAssetsPath}/RFUniverse.dll.bytes"));
            //Assembly extendAssembly = Assembly.Load(File.ReadAllBytes($"{Application.streamingAssetsPath}/RFUniverse.Extend.dll.bytes"));
            //Assembly editModeAssembly = Assembly.Load(File.ReadAllBytes($"{Application.streamingAssetsPath}/RFUniverse.EditMode.dll.bytes"));
            //Assembly multiPhysicsAssembly = Assembly.Load(File.ReadAllBytes($"{Application.streamingAssetsPath}/RFUniverse.MultiPhysics.dll.bytes"));

            foreach (var item in hotUpdateAsset.aotMetadata)
            {
                HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(item.bytes, HybridCLR.HomologousImageMode.SuperSet);
            }
            PlayerPrefs.SetInt("Patch", hotUpdateAsset.patchNumber);
#endif
            string[] commandLineArgs = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                if (commandLineArgs[i].ToLower() == "-edit")
                {
                    Addressables.LoadSceneAsync("EditScene");
                    //SceneManager.LoadScene("Edit");
                    return;
                }
            }
            Addressables.LoadSceneAsync("EmptyScene");
            //SceneManager.LoadScene("Empty");
            return;
        }
    }
}