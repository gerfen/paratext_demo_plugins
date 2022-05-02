using MediatR;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Hosting;
using Paratext.PluginInterfaces;
using Serilog;
using System;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using WebApiPlugin.Extensions;
using WebApiPlugin.Hubs;

namespace WebApiPlugin
{
    public partial class MainWindow : EmbeddedPluginControl
    {

        private IProject _project;
        private IVerseRef _verseRef;
        private IWindowPluginHost _host;
        //private IPluginChildWindow _parent;
        private IMediator _mediator;
        private IHubContext _hubContext;

        private WebHostStartup WebHostStartup { get; set; }

        private IDisposable WebAppProxy { get; set; }

        private delegate void AppendMsgTextDelegate(Color color, string text);


        public MainWindow()
        {
            InitializeComponent();
            ConfigureLogging();
            DisplayPluginVersion();
            Disposed += HandleWindowDisposed;
        }

        private void DisplayPluginVersion()
        {
            // get the version information
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text = string.Format($@"Plugin Version: {version}");
        }

        private static void ConfigureLogging()
        {
            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File("d:\\temp\\Plugin.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Logger = log;
        }


        private void HandleWindowDisposed(object sender, EventArgs e)
        {
            WebAppProxy?.Dispose();
            WebAppProxy = null;
        }

        private void OnExceptionOccurred(Exception exception)
        {
            // write to Serilog
            Log.Error($"OnLoad {exception.Message}");
            AppendText(Color.Red, $"OnLoad {exception.Message}");
        }

        public override void OnAddedToParent(IPluginChildWindow parent, IWindowPluginHost host, string state)
        {
            parent.SetTitle("Web API Plug-in");
            parent.ProjectChanged += ProjectChanged;
            parent.VerseRefChanged += VerseRefChanged;

            SetProject(parent.CurrentState.Project);
            SetVerseRef(parent.CurrentState.VerseRef);

            _host = host;
            _project = parent.CurrentState.Project;
            AppendText(Color.Green, $"OnAddedToParent called");
        }



        public override string GetState()
        {
            // override required by base class, return null string.
            return null;
        }

        public override void DoLoad(IProgressInfo progressInfo)
        {
            StartWebHost();
            ConfigureSignalRHubContext();
        }

        private Assembly FailedAssemblyResolutionHandler(object sender, ResolveEventArgs args)
        {
            // Get just the name of assembly without version and other metadata
            var truncatedName = new Regex(",.*").Replace(args.Name, string.Empty);

            //if (truncatedName == "ParatextData.XmlSerializers")
            //{
            //    return null;
            //}
            // Load the most up to date version
            var assembly = Assembly.Load(truncatedName);
            AppendText(Color.Red, $"Cannot load {args.Name}, loading {assembly.FullName} instead.");

            return assembly;
        }


        private void ConfigureSignalRHubContext()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<PluginHub>();
            if (_hubContext == null)
            {
                AppendText(Color.Red, "HubContext is null");
                return;
            }

            _mediator = WebHostStartup.ServiceProvider.GetService<IMediator>();

        }

        private void StartWebHost()
        {

            AppendText(Color.Green, $"StartWebApplication called");

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += FailedAssemblyResolutionHandler;

            try
            {
                var baseAddress = "http://localhost:9000/";

                WebAppProxy?.Dispose();

                // Start OWIN host 
                WebAppProxy = WebApp.Start(baseAddress,
                    (appBuilder) =>
                    {
                        WebHostStartup = new WebHostStartup(_project, _verseRef, this, _host);
                        WebHostStartup.Configuration(appBuilder);
                    });

                AppendText(Color.Green, "Owin Web Api host started");
            }
            finally
            {
                currentDomain.AssemblyResolve -= FailedAssemblyResolutionHandler;
            }
        }


        private void ProjectChanged(IPluginChildWindow sender, IProject newProject)
        {
            SetProject(newProject, reloadWebHost: true);
        }


        private void SetVerseRef(IVerseRef verseRef, bool reloadWebHost = false)
        {
            _verseRef = verseRef;
            if (reloadWebHost)
            {
                StartWebHost();
            }
        }

        private void SetProject(IProject newProject, bool reloadWebHost = false)
        {
            _project = newProject;
            if (reloadWebHost)
            {
                StartWebHost();
            }
        }

        private void VerseRefChanged(IPluginChildWindow sender, IVerseRef oldReference, IVerseRef newReference)
        {
            if (newReference != _verseRef)
            {
                _verseRef = newReference;
            }
        }

        public void AppendText(Color color, string message)
        {
            //check for threading issues
            if (this.InvokeRequired)
            {
                this.Invoke(new AppendMsgTextDelegate(AppendText), new object[] { color, message });
            }
            else
            {
                message += $"{Environment.NewLine}";
                this.rtb.AppendText(message, color);
                Log.Information(message);
            }
        }




        private void btnTest_Click(object sender, EventArgs e)
        {
            var hubProxy = GlobalHost.ConnectionManager.GetHubContext<PluginHub>();
            if (hubProxy == null)
            {
                AppendText(Color.Red, "HubContext is null");
                return;
            }

            hubProxy.Clients.All.Send(Guid.NewGuid(), @"Can you hear me?");
        }
    }
}
