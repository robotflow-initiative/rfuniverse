using UnityEngine;

public class GaussianBlurGPU : MonoBehaviour
{
    const int MaxKernel = 255;
    public ComputeShader shader;
    RenderTexture resultTexture;

    int setKernelID;
    int calculateKernel;
    int normalizedKernel;
    int blurID;
    private void Awake()
    {
        setKernelID = shader.FindKernel("SetKernel");
        calculateKernel = shader.FindKernel("CalculateKernel");
        normalizedKernel = shader.FindKernel("NormalizedKernel");
        blurID = shader.FindKernel("Blur");
    }
    public void SetGaussianKernel(int length, float weight)
    {
        shader.SetInt("Length", length);
        shader.SetFloat("Sigma", weight);
        shader.Dispatch(setKernelID, 1, 1, 1);
        shader.Dispatch(calculateKernel, 1, 1, MaxKernel);
        shader.Dispatch(normalizedKernel, 1, 1, MaxKernel);
    }

    public Texture2D Blur(Texture2D source)
    {
        resultTexture = RenderTexture.GetTemporary(source.width, source.height, 0);
        shader.SetTexture(blurID, "Result", resultTexture);
        shader.SetTexture(blurID, "Source", source);
        shader.Dispatch(setKernelID, source.width, source.height, MaxKernel);
        Texture2D tex = new Texture2D(source.width, source.height);
        RenderTexture.active = resultTexture;
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        return tex;
    }
}
