using JWTIntegrator.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;

namespace JWTIntegrator
{
    internal sealed class Integrator
    {
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("2298e3cc-ff51-4aac-afbb-2101d893dd63");

        private readonly Package _package;

        private Integrator(Package package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandId = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
                commandService.AddCommand(menuItem);
            }
        }

        public static Integrator Instance { get; private set; }

        private IServiceProvider ServiceProvider => _package;

        public static void Initialize(Package package)
        {
            Instance = new Integrator(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            string message = string.Format(CultureInfo.CurrentCulture, "Process Completed Successfully", GetType().FullName);
            string title = "JWT Integrator Tool";

            var project = ProjectHelpers.GetActiveProject();

            var projectpath = project.GetFullPath();

            try
            {
                FileHelper.CreateBackup(projectpath);

                FileHelper.AddtoConfigJson(FileHelper.GetJsonFile(projectpath));

                FileHelper.CreateSigningConfigurationsFile(projectpath);

                FileHelper.CreateTokenConfigurationsFile(projectpath);

                FileHelper.AddInfotoStartup(projectpath);
            }
            catch (Exception ex)
            {
                Logger.Log("Error during the operation: " + ex);
            }

            Logger.Log("Integration completed with success");

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}