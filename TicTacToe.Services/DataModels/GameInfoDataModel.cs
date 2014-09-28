namespace TicTacToe.Services.DataModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using AutoMapper;
    using Mapping;
    using TicTacToe.Models;

    public class GameInfoDataModel : IMappableFrom<Game>, IHaveCustomMappings
    {
        public Guid Id { get; set; }

        [MinLength(3)]
        [MaxLength(50)]
        public string Name { get; set; }

        public DateTime DateCreated { get; set; }

        public string FirstPlayerName { get; set; }

        public string SecondPlayerName { get; set; }

        public string Board { get; set; }

        public GameState State { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Game, GameInfoDataModel>()
                .ForMember(m => m.Name, opt => opt.MapFrom(game => game.Name))
                .ForMember(m => m.DateCreated, opt => opt.MapFrom(game => game.DateCreated))
                .ForMember(m => m.FirstPlayerName, opt => opt.MapFrom(game => game.FirstPlayer == null ? null : game.FirstPlayer.UserName))
                   .ForMember(m => m.SecondPlayerName, opt => opt.MapFrom(game => game.SecondPlayer == null ? null : game.SecondPlayer.UserName))
                   .ForMember(m => m.State, opt => opt.MapFrom(game => game.State));
        }
    }
}