using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using JWTIntegrator.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace JWTIntegrator
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Integrator
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("2298e3cc-ff51-4aac-afbb-2101d893dd63");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Integrator"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Integrator(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Integrator Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new Integrator(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "Integrator";

            var project = ProjectHelpers.GetActiveProject();

            var projectpath = project.GetFullPath();

            AddtoConfigJson(GetJsonFile(projectpath));

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void AddtoConfigJson(string file)
        {
            Logger.Log("Adding info to .csproj file");

            var sb = new StringBuilder();
            sb.Append(@"<Target Name=""PostBuild"" AfterTargets=""PostBuildEvent"">");
            sb.Append(@"<Exec Command=""copy /Y &quot;$(TargetDir)$(ProjectName).dll&quot; &quot;$(ProjectDir)\Minifly\modules&quot;&#xD;&#xA;copy /Y &quot;$(TargetDir)$(ProjectName).pdb&quot; &quot;$(ProjectDir)\Minifly\modules&quot;&#xD;&#xA;"" />");
            sb.Append(@"</Target>");
            sb.Append(@"</Project>");

            string text = File.ReadAllText(file);
            text = text.Replace("</Project>", sb.ToString());
            File.WriteAllText(file, text);
        }

        private string GetJsonFile(string projectpath)
        {
            var files = Directory.GetFiles(projectpath, "appsettings.json");
            Logger.Log($"Found .csproj file {files.First()}");

            return files.First();
        }
    }
}