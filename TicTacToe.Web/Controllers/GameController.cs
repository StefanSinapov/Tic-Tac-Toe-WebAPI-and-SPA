namespace TicTacToe.Web.Controllers
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Web.Http;
    using AutoMapper.QueryableExtensions;

    using Data;
    using DataModels;
    using GameLogic;
    using TicTacToe.Models;

    [Authorize]
    public class GamesController : BaseApiController
    {
        private readonly IGameResultValidator resultValidator;

        public GamesController(ITicTacToeData data, IGameResultValidator resultValidator)
            : base(data)
        {
            this.resultValidator = resultValidator;
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            return Ok(this.Data.Games.All().Project().To<GameInfoDataModel>());
        }

        [HttpPost]
        public IHttpActionResult Create()
        {
            var userId = this.GetUserId();
            var game = new Game { FirstPlayerId = userId };
            this.Data.Games.Add(game);
            this.Data.SaveChanges();

            var gameDataModel =
                this.Data.Games.All()
                    .Where(x => x.Id == game.Id)
                    .Project()
                    .To<GameInfoDataModel>()
                    .FirstOrDefault();

            return this.Ok(gameDataModel);
        }

        [HttpPost]
        public IHttpActionResult Join()
        {
            var userId = this.GetUserId();

            var firstAvailableGame =
                this.Data.Games.All()
                    .FirstOrDefault(x => x.State == GameState.WaitingForSecondPlayer && x.FirstPlayerId != userId);

            if (firstAvailableGame == null)
            {
                return this.NotFound();
            }

            firstAvailableGame.SecondPlayerId = userId;
            firstAvailableGame.State = GameState.TurnX;
            this.Data.SaveChanges();

            var gameDataModel =
                this.Data.Games.All()
                    .Where(x => x.Id == firstAvailableGame.Id)
                    .Project()
                    .To<GameInfoDataModel>()
                    .FirstOrDefault();

            return this.Ok(gameDataModel);
        }

        [HttpGet]
        public IHttpActionResult Status([Required] string gameId)
        {
            var userId = this.GetUserId();

            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var gameIdAsGuid = new Guid(gameId);
            var gameDataModel =
                this.Data.Games.All()
                    .Where(x => x.Id == gameIdAsGuid && (x.FirstPlayerId == userId || x.SecondPlayerId == userId))
                    .Project()
                    .To<GameInfoDataModel>()
                    .FirstOrDefault();

            if (gameDataModel == null)
            {
                return this.NotFound();
            }

            return this.Ok(gameDataModel);
        }

        [HttpPost]
        public IHttpActionResult Play([FromUri] PlayRequestDataModel request)
        {
            var userId = this.GetUserId();

            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var gameIdAsGuid = new Guid(request.GameId);
            var game =
                this.Data.Games.All()
                    .FirstOrDefault(
                        x => x.Id == gameIdAsGuid && (x.FirstPlayerId == userId || x.SecondPlayerId == userId));

            if (game == null)
            {
                return this.NotFound();
            }

            if (game.State != GameState.TurnX && game.State != GameState.TurnO)
            {
                return this.BadRequest(string.Format("Invalid game state: {0}", game.State));
            }

            if ((game.State == GameState.TurnX && game.FirstPlayerId != userId)
                || (game.State == GameState.TurnO && game.SecondPlayerId != userId))
            {
                return this.BadRequest(string.Format("It's not your turn!"));
            }

            int positionIndex = ((request.Row - 1) * 3) + request.Col - 1;
            if (game.Board[positionIndex] != '-')
            {
                return this.BadRequest("This position is already taken. Please choose a different one.");
            }

            var board = new StringBuilder(game.Board);
            board[positionIndex] = game.State == GameState.TurnX ? 'X' : 'O';
            var boardAsString = board.ToString();
            game.Board = boardAsString;
            game.State = game.State == GameState.TurnX ? GameState.TurnO : GameState.TurnX;

            var gameResult = this.resultValidator.GetResult(game.Board);
            switch (gameResult)
            {
                case GameResult.XWins:
                    game.State = GameState.GameWonByX;
                    break;
                case GameResult.OWins:
                    game.State = GameState.GameWonByO;
                    break;
                case GameResult.Draw:
                    game.State = GameState.GameDraw;
                    break;
            }

            this.Data.SaveChanges();

            var gameDataModel =
                this.Data.Games.All()
                    .Where(x => x.Id == gameIdAsGuid && (x.FirstPlayerId == userId || x.SecondPlayerId == userId))
                    .Project()
                    .To<GameInfoDataModel>()
                    .FirstOrDefault();

            return this.Ok(gameDataModel);
        }
    }
}
