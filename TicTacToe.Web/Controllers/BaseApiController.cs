namespace TicTacToe.Web.Controllers
{
    using System.Web.Http;
    using Data;
    using Microsoft.AspNet.Identity;

    [Authorize]
    public abstract class BaseApiController : ApiController
    {
        protected ITicTacToeData Data;

        protected BaseApiController(ITicTacToeData data)
        {
            this.Data = data;
        }

        protected string GetUserId()
        {
            string userId = this.User.Identity.GetUserId();
            return userId;
        }

        protected string GetUsername()
        {
            string username = this.User.Identity.GetUserName();
            return username;
        }
    }
}