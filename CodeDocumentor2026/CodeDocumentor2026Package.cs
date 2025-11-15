using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using CodeDocumentor;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Models;
using CodeDocumentor.Common.Services;
using CodeDocumentor.Vsix2022;
using CodeDocumentor2026.Commands.Context;
using CodeDocumentor2026.Commands.Menu;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CodeDocumentor2026
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(VsixOptions.PackageGuidString)]
    [ProvideService(typeof(ICommentBuilderService), IsAsyncQueryable = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionPageGrid), OptionPageGrid.Category, OptionPageGrid.SubCategory, 1000, 1001, true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class CodeDocumentor2026Package : AsyncPackage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeDocumentor2026Package"/> class.
        /// </summary>
        public CodeDocumentor2026Package()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            LogDebug("Package InitializeAsync - START");

            try
            {
                LogDebug("Package Switching to main thread...");
                // When initialized asynchronously, the current thread may be a background thread at this point.
                // Do any initialization that requires the UI thread after switching to the UI thread.
                await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                LogDebug("Package Successfully switched to main thread");

                // Ensure cancellation wasn't requested before continuing
                cancellationToken.ThrowIfCancellationRequested();
                LogDebug("Package Cancellation token check passed");

                var options = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                LogDebug("Package Got options page");

                var settings = new Settings();
                settings.SetFromOptionsGrid(options);
                LogDebug("Package Created settings from options");

                // TEMPORARILY COMMENTED OUT FOR TESTING - Register services first with proper error handling
                LogDebug("Package Starting service registration...");
                await RegisterServicesAsync(cancellationToken, settings);
                LogDebug("Package Service registration completed");

                LogDebug("Package Starting command initialization...");
                await InitializeCommandsAsync(cancellationToken);
                LogDebug("Package Command initialization completed");

                LogDebug("Package InitializeAsync - SUCCESS");
            }
            catch (OperationCanceledException ex)
            {
                LogDebug($"Package InitializeAsync - CANCELED: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                LogDebug($"Package InitializeAsync - ERROR: {ex}");
                throw;
            }

        }
        private async Task RegisterServicesAsync(CancellationToken cancellationToken, ISettings settings)
        {
            try
            {
                LogDebug("Package RegisterServicesAsync - START");

                var settingServiceCallback = new AsyncServiceCreatorCallback(async (IAsyncServiceContainer container, CancellationToken ct, Type serviceType) =>
                {
                    LogDebug($"Package Service callback for {serviceType?.Name ?? "null"}");

                    if (typeof(ICommentBuilderService) == serviceType)
                    {
                        LogDebug("Package Creating ICommentBuilderService...");

                        var logger = new Logger();
                        LogDebug("Package Created logger");

                        var svc = new CommentBuilderService(logger, settings);
                        LogDebug("Package Created CommentBuilderService successfully");

                        return svc;
                    }
                    return null;
                });

                LogDebug("Package Adding service to container...");
                AddService(typeof(ICommentBuilderService), settingServiceCallback, true);
                LogDebug("Package RegisterServicesAsync - SUCCESS");
            }
            catch (Exception ex)
            {
                LogDebug($"Package RegisterServicesAsync - ERROR: {ex}");
                throw;
            }
        }

        private async Task InitializeCommandsAsync(CancellationToken cancellationToken)
        {
            try
            {
                LogDebug("Package InitializeCommandsAsync - START");

                // Initialize commands with proper error handling
                LogDebug("Package Initializing CodeDocumentorFileMenu...");
                await CodeDocumentorFileMenu.InitializeAsync(this);
                LogDebug("Package CodeDocumentorFileMenu initialized");

                LogDebug("Package Initializing CodeDocumentorFileCommand...");
                await CodeDocumentorFileCommand.InitializeAsync(this);
                LogDebug("Package CodeDocumentorFileCommand initialized");

                LogDebug("Package InitializeCommandsAsync - SUCCESS");
            }
            catch (Exception ex)
            {
                LogDebug($"Package InitializeCommandsAsync - ERROR: {ex}");
                throw;
            }
        }

        private static void LogDebug(string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {message}");
            }
            catch
            {
                // Don't let logging failures crash the extension
            }
        }

        #endregion
    }
}
