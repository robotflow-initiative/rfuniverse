using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GaussianBlur : IDisposable
{
    KeyValuePair<Vector2Int, float>[] kernelArray;
    Texture2D result;
    public GaussianBlur(int radius, float sigma)
    {
        SetGaussianKernel(radius, sigma);
        result = new Texture2D(1, 1);
    }

    Color[,] colors;
    public Texture2D Blur(Texture2D tex)
    {
        Color[] col = tex.GetPixels();
        colors = Reshape(col, tex.height, tex.width);

        Color[,] resultColor = new Color[colors.GetLength(0), colors.GetLength(1)];
        Parallel.For(0, colors.GetLength(0), (i) =>
        {
            Parallel.For(0, colors.GetLength(1), (j) =>
            {
                resultColor[i, j] = GetBlurColor(new Vector2Int(i, j));
            });
        });

        result.Reinitialize(tex.width, tex.height, tex.graphicsFormat, false);
        result.SetPixels(Flatten(resultColor));
        result.Apply();
        return result;
    }
    Color GetBlurColor(Vector2Int pos)
    {
        Color color = new Color(0, 0, 0, 0);
        for (int i = 0; i < kernelArray.Length; i++)
        {
            var item = kernelArray[i];
            int x = Mathf.Clamp((pos + item.Key).x, 0, colors.GetLength(0) - 1);
            int y = Mathf.Clamp((pos + item.Key).y, 0, colors.GetLength(1) - 1);
            color += colors[x, y] * item.Value;
        }
        return color;
    }

    T[,] Reshape<T>(T[] input, int row, int column)
    {
        if (input.Length != row * column)
            throw new ArgumentException("Input data does not match given dimensions.");

        T[,] output = new T[row, column];
        for (int i = 0; i < row; i++)
            for (int j = 0; j < column; j++)
                output[i, j] = input[i * column + j];

        return output;
    }
    T[] Flatten<T>(T[,] input)
    {
        int row = input.GetLength(0);
        int column = input.GetLength(1);

        T[] output = new T[row * column];
        for (int i = 0; i < row; i++)
            for (int j = 0; j < column; j++)
                output[i * column + j] = input[i, j];

        return output;
    }
    public void SetGaussianKernel(int radius, float sigma)
    {
        Dictionary<Vector2Int, float> kernel = new Dictionary<Vector2Int, float>();
        float constant = 1f / (2 * Mathf.PI * sigma * sigma);
        float power = 2 * sigma * sigma;
        float kernelSum = 0;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                float rr = x * x + y * y;
                kernel[new Vector2Int(x, y)] = constant * Mathf.Exp(-rr / power);
                kernelSum += kernel[new Vector2Int(x, y)];
            }
        }

        foreach (var item in kernel.ToList())
        {
            kernel[item.Key] = item.Value / kernelSum;
        }

        kernelArray = kernel.ToArray();
    }

    public void Dispose()
    {
        GameObject.Destroy(result);
    }
}
