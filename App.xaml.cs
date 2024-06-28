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
            try
            {
                // Set the source for the embedded Python installation
                Python.Deployment.Installer.Source = new Python.Deployment.Installer.EmbeddedResourceInstallationSource()
                {
                    Assembly = typeof(App).Assembly,
                    ResourceName = "./Resources/python-3.12.4-embed-amd64.zip",
                };

                // Set the installation path
                Python.Deployment.Installer.InstallPath = Path.GetFullPath(".");

                // Setup Python environment
                await Python.Deployment.Installer.SetupPython();
                Console.WriteLine("Python setup completed.");

                // Install mpmath
                Console.WriteLine("Installing mpmath...");
                await Python.Deployment.Installer.InstallWheel("./Resources/mpmath-1.3.0-py3-none-any.whl");
                Console.WriteLine("mpmath installed successfully.");

                // Install sympy
                Console.WriteLine("Installing sympy...");
                await Python.Deployment.Installer.InstallWheel("./Resources/sympy-1.13.0rc3-py3-none-any.whl");
                Console.WriteLine("sympy installed successfully.");

                // Set the path to the Python DLL
                Runtime.PythonDLL = "./python-3.12.4-embed-amd64/python312.dll";
                PythonEngine.Initialize();
                Console.WriteLine("Python engine initialized.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Python environment setup failed: {ex.Message}");
            }
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            PythonEngine.Shutdown();
        }
    }

}
