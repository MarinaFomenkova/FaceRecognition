using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace FaceRecognition
{
    /// <summary>
    /// класс NeuralNetMethodPers реализует подход на основе нейронной сети
    /// </summary>
    class NeuralNetMethodPers
    {
        //первый слой нейронов (следует за входным)
        Neuron[] firstLayer;

        //выходной нейрон
        Neuron neuronOutput;

        public NeuralNetMethodPers()
        {
        }

        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="firstLayer_constr">первый слой нейронов  </param>
        /// <param name="neuronOutput_constr">выходной нейрон</param>
        public NeuralNetMethodPers(Neuron[] firstLayer_constr, Neuron neuronOutput_constr)
        {
            firstLayer = firstLayer_constr;
            neuronOutput = neuronOutput_constr;
        }


        /// <summary>
        /// основной алгоритм для определения лица
        /// </summary>
        /// <param name="img">входное изображение</param>
        /// <param name="coordinatesX">координаты Х верхнего левого угла участка лица</param>
        /// <param name="coordinatesY">координаты У верхнего левого угла участка лица</param>
        /// <returns>признак успешности нахождения лица: true - лицо найдено, false - не найдено</returns>
        public bool mainAlgorithmForRec(Bitmap img, int[] coordinatesX, int[] coordinatesY)
        {   
            //индекс  координат верхнего левого угла участка лица
            int l=0;
            for (int i = 0; i < img.Height - 59; i+=6)
            {
                for (int j = 0; j < img.Width - 54; j+=6)
                {
                    //создать участок изображения для проверки нейронной сетью
                    Bitmap imgForRec = createBitmap(img, i, j);

                    //проверка участка нейронной сетью
                    if (mainAlgorithmTrainNext(imgForRec, firstLayer, neuronOutput, true, TypeOfProgram.recognize))
                    {
                        //координаты  верхнего левого угла участка лица
                        coordinatesX[l] = j;
                        coordinatesY[l] = i;
                        l++;
                    }
                }
            }
            //проверка, найдено ли лицо
            if (l != 0)
            {
                //лицо найдено
                return true;
            }
            else
            {
                //лицо не найдено
                return false;
            }
        }

        /// <summary>
        /// создать изображения из участка исходного
        /// </summary>
        /// <param name="img">исходное изображение</param>
        /// <param name="y">начальная координата у исходного изображения</param>
        /// <param name="x">начальная координата х исходного изображения</param>
        /// <returns>изображение из участка исходного</returns>
        Bitmap createBitmap(Bitmap img, int y, int x)
        {
            //искомое изображение-участок
            Bitmap btm = new Bitmap(55, 60);
            for(int i=0; i<60; i++)
            {
                for(int j=0; j<55; j++)
                {
                    Color col = img.GetPixel(x+j, y+i);
                    btm.SetPixel(j, i, col);
                }
            }
            return btm;
        }

        /// <summary>
        /// обучение сети 
        /// </summary>
        /// <param name="imgsFaces">тренировочный набор лиц</param>
        /// <param name="nonFaces">тренировочный набор не-лиц</param>
        public void forAllTrainingSet(Bitmap[] imgsFaces, Bitmap [] nonFaces)
        {
            //кол-во нейронов на первом слое
            int countOfNeuronsFirstLayer = 101;

            //нейроны первого слоя
            Neuron[] firstLayer = new Neuron[countOfNeuronsFirstLayer];

            //выходной нейрон
            Neuron neuronOutput = new Neuron();

            //обучение сети на первом изображении
            mainAlgorithmTrain(imgsFaces[0], firstLayer, neuronOutput);

            //признак остановки обучения. true - продолжение обучения, false - остановка обучения
            bool ok = true;

            //кол-во итераций
            int count = 0;

            //обучение 
            while (ok)
            {
                count += 1;

                //кол-во правильно распознанных изображений
                int countOfImagesWright = 0;

                //признак, что все изображения прошли через обучение по 1 разу
                bool end = true;

                //индекс для изображений лиц
                int yes = 0;

                //индекс для изображений не-лиц
                int no = 0;

                //счетчик: четные значения - не-лица, нечетные - лица
                int k = 0;
                while (end)
                {
                    //признак успешности определения лица
                    bool isOk = false;

                    //для лиц
                    if (k % 2 != 0)
                    {
                        //алгоритм для лиц
                        isOk = mainAlgorithmTrainNext(imgsFaces[yes], firstLayer, neuronOutput, true, TypeOfProgram.training);
                        yes++;

                        //если лицо успешно определено
                        if (isOk == true)
                        {
                            countOfImagesWright += 1;
                        }
                    }
                    else
                    {
                        //алгоритм для не-лиц
                        isOk = mainAlgorithmTrainNext(nonFaces[no], firstLayer, neuronOutput, false, TypeOfProgram.training);
                        no++;

                        //если успешно определено, что изображение не является лицом
                        if (isOk == true)
                        {
                            countOfImagesWright += 1;
                        }

                    }
                    k+=1;
                    //проверка, что весь набор обучился по одному разу
                    if (k == imgsFaces.Count() + nonFaces.Count())
                    {
                        end = false;
                    }

                }

                //проверка, что обучение завершено или достигнуто макссимальное кол-во итераций
                //и сохраненение в файл
                if (countOfImagesWright == imgsFaces.Count() + nonFaces.Count() || count==100)
                {
                    ok = false;
                    ReadWriteXMLForNeuralNet rw = new ReadWriteXMLForNeuralNet();
                    rw.writeXML(firstLayer, neuronOutput);
                }
            }
        }

        /// <summary>
        /// обучение сети после инициализации весов
        /// </summary>
        /// <param name="img">исходное изображение</param>
        /// <param name="firstLayer">нейроны первого слоя</param>
        /// <param name="neuronOutput">выходной нейрон</param>
        /// <param name="isFace">признак лица (для обучения)</param>
        /// <param name="tp">тип программы (обучение или нет)</param>
        /// <returns>признак успешности нахождения лица: true - лицо найдено, false - не найдено</returns>
        public bool mainAlgorithmTrainNext(Bitmap img, Neuron[] firstLayer, Neuron neuronOutput, bool isFace, TypeOfProgram tp)
        {
            //кол-во нейронов на входном слое
            int countOfNeuronInput = 529;

            //кол-во нейронов на первом слое
            int countOfNeuronsFirstLayer = 101;

            //веса нейронов первого слоя
            float[] outputsOfFirstLayerWeights = new float[countOfNeuronInput];

            Bitmap gray = new Bitmap(img.Width, img.Height);
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Color col = img.GetPixel(j, i);
                 
                        double color = 0.3 * col.R + 0.59 * col.G + 0.11 * col.B;
                        gray.SetPixel(j, i, Color.FromArgb((int)color, (int)color, (int)color));
                 
                        gray.SetPixel(j, i, col);
                 
                }
            }






            //текущие координаты пикселей изображения
            int x = 0, y = 0;

            //нахождение выходных значений связей первого слоя
            for (int i = 0; i < countOfNeuronInput; i++)
            {
                //цвет пикселя изображения
                float color = (float)gray.GetPixel(x, y).R;
                outputsOfFirstLayerWeights[i] = color / 255;
                x++;
                //пересчет координат
                if (x == gray.Width)
                {
                    x = 0;
                    y++;
                }
            }

            //работа с нейронами первого слоя 
            for (int i = 0; i < countOfNeuronsFirstLayer; i++)
            {
                //нейрон первого слоя
                Neuron neuronOfFirstLayer = new Neuron();

                //связи нейрона первого слоя с входом
                Link[] linksOfFirstWeights = new Link[countOfNeuronInput];

                //нахождение весов связей и выходного значения с предыдущего слоя
                for (int j = 0; j < countOfNeuronInput; j++)
                {
                    linksOfFirstWeights[j] = firstLayer[i].links[j];
                    linksOfFirstWeights[j].neuronOutput = outputsOfFirstLayerWeights[j];
                }
                neuronOfFirstLayer.links = linksOfFirstWeights;

                //выход нейрона первого слоя
                float outputNeuronFirstLayer = 0;

                //нахождение выхода нейрона первого слоя
                for (int j = 0; j < countOfNeuronInput - 1; j++)
                {
                    outputNeuronFirstLayer += linksOfFirstWeights[j].weight * linksOfFirstWeights[j].neuronOutput;
                }
                //добавление смещения
                outputNeuronFirstLayer += linksOfFirstWeights[countOfNeuronInput - 1].weight;

                //активационная функция
                outputNeuronFirstLayer = (float)1 / ((float)1 + (float)Math.Exp(-outputNeuronFirstLayer));
                neuronOfFirstLayer.output = outputNeuronFirstLayer;

                
                firstLayer[i] = neuronOfFirstLayer;
            }


            //нахождения выходов для связей нейрона выхода с первым слоем
            for (int i = 0; i < countOfNeuronsFirstLayer; i++)
            {
                neuronOutput.links[i].neuronOutput = firstLayer[i].output;
            }

            //нахождение выхода выходного нейрона
            float outputNeuronOutput = 0;
            for (int i = 0; i < countOfNeuronsFirstLayer - 1; i++)
            {
                outputNeuronOutput += neuronOutput.links[i].weight * neuronOutput.links[i].neuronOutput;
            }
            //добавление смещения
            outputNeuronOutput += neuronOutput.links[countOfNeuronsFirstLayer - 1].weight;

            //активационная функция
            outputNeuronOutput = (float)1 / ((float)1 + (float)Math.Exp(-outputNeuronOutput));
            neuronOutput.output = outputNeuronOutput;

            //проверка, за что отвечает фукнция
            if (tp == TypeOfProgram.training) //функция отвечает за обучение
            {
                //проверка, что за изображение обучается
                if (isFace == true) //лицо
                {
                    //проверка, правильно ли определилось лицо
                    if (neuronOutput.output < 0.8) //неправильно
                    {
                        //обратное распространение ошибки
                        backPropagation(neuronOutput.output, firstLayer, neuronOutput, true);
                        return false;
                    }
                    else //правильно
                    {
                        return true;
                    }
                }
                else //не-лицо
                {
                    //проверка, правильно ли определилось не-лицо
                    if (neuronOutput.output >= 0.8) //неправильно
                    {
                        //обратное распространение ошибки
                        backPropagation(neuronOutput.output, firstLayer, neuronOutput, false);
                        return false;
                    }
                    else //правильно
                    {
                        return true;
                    }
                }
            }
            else if (TypeOfProgram.recognize == TypeOfProgram.recognize) //функция отвечает за определения лица (не для режима обучения)
            {
                //проверка, определилось ли лицо
                if (neuronOutput.output < 0.8) //не определилось
                {
                    return false;
                }
                else //определилось
                {
                    return true;
                }
            }

        }

        /// <summary>
        /// первая функция для обучения сети (веса еще не инициализированы)
        /// </summary>
        /// <param name="img">исходное изображение</param>
        /// <param name="firstLayer">нейроны первого уровня</param>
        /// <param name="neuronOutput">выходной нейрон</param>
        void mainAlgorithmTrain(Bitmap img, Neuron[] firstLayer, Neuron neuronOutput)
        {
            //кол-во нейронов входного слоя
            int countOfNeuronInput = 529;

            //кол-во нейронов первого слоя
            int countOfNeuronsFirstLayer = 101;

            //веса нейронов первого слоя
            float[] outputsOfFirstLayerWeights = new float[countOfNeuronInput];

            //текущие координаты пикселей изображения
            int x = 0, y = 0;

            //нахождение выходных значений связей первого слоя
            for (int i = 0; i < countOfNeuronInput; i++)
            {
                //цвет пикселя изображения
                float color = (float)img.GetPixel(x, y).R;
                outputsOfFirstLayerWeights[i] = color / 255;
                x++;
                //пересчет координат
                if (x == img.Width)
                {
                    x = 0;
                    y++;
                }
            }

            //работа с нейронами первого слоя 
            for (int i = 0; i < countOfNeuronsFirstLayer; i++)
            {
                //нейрон первого слоя
                Neuron neuronOfFirstLayer = new Neuron();

                //связи нейрона первого слоя с входом
                Link[] linksOfFirstWeights = new Link[countOfNeuronInput];

                //веса нейрона первого слоя
                float[] rndWeight = new float[countOfNeuronInput];
 
                //инициализация весов рандомными  числами
                Random rnd = new Random();
                for (int j = 0; j < countOfNeuronInput; j++)
                {     
                    int rndInt = rnd.Next(-5000, 5000);
                    float rndFl = (float)rndInt / 10000;
                    rndWeight[j] = rndFl;
                }

                //нахождение весов связей и выходного значения с предыдущего слоя
                for (int j = 0; j < countOfNeuronInput; j++)
                {
                    linksOfFirstWeights[j] = new Link();
                    linksOfFirstWeights[j].neuronOutput = outputsOfFirstLayerWeights[j];
                    float weight = rndWeight[j];
                    linksOfFirstWeights[j].weight = weight;
                }
                neuronOfFirstLayer.links = linksOfFirstWeights;

                //выход нейрона первого слоя
                float outputNeuronFirstLayer = 0;

                //нахождение выхода нейрона первого слоя
                for (int j = 0; j < countOfNeuronInput - 1; j++)
                {
                    outputNeuronFirstLayer += linksOfFirstWeights[j].weight * linksOfFirstWeights[j].neuronOutput;
                }
                //добавление смещения
                outputNeuronFirstLayer += linksOfFirstWeights[countOfNeuronInput - 1].weight;
                //активационная функция
                outputNeuronFirstLayer = (float)1 / ((float)1 + (float)Math.Exp(-outputNeuronFirstLayer));
                neuronOfFirstLayer.output = outputNeuronFirstLayer;
                firstLayer[i] = neuronOfFirstLayer;
            }

            //связи выходного нейрона с первым слоем
            Link[] linksOfSecondWeights = new Link[countOfNeuronsFirstLayer];
            //инициализация весов рандомными значениями 
            Random rndSecond = new Random();
            for (int i = 0; i < countOfNeuronsFirstLayer; i++)
            {

                int rndInt = rndSecond.Next(-5000, 5000);
                float rndFl = (float)rndInt / 10000;

                linksOfSecondWeights[i] = new Link();
                Link link = new Link();
                link.weight = rndFl;
                link.neuronOutput = firstLayer[i].output;
                linksOfSecondWeights[i] = link;
            }

            neuronOutput.links = linksOfSecondWeights;

            //выход выходного нейрона
            float outputNeuronOutput = 0;
            for (int i = 0; i < countOfNeuronsFirstLayer - 1; i++)
            {
                outputNeuronOutput += neuronOutput.links[i].weight * neuronOutput.links[i].neuronOutput;
            }
            //добавление смещения
            outputNeuronOutput += neuronOutput.links[countOfNeuronsFirstLayer - 1].weight;

            //активационная функция
            outputNeuronOutput = (float)1 / ((float)1 + (float)Math.Exp(-outputNeuronOutput));
            neuronOutput.output = outputNeuronOutput;

            //распространение ошибки
            if (neuronOutput.output < 0.8)
            {
                backPropagation(neuronOutput.output, firstLayer, neuronOutput, true);
            }

        }

        /// <summary>
        /// реализация обратного распространения ошибки
        /// </summary>
        /// <param name="outputNeuronOut">выход выходного нейрона</param>
        /// <param name="firstLayer">нейроны первого слоя</param>
        /// <param name="output">выходной нейрон</param>
        /// <param name="isFace">признак лица или не-лица</param>
        void backPropagation(float outputNeuronOut, Neuron[] firstLayer, Neuron output, bool isFace)
        {
            //скорость обучения
            float speedEd = (float)0.3;

            //ошибка выхода
            float errorOut;

            //проверка, лицо или нет 
            if (isFace == true) //для лица
            {
                errorOut = (float)1.0 - output.output;
            }
            else //для не-лица
            {
                errorOut = (float)0.0 - output.output;
            }

            //производная функции активации
            float sigmoidProizv = output.output * (1 - output.output);

            //произведение ошибки на производную
            float sigmaOut = errorOut * sigmoidProizv;

            //ошибки для распространения на предыдущий слой
            float[] errorToPrevLayer = new float[firstLayer.Count()];

            //для нейрона последнего слоя корректировка весов и нахождение ошибок для предыдущего слоя
            for (int i = 0; i < firstLayer.Count()-1; i++)
            {
                //дельта W
                float deltaW = speedEd * sigmaOut * firstLayer[i].output;
                //корректировка весов
                output.links[i].weight = output.links[i].weight + deltaW;
                //ошибка на предыдущий слой
                errorToPrevLayer[i] = firstLayer[i].output * (1 - firstLayer[i].output) * output.links[i].weight * sigmaOut;
            }

            //корректировка связи смещения
            float deltaWSmeshLastLayer = speedEd * sigmaOut;
            output.links[firstLayer.Count() - 1].weight = output.links[firstLayer.Count() - 1].weight + deltaWSmeshLastLayer;


            //для нейрона скрытого слоя корректировка весов
            for (int i = 0; i < firstLayer.Count()-1; i++)
            {
                for (int j = 0; j < firstLayer[i].links.Count()-1; j++)
                {
                    //дельта W
                    float deltaW = speedEd * errorToPrevLayer[i] * firstLayer[i].links[j].neuronOutput;
                    //корректировка весов
                    firstLayer[i].links[j].weight = firstLayer[i].links[j].weight + deltaW;
                }
                //коррекировка связи смещения
                float deltaWSmeshFirstLayer = speedEd * errorToPrevLayer[i];
                firstLayer[i].links[firstLayer[i].links.Count() - 1].weight = firstLayer[i].links[firstLayer[i].links.Count() - 1].weight + deltaWSmeshFirstLayer;
            }

        }


        [Flags]
        //тип режима алгоритма
        public enum TypeOfProgram
        {
            /// <summary>
            /// обучение
            /// </summary>
            training,

            /// <summary>
            /// определение лица
            /// </summary>
            recognize,
        }
    }
}
