using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using LanerAvaApp.Services;
using LanerAvaApp.Views;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LanerAvaApp.UserControls
{
    public partial class AlphabetsPage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;
        private readonly IUserSessionProvider _sessionService;

        private ObservableCollection<AlphabetCard> _alphabets { get; } = new();

        public AlphabetsPage(IDbContextFactory<AppDbContext> context, IUserSessionProvider sessionService)
        {
            InitializeComponent();
            _context = context;
            _sessionService = sessionService;

            LoadLanguages();
            UpdateArrowState();
        }

        private async void LoadLanguages()
        {
            try
            {
                _alphabets.Clear();

                GeneralVariables.SelectedAlph = null!;

                var currentUser = _sessionService.Current;

                if (currentUser is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(currentUser));

                await using var db = await _context.CreateDbContextAsync();
                var userLangs = await db.UsersLangs
                    .Where(x => x.UserId == currentUser.UserId)
                    .ToListAsync();

                var langIds = userLangs.Select(x => x.LangId).ToList();

                var source = await db.Alphabets
                    .Include(a => a.Lang)
                    .Where(x => langIds.Contains(x.LangId))
                    .ToListAsync();

                if (source is null)
                    throw new ArgumentNullException("Не удалось получить курсы!", nameof(source));

                foreach (var alphabet in source)
                {
                    IBrush cardBg = Brushes.White;

                    switch (alphabet.LangId)
                    {
                        case 1:
                            cardBg = Brush.Parse("#E1F8EB");
                            break;
                        case 2:
                            cardBg = Brush.Parse("#FDF9CB");
                            break;
                        case 3:
                            cardBg = Brush.Parse("#F9D7D7");
                            break;
                        case 4:
                            cardBg = Brush.Parse("#D7E7F9");
                            break;
                        case 5:
                            cardBg = Brush.Parse("#E8D7F9");
                            break;
                    }

                    _alphabets.Add(new AlphabetCard
                    {
                        IdAlphabet = alphabet.IdAlphabet,
                        Name = alphabet.Name,
                        LangId = alphabet.LangId,
                        LangName = alphabet.Lang.Name,
                        CardBackground = cardBg
                    });
                }

                AlphabetsCarousel.ItemsSource = _alphabets;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void UpdateArrowState()
        {
            try
            {
                PrevButton.IsEnabled = AlphabetsCarousel.SelectedIndex > 0;
                NextButton.IsEnabled = AlphabetsCarousel.SelectedIndex < _alphabets.Count - 1;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void PrevButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (AlphabetsCarousel.SelectedIndex > 0)
                    AlphabetsCarousel.SelectedIndex--;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void NextButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (AlphabetsCarousel.SelectedIndex < _alphabets.Count - 1)
                    AlphabetsCarousel.SelectedIndex++;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private void AlphabetsCarousel_SelectionChanged(object? sender, SelectionChangedEventArgs e)
            => UpdateArrowState();

        private async void CardBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (AlphabetsCarousel.SelectedItem is not AlphabetCard alphabetCard) return;
                GeneralVariables.SelectedAlph = alphabetCard;

                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Не видит MainWindow!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                mainWindow.NavigateTo<SymbolsPage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

    }
}