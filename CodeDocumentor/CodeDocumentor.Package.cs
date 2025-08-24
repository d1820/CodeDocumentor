using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using CodeDocumentor.Analyzers;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

[assembly: InternalsVisibleTo("CodeDocumentor.Test")]

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    /// <summary>
    ///  This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   The minimum requirement for a class to be considered a valid package for Visual Studio is to implement the
    ///   IVsPackage interface and register itself with the shell. This package uses the helper classes defined inside
    ///   the Managed Package Framework (MPF) to do it: it derives from the Package class that provides the
    ///   implementation of the IVsPackage interface and uses the registration attributes defined in the framework to
    ///   register itself and its components with the shell. These attributes tell the pkgdef creation utility what data
    ///   to put into .pkgdef file.
    ///  </para>
    ///  <para>
    ///   To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage"
    ///   ...&gt; in .vsixmanifest file.
    ///  </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(VsixOptions.PackageGuidString)]
    [InstalledProductRegistration("#110", "#112", VsixOptions.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionPageGrid), OptionPageGrid.Category, OptionPageGrid.SubCategory, 1000, 1001, true)]
    //[ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class CodeDocumentorPackage : AsyncPackage
    {

#pragma warning disable IDE1006 // Naming Styles
        internal static IOptionPageGrid _options;
        internal static OptionsService _optService;
#pragma warning restore IDE1006 // Naming Styles

        #region Package Members

        /// <summary>
        ///  Initialization of the package; this method is called right after the package is sited, so this is the place
        ///  where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">
        ///  A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.
        /// </param>
        /// <param name="progress"> A provider for progress updates. </param>
        /// <returns>
        ///  A task representing the async work of package initialization, or an already completed task if there is
        ///  none. Do not return null from this method.
        /// </returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point. Do any
            // initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _options = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));

            _optService = new OptionsService();
            _optService.SetDefaults(_options);
            BaseCodeFixProvider.SetOptionsService(_optService);
            BaseDiagnosticAnalyzer.SetOptionsService(_optService);
            BaseAnalyzerSettings.SetOptionsService(_optService);
        }

        #endregion
    }
}
