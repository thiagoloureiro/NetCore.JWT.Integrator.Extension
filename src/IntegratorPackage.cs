using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace JWTIntegrator
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(IntegratorPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class IntegratorPackage : Package
    {
        public DTE2 Dte;

        public static IntegratorPackage Instance;

        public const string PackageGuidString = "1aa8ae30-757b-4887-8b02-c024e0ddc6e9";

        #region Package Members

        protected override void Initialize()
        {
            Integrator.Initialize(this);
            base.Initialize();

            Dte = GetService(typeof(DTE)) as DTE2;
            Instance = this;

            Logger.Initialize(this, "JWT Integrator");
        }

        #endregion Package Members
    }
}