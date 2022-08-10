using UnityEngine;
using System.IO;
using System.Collections;
using UnityEditor;


public class Cap : MonoBehaviour
{
#if UNITY_EDITOR
    public Camera cropCamera; //待截图的目标摄像机
    public GameObject a;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Go());
        }
    }
    IEnumerator Go()
    {
        for (int i = 0; i < a.transform.childCount; i++)
        {
            a.transform.GetChild(i).gameObject.SetActive(true);
            CameraCapture(cropCamera, new Rect(0, 0, 512, 512), a.transform.GetChild(i).name);
            a.transform.GetChild(i).gameObject.SetActive(false);
            yield return new WaitForSeconds(1);


        }
    }
    public void CameraCapture(Camera camera, Rect rect, string fileName)
    {
        RenderTexture render = new RenderTexture((int)rect.width, (int)rect.height, -1);//创建一个RenderTexture对象 
        camera.targetTexture = render;//设置截图相机的targetTexture为render
        camera.Render();//手动开启截图相机的渲染

        RenderTexture.active = render;//激活RenderTexture
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);//新建一个Texture2D对象
        tex.ReadPixels(rect, 0, 0);//读取像素
        tex.Apply();//保存像素信息

        camera.targetTexture = null;//重置截图相机的targetTexture
        RenderTexture.active = null;//关闭RenderTexture的激活状态
        Destroy(render);//删除RenderTexture对象

        byte[] bytes = tex.EncodeToJPG();//将纹理数据，转化成一个png图片
        File.WriteAllBytes(Application.dataPath + "\\Assets\\EditMode\\Image\\" + fileName + ".jpg", bytes);//写入数据
        Debug.Log(string.Format("截取了一张图片: {0}", fileName));
    }
#endif
}
