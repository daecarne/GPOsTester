
namespace GPOsTester
{
    partial class InstaladorServicioGposTester
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.instaladorProcesoGposTester = new System.ServiceProcess.ServiceProcessInstaller();
            this.instaladorGposTester = new System.ServiceProcess.ServiceInstaller();
            // 
            // instaladorProcesoGposTester
            // 
            this.instaladorProcesoGposTester.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.instaladorProcesoGposTester.Password = null;
            this.instaladorProcesoGposTester.Username = null;
            // 
            // instaladorGposTester
            // 
            this.instaladorGposTester.Description = "Servicio para el testeo de GPOs";
            this.instaladorGposTester.DisplayName = "Tester de GPOs";
            this.instaladorGposTester.ServiceName = "ServicioGposTester";
            this.instaladorGposTester.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.instaladorProcesoGposTester,
            this.instaladorGposTester});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller instaladorProcesoGposTester;
        private System.ServiceProcess.ServiceInstaller instaladorGposTester;
    }
}