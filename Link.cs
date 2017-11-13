using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceRecognition
{
    /// <summary>
    /// класс связи нейрона для подхода на основн нейронной сети 
    /// </summary>
    class Link
    {
        //вес нейроной связи
        public float weight;

        //выход предыдущего нейрона
        public float neuronOutput;
    }
}
