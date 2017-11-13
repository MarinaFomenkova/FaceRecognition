using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;


namespace FaceRecognition
{
    public partial class ClearImage : Form
    {
        Neuron[] firstLayer;
        Neuron neuronOutput;

        public ClearImage()
        {
            InitializeComponent();
            Methods.SelectedIndex = 0;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        Bitmap img;


        private void ToolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = ".jpg";
            openFile.Filter = "Image Files(*.jpg)|*.jpg|Image Files(*.bmp)|*.bmp|Image Files(*.png)|*.png";
            try
            {
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    pictureBox.Image = null;
                    string fileName = openFile.FileName;
                    img = new Bitmap(fileName);
                    if (img.Width != 320 || img.Height != 240)
                    {
                        MessageBox.Show("Длина и ширина должны быть равны 240 и 320!", "Неподходящие параметры изображения!");
                        return;
                    }
                    pictureBox.Image = img;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка открытия файла", "Ошибка!");
                return;
            }
        }

        private void Methods_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Rec_Click(object sender, EventArgs e)
        {

            //определение лица на изображении методом градиентов
            if (Methods.SelectedIndex == 0)
            {
                if(img == null)
                {
                    MessageBox.Show("Нет изображения!", "Загрузите изображение!");
                    return;
                }
                if (img.Width !=320 || img.Height !=240)
                {
                    MessageBox.Show("Длина и ширина должны быть равны 240 и 320!", "Неподходящие параметры изображения!");
                    return;

                }
                //копия исходного изображения
                Bitmap copy = new Bitmap(img, img.Width, img.Height);

                //результирующее изображение 
                Bitmap rezult = new Bitmap(img.Width, img.Height);

                //для градиентного метода
                OvalMethod oval = new OvalMethod(true);

                //изображения для результатов градиентного метода
                Bitmap[] bitmaps = new Bitmap[2];
                for (int i = 0; i < 2; i++)
                {
                    bitmaps[i] = new Bitmap(img.Width, img.Height);
                }

                //реализация градиентного метода
                bool ok = oval.mainAlgorithm(copy, bitmaps);

                //проверка, было ли найдено лицо
                if (ok == true)
                {
                    //определение и отображение результата
                    rezult = bitmaps[0];
                    pictureBox.Image = rezult;
                }

            }
            else if (Methods.SelectedIndex == 1) //распознавание комбинированным методом 
            {     
               
               if(img == null)
                {
                    MessageBox.Show("Нет изображения!", "Загрузите изображение!");
                    return;
                }
                if (img.Width !=320 || img.Height !=240)
                {
                    MessageBox.Show("Длина и ширина должны быть равны 240 и 320!", "Неподходящие параметры изображения!");
                    return;

                }
                 //рапознавание методом овалов
                //копия исходного изображения
                Bitmap copy = new Bitmap(img, img.Width, img.Height);

                //для градиентного метода
                OvalMethod oval = new OvalMethod(false);

                //изображения для результатов градиентного метода
                Bitmap[] bitmaps = new Bitmap[2];

                //реализация градиентного метода
                bool ok = oval.mainAlgorithm(copy, bitmaps);

                //проверка, было ли найдено лицо
                if (ok == true)
                {
                    //изображение участка лица для проверки по цвету кожи
                    Bitmap forSkin = bitmaps[1];
                    
                    //проверка по цвету кожи
                    SkinMethod skinMeth = new SkinMethod();

                    //реализация проверки по цвету кожи
                    Bitmap rezultSkin = skinMeth.mainAlgorithm(forSkin);
   
                    //определени доли пикселей цвета кожи
                    double persent = skinMeth.percentSkin(rezultSkin);

                    //сверка доли пикселей цвета кожи с пороговым значением
                    if (persent > 0.6)
                    {
                        //отображение результата, если проверка прошла успешно
                        pictureBox.Image = new Bitmap(bitmaps[0]);
                    }
                }

            }
            else if (Methods.SelectedIndex == 2)
            {
                //распознавание нейронными сетями
                if(img == null)
                {
                    MessageBox.Show("Нет изображения!", "Загрузите изображение!");
                    return;
                }
                if (img.Width !=320 || img.Height !=240)
                {
                    MessageBox.Show("Длина и ширина должны быть равны 240 и 320!", "Неподходящие параметры изображения!");
                    return;

                }
               /* OpenFileDialog fdFaces = new OpenFileDialog();
                fdFaces.Multiselect = true;
                DialogResult drFaces = fdFaces.ShowDialog();


                OpenFileDialog fdNoFaces = new OpenFileDialog();
                fdNoFaces.Multiselect = true;
                DialogResult drNoFaces = fdNoFaces.ShowDialog();
                if (drFaces == System.Windows.Forms.DialogResult.OK && drNoFaces == System.Windows.Forms.DialogResult.OK)
                {
                    string[] imgsFaces = fdFaces.FileNames;
                    Bitmap[] btmsFaces = new Bitmap[imgsFaces.Count()];
                    for (int i = 0; i < imgsFaces.Count(); i++)
                    {
                        btmsFaces[i] = new Bitmap(imgsFaces[i]);
                    }

                    string[] imgsNoFaces = fdNoFaces.FileNames;
                    Bitmap[] btmsNoFacesFaces = new Bitmap[imgsNoFaces.Count()];
                    for (int i = 0; i < imgsNoFaces.Count(); i++)
                    {
                        btmsNoFacesFaces[i] = new Bitmap(imgsNoFaces[i]);
                    }


                    NeuralNetMethodPers met = new NeuralNetMethodPers();
                    met.forAllTrainingSet(btmsFaces, btmsNoFacesFaces);
                }
                */


                //проверка, инициализированы нейроны первого слоя и выходной
                if (firstLayer == null || neuronOutput == null) //нет
                {
                    //инициализировать и считать из файла веса
                    readNeuronWeights();
                }

                //копия исходного изображения
                Bitmap copy = new Bitmap(img, img.Width/2, img.Height/2);

                //для подхода на основе нейронной сети 
                NeuralNetMethodPers nnet = new NeuralNetMethodPers(firstLayer, neuronOutput);

                //координаты X верхнего левого квадрата лица 
                int[] coordsX = new int[1000];

                //координаты Y верхнего левего квадрата лица
                int[] coordsY = new int[1000];

                //начальные значения
                for (int i = 0; i < 1000; i++)
                {
                    coordsX[i] = -1;
                    coordsY[i] = -1;
                }

                //реализация подхода на основе нейронной сети
                bool ok =  nnet.mainAlgorithmForRec(copy, coordsX, coordsY);
                Bitmap copy1 = new Bitmap(img);
                //проверка, обнаружено ли лицо
                if (ok)
                {
                    //количество найденных лиц
                    int countfaces = 1;

                    //новые координаты лиц
                    int[] newX = new int[100];
                    int[] newY = new int[100];

                    //ширина прямоугольника с лицом
                    int[] width = new int[100];

                    //высота прямоугольника с лицом
                    int[] height = new int[100];

                    //начальные значения вышеперечисленных параметров
                    newX[0] = coordsX[0];
                    newY[0] = coordsY[0];
                    width[0] = 55;
                    height[0] = 60;
                    for (int i = 1; i < 100; i++)
                    {
                        for (int j = 0; j < countfaces; j++)
                        {
                            if (coordsX[i] != -1 && coordsY[i] != -1)
                            {
                                //проверка совмещений найденных прямоугольников лиц
                                if ((coordsX[i] >= newX[j] && coordsX[i] <= newX[j] + 55 && coordsY[i] >= newY[j] && coordsY[i] <= newY[j] + 60) ||
                                    (coordsX[i] >= newX[j] && coordsX[i] <= newX[j] + 55 && coordsY[i] + 60 >= newY[j] && coordsY[i] + 60 <= newY[j] + 60) ||
                                    (coordsX[i] + 55 >= newX[j] && coordsX[i] + 55 <= newX[j] + 55 && coordsY[i] >= newY[j] && coordsY[i] <= newY[j] + 60) ||
                                    (coordsX[i] + 55 >= newX[j] && coordsX[i] + 55 <= newX[j] + 55 && coordsY[i] + 60 >= newY[j] && coordsY[i] + 60 <= newY[j] + 60))
                                {
                                    //если есть совмещение, соединить прямоугольники в один
                                    width[j] = 55 + Math.Abs(coordsX[i] - newX[j]);
                                    height[j] = 60 + Math.Abs(coordsY[i] - newY[j]);
                                    coordsX[i] = -1;
                                    coordsY[i] = -1;
                                   
                                }
                                else
                                {
                                    newX[countfaces] = coordsX[i];
                                    newY[countfaces] = coordsY[i];
                                    width[countfaces] = 55;
                                    height[countfaces] = 60;
                                    countfaces++;
                                }
                            }
                        }
                    }

                    //очерчивание прямоугольников

                   locFace(copy1, newX[0], newY[0], width[0], height[0]);
   
                }
                 pictureBox.Image = copy1;
            }
        }

