using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace KuronaiAudioConverter;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public ServiceProvider ServiceProvider { get; set; }

    public App()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton<MainWindow>()
            .AddSingleton<AudioConverterViewModel>();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var mainWindow = ServiceProvider.GetService<MainWindow>();
        mainWindow.Show();
    }
}
