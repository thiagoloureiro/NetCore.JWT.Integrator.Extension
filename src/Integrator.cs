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
            string message = string.Format(CultureInfo.CurrentCulture, "Process Completed Successfully", this.GetType().FullName);
            string title = "Integrator";

            var project = ProjectHelpers.GetActiveProject();

            var projectpath = project.GetFullPath();

            AddtoConfigJson(GetJsonFile(projectpath));

            CreateSigningConfigurationsFile(projectpath);

            CreateTokenConfigurationsFile(projectpath);

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

            // Removing the First Line of file
            var lines = File.ReadAllLines(file);
            File.WriteAllLines(file, lines.Skip(1).ToArray());

            var currentContent = File.ReadAllText(file);

            var sb = new StringBuilder();
            sb.AppendLine(@"{");
            sb.AppendLine(@"  ""TokenConfigurations"": {");
            sb.AppendLine(@"    ""Audience"": ""ExemploAudience"",");
            sb.AppendLine(@"    ""Issuer"": ""ExemploIssuer"",");
            sb.AppendLine(@"    ""Seconds"": 120");
            sb.AppendLine(@" },");

            File.WriteAllText(file, sb.ToString() + currentContent);
        }

        private string GetJsonFile(string projectpath)
        {
            var files = Directory.GetFiles(projectpath, "appsettings.json");
            Logger.Log($"Found .csproj file {files.First()}");

            return files.First();
        }

        private void CreateSigningConfigurationsFile(string projectpath)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"using System.Security.Cryptography;");
            sb.AppendLine(@"using Microsoft.IdentityModel.Tokens;");
            sb.AppendLine(@"");
            sb.AppendLine(@"    public class SigningConfigurations");
            sb.AppendLine(@"    {");
            sb.AppendLine(@"        public SecurityKey Key { get; }");
            sb.AppendLine(@"        public SigningCredentials SigningCredentials { get; }");
            sb.AppendLine(@" ");
            sb.AppendLine(@"        public SigningConfigurations()");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"            using (var provider = new RSACryptoServiceProvider(2048))");
            sb.AppendLine(@"            {");
            sb.AppendLine(@"                Key = new RsaSecurityKey(provider.ExportParameters(true));");
            sb.AppendLine(@"            }");
            sb.AppendLine(@" ");
            sb.AppendLine(@"            SigningCredentials = new SigningCredentials(");
            sb.AppendLine(@"                Key, SecurityAlgorithms.RsaSha256Signature);");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"    }");

            File.WriteAllText(projectpath + "SigningConfigurations.cs", sb.ToString());
        }

        private void CreateTokenConfigurationsFile(string projectpath)
        {
            var sb = new StringBuilder();

            sb.AppendLine(@"public class TokenConfigurations");
            sb.AppendLine(@"{");
            sb.AppendLine(@"    public string Audience { get; set; }");
            sb.AppendLine(@"    public string Issuer { get; set; }");
            sb.AppendLine(@"    public int Seconds { get; set; }");
            sb.AppendLine(@"}");

            File.WriteAllText(projectpath + "TokenConfigurations.cs", sb.ToString());
        }
    }
}