using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LanerAvaApp.Data;
using LanerAvaApp.Models;
using LanerAvaApp.Services;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LanerAvaApp.UserControls
{
    public partial class CompareWordsPage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;
        private readonly IUserSessionProvider _sessionService;

        private ObservableCollection<GameCard> _words { get; } = new();
        private ObservableCollection<GameCard> _translations { get; } = new();
        private int _matchedQty = 0;
        private GameCard? _firstCard;
        private GameCard? _secondCard;

        public CompareWordsPage(IDbContextFactory<AppDbContext> context, IUserSessionProvider sessionService)
        {
            InitializeComponent();
            _context = context;
            _sessionService = sessionService;

            RestartBtn.Click += async (_, _) => await ReloadGame();
            PlayAgainBtn.Click += async (_, _) => await ReloadGame();
            ReloadGame();
        }

        private async void UpdateStats()
        {
            try
            {
                _matchedQty++;
                StatsText.Text = $"Найдено: {_matchedQty} / 6";

                var wordsQty = _words is not null ? _words.Count : 0;
                var translationsQty = _translations is not null ? _translations.Count : 0;

                if (_matchedQty >= (wordsQty + translationsQty) / 2)
                {
                    VictoryPanel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async Task ReloadGame()
        {
            try
            {
                VictoryPanel.IsVisible = false;
                _matchedQty = 0;
                _firstCard = null!;
                _secondCard = null!;
                StatsText.Text = "Найдено: 0 / 6";
                LoadWords();
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
                _words.Clear();
                _translations.Clear();

                await using var db = await _context.CreateDbContextAsync();

                var learnedWords = await db.LearnedWords
                    .Include(x => x.Word)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(6).ToListAsync();

                var words = learnedWords
                    .Select(x => new GameCard
                    {
                        Id = x.WordId,
                        Text = x.Word.Word1,
                        IsMatched = false,
                        IsFaceUp = false,
                    })
                    .ToList();

                var translations = learnedWords
                    .Select(x => new GameCard
                    {
                        Id = x.WordId,
                        Text = x.Word.Translation,
                        IsMatched = false,
                        IsFaceUp = false,
                    })
                    .ToList();

                foreach (var item in words)
                    _words.Add(item);
                foreach (var item in translations)
                    _translations.Add(item);

                WordsList.ItemsSource = _words.OrderBy(x => Guid.NewGuid());
                TranslationsList.ItemsSource = _translations.OrderBy(x => Guid.NewGuid());
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void ComparisonCards(GameCard firstCard, GameCard secondCard)
        {
            try
            {
                if (firstCard.Id == secondCard.Id)
                {
                    firstCard.IsMatched = true;
                    secondCard.IsMatched = true;
                    UpdateStats();
                    return;
                }
                await Task.Delay(800);
                firstCard.IsFaceUp = false;
                secondCard.IsFaceUp = false;
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void Border_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            try
            {
                if (sender is not Border { DataContext: GameCard card }) return;
                if (_firstCard is null)
                {
                    _firstCard = card;
                    card.IsFaceUp = true;
                }
                else
                {
                    _secondCard = card;
                    card.IsFaceUp = true;
                }

                if (_firstCard is not null && _secondCard is not null)
                {
                    ComparisonCards(_firstCard, _secondCard);
                    _firstCard = null!;
                    _secondCard = null!;
                }
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }
    }
}