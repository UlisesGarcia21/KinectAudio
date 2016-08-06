using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
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
using System.Windows.Resources;
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
        KinectSensor sensorActivo;   //Campo que es instancia de clase KinectSensor
        private const int intervaloPoleoAudio = 50; //Campo que define tiempo en ms entre cada lectura de audio
        private const int muestraPorMilisegundo = 16;   //Campo que define cantidad de muestras obtenidas cada milisegundo
        private const int bytesPorMuestra = 2;  //Campo que define número de bytes en cada muestra de audio
        private readonly byte[] bufferAudio = new byte[intervaloPoleoAudio * muestraPorMilisegundo * bytesPorMuestra];  //Campo de buffer para almacenar datos de registro de audio

        private readonly double[] energia = new double[(uint)(780*1.25)];   //Campo para buffer que almacena datos de energía conforme se lee el audio
        private readonly object bloqueoEnergia = new object();  //Campo que hace referencia a un objeto que asegura buffer de energía para sincronización de canales
        
        private Stream registroAudio; //Campo que hace referencia a clase Stream de System.IO
        private bool lectura;   //Campo que permite activar o desactivar la captura de sonido
        private Thread canalLectura;     //Campo que hace referencia a conducto independiente de adquisición de sonido

        public MainWindow() //Constructor que inicia aplicación WPF
        {
            InitializeComponent();  //Iniciar componentes de ventana
        }

        private void IniciarKinect(object sender, RoutedEventArgs e) //Método privado, estático y sin retorno que inicia sensor Kinect
        {
            foreach (var sensorPotencial in KinectSensor.KinectSensors)
            {
                if(sensorPotencial.Status == KinectStatus.Connected)
                {
                    this.sensorActivo = sensorPotencial;
                    break;
                }
            }

            if(null != this.sensorActivo)
            {
                try
                {
                    this.sensorActivo.Start();
                }
                catch(IOException)
                {
                    this.sensorActivo = null;
                }
            }
            if(null == this.sensorActivo)
            {
                MessageBox.Show("Sensor desconectado");
            }
                        
                this.sensorActivo.AudioSource.BeamAngleChanged += this.CampoAudioModificado; //Controlador de evento BeamAngleChanged
                this.sensorActivo.AudioSource.SoundSourceAngleChanged += this.FuenteAudioModificada;  //Controlador de evento SoundSourceAngleChanged
                this.registroAudio = this.sensorActivo.AudioSource.Start();  //Iniciar registro de audio

                //Iniciar canal separado para adquisición de audio
                this.lectura = true;    //Activar registro de audio
                this.canalLectura = new Thread(CanalLecturaAudio);  //Crear nuevo canal para registro de audio
                this.canalLectura.Start();  //Iniciar registro de audio
        }

        private void DetenerRegistroAudio(object sender, CancelEventArgs e) //Método para finalizar adquisición de audio
        {
            this.lectura = false;   //Desactivar registro de audio
            if (null != canalLectura)
            {
                canalLectura.Join();    //Detener ejecución de canal creado previamente
            }
            if (null != this.sensorActivo)
            {
                this.sensorActivo.AudioSource.BeamAngleChanged -= this.CampoAudioModificado; //Desenlazar controlador de evento BeamAngleChanged
                this.sensorActivo.AudioSource.SoundSourceAngleChanged -= this.FuenteAudioModificada; //Desenlazar controlador de evento SoundSourceAngleChanged
                this.sensorActivo.AudioSource.Stop();    //Detener registro de audio
                this.sensorActivo.Stop();    //Inhabilitar sensor detectado
                this.sensorActivo = null;
            }
        }

        private void CampoAudioModificado(object sender, BeamAngleChangedEventArgs e)   //Método que actúa como controlador de evento BeamAngleChanged
        {
            rotacionCono.Angle = -e.Angle;  //Mostrar dirección de campo de audio cada vez que se modifica
        }

        private void FuenteAudioModificada(object sender, SoundSourceAngleChangedEventArgs e)   //Método que actúa como controlador de evento SoundSourceAngleChanged
        {
            const double anchoMinimoGradiente = 0.04;   //Máxima confianza con este ancho de gradiente
            double mitadAncho = Math.Max((1 - e.ConfidenceLevel), anchoMinimoGradiente) / 2;
            this.sourceGsPre.Offset = Math.Max(this.sourceGsMain.Offset - mitadAncho, 0);
            this.sourceGsPost.Offset = Math.Min(this.sourceGsMain.Offset+mitadAncho,1);
            fuenteRotacion.Angle = -e.Angle;
        }
        
        private void CanalLecturaAudio()    //Método que controla canal independiente para registro de sonidos
        {
            while(this.lectura) //Mientras el campo lectura sea verdadero
            {
                int conteoLectura = registroAudio.Read(bufferAudio,0,bufferAudio.Length);
                lock(this.bloqueoEnergia)
                {
                    for (int i = 0; i < conteoLectura; i += 2)
                    {
                        short muestraAudio = BitConverter.ToInt16(bufferAudio, i);
                    }
                }
            }
        }

    }
}
