using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LanerAvaApp.Models;
using LanerAvaApp.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;

namespace LanerAvaApp.UserControls
{
    public partial class AdminPanelPage : UserControl
    {
        public AdminPanelPage()
        {
            InitializeComponent();
        }

        private async void LangsBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Не видит MainWindow!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                mainWindow.NavigateTo<AdminLanguagesPage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void AlphsBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Не видит MainWindow!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                mainWindow.NavigateTo<AdminAlphabetsPage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void WordsBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Не видит MainWindow!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                mainWindow.NavigateTo<AdminWordsPage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}