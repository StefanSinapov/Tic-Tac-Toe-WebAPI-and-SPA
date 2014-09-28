namespace TicTacToe.Services.Controllers
{
    using System.Web.Http;
    using AutoMapper.QueryableExtensions;
    using Data;
    using DataModels;

    public class UsersController : BaseApiController
    {
        public UsersController()
            : this(new TicTacToeData(new ApplicationDbContext()))
        {

        }

        public UsersController(ITicTacToeData data)
            : base(data)
        {
        }

        [HttpGet]
        public IHttpActionResult All()
        {
            var users = this.Data.Users
                .All()
                .Project()
                .To<UserDataModel>();

            return Ok(users);
        } 
    }
}