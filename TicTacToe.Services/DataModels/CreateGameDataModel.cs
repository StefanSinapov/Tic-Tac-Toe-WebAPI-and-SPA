namespace TicTacToe.Services.DataModels
{
    using System.ComponentModel.DataAnnotations;

    public class CreateGameDataModel
    {
        [Required]
        [MinLength(3), MaxLength(50)]
        public string Name { get; set; }
    }
}