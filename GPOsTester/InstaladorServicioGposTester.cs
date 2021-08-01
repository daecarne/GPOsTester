using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace GPOsTester
{
    [RunInstaller(true)]
    public partial class InstaladorServicioGposTester : System.Configuration.Install.Installer
    {
        public InstaladorServicioGposTester()
        {
            InitializeComponent();
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            string parameter = "GPOsTester\" \"GPOsTester Bitacora";
            Context.Parameters["assemblypath"] = "\"" + Context.Parameters["assemblypath"] + "\" \"" + parameter + "\"";
            base.OnBeforeInstall(savedState);
        }
    }
}