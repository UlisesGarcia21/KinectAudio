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

namespace kinectpruebasonido
{
    /// <summary>
    /// Interaction logic for RegistroMovimiento.xaml
    /// </summary>
    public partial class RegistroMovimiento : Window
    {
        //Campos de clase
        KinectSensor sensorActivo;  //Instancia que permite controlar sensor Kinect conectado y detectado
        KinectSensorChooser sensorEncendido = new KinectSensorChooser();    //Instancia que permite localizar sensor automáticamente

        public RegistroMovimiento()
        {
            InitializeComponent();
        }

        private void ActivarSensor(object sender, EventArgs e)
        {
            this.sensorEncendido.KinectChanged += SensorDetectado;  //Controlador de evento KinectChanged
            this.sensorEncendido.Start();   //Comenzar búsqueda de sensor Kinect
            this.logoKinect.KinectSensorChooser = this.sensorEncendido;    //Relacionar estado de sensor con logo indicador
            this.sensorActivo = this.sensorEncendido.Kinect;
            
            if (this.sensorActivo == null)
            {
                this.mensajeKinect.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.Reiniciar);
            }
            else if (this.sensorActivo.Status.Equals(KinectStatus.Connected))
            {
                this.mensajeKinect.Text = string.Format(CultureInfo.CurrentCulture, this.sensorEncendido.Status.ToString());
                this.sensorActivo.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30); //Cámara de color
                this.sensorActivo.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);    //Cámara de profundidad
                this.sensorActivo.SkeletonStream.Enable();    //Rastreo de esqueleto
                this.sensorActivo.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);    //Cámara infrarroja
                this.sensorActivo.Start();   //Iniciar registro de cámaras de sensor kinect
                this.visorUsuario.KinectSensor = this.sensorActivo; //Asociar cámara conectada con KinectViewer
            }
        }

        private void SensorDetectado(object sender, KinectChangedEventArgs e)
        {
            this.sensorEncendido.Start();   //Iniciar búsqueda de sensor Kinect

            if (e.NewSensor == null)
            {
                this.mensajeKinect.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.Reiniciar, 0);
                this.sensorActivo = e.NewSensor;
            }

            if (e.NewSensor != null)    //Si nuevo sensor no es nulo
            {
                this.sensorActivo = e.NewSensor;
                this.mensajeKinect.Text = string.Format(CultureInfo.CurrentCulture, Properties.Resources.KinectListo, this.sensorEncendido.Status.ToString());
                e.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30); //Cámara de color
                e.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);    //Cámara de profundidad
                e.NewSensor.SkeletonStream.Enable();    //Rastreo de esqueleto
                e.NewSensor.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);    //Cámara infrarroja
                this.sensorActivo.Start();   //Iniciar registro de cámaras de sensor kinect
                this.visorUsuario.KinectSensor = this.sensorActivo; //Asociar cámara conectada con KinectViewUser
            }
        }

    }
}
