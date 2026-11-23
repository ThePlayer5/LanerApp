using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using HarfBuzzSharp;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using LanerAvaApp.Services;
using LanerAvaApp.Views;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LanerAvaApp.UserControls
{
    public partial class HomePage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;
        private readonly IUserSessionProvider _sessionService;

        private ObservableCollection<LanguageCard> _languages { get; } = new();
        private ObservableCollection<LangOption> _selectLangs { get; } = new();

        public HomePage(IDbContextFactory<AppDbContext> context, IUserSessionProvider sessionService)
        {
            InitializeComponent();
            _context = context;
            _sessionService = sessionService;

            CancelButton.Click += async (_, _) => await CancelDialog();

            LoadUserDataUI();
            LoadLanguages();
            UpdateArrowState();
        }

        private async void LoadUserDataUI()
        {
            try
            {
                GeneralVariables.SelectedLang = null!;
                GeneralVariables.SelectedLevel = null!;

                var user = _sessionService.Current;

                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                await using var db = await _context.CreateDbContextAsync();
                var findUser = await db.Users.FirstOrDefaultAsync(u => u.IdUser == user.UserId);

                if (findUser is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(findUser));

                WelcomeText.Text = $"Добро пожаловать {findUser.Fullname}!";
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void LoadLanguages()
        {
            try
            {
                _languages.Clear();

                var currentUser = _sessionService.Current;

                if (currentUser is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(currentUser));

                await using var db = await _context.CreateDbContextAsync();
                var source = await db.UsersLangs.Include(x => x.User)
                    .Include(x => x.Lang).Where(x => x.UserId == currentUser.UserId)
                    .ToListAsync();

                if (source is null)
                    throw new ArgumentNullException("Не удалось получить курсы!", nameof(source));

                foreach (var userLang in source)
                {
                    decimal percent = 0m;

                    if (userLang.User is null || userLang.Lang is null)
                        throw new ArgumentNullException("Не удалось получить вложенные данные!", nameof(source));

                    var progress = await db.LearningProgresses
                        .FirstOrDefaultAsync(lp => lp.UserId == currentUser.UserId
                        && string.Equals(lp.Language, userLang.Lang.Name));

                    if (progress is null)
                        percent = 0m;
                    else
                        percent = progress!.Percent ?? 0m;

                    Bitmap? imagePath = null;

                    switch (userLang.LangId)
                    {
                        case 1:
                            imagePath = new Bitmap(AssetLoader.Open(new Uri("avares://LanerAvaApp/Assets/fr-lang-img.jpg")));
                            break;
                        case 2:
                            imagePath = new Bitmap(AssetLoader.Open(new Uri("avares://LanerAvaApp/Assets/gm-lang-img.jpg")));
                            break;
                        case 3:
                            imagePath = new Bitmap(AssetLoader.Open(new Uri("avares://LanerAvaApp/Assets/kr-lang-img.jpg")));
                            break;
                        case 4:
                            imagePath = new Bitmap(AssetLoader.Open(new Uri("avares://LanerAvaApp/Assets/en-lang-img.jpg")));
                            break;
                        case 5:
                            imagePath = new Bitmap(AssetLoader.Open(new Uri("avares://LanerAvaApp/Assets/jp-lang-img.jpg")));
                            break;
                    }

                    _languages.Add(new LanguageCard
                    {
                        LangId = userLang.LangId,
                        LanguageName = userLang.Lang.Name,
                        ImagePath = imagePath,
                        Progress = percent
                    });
                }

                if (_languages.Count <= 0)
                    EmptyHint.IsVisible = true;
                else EmptyHint.IsVisible = false;

                LanguagesCarousel.ItemsSource = _languages;
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
                PrevButton.IsEnabled = LanguagesCarousel.SelectedIndex > 0;
                NextButton.IsEnabled = LanguagesCarousel.SelectedIndex < _languages.Count - 1;
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
                if (LanguagesCarousel.SelectedIndex > 0)
                    LanguagesCarousel.SelectedIndex--;
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
                if (LanguagesCarousel.SelectedIndex < _languages.Count - 1)
                    LanguagesCarousel.SelectedIndex++;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private void LanguagesCarousel_SelectionChanged(object? sender, SelectionChangedEventArgs e)
            => UpdateArrowState();

        private async void CardBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (LanguagesCarousel.SelectedItem is not LanguageCard langCard) return;
                GeneralVariables.SelectedLang = langCard;

                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Не видит MainWindow!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                mainWindow.NavigateTo<LevelsPage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void AddCourseBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                AddCourseDialog.IsVisible = true;
                await ShowAvailableLangs();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async Task ShowAvailableLangs()
        {
            try
            {
                _selectLangs.Clear();

                var user = _sessionService.Current;

                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                await using var db = await _context.CreateDbContextAsync();
                var userLangs = await db.UsersLangs
                    .Where(x => x.UserId == user.UserId)
                    .ToListAsync();

                var langsId = userLangs.Select(x => x.LangId).ToList();

                var langs = await db.Languages
                    .Where(x => !langsId.Contains(x.IdLang))
                    .Select(x => new LangOption
                    {
                        IdLang = x.IdLang,
                        Name = x.Name
                    })
                    .ToListAsync();

                foreach (var item in langs)
                    _selectLangs.Add(item);

                LangsList.ItemsSource = _selectLangs;

                EmptyLangsList.IsVisible = _selectLangs.Count == 0;
                AddButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void Border_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            try
            {
                await CancelDialog();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void CheckBox_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (LangsList.ItemsSource is not IEnumerable<LangOption> items)
                    return;

                AddButton.IsEnabled = items.Any(x => x.IsSelected);
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async Task CancelDialog()
        {
            try
            {
                AddCourseDialog.IsVisible = false;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (LangsList.ItemsSource is not IEnumerable<LangOption> items)
                    return;

                var user = _sessionService.Current;

                if (user is null)
                    throw new ArgumentNullException("Не удалось найти пользователя!", nameof(user));

                await using var db = await _context.CreateDbContextAsync();

                foreach (var item in items)
                {
                    if (item.IsSelected)
                    {
                        var userLang = new UsersLang
                        {
                            LangId = item.IdLang,
                            UserId = user.UserId,
                        };

                        await db.UsersLangs.AddAsync(userLang);
                        await db.SaveChangesAsync();
                    }
                }

                LoadLanguages();
                await Task.Delay(500);
                UpdateArrowState();
                await CancelDialog();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}