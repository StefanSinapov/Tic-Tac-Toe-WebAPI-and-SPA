namespace TicTacToe.Services.DataModels
{
    using System.ComponentModel.DataAnnotations;

    public class GameIdDataModel
    {
        [Required]
        public string GameId { get; set; }
    }
}