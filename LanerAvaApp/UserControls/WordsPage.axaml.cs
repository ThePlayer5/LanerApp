using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using HarfBuzzSharp;
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
using System.Threading.Tasks;

namespace LanerAvaApp.UserControls
{
    public partial class WordsPage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;
        private readonly IUserSessionProvider _sessionService;

        private ObservableCollection<WordCard> _words { get; } = new();

        private int _repeatedQty = 0;
        private int _learnedQty = 0;

        private record LearnedRow(int IdWord, string Word1, string Translation,
            string Transcription, int LangId, int LevelId, DateTime Timing);

        private static WordCard ToCard(LearnedRow word, string section, bool isLearned) => new()
        {
            IdWord = word.IdWord,
            Word1 = word.Word1,
            Translation = word.Translation,
            Transcription = word.Transcription,
            LangId = word.LangId,
            LevelId = word.LevelId,
            IsLearned = isLearned,
            Section = section
        };

        public WordsPage(IDbContextFactory<AppDbContext> context, IUserSessionProvider sessionService)
        {
            InitializeComponent();
            _context = context;
            _sessionService = sessionService;

            LoadLangDataUI();
            LoadWords();
            UpdateArrowState();
        }

        private async void LoadLangDataUI()
        {
            try
            {
                var lang = GeneralVariables.SelectedLang;
                var level = GeneralVariables.SelectedLevel;

                if (lang is null)
                    throw new ArgumentNullException("Не удалось найти язык!", nameof(lang));
                if (level is null)
                    throw new ArgumentNullException("Не удалось найти уровень сложности!", nameof(level));

                WelcomeText.Text = $"{lang.LanguageName}  -  Уровень: {level.LevelName}";
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void LoadWords()
        {
            try
            {
                if (_sessionService.Current is null)
                    throw new ArgumentNullException("Не удалось получить пользователя!", nameof(_sessionService.Current));
                if (GeneralVariables.SelectedLang is null)
                    throw new ArgumentNullException("Не удалось получить язык!", nameof(GeneralVariables.SelectedLang));
                if (GeneralVariables.SelectedLevel is null)
                    throw new ArgumentNullException("Не удалось получить уровень сложности!", nameof(GeneralVariables.SelectedLevel));

                _words.Clear();

                int userId = _sessionService.Current.UserId;
                int langId = GeneralVariables.SelectedLang.LangId;
                int levelId = GeneralVariables.SelectedLevel.LevelId;

                var today = DateTime.Today;
                var weekAgo = today.AddDays(-7);
                var monthAgo = today.AddMonths(-1);

                await using var db = await _context.CreateDbContextAsync();

                var learned = await db.Words
                    .Where(w => w.LangId == langId && w.LevelId == levelId)
                    .Where(w => w.LearnedWords.Any(lw => lw.UserId == userId && lw.Timing >= monthAgo))
                    .Select(w => new LearnedRow(
                        w.IdWord, w.Word1, w.Translation, w.Transcription, w.LangId, w.LevelId,
                        w.LearnedWords.Where(lw => lw.UserId == userId).Max(lw => lw.Timing)))
                    .ToListAsync();

                var todayList = learned
                    .Where(l => l.Timing >= today)
                    .Select(l => ToCard(l, "Сегодня", true))
                    .ToList();

                var weekList = learned
                    .Where(l => l.Timing >= weekAgo && l.Timing < today)
                    .Select(l => ToCard(l, "За 7 дней", true))
                    .ToList();

                var monthList = learned
                    .Where(l => l.Timing >= monthAgo && l.Timing < weekAgo)
                    .Select(l => ToCard(l, "За месяц", true))
                    .ToList();

                var newWords = await db.Words
                    .Where(w => w.LangId == langId && w.LevelId == levelId)
                    .Where(w => !w.LearnedWords.Any(lw => lw.UserId == userId))
                    .Select(w => new WordCard
                    {
                        IdWord = w.IdWord,
                        Word1 = w.Word1,
                        Translation = w.Translation,
                        Transcription = w.Transcription,
                        LangId = w.LangId,
                        LevelId = w.LevelId,
                        IsLearned = false,
                        Section = "Новое"
                    })
                    .ToListAsync();

                foreach (var w in monthList)
                    _words.Add(w);
                foreach (var w in weekList)
                    _words.Add(w);
                foreach (var w in todayList)
                    _words.Add(w);
                foreach (var w in newWords)
                    _words.Add(w);

                WordsCarousel.ItemsSource = _words;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void UpdateUIState(WordCard wordCard)
        {
            try
            {
                if (EndLessonBtn.IsVisible) return;

                if (wordCard is null) return;

                if (wordCard.IsLearned)
                    _repeatedQty++;
                else _learnedQty++;

                if (WordsCarousel.SelectedIndex >= _words.Count - 1 && _words.Count != 0)
                {
                    EndLessonBtn.IsVisible = true;
                    if (_repeatedQty > 0) _repeatedQty++;
                    if (_learnedQty > 0) _learnedQty++;
                }

                ReapetedQty.Text = $"Кол-во повторённых слов: {_repeatedQty}";
                LearnedQty.Text = $"Кол-во изученных слов: {_learnedQty}";
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
                PrevButton.IsEnabled = WordsCarousel.SelectedIndex > 0;
                NextButton.IsEnabled = WordsCarousel.SelectedIndex < _words.Count - 1;
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
                if (WordsCarousel.SelectedIndex > 0)
                    WordsCarousel.SelectedIndex--;
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
                if (WordsCarousel.SelectedIndex < _words.Count - 1)
                    WordsCarousel.SelectedIndex++;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void WordsCarousel_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                UpdateArrowState();

                var user = _sessionService.Current;

                if (user is null)
                    throw new ArgumentNullException("Не удалось получить пользователя!", nameof(_sessionService.Current));

                if (WordsCarousel.SelectedItem is not WordCard wordCard) return;

                await using var db = await _context.CreateDbContextAsync();
                var findWord = await db.LearnedWords.FirstOrDefaultAsync(lw => lw.WordId == wordCard.IdWord && lw.UserId == user.UserId);

                UpdateUIState(wordCard);

                if (findWord is not null) return;

                var learnedWord = new LearnedWord
                {
                    Timing = DateTime.Now,
                    WordId = wordCard.IdWord,
                    UserId = user.UserId
                };

                await db.LearnedWords.AddAsync(learnedWord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void ExitToMenuBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (TopLevel.GetTopLevel(this) is not MainWindow mainWindow)
                {
                    var error = MessageBoxManager.GetMessageBoxStandard("Error", $"Не видит MainWindow!", ButtonEnum.Ok);
                    await error.ShowAsync();
                    return;
                }
                mainWindow.NavigateTo<HomePage>();
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void EndLessonBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {                
                CongratsPanel.IsVisible = true;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}