using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Zetes.ZStock.Email
{
    [RunInstaller(true)]
    public class MyWindowsServiceInstaller : Installer
    {
        public MyWindowsServiceInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            //set the privileges
            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.DisplayName = Program.Named;
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            //must be the same as what was set in Program's constructor
            serviceInstaller.ServiceName = Program.Named;

            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
