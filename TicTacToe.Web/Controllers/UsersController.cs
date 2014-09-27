namespace TicTacToe.Web.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using AutoMapper.QueryableExtensions;
    using Data;
    using DataModels;

    public class UsersController : BaseApiController
    {
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