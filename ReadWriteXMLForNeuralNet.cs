using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace FaceRecognition
{
    /// <summary>
    /// чтение и запись в XML файл весов нейрона
    /// </summary>
    class ReadWriteXMLForNeuralNet
    {
        /// <summary>
        /// чтение весов нейрона
        /// </summary>
        /// <param name="firstLayer">первый слой (следует за входным)</param>
        /// <param name="output">выходной нейрон</param>
        public void readXml(Neuron[] firstLayer, Neuron output)
        {
            //XML файл
            string path = "weights.xml";

            //XmlTextReader 
            XmlTextReader reader = new XmlTextReader(path);

            //кол-во нейронов на первом слое
            int countOfNeuronsFirstLayer = 101;

            //кол-во нейронов на входном слое (связаны с первым)
            int countOfLinksFirstLayer = 529;

            //чтение из файла
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "Neuron") //если элемент - нейрон первого слоя
                    {
                        //индекс теущего нейрона
                        int indexOfNeuronFirstLayer = Convert.ToInt32(reader[0]);

                        //считывание весов текущего нейрона
                        for (int i = 0; i < countOfLinksFirstLayer; i++)
                        {
                            bool ok = true;
                            while (ok)
                            {
                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    firstLayer[indexOfNeuronFirstLayer].links[i].weight = (float)Convert.ToDouble(reader.Value);
                                    ok = false;
                                }
                            }
                        }
                    }
                    if (reader.Name == "OutputNeuron") //если элемент - нейрон последнего слоя
                    {
                        //считывание весов нейрона выхода
                        for (int i = 0; i < countOfNeuronsFirstLayer; i++)
                        {
                            bool ok = true;
                            while (ok)
                            {

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    output.links[i].weight = (float)Convert.ToDouble(reader.Value);
                                    ok = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// запись в XML файл 
        /// </summary>
        /// <param name="firstLayer">нейроны первого слоя</param>
        /// <param name="output">выходной нейрон</param>
        public void writeXML(Neuron[] firstLayer, Neuron output)
        {
            //XmlTextWriter
            XmlTextWriter textWritter = new XmlTextWriter("weights.xml", Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement("head");
            textWritter.WriteEndElement();
            textWritter.Close();
            //XmlDocument
            XmlDocument document = new XmlDocument();
            document.Load("weights.xml");

            //запись в документ весов нейронов первого слоя
            for (int i = 0; i < firstLayer.Count(); i++)
            {
                //узел - нейрон первого слоя
                XmlNode neuron = document.CreateElement("Neuron");
                document.DocumentElement.AppendChild(neuron);
                XmlAttribute attribute = document.CreateAttribute("number");
                attribute.Value = i.ToString(); 
                //запись весов нейронов первого слоя
                for (int j = 0; j < firstLayer[i].links.Count(); j++)
                {
                    neuron.Attributes.Append(attribute);
                    XmlNode subElement = document.CreateElement("weight" + j.ToString());
                    subElement.InnerText = firstLayer[i].links[j].weight.ToString(); 
                    neuron.AppendChild(subElement); 
                }
            }

            //узел - выходной нейрон
            XmlNode neuronOutputXML = document.CreateElement("OutputNeuron");
            document.DocumentElement.AppendChild(neuronOutputXML); 
            XmlAttribute attributeNeuronOutput = document.CreateAttribute("number"); 
            attributeNeuronOutput.Value = "1"; 
            //запись весов нейронов выходного слоя
            for (int i = 0; i < firstLayer.Count(); i++)
            {
                neuronOutputXML.Attributes.Append(attributeNeuronOutput);
                XmlNode subElement = document.CreateElement("weight" + i.ToString()); 
                subElement.InnerText = output.links[i].weight.ToString(); 
                neuronOutputXML.AppendChild(subElement); 
            }

            //сохранение файла
            document.Save("weights.xml");
        }


    }
}
