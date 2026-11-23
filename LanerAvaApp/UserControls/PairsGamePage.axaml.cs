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
    public partial class PairsGamePage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;

        private ObservableCollection<GameCard> _words { get; } = new();
        private int _matchedQty = 0;
        private GameCard? _firstCard;
        private GameCard? _secondCard;

        public PairsGamePage(IDbContextFactory<AppDbContext> context)
        {
            InitializeComponent();
            _context = context;

            RestartBtn.Click += async (_, _) => await ReloadGame();
            PlayAgainBtn.Click += async (_, _) => await ReloadGame();
            ReloadGame();
        }

        private async void UpdateStats()
        {
            try
            {
                _matchedQty++;
                StatsText.Text = $"Найдено: {_matchedQty} / 8";

                if (_matchedQty >= _words.Count / 2)
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
                StatsText.Text = "Найдено: 0 / 8";
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

                await using var db = await _context.CreateDbContextAsync();

                var learnedWords = await db.LearnedWords
                    .OrderBy(x => Guid.NewGuid())
                    .Take(8).ToListAsync();

                var wordsIds = learnedWords.Select(x => x.WordId).ToList();

                var wordsCards = await db.Words
                    .Where(x => wordsIds.Contains(x.IdWord))
                    .Select(x => new WordCard
                    {
                        IdWord = x.IdWord,
                        Word1 = x.Word1,
                        Translation = x.Translation,
                    })
                    .ToListAsync();

                var words = wordsCards
                    .Select(x => new GameCard
                    {
                        Id = x.IdWord,
                        Text = x.Word1,
                        IsFaceUp = false,
                        IsMatched = false,
                    }).ToList();

                var translations = wordsCards
                    .Select(x => new GameCard
                    {
                        Id = x.IdWord,
                        Text = x.Translation,
                        IsFaceUp = false,
                        IsMatched = false,
                    }).ToList();

                foreach (var item in words)
                    _words.Add(item);
                foreach (var item in translations)
                    _words.Add(item);

                CardsList.ItemsSource = _words.OrderBy(x => Guid.NewGuid());
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
    }
}