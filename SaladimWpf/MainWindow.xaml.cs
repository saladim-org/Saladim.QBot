using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;
using SaladimQBot.Shared;

namespace SaladimWpf;

public partial class MainWindow : Window
{
    protected Logger logger;
    protected BotService botService;

    public MainWindow()
    {
        InitializeComponent();
        App app = App.Current;
        app.TextBoxLogging = TextBoxLogging;
        app.ScrollToEndCheckBox = ScrollToEndCheckBox;
        logger = app.Logger;

        botService = App.Current.Service.GetRequiredService<BotService>();
        botService.OnLog += s =>
        {
            logger.LogRaw(LogLevel.Info, s);
        };
        botService.OnClientLog += s =>
        {
            logger.LogInfo("Client", s);
        };
    }

    private void UpdateGuessNumBotDelay_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (int.TryParse(GuessNumBotDelayTextBox.Text, out int v))
        {
            botService.GuessNumberBotDelay = v;
            logger.LogInfo("WpfConsole", $"更新完成! 延迟为{v}ms");
        }
        else
        {
            logger.LogInfo("WpfConsole", $"更新失败! 解析值时出错");
        }
    }

    private void ClearOutPutButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        TextBoxLogging.Text = "";
        logger.LogInfo("WpfConsole", "Wpf控制台清空完成.");
    }

    private void GuessNumBotCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        botService.OpenGuessNumberBot = true;
        logger.LogInfo("WpfConsole", "开启自动猜数.");
    }

    private void GuessNumBotCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        botService.OpenGuessNumberBot = false;
        logger.LogInfo("WpfConsole", "关闭自动猜数.");
    }

    private async void ClientStateCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        logger.LogInfo("WpfConsole", "开启Client中...");
        try
        {
            await botService.StartAsync().ConfigureAwait(false);
        }
        catch (ClientException ex)
        {
            logger.LogWarn("WpfConsole", "Exception", ex);
            return;
        }
        logger.LogInfo("WpfConsole", "开启完成.");
    }

    private async void ClientStateCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        logger.LogInfo("WpfConsole", "关闭Client中...");
        try
        {
            await botService.StopAsync().ConfigureAwait(false);
        }
        catch (ClientException ex)
        {
            logger.LogWarn("WpfConsole", "Exception", ex);
            return;
        }
        logger.LogInfo("WpfConsole", "关闭完成.");
    }

    private void FlushLogButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        App.Current.FlushLogFile();
        logger.LogInfo("WpfConsole", "Flush完成.");
    }
}
