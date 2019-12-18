using System.ComponentModel;
using System.Configuration.Install;

namespace OpenTax.Engine.Reporter
{
    [RunInstaller(true)]
    public partial class eTaxInstaller : Installer
    {
        public eTaxInstaller()
        {
            InitializeComponent();
        }
    }
}
