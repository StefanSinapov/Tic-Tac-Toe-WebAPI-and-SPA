namespace TicTacToe.Web.DataModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class GameIdDataModel
    {
        [Required]
        public string GameId { get; set; }
    }
}