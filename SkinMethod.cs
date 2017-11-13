using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

namespace FaceRecognition
{
    /// <summary>
    /// класс SkinMethod отвечает за реализацию проверки по цвету кожи
    /// </summary>
    class SkinMethod
    {

        public SkinMethod()
        {
        }

        /// <summary>
        /// перевод изображения в пространство HSV
        /// </summary>
        /// <param name="imgRGB"> исходное изображение </param>
        /// <param name="imgH"> массив значений компоненты H</param>
        /// <param name="imgS"> массив значений компоненты S</param>
        /// <param name="imgV"> массив значений компоненты V</param>
        public void toHSV(Bitmap imgRGB, double[,] imgH, double[,] imgS, double[,] imgV)
        {
            for (int i = 0; i < imgRGB.Height; i++)
            {
                for (int j = 0; j < imgRGB.Width; j++)
                {
                    //цвет текущего пикселя
                    Color pixel = imgRGB.GetPixel(j, i);

                    //красная компонента текущего пикселя 
                    int R = pixel.R;

                    //зеленая компонента текущего пикселя
                    int G = pixel.G;

                    //синяя компонента текущего пикселя
                    int B = pixel.B;

                    //наждение максимального значения среди трех компонент
                    int MAX = getMax(R, G, B);

                    //нахождение минимального значения среди трех компонент
                    int MIN = getMin(R, G, B);

                    //компонента H
                    double H;

                    //компонента S
                    double S;

                    //компонента V
                    double V;

                    //нхождение V
                    V = (double)MAX;

                    //нахождение S
                    if ((double)MAX == 0)
                    {
                        S = 0;
                    }
                    else
                    {
                        S = ((double)MAX - (double)MIN) / (double)MAX;
                    }

                    //нахождение H
                    if (S == 0)
                    {
                        H = (float)220;
                    }
                    else
                    {
                        double delta = (double)MAX - (double)MIN;
                        if (R == (int)MAX)
                        {
                            H = ((double)G - (double)B) / delta;
                        }
                        else if (G == (int)MAX)
                        {
                            H = 2 + ((double)B - (double)R) / delta;
                        }
                        else 
                        {
                            H = 4 + ((double)R - (double)G) / delta;
                        }

                        H = H * 60;
                        if (H < 0)
                        {
                           
                            H = H + 360;
                        }
                    }
    
                    //результирующие массивы
                    imgH[j, i] = H;
                    imgS[j, i] = S;
                    imgV[j, i] = V;
                }
            }


        }

        /// <summary>
        /// нахождение максимума среди трех чисел
        /// </summary>
        /// <param name="a"> 1е число</param>
        /// <param name="b"> 2е число</param>
        /// <param name="c"> 3е число</param>
        /// <returns>максимум</returns>
        int getMax(int a, int b, int c)
        {
            //максимум
            int maximum = a;

            if (b > maximum)
            {
                maximum = b;
            }
            if (c > maximum)
            {
                maximum = c;
            }

            return maximum;
        }

        /// <summary>
        /// нахождение минимума среди трехз чисел
        /// </summary>
        /// <param name="a">1е число</param>
        /// <param name="b">2е число</param>
        /// <param name="c">3е число</param>
        /// <returns>минимум</returns>
        int getMin(int a, int b, int c)
        {
            //минимум
            int minimum = a;

            if (b < minimum)
            {
                minimum = b;
            }
            if (c < minimum)
            {
                minimum = c;
            }

            return minimum;
        }

