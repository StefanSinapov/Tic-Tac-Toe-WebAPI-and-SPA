namespace TicTacToe.Web.Controllers
{
    using System.Web.Http;
    using Data;

    public abstract class BaseApiController : ApiController
    {
        protected ITicTacToeData Data;

        protected BaseApiController(ITicTacToeData data)
        {
            this.Data = data;
        }
    }
}