using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Client.Services;
using Shared.Messages;
using System.Collections.ObjectModel;
using Client.Controls;
using System.Windows.Media;

namespace Client.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        private readonly INetworkClient _networkClient;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private string _boardState = "rnbqkbnr/pppp1ppp/5n2/5p2/5P2/5N2/PPPP1PPP/RNBQKB1R w KQkq - 1 2";

        [ObservableProperty]
        private ObservableCollection<ChessSquare> _boardSquares = new();

        public GameViewModel(INetworkClient networkClient, IMessenger messenger)
        {
            _networkClient = networkClient;
            _messenger = messenger;
            _messenger.Register<MoveResponse>(this, (r, m) => OnMoveResponse(m));
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            BoardSquares.Clear();
            string[] rows = BoardState.Split('/');
            for (int row = 0; row < 8; row++)
            {
                string rank = rows[7 - row]; // Inverter para corresponder a notação de xadrez (8 na base)
                int col = 0;
                foreach (char c in rank)
                {
                    if (char.IsDigit(c))
                    {
                        col += c - '0'; // Pular espaços vazios
                    }
                    else
                    {
                        string position = $"{(char)('a' + col)}{8 - row}";
                        BoardSquares.Add(new ChessSquare
                        {
                            Position = position,
                            Color = (row + col) % 2 == 0 ? Brushes.White : Brushes.Gray,
                            Piece = c.ToString() // Mapear peça (simplificado)
                        });
                        col++;
                    }
                }
            }
        }

        //[RelayCommand]
       // private async Task MakeMoveAsync(string from, string to)
       // {
        //    try
         //   {
           //     var moveRequest = new MoveRequest { From = from, To = to };
           //     await _networkClient.SendMessageAsync(moveRequest);
          //  }
          //  catch (Exception ex)
          //  {
                // Lidar com erro
         //   }
     //   }

        private void OnMoveResponse(MoveResponse response)
        {
            if (response.IsValid)
            {
                BoardState = response.BoardState ?? BoardState;
                InitializeBoard(); // Atualizar o tabuleiro
            }
            else
            {
                // Exibir erro
            }
        }
    }
}