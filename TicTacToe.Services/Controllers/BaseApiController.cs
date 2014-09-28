namespace TicTacToe.Services.Controllers
{
    using System.Web.Http;
    using System.Web.Http.Cors;
    using Data;

    [EnableCors("*", "*", "*")]
    public abstract class BaseApiController : ApiController
    {
        protected ITicTacToeData Data;

        protected BaseApiController(ITicTacToeData data)
        {
            this.Data = data;
        }
    }
}