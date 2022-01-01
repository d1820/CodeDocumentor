using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using CodeDocumentor.Settings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CodeDocumentor.Test")]
// For definitions of XML nodes see: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments
// see also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
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
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(VsixOptions.PackageGuidString)]
    [InstalledProductRegistration("#110", "#112", VsixOptions.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionPageGrid), "CodeDocumentor", "General", 0, 0, true)]
    public sealed class CodeDocumentorPackage : AsyncPackage
    {
        private static OptionPageGrid _options;
        private static readonly object _syncRoot = new object();

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
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _options = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));

        }

        public static OptionPageGrid Options
        {
            get
            {
                if (_options == null)
                {
                    lock (_syncRoot)
                    {
                        if (_options == null)
                        {
                            LoadPackage().GetAwaiter().GetResult();
                        }
                    }

                }

                return _options;
            }
            internal set
            {
                //This is used for testing
                _options = value;
            }
        }

        private static async Task LoadPackage()
        {
            var shell = (IVsShell)GetGlobalService(typeof(SVsShell));
            var guid = new Guid(VsixOptions.PackageGuidString);
            if (shell != null)
            {
                if (shell.IsPackageLoaded(ref guid, out IVsPackage package) != VSConstants.S_OK)
                {
                    await Task.Run(() => shell.LoadPackage(ref guid, out package)).ContinueWith(result =>
                    {
                        ErrorHandler.Succeeded(result.Result);
                    }, TaskScheduler.Default);

                }
            }
            else
            {
                //This is used for unit testing to work, this would not get triggered when running as real vsix
                ErrorHandler.Succeeded(1);
            }

        }

        #endregion

    }

    public class WordMap
    {
        public string Word { get; set; }
        public string Translation { get; set; }
    }
    //This has to live in this project so context thread is valid
    public class OptionPageGrid : DialogPage
    {
        [Category("CodeDocumentor")]
        [DisplayName("Enable comments for public members only")]
        [Description("When documenting classes, fields, methods, and properties only add documentation headers if the item is public")]
        public bool IsEnabledForPublishMembersOnly { get; set; }

        [Category("CodeDocumentor")]
        [DisplayName("Use natural language for return comments")]
        [Description("When documenting members if the return type contains a generic then translate that item into natural language. The default uses CDATA nodes to show the exact return type. Example: <retrun>A List of Strings</return>")]
        public bool UseNaturalLanguageForReturnNode { get; set; }

        [Category("CodeDocumentor")]
        [DisplayName("Exclude async wording from comments")]
        [Description("When documenting members skip adding asynchronously to the comment.")]
        public bool ExcludeAsyncSuffix { get; set; }

        [Category("CodeDocumentor")]
        [DisplayName("Include <value> node in property comments")]
        [Description("When documenting properties add the value node with the return type")]
        public bool IncludeValueNodeInProperties { get; set; }

        [Category("CodeDocumentor")]
        [DisplayName("Use TODO comment when summary can not be determined")]
        [Description("When documenting methods that can not create a valid summary insert TODO instead. Async is ignored")]
        public bool UseToDoCommentsOnSummaryError { get; set; }

        [Category("CodeDocumentor")]
        [DisplayName("Word mappings for creating comments")]
        [Description("When documenting if certain word are matched it will swap out to the translated mapping.")]
        public List<WordMap> WordMaps { get; set; } = new List<WordMap> {
            new WordMap { Word = "int", Translation = "integer" },
            new WordMap { Word = "OfList", Translation = "OfLists" },
            new WordMap { Word = "OfCollection", Translation = "OfCollections" },
            new WordMap { Word = "OfEnumerable", Translation = "OfEnumerables" },
        };

    }

}
