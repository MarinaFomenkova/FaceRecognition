using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceRecognition
{
    /// <summary>
    /// класс нейрона для подхода на основе нейронной сети
    /// </summary>
    class Neuron
    {
        //связи нейрона 
        public Link[] links;

        //выход нейрона
        public float output;
    }
}
