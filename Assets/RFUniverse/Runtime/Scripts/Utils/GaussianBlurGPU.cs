using System;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class GaussianBlurGPU : MonoBehaviour
{
    const int MaxKernel = 256;
    public ComputeShader shader;
    RenderTexture resultTexture;

    int calculateKernel;
    int blur;
    int getResult;
    ComputeBuffer weightsSum;
    private void Awake()
    {
        calculateKernel = shader.FindKernel("CalculateKernel");
        blur = shader.FindKernel("Blur");
        getResult = shader.FindKernel("GetResult");
    }
    public void SetGaussianKernel(int radius, float sigma)
    {
        shader.SetInt("Radius", radius);
        shader.SetFloat("Sigma", sigma);
        RenderTexture kernelRT = RenderTexture.GetTemporary(MaxKernel, MaxKernel, 0, RenderTextureFormat.RFloat);
        kernelRT.enableRandomWrite = true;
        shader.SetTexture(calculateKernel, "Kernel", kernelRT);
        shader.SetTexture(blur, "Kernel", kernelRT);

        weightsSum = new ComputeBuffer(1, sizeof(uint));
        weightsSum.SetData(new uint[] { 0 });
        shader.SetBuffer(calculateKernel, "WeightsSum", weightsSum);
        shader.SetBuffer(blur, "WeightsSum", weightsSum);

        shader.Dispatch(calculateKernel, 1, 1, MaxKernel);
    }

    public Texture2D Blur(Texture2D source)
    {
        RenderTexture resultRTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.RInt);
        resultRTexture.enableRandomWrite = true;
        shader.SetTexture(blur, "ResultR", resultRTexture);
        shader.SetTexture(getResult, "ResultR", resultRTexture);
        RenderTexture resultGTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.RInt);
        resultGTexture.enableRandomWrite = true;
        shader.SetTexture(blur, "ResultG", resultGTexture);
        shader.SetTexture(getResult, "ResultG", resultGTexture);
        RenderTexture resultBTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.RInt);
        resultBTexture.enableRandomWrite = true;
        shader.SetTexture(blur, "ResultB", resultBTexture);
        shader.SetTexture(getResult, "ResultB", resultBTexture);
        RenderTexture resultATexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.RInt);
        resultATexture.enableRandomWrite = true;
        shader.SetTexture(blur, "ResultA", resultATexture);
        shader.SetTexture(getResult, "ResultA", resultATexture);

        shader.SetTexture(blur, "Source", source);
        shader.SetTexture(getResult, "Source", source);

        resultTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        resultTexture.enableRandomWrite = true;
        shader.SetTexture(blur, "Result", resultTexture);
        shader.SetTexture(getResult, "Result", resultTexture);

        shader.Dispatch(blur, source.width, source.height, MaxKernel);
        shader.Dispatch(getResult, source.width, source.height, 1);

        Texture2D tex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        RenderTexture.active = resultTexture;
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        File.WriteAllBytes(Application.streamingAssetsPath + "/asd.png", tex.EncodeToPNG());
        return tex;
    }
}
