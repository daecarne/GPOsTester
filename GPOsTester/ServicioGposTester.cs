using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;

namespace GPOsTester
{
    public partial class ServicioGposTester : ServiceBase
    {
        // Campos
        private int eventoId = 1;
        ManageEdgeGPOs.ManageEdgeGPOs gposControladas1;

        public enum EstadosServicio
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EstadoServicio
        {
            public int dwServiceType;
            public EstadosServicio dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        // Ensamblados
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref EstadoServicio serviceStatus);

        public ServicioGposTester(string[] args)
        {
            InitializeComponent();

            string origenEvento = "GPOsTester";
            string bitacoraEvento = "GPOsTester Bitacora";

            // Tratamiento de argumentos, si se han pasado.
            if (args.Length > 0)
            {
                origenEvento = args[0];
            }

            if (args.Length > 1)
            {
                bitacoraEvento = args[1];
            }

            eventoBitacora = new EventLog();
            if (!EventLog.SourceExists(origenEvento))
            {
                // Esto borra la bitacora creada caso de que se quede y la
                // deseemos eliminar.
                //EventLog.Delete("BitacoraGposTester");
                EventLog.CreateEventSource(origenEvento, bitacoraEvento);
            }
            eventoBitacora.Source = origenEvento;
            eventoBitacora.Log = bitacoraEvento;

            // El servicio puede realizar acciones mediante eventos o
            // alarmas. En este ejemplo vamos a usar una alarma.
            // Configuramos una alarma que se dispara cada minuto.
            Timer timer = new Timer
            {
                Interval = 60000 // 60 segundos (en milisegundos)
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected override void OnStart(string[] args)
        {
            // El modelo que vamos a usar aquí es el de monohilo, donde todo
            // el trabajo lo realizamos en el hilo principal. Pero lo suyo es
            // usar el modelo multihilo y descargar el trabajo del servicio
            // en un hilo secundario. Para ejemplos ver el siguiente enlace:
            // https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.backgroundworker

            // Actualizamos el estado del servicio a "Arrancando el servicio." y
            // lo añadimos a la bitacora. (solo tiene sentido si el proceso de
            // arranque es largo)
            EstadoServicio estadoServicio = new EstadoServicio
            {
                dwCurrentState = EstadosServicio.SERVICE_START_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref estadoServicio);    
            eventoBitacora.WriteEntry("Arrancando el servicio GPOsTester.");

            // Comenzamos la primera tarea a controlar, en este caso las GPOs de Edge. Basta
            // con llamar a la clase que hemos definido para controlar cambios del registro
            // relacionados con Edge y que deseemos eliminar (lo que controlamos es lo que borramos)
            // Dado que contiene un bucle infinito, solo se saldrá por un error o por que el servicio
            // termine.
            gposControladas1 = new(
                eventoBitacora,
                "HKEY_LOCAL_MACHINE",
                "SOFTWARE\\\\Policies\\\\Microsoft",
                "Edge",
                "HomepageLocation"
                );

            // Una vez ya hemos ejecutado lo que sea, ponemos el estado en ejecucion
            // e informamos en la bitacora.
            estadoServicio.dwCurrentState = EstadosServicio.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref estadoServicio);
            eventoBitacora.WriteEntry("Servicio GPOsTester arrancado.");
        }

        protected override void OnStop()
        {
            // Actualizamos el estado del servicio a "Parando el servicio." y
            // lo añadimos a la bitacora. (solo tiene sentido si el proceso de
            // paro es largo)
            EstadoServicio estadoServicio = new EstadoServicio
            {
                dwCurrentState = EstadosServicio.SERVICE_STOP_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref estadoServicio);
            eventoBitacora.WriteEntry("Parando el servicio GPOsTester.");

            // Finalizamos la primera tarea a controlar, en este caso las GPOs de Edge. Basta
            // con hacer nula la instancia.
            gposControladas1 = null;

            // Actualizamos el estado del servicio a "Servicio parado." y
            // lo añadimos a la bitacora.
            estadoServicio.dwCurrentState = EstadosServicio.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref estadoServicio);
            eventoBitacora.WriteEntry("Servicio GPOsTester parado.");
        }

        protected override void OnPause()
        {
            // Anunciamos que el servicio se va a pausar.
            eventoBitacora.WriteEntry("Pausando el servicio GPOsTester.");
        }

        protected override void OnContinue()
        {
            // Anunciamos que el servicio se va a reanudar.
            eventoBitacora.WriteEntry("Reanudando el servicio GPOsTester.");
        }

        protected override void OnShutdown()
        {
            // Anunciamos que el servicio se va a detener por un shutdown.
            eventoBitacora.WriteEntry("Parando el servicio GPOsTester por un shutdown.");

            // Finalizamos la primera tarea a controlar, en este caso las GPOs de Edge. Basta
            // con hacer nula la instancia.
            gposControladas1 = null;
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Inserta aqui las actividades de monitorizacion.
            eventoBitacora.WriteEntry("Monitorizando el sistema.", EventLogEntryType.Information, eventoId++);
        }

        // Metodo para ejecutar el servicio como un comando interactivo para depurarlo.
        internal void DepuraServicioGposTester(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }
    }
}