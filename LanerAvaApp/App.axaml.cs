using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LanerAvaApp.Data;
using LanerAvaApp.Services;
using LanerAvaApp.UserControls;
using LanerAvaApp.ViewModels;
using LanerAvaApp.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace LanerAvaApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseNpgsql("Host=localhost;Port=5432;Database=LanerDB;Username=postgres;Password=postgres"), ServiceLifetime.Scoped);

        services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7171/") });

        // как в апишке регистрирую сервисы в синголтон
        services.AddSingleton<IUserSessionProvider, UserSessionProvider>();
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddScoped<ChatService>();

        services.AddTransient<AuthPage>();
        services.AddTransient<RegPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<LevelsPage>();
        services.AddTransient<WordsPage>();
        services.AddTransient<ProfilePage>();
        services.AddTransient<TechSupportPage>();
        services.AddTransient<ChatsPage>();
        services.AddTransient<AlphabetsPage>();
        services.AddTransient<SymbolsPage>();
        services.AddTransient<AdminWordsPage>();
        services.AddTransient<AdminAlphabetsPage>();
        services.AddTransient<AdminLanguagesPage>();
        services.AddTransient<AdminPanelPage>();
        services.AddTransient<PairsGamePage>();
        services.AddTransient<CompareWordsPage>();
        services.AddTransient<RatingPage>();
        services.AddTransient<MainWindow>();

        var serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = serviceProvider.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}