using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Kinect;
using Microsoft.Kinect.Interop;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Properties;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Speech.AudioFormat;
using System.Speech.Recognition;

namespace kinectpruebasonido
{
    /// <summary>
    /// Interaction logic for RegistroMovimiento.xaml
    /// </summary>
    public partial class RegistroMovimiento : Window
    {
        //Campos de clase
        KinectSensor sensorActivo1;  //Instancia que permite controlar sensor Kinect conectado y detectado
        KinectSensorChooser sensorEncendido1 = new KinectSensorChooser();    //Instancia que permite localizar sensor automáticamente
        private Stream registroAudio; //Campo que hace referencia a clase Stream de System.IO
        int i = 0;  //Constante de control para repetición de palabra de inicio
        
        private SpeechRecognitionEngine aparatoVoz; //Campo para instancia de SpeechRecognitionEngine que se utiliza para kinect como fuente de datos de audio
        private List<Span> palabraReconocimiento;   //Lista de los elementos en UI que resaltarán con reconocimiento de voz 

        public RegistroMovimiento()
        {
            InitializeComponent();
        }

        private static RecognizerInfo ConseguirKinectReconocedor()  //Método que devuelve información de un reconocedor de voz
        {
            foreach (RecognizerInfo reconocedor in SpeechRecognitionEngine.InstalledRecognizers())
            {
                if (reconocedor.Culture.TwoLetterISOLanguageName.Equals("es"))
                {
                    return reconocedor;
                }
            }
            return null;
        }

        private void ActivarSensor(object sender, EventArgs e)
        {
            this.sensorEncendido1.KinectChanged += SensorEncontrado;  //Controlador de evento KinectChanged
            this.sensorEncendido1.Start();   //Comenzar búsqueda de sensor Kinect
            this.logoKinect.KinectSensorChooser = this.sensorEncendido1;    //Relacionar estado de sensor con logo indicador
            this.sensorActivo1 = this.sensorEncendido1.Kinect;
            
            if (this.sensorActivo1 == null)
            {
                this.mensajeKinect.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.Reiniciar);
            }
            else if (this.sensorActivo1.Status.Equals(KinectStatus.Connected))
            {
                this.mensajeKinect.Text = string.Format(CultureInfo.CurrentCulture, this.sensorEncendido1.Status.ToString());
                this.sensorActivo1.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30); //Cámara de color
                this.sensorActivo1.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);    //Cámara de profundidad
                this.sensorActivo1.SkeletonStream.Enable();    //Rastreo de esqueleto
                this.sensorActivo1.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);    //Cámara infrarroja
                this.sensorActivo1.Start();   //Iniciar registro de cámaras de sensor kinect
                //this.visorUsuario.KinectSensor = this.sensorActivo; //Asociar cámara conectada con KinectViewer

                RecognizerInfo microfono = ConseguirKinectReconocedor();
                if (null != microfono)
                {
                    palabraReconocimiento = new List<Span> { hola };
                    this.aparatoVoz = new SpeechRecognitionEngine(microfono.Id);

                    var palabrasReconocimiento = new Choices();
                    palabrasReconocimiento.Add(new SemanticResultValue("Hola", "HOLA"));

                    var gb = new GrammarBuilder { Culture = microfono.Culture };
                    gb.Append(palabrasReconocimiento);
                    var g = new Grammar(gb);
                    aparatoVoz.LoadGrammar(g);

                    aparatoVoz.SpeechRecognized += vozReconocida;
                    aparatoVoz.SpeechRecognitionRejected += vozRechazada;
                    this.registroAudio = this.sensorActivo1.AudioSource.Start();
                    aparatoVoz.SetInputToAudioStream(this.registroAudio, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                }
            }
        }

        private void SensorEncontrado(object sender, KinectChangedEventArgs e)
        {
            this.sensorEncendido1.Start();   //Iniciar búsqueda de sensor Kinect

            if (e.NewSensor == null)
            {
                this.mensajeKinect.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.Reiniciar, 0);
                this.sensorActivo1 = e.NewSensor;
            }

            if (e.NewSensor != null)    //Si nuevo sensor no es nulo
            {
                this.sensorActivo1 = e.NewSensor;
                this.mensajeKinect.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.KinectListo, this.sensorEncendido1.Status.ToString());
                e.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30); //Cámara de color
                e.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);    //Cámara de profundidad
                e.NewSensor.SkeletonStream.Enable();    //Rastreo de esqueleto
                e.NewSensor.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);    //Cámara infrarroja
                this.sensorActivo1.Start();   //Iniciar registro de cámaras de sensor kinect

                RecognizerInfo microfono = ConseguirKinectReconocedor();
                if (null != microfono)
                {
                    palabraReconocimiento = new List<Span> { hola };
                    this.aparatoVoz = new SpeechRecognitionEngine(microfono.Id);

                    var palabrasReconocimiento = new Choices();
                    palabrasReconocimiento.Add(new SemanticResultValue("Hola", "HOLA"));

                    var gb = new GrammarBuilder { Culture = microfono.Culture };
                    gb.Append(palabrasReconocimiento);
                    var g = new Grammar(gb);
                    aparatoVoz.LoadGrammar(g);

                    aparatoVoz.SpeechRecognized += vozReconocida;
                    aparatoVoz.SpeechRecognitionRejected += vozRechazada;
                    aparatoVoz.SetInputToAudioStream(this.sensorActivo1.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                    aparatoVoz.RecognizeAsync(RecognizeMode.Multiple);
                }
                else
                {
                    MessageBox.Show("No funciono :'(");
                }
            }
        }

        private void vozReconocida(object sender, SpeechRecognizedEventArgs e)
        {
            const double umbralConfianza = 0.3;
            LimpiarLetrasReconocidas();
            if (e.Result.Confidence >= umbralConfianza)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "HOLA":
                        hola.Foreground = Brushes.DeepSkyBlue;
                        hola.FontWeight = FontWeights.Bold;
                        this.i++;
                        if(this.i == 3)
                        {
                            this.visorUsuario.KinectSensor = this.sensorActivo1; //Asociar cámara conectada con KinectViewUser
                        }

                        break;
                }
            }
        }

        private void vozRechazada(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            LimpiarLetrasReconocidas();
        }

        private void LimpiarLetrasReconocidas()
        {
            foreach (Span span in palabraReconocimiento)
            {
                span.Foreground = Brushes.Black;
                span.FontWeight = FontWeights.Normal;
            }
        }

        private void CerrarVentana(object sender, EventArgs e)
        {
            if (null != this.sensorActivo1)
            {
                this.sensorActivo1.AudioSource.Stop();    //Detener registro de audio
                this.sensorActivo1.Stop();    //Inhabilitar sensor detectado
                this.sensorActivo1 = null;   //Detener registro de información
            }

            if (null != this.aparatoVoz)
            {
                this.aparatoVoz.SpeechRecognized -= vozReconocida;
                this.aparatoVoz.SpeechRecognitionRejected -= vozRechazada;
                this.aparatoVoz.RecognizeAsyncStop();
            }
        }

    }
}
