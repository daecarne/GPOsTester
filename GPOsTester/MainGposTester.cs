using GPOsTester;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

//
// Usamos la nueva especificacion de C# 9.0 donde usamos top-level statements en lugar de Main
// La parte Main acaba en el namespace, el propio Main es el namespace general.
//

// Comenzamos la utilidad en si.
//
// Entorno de debug. Esto nos permite ejecutar el servicio como
// un comando interactivo y poder depurarlo sin tener que instalarlo
// y toda la parafernalia asociada a los servicios. Si detectamos
// el uso interactivo entramos en modo debug.
if (Environment.UserInteractive)
{
    // Esta utilidad necesita privilegios de administracion. Esto lo podriamos resolver
    // añadiendo un manifiesto y configurando la UAC en el, pero este metodo permite capturar
    // las excepciones con la UAC de un modo mas preciso y menos complicado que con manifiesto.
    GPOsTester.LanzaUAC SoyAdmin = new(args);
    ServicioGposTester servicioDepurar = new ServicioGposTester(args);
    servicioDepurar.DepuraServicioGposTester(args);
}
else
{
    // Si no detectamos el uso interactivo entramos en modo servicio.
    ServiceBase[] ServicesToRun;
    ServicesToRun = new ServiceBase[]
    {
                    new ServicioGposTester(args)
    };
    ServiceBase.Run(ServicesToRun);
}

namespace GPOsTester
{
    internal class LanzaUAC
    {
        /// <summary>
        ///     Si la cuenta de ejecucion no es admin relanzamos la UAC.
        /// </summary>
        /// 
        public LanzaUAC(string[] args)
        {
            // Si nos han pasado argumentos los covertimos en una string
            string argumentos = "";
            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    argumentos = argumentos + arg + " ";
                }
            }

            if (!SoyAdmin())
            {
                ProcessStartInfo proceso = new();
                proceso.UseShellExecute = true;
                proceso.WorkingDirectory = Environment.CurrentDirectory;
                proceso.FileName = Assembly.GetEntryAssembly().CodeBase;
                proceso.Arguments = argumentos;

                proceso.Verb = "runas";

                try
                {
                    Process.Start(proceso);
                    Process.GetCurrentProcess().Kill();
                }
                catch (Exception ex)
                {
                    _ = ex.ToString();
                    Console.WriteLine("\nLa depuración del servicio debe ejecutarse como administrador.\n");
                    Process.GetCurrentProcess().Kill();
                }
            }
        }

        /// <summary>
        ///     Comprobamos si la cuenta de ejecucion tiene privilegios de admin.
        /// </summary>
        /// 
        private bool SoyAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(id);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}