        /// <summary>
        /// инициализировать нейроны и считать из файла их веса
        /// </summary>
        void readNeuronWeights()
        {
            //нейроны первого слоя
            firstLayer = new Neuron[101];

            for (int i = 0; i < 101; i++)
            {
                firstLayer[i] = new Neuron();
                Link[] lnk = new Link[529];
                for (int j = 0; j < 529; j++)
                {
                    Link l = new Link();
                    lnk[j] = l;
                }

                firstLayer[i].links = lnk;

            }

            //выходной нейрон
            neuronOutput = new Neuron();
            Link[] outLinks = new Link[101];
            for (int i = 0; i < 101; i++)
            {
                outLinks[i] = new Link();
            }
            neuronOutput.links = outLinks;

            //считать из файла веса
            ReadWriteXMLForNeuralNet np = new ReadWriteXMLForNeuralNet();
            try
            {
                np.readXml(firstLayer, neuronOutput);
            }
            catch
            {
                MessageBox.Show("Ошибка считывания из файла весов!", "Ошибка!");
                return;
            }
        }

        /// <summary>
        /// очертить прямоугольник лица 
        /// </summary>
        /// <param name="img">исходног изображение</param>
        /// <param name="X">координата Х верхнего левого угла прямоугольника</param>
        /// <param name="Y">координата У верхнего левого угла прямоугольника</param>
        /// <param name="width">ширина прямоугольника</param>
        /// <param name="height">высота прямоугольника</param>
        void locFace(Bitmap img, int X, int Y, int width, int height)
        {
            int X1 = X * 2;
            int Y1 = Y * 2;
            int width1 = width * 2;
            int height1 = height * 2;

      
            for (int i = X1; i < X1+width1-1; i++)
            {
                img.SetPixel(i, Y1, Color.Red);
                img.SetPixel(i, Y1 + height1-1, Color.Red);
            }

            for (int i = Y1; i < Y1 + height1-1; i++)
            {
                img.SetPixel(X1, i, Color.Red);
                img.SetPixel(X1+width1-1, i, Color.Red);
            }

        }

        /// <summary>
        /// очистить выделение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clear_Click(object sender, EventArgs e)
        {
            if (img != null)
            {
                pictureBox.Image = img;
            }
        }

        private void ToolStripMenuItemSave_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
            {
                Image imgToSave = (Bitmap)pictureBox.Image;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Сохранить картинку как ...";
                sfd.CheckPathExists = true; 
                sfd.Filter = "Image Files(*.bmp)|*.bmp|Image Files(*.jpg)|*.jpg|Image Files(*.png)|*.png";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        imgToSave.Save(sfd.FileName);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


    }
}
