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
    internal sealed class Integrator
    {
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("2298e3cc-ff51-4aac-afbb-2101d893dd63");

        private readonly Package package;

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

        public static Integrator Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static void Initialize(Package package)
        {
            Instance = new Integrator(package);
        }

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