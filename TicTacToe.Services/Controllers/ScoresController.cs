namespace TicTacToe.Services.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using AutoMapper.QueryableExtensions;
    using Data;
    using DataModels;

    public class ScoresController : BaseApiController
    {
        private const int DefaultReturnSize = 10;
        public ScoresController()
            : this(new TicTacToeData(new ApplicationDbContext()))
        {
            
        }

        public ScoresController(ITicTacToeData data)
            : base(data)
        {

        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            var users = this.Data.Users.All()
                .Project()
                .To<UserScoreDataModel>()
                .OrderByDescending(u => u.Score)
                .ThenBy(u => u.Username)
                .Take(DefaultReturnSize);

            return Ok(users);
        }
    }
}