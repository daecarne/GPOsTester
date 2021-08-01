// --------------------------------------------------------------------------------------------------------------------- 
// <copyright file="ManageEdgeGPOs.cs" company="Jose Marti">
//   Es propiedad de Jose Marti, 2021
// </copyright>
// <summary>
//   Define la clase ManageEdgeGPOs
// </summary>
// ---------------------------------------------------------------------------------------------------------------------

using System;
using System.Management;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;

namespace ManageEdgeGPOs
{
    /// <summary>
    ///     Gestion de eventos relacionados con las GPOs de Edge (en el registro)
    /// </summary>
    /// 
    internal class ManageEdgeGPOs
    {
        /// Campos
        internal string _hive;
        internal string _rootPath;
        internal string _keyName;
        internal string _valueName;
        internal EventLog _bitacora;
        ManagementEventWatcher _watcher;

        /// Propiedades
        public string Hive
        {
            get => _hive;
            set
            {
                _hive = value;
            }
        }
        public string RootPath
        {
            get => _rootPath;
            set
            {
                _rootPath = value;
            }
        }
        public string KeyName
        {
            get => _keyName;
            set
            {
                _keyName = value;
            }
        }
        public string ValueName
        {
            get => _valueName;
            set
            {
                _valueName = value;
            }
        }

        /// <summary>
        ///     Inicializa una instancia nueva de la clase <see cref="WmiChangeEventTester"/>
        /// </summary>
        /// 
        public ManageEdgeGPOs(EventLog b, string h, string r, string k, string v)
        {
            // Incializamos la bitacora, que es un campo.
            _bitacora = b;

            // Inicializamos los campos vía las propiedades.
            Hive = h;
            RootPath = r;
            KeyName = k;
            ValueName = v;

            // Configuramos el evento a controlar
            try
            {
                // Consulta para vigilar el registro, su composicion depende del evento
                // que deseemos vigilar. En este caso un cambio en el arbol, para ello
                // damos la rama, Hive, y el camino base desde el que vigilar cambios,
                // RootPath. Las cadenas deberan contener doble barra, \\, en lugar de
                // una sola, \, ello debido a que hay que escaparla.
                WqlEventQuery query = new(
                     "SELECT * FROM RegistryTreeChangeEvent WHERE " +
                     "hive='" + _hive + "' " +
                     @"AND rootpath='" + _rootPath + "'");

                // Depuracion
                //_bitacora.WriteEntry(query.QueryString);

                _watcher = new(query);
                // Depuracion
                //_bitacora.WriteEntry("Esperando eventos...");

                // Configuramos el delegado que manejara el evento de cambio.
                _watcher.EventArrived += new EventArrivedEventHandler(ManipulaEventoDeRegistro);

                // Comenzamos a escuchar eventos.
                _watcher.Start();

                // Lanzamos un evento inicial para asegurarnos de que examinamos el registro
                // nada mas comenzar la vigilancia y evitar que ya exista lo que hay que borrar.
                ManipulaEventoDeRegistro(this, null);

                // Depuracion.
                // Hacemos algo mientras esperamos los eventos, en este caso una espera infinita.
                // En una aplicacion normal, simplemente continuariamos con la logica de negocio.
                //System.Threading.Thread.Sleep(-1);
            }
            catch (ManagementException managementException)
            {
                _bitacora.WriteEntry("Ocurrió un error: " + managementException.Message);
            }
        }

        /// <summary>
        ///     Finalizador (destructor)
        /// </summary>
        ~ManageEdgeGPOs()
        {
            // Dado que nos cierran, dejamos de escuchar eventos.
            _watcher.Stop();
        }

        /// <summary>
        ///     Manejador de los eventos
        /// </summary>
        /// <param name="sender">
        ///     El remitente
        /// </param>
        /// <param name="e">
        ///     El evento
        /// </param>
        private void ManipulaEventoDeRegistro(object sender, EventArrivedEventArgs e)
        {
            // Depuracion
            //_bitacora.WriteEntry("He recibido un evento.");

            // RegistryKeyChangeEvent sucede aqui; ya podemos hacer algo.
            try
            {
                RegistryKey key = null;
                switch (_hive)
                {
                    case "HKEY_CLASSES_ROOT":
                        key = Registry.ClassesRoot.OpenSubKey(_rootPath, true);
                        break;
                    case "HKEY_CURRENT_USER":
                        key = Registry.CurrentUser.OpenSubKey(_rootPath, true);
                        break;
                    case "HKEY_LOCAL_MACHINE":
                        key = Registry.LocalMachine.OpenSubKey(_rootPath, true);
                        break;
                    case "HKEY_USERS":
                        key = Registry.Users.OpenSubKey(_rootPath, true);
                        break;
                    case "HKEY_CURRENT_CONFIG":
                        key = Registry.CurrentConfig.OpenSubKey(_rootPath, true);
                        break;
                }

                // Verificamos que la ruta existe y seguimos con la clave a borrar.
                if (key != null)
                {
                    // Verificamos que la clave existe y la borramos.
                    Object o = key.OpenSubKey(_keyName, true);
                    if (o != null)
                    {
                        key.DeleteSubKeyTree(_keyName, true);
                        _bitacora.WriteEntry("Encontrada la GPO de '" + _keyName + "', clave y subclaves borradas.");
                    }
                    else
                    {
                        // Depuracion
                        // No existe la clave que queremos vigilar.
                        //_bitacora.WriteEntry("No existe la clave '" + _keyName + "'.");
                    }
                }
                else
                {
                    // Depuracion
                    // No existe la ruta de la clave que queremos vigilar.
                    //_bitacora.WriteEntry("No existe la ruta '" + _rootPath + "'.");
                }
            }
            // Errores imprevistos, volcamos toda la informacíon y seguimos funcionando si lo permite el sistema.
            catch (Exception ex)
            {
                _bitacora.WriteEntry(ex.ToString());
            }
        }
    }
}