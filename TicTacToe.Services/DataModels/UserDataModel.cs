namespace TicTacToe.Services.DataModels
{
    using System;
    using AutoMapper;
    using Mapping;
    using TicTacToe.Models;

    public class UserDataModel : IMappableFrom<User>, IHaveCustomMappings
    {
        public string Username { get; set; }
        public DateTime DateRegistration { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<User, UserDataModel>()
                .ForMember(m => m.Username, opt => opt.MapFrom(user => user.UserName))
                .ForMember(m => m.DateRegistration, opt => opt.MapFrom(user => user.DateRegistration));
        }
    }
}