using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
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
    public partial class SymbolsPage : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _context;

        private ObservableCollection<Symbol> _symbols = new();

        public SymbolsPage(IDbContextFactory<AppDbContext> context)
        {
            InitializeComponent();
            _context = context;

            LoadLangDataUI();
            LoadSymbols();
        }

        private async void LoadLangDataUI()
        {
            try
            {
                var alph = GeneralVariables.SelectedAlph;

                if (alph is null)
                    throw new ArgumentNullException("Не удалось найти алфавит!", nameof(alph));

                WelcomeText.Text = $"{alph.Name}";
            }
            catch (Exception ex)
            {
                var error = MessageBoxManager.GetMessageBoxStandard("Error", $"{ex.Message}", ButtonEnum.Ok);
                await error.ShowAsync();
            }
        }

        private async void LoadSymbols()
        {
            try
            {
                _symbols.Clear();
                EmptyHint.IsVisible = false;

                var alphabet = GeneralVariables.SelectedAlph;

                if (alphabet is null)
                    throw new ArgumentNullException("Не удалось получить алфавит!", nameof(alphabet));

                await using var db = await _context.CreateDbContextAsync();

                var symbols = await db.Symbols
                    .Where(s => s.AlphabetId == alphabet.IdAlphabet)
                    .OrderBy(s => s.IdSymbol)
                    .ToListAsync();

                foreach (var s in symbols)
                    _symbols.Add(s);

                SymbolsList.ItemsSource = _symbols;

                if (symbols.Count == 0)
                {
                    EmptyHint.Text = "В этом алфавите пока нет символов";
                    EmptyHint.IsVisible = true;
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