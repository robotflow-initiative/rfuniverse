using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using UnityEngine;

public class GaussianBlur 
{
        /// <summary>
        /// 模糊半径
        /// </summary>
        public int BlurRadius { get; private set; }

        private Color[,] SourceColor = null;
        private List<float> BlurArray { get; set; }

        public GaussianBlur(int blurRadius) 
        {
            BlurArray = new List<float>();
            BlurRadius = blurRadius;
            SetBlurArray();
        }
 
        /// <summary>
        /// 设置需要模糊的图片
        /// </summary>
        /// <param name="img"></param>
        public void SetSourceImage(Texture2D img)
        {
            Color[] colors = img.GetPixels();
            SourceColor = new Color[img.width, img.height];
            for (int i = 0; i < img.width; i++)
            {
                for (int j = 0; j < img.height; j++)
                {
                    SourceColor[i, j] = colors[img.width * j + i];
                }
            }
        }
        public void SetSourceImage(Color[] colors, int width , int height)
        {
            SourceColor = new Color[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    SourceColor[i, j] = colors[width * j + i];
                }
            }
        }
        public void SetSourceImage(List<Color> colors, int width , int height)
        {
            SetSourceImage(colors.ToArray(), width, height);
        }
        
        public void SetSourceImage(Color[,] colors)
        {
            SourceColor = colors;
        }

        /// <summary>
        /// 获取模糊之后的图片
        /// </summary>
        /// <returns></returns>
        public Color[] GetBlurImage()
        {
            Color[] colors = new Color[SourceColor.Length];
            if (SourceColor == null) return null;
            for (int x = 0; x < SourceColor.GetLength(0); x++) 
            {
                for (int y = 0; y < SourceColor.GetLength(1); y++) 
                {
                    colors[SourceColor.GetLength(0)*y+x] = GetBlurColor1(x, y);
                }
            }

            return colors;
        }
        
        public void GetBlurImage(ref Texture2D newImage) 
        {
            if (SourceColor == null) return;
            for (int x = 0; x < SourceColor.GetLength(0); x++) 
            {
                for (int y = 0; y < SourceColor.GetLength(1); y++) 
                {
                    var c = GetBlurColor(x, y);
                    //return null;
                    newImage.SetPixel(x, y, c);
                }
            }
            newImage.Apply();
        }
        /// <summary>
        /// 获取高斯模糊的颜色值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Color GetBlurColor(int x, int y)
        {
            float r = 0, g = 0 , b = 0;
            int index = 0;
            for (var t = y - BlurRadius; t <= y + BlurRadius; t++) 
            {
                for (var l = x - BlurRadius; l <= x + BlurRadius; l++) 
                {
                    var color = GetDefautColor(l, t);
                    var weighValue = BlurArray[index];
                    r += color.r * weighValue;
                    g += color.g * weighValue;
                    b += color.b * weighValue;
                    index++;
                }
            }
            return new Color(r, g, b, 1);
        }
        
        private Color GetBlurColor1(int x, int y) {
            float r = 0, g = 0 , b = 0;
            int index = 0;
            for (var t = y - BlurRadius; t <= y + BlurRadius; t++) 
            {
                for (var l = x - BlurRadius; l <= x + BlurRadius; l++) 
                {
                    var color = GetDefautColor(l, t);
                    var weighValue = BlurArray[index];
                    r = Mathf.Max(color.r * weighValue,r);
                    g = Mathf.Max(color.g * weighValue,g);
                    b = Mathf.Max(color.b * weighValue,b);
                    index++;
                }
            }
            return new Color(r, g, b, 1);
        }
        private Color GetDefautColor(int x, int y)
        {
            x = Mathf.Max(0, Mathf.Min(SourceColor.GetLength(0)-1,x));
            y = Mathf.Max(0, Mathf.Min(SourceColor.GetLength(1)-1,y));
            return SourceColor[x, y];
        }
 
        private void SetBlurArray() {
            int blur = BlurRadius;
            float sum = 0;
            for (var y = blur; y >= blur * -1; y--) {
                for (var x = blur * -1; x <= blur; x++) {                   
                    var d = GetWeighing1(x, y);
                    BlurArray.Add(d);
                    sum += d;
                }
            }
            for (var i = 0; i < BlurArray.Count; i++)
                BlurArray[i] /= sum;
 
            //sum = 0;
            //foreach (var item in this.BlurArray)
            //    sum += item;
        }
 
        /// <summary>
        /// 获取权重
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private float GetWeighing(int x, int y) 
        {
            float q = (BlurRadius * 2 + 1) / 2f;
            return 1 / (2 * Mathf.PI * Mathf.Pow(q, 2)) * Mathf.Exp(-(x * x + y * y) / (2 * q * q));
        }
        
        private float GetWeighing1(int x, int y)
        {
            float distance = Mathf.Min(Mathf.Sqrt(x * x + y * y) / BlurRadius, 1);
            return 1 - distance * distance;
        }
    }
