using Client.Services;
using Client.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client.Controls
{
    public partial class ChessBoardControl : UserControl
    {
        public ChessBoardControl()
        {
            InitializeComponent();
            DataContext = new GameViewModel(
                App.Services.GetService<INetworkClient>()!,
                WeakReferenceMessenger.Default
            );
        }

        // Propriedade de Dependência para o estado do tabuleiro
        public static readonly DependencyProperty BoardSquaresProperty =
            DependencyProperty.Register(
                nameof(BoardSquares),
                typeof(ObservableCollection<ChessSquare>),
                typeof(ChessBoardControl),
                new PropertyMetadata(null)
            );

        public ObservableCollection<ChessSquare> BoardSquares
        {
            get { return (ObservableCollection<ChessSquare>)GetValue(BoardSquaresProperty); }
            set { SetValue(BoardSquaresProperty, value); }
        }
    }

    // Classe auxiliar para representar uma casa do tabuleiro
    public class ChessSquare
    {
        public required string Position { get; set; } // Ex.: "a1", "h8"
        public required Brush Color { get; set; } // Cor da casa (ex.: preto ou branco)
        public string? Piece { get; set; } // Opcional: peça na casa (ex.: "♔", "♟")
    }
}