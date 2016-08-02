using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Microsoft.Kinect;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kinectpruebasonido
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {   
        //Campos de clase principal
        KinectSensor sensorKinect;  //Campo que hace referencia a clase KinectSensor de Microsoft.Kinect
        private Stream registroAudio; //Campo que hace referencia a clase Stream de System.IO
        private bool lectura;   //Campo que permite activar o desactivar la captura de sonido
        private Thread RamaLectura;     //Campo que hace referencia a conducto independiente de adquisición de sonido

        public MainWindow() //Constructor que inicia aplicación WPF
        {
            InitializeComponent();  //Iniciar componentes de ventana
            Loaded += IniciarKinect;    //Evento que inicia sensor Kinect
        }
        
        private void IniciarKinect(object sender, EventArgs e) //Método privado, estático y sin retorno que inicia sensor Kinect
        {
            foreach (var sensorpotencial in KinectSensor.KinectSensors) //Buscar en colección de KinectSensors
            {
                if (sensorpotencial.Status == KinectStatus.Connected)   //Si estado de sensor es conectado
                {
                    this.sensorKinect = sensorpotencial;    //Variable sensorpotencial será asignada a campo sensorKinect
                    break;                                  //Interrumpir búsqueda
                }
            }
            if (this.sensorKinect != null)  //Si se encontró un sensor no nulo
            {
                this.sensorKinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);   //Registro de color en formato RGB, resolución 640x480 píxeles y muestreo de 30 Hz
                this.sensorKinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);      //Registro de profundidad con resolución 640x480 píxeles y muestreo de 30 Hz
                this.sensorKinect.SkeletonStream.Enable();  //Activar rastreo de esqueleto con propiedades de suavizado por defecto
                this.sensorKinect.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);  //Registro de infrarrojo con resolución 640x480 píxeles y muestreo de 30 Hz
                this.sensorKinect.Start();    //Inicia sensor Kinect 
            }
        }
        
    }
}
