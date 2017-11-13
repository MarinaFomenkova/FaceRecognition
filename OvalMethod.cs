using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace FaceRecognition
{
    /// <summary>
    /// Класс OvalMethod реализует классический градиентный метод
    /// Он же применяется и в комбинированном методе
    /// </summary>
    class OvalMethod
    {
        //Признак, с каким методо работает алгоритм:
        //true - классический гражиентный метод;
        //false - комбинированный метод;
        private bool onlyOval; 

        /// <summary>
        /// конструктор 
        /// </summary>
        /// <param name="method">Признак, с каким методом работает алгоритм: true - классический гражиентный метод; false - комбинированный</param>
        public OvalMethod(bool method)
        {
            if (method == true)
            {
                //работа с классическим градиентный методом
                onlyOval = true; 
            }
            else
            {
                //работа с комбинированным методом
                onlyOval = false;
            }
        }

        /// <summary>
        /// Реализует алгоритм градиентного метода
        /// </summary>
        /// <param name="img">Входное изображение</param>
        /// <param name="arrBitmap">Массив изображений: arrBitmap[0] - изображение с выделенным лицом; arrBitmap[1] - участок выделенного лица</param>
        /// <returns>Признак успешности нахождения лица: true - лицо найдено, false - не найдено</returns>
        public bool mainAlgorithm(Bitmap img, Bitmap[] arrBitmap)
        {
         /*   Bitmap gray = new Bitmap(img.Width, img.Height);
            //проверка, если используется классический градиентный метод, изображение не должно быть цветным
            if (onlyOval)
            {
                for (int i = 0; i < img.Height; i++)
                {
                    for (int j = 0; j < img.Width; j++)
                    {
                        Color col = img.GetPixel(j, i);
                        if (col.R != col.G && col.R != col.B && col.G != col.B)
                        {
                            double color = 0.3 * col.R + 0.59 * col.G + 0.11 * col.B;
                            gray.SetPixel(j, i, Color.FromArgb((int)color, (int)color, (int)color));
                        }
                        else
                        {
                            gray.SetPixel(j, i, col);
                        }
                    }
                }
            }*/

            //ширина входного изображения
            int width = img.Width/4;

            //высота входного изображения
            int height = img.Height/4;

            //увеличенная на 30% ширина изображения
            int newWidth = (width*130)/100;

            //изображение, полученное из исходного, с отмасштабированной шириной
            Bitmap imgScaled = new Bitmap(img, newWidth, height);

            //массив яркостей изображения
            //для классического градиентного метода массив яркостей исходного изображения
            //для комбинированного метода перевод в пространство I1I2I3 по компоненте I2
            int[,] I2 = new int[newWidth, height];

            //если работа идет с комбинированным методом
         /*   if (onlyOval == false)
            {*/
                //перевод в пространство I1I2I3 по компоненте I2
                toI2(imgScaled, I2);
         /*   }
            else //если работа изет с классическим градиентным методом
            {
                Bitmap imgScaledGray = new Bitmap(gray, newWidth, height);
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < newWidth; j++)
                    {
                        //создается массив яркостей исходного изображения
                        I2[j, i] = imgScaledGray.GetPixel(j, i).R;
                    }
                }
            }*/

            //градиент по X
            double[,] gradientX = new double[newWidth, height];

            //градиент по Y
            double[,] gradientY = new double[newWidth, height];

            //общий градиент
            double[,] grad = new double[newWidth, height];

            //нахождение градиентов по X,Y и общего
            gradient(I2, height, newWidth, gradientX, gradientY, grad);

            //количество радиусов лица 
            int countR;
            countR = (newWidth / 2 - 5);
            //массив радиусов лица
            int[] R = new int[countR];

            //нахождение значений радиусов
            R = findRadius(newWidth);


            //матрица центров лиц
            int[][,] coordMask = new int[countR][,];

            //начальное заполнение матрицы центров лиц нулями
            for (int i = 0; i < countR; i++)
            {
                coordMask[i] = new int[newWidth, height];
                for (int j = 0; j < height; j++)
                {
                    for (int l = 0; l < newWidth; l++)
                    {
                        coordMask[i][l, j] = 0;
                    }
                }
            }


            //нахождение координат центров лиц 
            for (int i = 0; i < countR; i++) 
            {
                for (int j = 0; j < height; j++)
                {
                    for (int l = 0; l < newWidth; l++)
                    {
                        //координаты центров лиц по Х и по У
                        double[] coord = new double[2];

                        //проверка, если градиент мал, то с ним работать не нужно
                        if ((gradientX[l, j] < -1) || (gradientY[l, j] > 1))
                        {
                            //нахождение искомых координат
                            coord = findCenter(R[i], gradientX[l, j], gradientY[l, j]);

                            //проверка, что найденный центр находится в области, отстоящей от границ на расстояние, большее радиуса
                            if (((int)coord[0] + l < newWidth - R[i]) && ((int)coord[1] + j < height - R[i]))
                            {
                                if ((int)coord[0] + l > 0 + R[i] && (int)coord[1] + j > 0 + R[i])
                                {
                                    //увеличение значения в маске по найденным координатам на 1
                                    coordMask[i][(int)coord[0] + l, (int)coord[1] + j] += 1;
                                }
                            }

                        }

                    }
                }

            }

            //максимальные значения в масках для разных радиусов
            int[] ctr = new int[countR];

            //координаты X максимальных значений в масках для разных радиусов
            int[] X = new int[countR];

            //координаты У максимальных значений в масках для разных радиусов
            int[] Y = new int[countR];

            // максимальных значений и их координат в масках для разный радиусов
            for (int i = 0; i < countR; i++)
            {
               // int[] centers = new int[100];
                int[] coordMax = new int[2];
                int maxCenter = findCentersInMask(coordMask[i], coordMax, height, newWidth);
                ctr[i] = maxCenter;
                X[i] = coordMax[0];
                Y[i] = coordMax[1];
            }


            //единсвенное наибольшее значение центра лица
            int max = ctr[0];

            //координата X наибольшее значение центра лица
            int xMax = -1;

            //координата Y наибольшее значение центра лица
            int yMax = -1;

            //соответствующий радиус для выше названных значений
            int RMax = -1; 

            //индекс радиуса в матрице центров лиц
            int index = -1;
            
            //нахождение единственного наибольшего значения и его координат
            for (int i = 0; i < countR; i++)
            {

                if (ctr[i] >= max)
                {
                    max = ctr[i];
                    xMax = X[i];
                    yMax = Y[i];
                    RMax = R[i];
                    index = i;
                }
            }

            //проверка, если наибольшее значение не найдено 
            if (RMax == -1)
            {
                return false;
            }

        /*    //количество начений, близких к максимуму
            int countMaxes = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    if (Math.Abs(coordMask[index][j, i] - max) < 5)
                    {
                        countMaxes++;
                    }
                }
            }

            //если количество максимумов велико, считаем лицо не найденым 
            if (countMaxes > 70)
            {
                return false;
            }*/



            //результирующее изображение с выделенным лицом
            Bitmap rezutScaledFase = new Bitmap(img.Width, img.Height);

            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    rezutScaledFase.SetPixel(j, i, Color.FromArgb(img.GetPixel(j, i).R, img.GetPixel(j, i).G, img.GetPixel(j, i).B));
                }
            }

            //выделение прямоугольника с лицом красным цветом
            for (int i = (int)((double)xMax*4.0*(100.0/130.0)) - RMax*4; i <= (int)((double)xMax*4.0*(100.0/130.0)) + 4*RMax; i++)
            {
                rezutScaledFase.SetPixel(i, yMax*4 + RMax*4, Color.Red);
                rezutScaledFase.SetPixel(i, yMax*4 - RMax*4, Color.Red);
            }

            for (int j = yMax*4 - RMax*4; j <= yMax*4 + RMax*4; j++)
            {
                rezutScaledFase.SetPixel((int)((double)xMax*4.0*(100.0/130.0)) + RMax*4, j, Color.Red);
                rezutScaledFase.SetPixel((int)((double)xMax * 4.0 * (100.0 / 130.0)) - RMax*4, j, Color.Red);
            }



            //участок лица (прямоугольник)
            Bitmap face = new Bitmap(2*RMax, 2*RMax);
            //текущая коодината x участка
            int x=0;
            //текущая координата y участка
            int y=0;
            //создание изображения участка лица 
            for (int i = yMax - RMax; i < yMax + RMax; i++)
            {
                for (int j = xMax - RMax; j < xMax + RMax; j++)
                {
                    Color col = imgScaled.GetPixel(j,i);
                    face.SetPixel(x, y, Color.FromArgb(col.R, col.G, col.B));
                    x++;
                    if (j == xMax + RMax-1)
                    {
                        x=0;
                        y++;
                    }
                }
            }

            //результирующее изображение масштабируется до исходных размеров
            Bitmap rezult = new Bitmap(rezutScaledFase);

            //массив полученных изображений (с выделенным лицом и отдельно выделенное лицо)
            arrBitmap[0] = rezult;
            arrBitmap[1] = face;
            return true;
        }


        /// <summary>
        /// найти координаты центра лица (наибольшего значения) в маске центров лиц 
        /// </summary>
        /// <param name="Mask"> маска</param>
        /// <param name="coord"> координаты </param>
        /// <param name="height"> высота маски</param>
        /// <param name="width"> ширина маски </param>
        /// <returns>максимальное значение в маске</returns>
        int findCentersInMask(int[,] Mask,  int[] coord, int height, int width)
        {
            //максимальное значение в маске
            int max = 0;
            //нахождение максимального значения в маске и его координат
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (Mask[j, i] > max)
                    {
                        max = Mask[j, i];
                        coord[0] = j;
                        coord[1] = i;
                    }
                }
            }

            return max;
        }


        /// <summary>
        /// перевод в пространство I1I2I3 согласно компоненте I2
        /// </summary>
        /// <param name="img"> исходное изображение</param>
        /// <param name="I2"> компонента I2</param>
        void toI2(Bitmap img, int[,] I2)
        {
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    //цвет пикселя изображения
                    Color color = img.GetPixel(j, i);
                    //красная компонента
                    int R = (int)color.R;
                    //синяя компонента
                    int B = (int)color.B;
                    I2[j, i] = R - B;
                    //обнулить, если значение получилось меньше 0
                    if (I2[j, i] < 0)
                    {
                        I2[j, i] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// нахождение градиента по X, Y и общего 
        /// </summary>
        /// <param name="imgOld">массив значений пикселей исходного изображения</param>
        /// <param name="height">высота изображения</param>
        /// <param name="width">ширина изображения</param>
        /// <param name="gradientX">градиент по X</param>
        /// <param name="gradientY">градиент по Y</param>
        /// <param name="gradient">общий градиент</param>
        void gradient(int[,] imgOld, int height, int width, double[,] gradientX, double[,] gradientY, double[,] gradient)
        {
            for (int i = 1; i < height - 1; i++)
            {
                for (int j = 1; j < width - 1; j++)
                {
                    gradientX[j, i] = (imgOld[j, i + 1] - imgOld[j, i - 1]) / 2;
                    gradientY[j, i] = (imgOld[j + 1, i] - imgOld[j - 1, i]) / 2;
                    gradient[j, i] = Math.Sqrt(gradientX[j, i] * gradientX[j, i] + gradientY[j, i] * gradientY[j, i]);
                }
            }
        }

        /// <summary>
        /// нахождение координат центра лица согласно значениям градиента в пикселе
        /// </summary>
        /// <param name="R">радиус</param>
        /// <param name="gradientX">градиент по X пикселя</param>
        /// <param name="gradientY">градиент по Y пикселя</param>
        /// <returns>координаты искомого центра</returns>
        double[] findCenter(int R, double gradientX, double gradientY)
        {
            //искомые координаты центра
            double[] coord = new double[2];

            //расстояние по X до центра
            double forX;

            //растояние по Y до центра
            double forY;

            //общий градиент 
            double gradient = Math.Sqrt(gradientX * gradientX + gradientY * gradientY);

            forX = (R * gradientX) / gradient;
            forY = (R * gradientY) / gradient;

            coord[0] = forY;
            coord[1] = forX;
            return coord;
        }

        /// <summary>
        /// нахождение радиусов лиц
        /// </summary>
        /// <param name="width">ширина изображения</param>
        /// <returns>радиусы</returns>
        int[]  findRadius(int width)
        {
            //радиусы 
            int[] R = new int[width / 2 - 5];
            //индекс радиусов
            int j = 0;
            for (int i = 5; i < width / 2; i++)
            {
                R[j] = i;
                j++;
            }
            return R;
        }

    }
}