        /// <summary>
        /// алгоритм реализации подхода по цвету кожи 
        /// </summary>
        /// <param name="img">исходное изображение</param>
        /// <returns>черно-белое изображение. черные пиксели - не цвет кожи, белые пиксели - цвет кожи</returns>
        public Bitmap mainAlgorithm(Bitmap img)
        {
            //высота изображения 
            int height = img.Height;

            //ширина изображения
            int width = img.Width;

            //маска, в которой черные значения - не цвет кожи, белые - цвет кожи
            int[,] MASKA = new int[width, height];

            //компонент H изображения
            double[,] H = new double[width, height];

            //компонент S изображения
            double[,] S = new double[width, height];

            //компонент V изображения
            double[,] V = new double[width, height];

            //перевод изображения в пространство H,S,V
            toHSV(img, H, S, V);

            //заполнении маски 
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //проверка на принадлежность пикселя цвету кожи
                    if(((H[j,i]>=0 && H[j,i]<=36) || (H[j,i]>=340 && H[j,i]<=360)) &&
                       (S[j,i]>=0.2 && S[j,i]<=0.7) &&
                       (V[j,i]>=40))
                    {
                       
                        MASKA[j,i] = 255;
                    }
                    else
                    {
                        MASKA[j,i] = 0;
                    }
                }

      
            }

            //новая маска для удаления мелких объектов
            double[,] MaskNew = new double[width, height];

            //медианный фильтр для удаления мелких объектов
            MedianFiltr(MASKA, height, width, MaskNew);

            //результирующее изображение 
            Bitmap medbtm = new Bitmap(width, height);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    medbtm.SetPixel(j, i, Color.FromArgb((int)MaskNew[j, i], (int)MaskNew[j, i], (int)MaskNew[j, i]));
                }
            }


            return medbtm;
            
        }

        /// <summary>
        /// расчет доли пикселей цвета кожи от всего кол-ва пикселей изображения
        /// </summary>
        /// <param name="img">исходное изображение (черно-белое)</param>
        /// <returns>доля пикселей цвета кожи (белые пиксели) от всего кол-ва пикселей изображения</returns>
        public double percentSkin(Bitmap img)
        {
            //кол-во пикселей цвета кожи 
            int blackColor = 0;

            //кол-во пикселей не цвета кожи
            int whiteColor = 0;
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Color col = img.GetPixel(j, i);
                    if (col.B == 255)
                    {
                        whiteColor++;
                    }
                    else
                    {
                        blackColor++;
                    }
                }
            } 

            //результирующее отношение
            double rezult = (double)whiteColor / (double)(blackColor + whiteColor);
            return rezult;
        }



        /// <summary>
        /// медианный фильтр, чтоб убрать мелкие объекты
        /// </summary>
        /// <param name="Mask">входная маска</param>
        /// <param name="height">высота маски</param>
        /// <param name="width">ширина маски</param>
        /// <param name="MaskNew">новая маска после реализации медианного фильтра</param>
        void MedianFiltr(int[,] Mask, int height, int width, double[,] MaskNew)
        {
            //заполнение результирующей маски значениями исходной
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    MaskNew[j, i] = Mask[j, i];
                }
            }

            //сортировка значений в окне 5*5 и присвоение среднего значения центру окна 
            for (int i = 0; i < height - 5; i++)
            {
                for (int j = 0; j < width - 5; j++)
                {
                    MaskNew[j + 2, i + 2] = sort(Mask, i, j);
                }
            }
        }

        /// <summary>
        /// сортировка значений окна в максе 5*5 
        /// </summary>
        /// <param name="Mask">исходная маска</param>
        /// <param name="y">позиция y, с которой начинается сортировка в маске</param>
        /// <param name="x">позиция х, с которой начинается сортировка в маске</param>
        /// <returns>серединное значение в полученном отсотрированном массиве</returns>
        int sort(int[,] Mask, int y, int x)
        {
            //массив значений, с которыми работает фильтр
            int[] array = new int[25];

            //индекс массива
            int l = 0;
            //заполнение значений массива значениями участка маски 
            for(int i=y; i<y+5; i++)
            {
                for(int j=x; j<x+5; j++)
                {
                    array[l] = Mask[j,i];
                    l++;
                }
            }

            //промежуточная переменная для сортировки
            int temp;
            //сортировка значений 
            for (int i = 0; i < 25; i++)
            {
                for (int j = 24; j > i; j--)
                {
                    if (array[j] < array[j - 1])
                    {
                        temp = array[j];
                        array[j] = array[j - 1];
                        array[j - 1] = temp;

                    }
                }
            }

            //возвращается серединное значение
            return array[12];
        }
    }

}
