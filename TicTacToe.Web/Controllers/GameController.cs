namespace TicTacToe.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using AutoMapper.QueryableExtensions;

    using Data;
    using DataModels;
    using GameLogic;
    using Infrastructure;
    using TicTacToe.Models;

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
        [Authorize]
        public IHttpActionResult Get()
        {
            return Ok(this.Data.Games.All().Project().To<GameInfoDataModel>());
        }

        [HttpGet]
        [Authorize]
        public IQueryable<ListGameDataModel> MyGames()
        {
            var userId = this.userIdProvider.GetUserId();
            var games = this.Data.Games
                            .All()
                            .Where(g => (g.FirstPlayerId == userId ||
                                         (g.SecondPlayerId == userId && g.State != GameState.TurnO &&
                                          g.State != GameState.TurnX && g.State != GameState.WaitingForSecondPlayer)))
                            .Select(ListGameDataModel.FromGame);
            return games;
        }
        [HttpGet]
        [Authorize]
        public IQueryable<ListGameDataModel> JoinedGames()
        {
            var userId = this.userIdProvider.GetUserId();
            var games = this.Data.Games
                            .All()
                            .Where(g => g.SecondPlayerId == userId && (g.State == GameState.TurnO || g.State == GameState.TurnX))
                            .Select(ListGameDataModel.FromGame);
            return games;
        }

        [HttpGet]
        [Authorize]
        public IQueryable<ListGameDataModel> AvailableGames()
        {
            var userId = this.userIdProvider.GetUserId();
            var games = this.Data.Games
                            .All()
                            .Where(g => g.State == GameState.WaitingForSecondPlayer && g.FirstPlayerId != userId)
                            .OrderByDescending(g => g.DateCreated)
                            .Select(ListGameDataModel.FromGame);
            return games;
        }

        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(CreateGameDataModel model)
        {
            var currentUserId = this.userIdProvider.GetUserId();

            var newGame = new Game
            {
                Name = model.Name,
                FirstPlayerId = currentUserId,
                DateCreated = DateTime.Now
            };

            this.Data.Games.Add(newGame);
            this.Data.SaveChanges();

            return this.Ok(newGame.Id);
        }

        [HttpPost]
        [Authorize]
        public IHttpActionResult Join(GameIdDataModel gameModel)
        {
            var currentUserId = this.userIdProvider.GetUserId();

            var game = this.Data.Games
                .All()
                .FirstOrDefault(g => g.Id.ToString() == gameModel.GameId &&
                                           g.State == GameState.WaitingForSecondPlayer &&
                                           g.FirstPlayerId != currentUserId);

            if (game == null)
            {
                return this.NotFound();
            }

            game.SecondPlayerId = currentUserId;
            game.State = GameState.TurnX;
            this.Data.SaveChanges();

            return this.Ok();
        }

        [Authorize]
        [HttpPost]
        public IHttpActionResult Status(GameIdDataModel gameModel)
        {
           var currentUserId = this.userIdProvider.GetUserId();
            var idAsGuid = new Guid(gameModel.GameId);

            var game = this.Data.Games.All()
                           .Where(x => x.Id == idAsGuid)
                           .Select(x => new { x.FirstPlayerId, x.SecondPlayerId })
                           .FirstOrDefault();

            if (game == null)
            {
                return this.NotFound();
            }

            if (game.FirstPlayerId != currentUserId && game.SecondPlayerId != currentUserId)
            {
                return this.BadRequest("This is not your game!");
            }

            var gameInfo = this.Data.Games.All()
                               .Where(g => g.Id == idAsGuid)
                                .Project()
                                .To<GameInfoDataModel>()
                               .FirstOrDefault();

            return this.Ok(gameInfo);
        }

        [HttpPost]
        [Authorize]
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

            ChangeGameState(gameResult, game);

            this.Data.SaveChanges();

            var gameDataModel =
                this.Data.Games.All()
                    .Where(x => x.Id == gameIdAsGuid && (x.FirstPlayerId == userId || x.SecondPlayerId == userId))
                    .Project()
                    .To<GameInfoDataModel>()
                    .FirstOrDefault();

            return this.Ok(gameDataModel);
        }

        private void ChangeGameState(GameResult gameResult, Game game)
        {
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
        }
    }
}
