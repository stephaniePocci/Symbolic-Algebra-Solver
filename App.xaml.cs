using Python.Runtime;
using System.IO;
using System.Windows;


namespace Symbolic_Algebra_Solver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void OnStartUp(object sender, StartupEventArgs e)
        {
            Python.Deployment.Installer.Source = new Python.Deployment.Installer.EmbeddedResourceInstallationSource()
            {
                Assembly = typeof(App).Assembly,
                ResourceName = "./Resources/python-3.12.4-embed-amd64.zip",
            };

            Python.Deployment.Installer.InstallPath = Path.GetFullPath(".");

            await Python.Deployment.Installer.SetupPython();

            await Python.Deployment.Installer.InstallWheel("./Resources/mpmath-1.3.0-py3-none-any.whl");
            await Python.Deployment.Installer.InstallWheel("./Resources/sympy-1.13.0rc3-py3-none-any.whl");

            Runtime.PythonDLL = "./python-3.12.4-embed-amd64/python312.dll";
            PythonEngine.Initialize();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            PythonEngine.Shutdown();
        }
    }

}
