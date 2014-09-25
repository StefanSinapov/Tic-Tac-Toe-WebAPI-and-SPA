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
    using Infrastructure;
    using TicTacToe.Models;

    [Authorize]
    public class GamesController : BaseApiController
    {
        private readonly IGameResultValidator resultValidator;
        private readonly IUserIdProvider userIdProvider;

        public GamesController(ITicTacToeData data, 
            IGameResultValidator resultValidator, 
            IUserIdProvider userIdProvider)
            : base(data)
        {
            this.resultValidator = resultValidator;
            this.userIdProvider = userIdProvider;
        }

        [HttpGet]
        
        public IHttpActionResult Get()
        {
            return Ok(this.Data.Games.All().Project().To<GameInfoDataModel>());
        }

        [HttpPost]
        public IHttpActionResult Create()
        {
            var userId = this.userIdProvider.GetUserId();
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
            var userId = this.userIdProvider.GetUserId();

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
            var userId = this.userIdProvider.GetUserId();

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
        public IHttpActionResult Play(PlayRequestDataModel request)
        {
            var userId = this.userIdProvider.GetUserId();

            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var gameIdAsGuid = new Guid(request.GameId);

            var game = this.Data.Games.Find(gameIdAsGuid);
            if (game == null)
            {
                return this.BadRequest("Invalid game id!");
            }

            if (game.FirstPlayerId != userId &&
                game.SecondPlayerId != userId)
            {
                return this.BadRequest("This is not your game!");
            }

            if (game.State != GameState.TurnX &&
                game.State != GameState.TurnO)
            {
                var message = string.Format("The game is not currently playing! Game state is '{0}'", game.State);
                return this.BadRequest(message);
            }

            if ((game.State == GameState.TurnX &&
                game.FirstPlayerId != userId)
                ||
                (game.State == GameState.TurnO &&
                game.SecondPlayerId != userId))
            {
                return this.BadRequest("It's not your turn!");
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
                case GameResult.WonByX:
                    game.State = GameState.GameWonByX;
                    game.FirstPlayer.Wins++;
                    game.SecondPlayer.Losses++;
                    break;
                case GameResult.WonByY:
                    game.State = GameState.GameWonByO;
                    game.SecondPlayer.Wins++;
                    game.FirstPlayer.Losses++;
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
