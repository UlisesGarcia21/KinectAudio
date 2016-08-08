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
    public partial class VentanaPrincipal : Window
    {   
        //Campos de clase principal
        KinectSensorChooser sensorEncendido = new KinectSensorChooser();    //Campo que es instancia de objeto KinectSensorChooser
        KinectSensor sensorActivo;  //Campo que permite control de sensor Kinect
        private const int intervaloPoleoAudio = 50; //Campo que define tiempo en ms entre cada lectura de audio
        private const int muestraPorMilisegundo = 16;   //Campo que define cantidad de muestras obtenidas cada milisegundo
        private const int bytesPorMuestra = 2;  //Campo que define número de bytes en cada muestra de audio
        private readonly byte[] bufferAudio = new byte[intervaloPoleoAudio * muestraPorMilisegundo * bytesPorMuestra];  //Campo de buffer para almacenar datos de registro de audio

        private readonly double[] energia = new double[(uint)(780*1.25)];   //Campo para buffer que almacena datos de energía conforme se lee el audio
        private readonly object bloqueoEnergia = new object();  //Campo que hace referencia a un objeto que asegura buffer de energía para sincronización de canales
        
        private Stream registroAudio; //Campo que hace referencia a clase Stream de System.IO
        private bool lectura;   //Campo que permite activar o desactivar la captura de sonido
        private Thread canalLectura;     //Campo que hace referencia a conducto independiente de adquisición de sonido

        public VentanaPrincipal() //Constructor que inicia aplicación WPF
        {
            InitializeComponent();  //Iniciar componentes de ventana
            this.sensorEncendido.Start();   //Comenzar búsqueda de sensor Kinect
        }

        private void IniciarKinect(object sender, RoutedEventArgs e) //Método privado, estático y sin retorno que indica modo de iniciar la ventana principal
        {
            this.sensorEncendido.KinectChanged += SensorDetectado;  //Controlador de evento KinectChanged
            this.indicadorKinect.KinectSensorChooser = this.sensorEncendido;    //Relacionar estado de sensor con logo indicador
            this.sensorActivo = this.sensorEncendido.Kinect;

            if (this.sensorActivo != null)
            {
                this.sensorEncendido.Kinect.AudioSource.BeamAngleChanged += this.CampoAudioModificado; //Controlador de evento BeamAngleChanged
                this.sensorEncendido.Kinect.AudioSource.SoundSourceAngleChanged += this.FuenteAudioModificada;  //Controlador de evento SoundSourceAngleChanged
                this.registroAudio = this.sensorActivo.AudioSource.Start();  //Iniciar registro de audio de sensor

                //Iniciar canal separado para adquisición de audio
                this.lectura = true;    //Activar registro de audio
                this.canalLectura = new Thread(CanalLecturaAudio);  //Crear nuevo canal para registro de audio
                this.canalLectura.Start();  //Iniciar registro de audio
            }
        }

        private void CanalLecturaAudio()    //Método que controla canal independiente para registro de sonidos
        {
            while (this.lectura) //Mientras el campo lectura sea verdadero
            {
                int conteoLectura = registroAudio.Read(bufferAudio, 0, bufferAudio.Length);
                lock (this.bloqueoEnergia)
                {
                    for (int i = 0; i < conteoLectura; i += 2)
                    {
                        short muestraAudio = BitConverter.ToInt16(bufferAudio, i);
                    }
                }
            }
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
                this.sensorActivo = null;   //Detener registro de información
            }
        }

        private void CampoAudioModificado(object sender, BeamAngleChangedEventArgs e)   //Método que actúa como controlador de evento BeamAngleChanged
        {
            rotacionCono.Angle = -e.Angle;  //Mostrar dirección de campo de audio cada vez que se modifica
            textoAnguloCono.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.CampoSonido, e.Angle.ToString("0", CultureInfo.CurrentCulture));
        }

        private void FuenteAudioModificada(object sender, SoundSourceAngleChangedEventArgs e)   //Método que actúa como controlador de evento SoundSourceAngleChanged
        {
            const double anchoMinimoGradiente = 0.04;   //Máxima confianza con este ancho de gradiente
            double mitadAncho = Math.Max((1 - e.ConfidenceLevel), anchoMinimoGradiente) / 2;
            this.sourceGsPre.Offset = Math.Max(this.sourceGsMain.Offset - mitadAncho, 0);
            this.sourceGsPost.Offset = Math.Min(this.sourceGsMain.Offset+mitadAncho,1);
            fuenteRotacion.Angle = -e.Angle;
            textoAnguloFuente.Text = string.Format(CultureInfo.CurrentCulture,Properties.Resources.FuenteSonido,e.Angle.ToString("0",CultureInfo.CurrentCulture));
        }

        private void SensorDetectado(object sender, KinectChangedEventArgs e)
        {
            this.sensorEncendido.Start();   //Iniciar búsqueda de sensor Kinect

            if (e.NewSensor == null)
            {
                this.textoEstadoKinect.Text = string.Empty;
                this.textoIDKinect.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.NoKinectReady, this.sensorEncendido.RequiredConnectionId);
                this.sensorActivo = e.NewSensor;
            }

            if (e.NewSensor != null)    //Si nuevo sensor no es nulo
            {
                this.sensorActivo = e.NewSensor;
                this.textoIDKinect.Text = string.Empty;
                this.textoEstadoKinect.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.KinectListo, this.sensorEncendido.Status.ToString());
                e.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30); //Cámara de color
                e.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);    //Cámara de profundidad
                e.NewSensor.SkeletonStream.Enable();    //Rastreo de esqueleto
                e.NewSensor.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);    //Cámara infrarroja
                this.sensorActivo.Start();   //Iniciar registro de cámaras de sensor kinect
            }
        }
    }
}
