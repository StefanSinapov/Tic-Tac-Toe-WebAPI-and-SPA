namespace TicTacToe.Web.DataModels
{
    using System;
    using System.Linq.Expressions;
    using AutoMapper;
    using Mapping;
    using TicTacToe.Models;

    public class UserScoreDataModel : IMappableFrom<User>, IHaveCustomMappings
    {

        public static Expression<Func<User, UserScoreDataModel>> FromEntityToModel
        {
            get
            {
                return u => new UserScoreDataModel
                {
                    Username = u.UserName,
                    Wins = u.Wins,
                    Losses = u.Losses,
                    Score = (u.Wins * 100) + (15 * u.Losses)
                };
            }
        }

        public string Username { get; set; }

        public int Wins { get; set; }

        public int Losses { get; set; }

        public int Score { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<User, UserScoreDataModel>()
                .ForMember(m => m.Username, opt => opt.MapFrom(user => user.UserName))
                .ForMember(m => m.Wins, opt => opt.MapFrom(user => user.Wins))
                .ForMember(m => m.Losses, opt => opt.MapFrom(user => user.Losses))
                .ForMember(m => m.Score, opt => opt.MapFrom(user => (user.Wins * 100) + (15 * user.Losses)));
        }
    }
}