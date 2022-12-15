using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Saladim.SalLogger;
using Microsoft.Extensions.DependencyInjection;
using SaladimQBot.SimCommand;
using SaladimQBot.Core;
using SaladimWpf.Services;
using SaladimQBot.GoCqHttp;

namespace SaladimWpf;

public partial class App : Application
{
    public TextBox? TextBoxLogging;
    public CheckBox? ScrollToEndCheckBox;
    public Logger Logger;
    public IServiceCollection Services { get; protected set; }
    public IServiceProvider Service { get; protected set; }
    public static new App Current => (Application.Current as App)!;

    protected StreamWriter streamWriter;

    public App()
    {
        streamWriter = new(GetLogFileName());
        BotServiceConfig botServiceConfig = new("ws://127.0.0.1:5000");
        SimCommandServiceConfig simCommandServiceConfig = new("/", typeof(App).Assembly);
        Services = new ServiceCollection();
        {
            Services.AddSingleton<IClient>(s => s.GetRequiredService<BotService>().Client);
            Services.AddSingleton<Logger>(_ => MakeLogger());
            Services.AddSingleton<SimCommandService>();
            Services.AddSingleton<BotService>();
            Services.AddSingleton(botServiceConfig);
            Services.AddSingleton(simCommandServiceConfig);
            Services.AddSingleton<HomoService>();
            Services.AddSingleton<HttpService>();
            Services.AddSingleton(Services);
        }
        Service = Services.BuildServiceProvider();
        Logger = Service.GetRequiredService<Logger>();
    }

    public void FlushLogFile()
    {
        streamWriter.Flush();
    }

    private static string GetLogFileName()
    {
        DateTime now = DateTime.Now;
        string path = @"Logs\";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        string unindexedFileName = $"{now.Year}.{now.Month}.{now.Day}";
        string filePath = string.Empty;
        int index = 0;
        do
        {
            string indexedFileName = $"{unindexedFileName} {index}.log";
            string combinedPath = Path.Combine(path, indexedFileName);
            if (!File.Exists(combinedPath))
            {
                filePath = combinedPath;
                break;
            }
            else
            {
                index++;
                continue;
            }
        }
        while (true);
        return filePath;
    }

    private Logger MakeLogger()
    {
        return new LoggerBuilder().WithAction(s =>
        {
            Current.Dispatcher.Invoke(() =>
            {
                TextBoxLogging?.AppendText(s + Environment.NewLine);
                if (ScrollToEndCheckBox?.IsChecked is true)
                {
                    TextBoxLogging?.ScrollToEnd();
                }
            });
            streamWriter.WriteLine(s);
        })
        .WithLevelLimit(LogLevel.Trace)
        .Build();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        DispatcherUnhandledException += this.App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.ProcessExit += this.CurrentDomain_ProcessExit;
        AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            if (e.ExceptionObject is Exception ex)
                Logger.LogFatal("WpfConsole", "CurrentDomainUnhandled", ex);
            streamWriter.Dispose();
        }
        catch { };
    }

    private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
    {
        try
        {
            streamWriter.Dispose();
        }
        catch { };
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            Logger.LogFatal("WpfConsole", "Domain", e.Exception);
            streamWriter.Dispose();
            e.Handled = true;
            this.Shutdown();
        }
        catch { }
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        try
        {
            streamWriter.Dispose();
        }
        catch { };
    }
}
