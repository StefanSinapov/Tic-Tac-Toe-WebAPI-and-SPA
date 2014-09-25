namespace TicTacToe.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using Data.Contracts;
    using GameLogic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Models;
    using Moq;
    using Web.Controllers;
    using Web.DataModels;
    using Web.Infrastructure;

    [TestClass]
    public class PlayActionTests
    {
        [TestMethod]
        public void WhenIsXTurnChangeStateToO()
        {
            const string userId = "Niki";
            var gameId = Guid.NewGuid();
            var game = new Game
            {
                Id = gameId,
                FirstPlayerId = userId,
                State = GameState.TurnX
            };
            game.Id = gameId;

            var userIdProviderMock = new Mock<IUserIdProvider>();
            userIdProviderMock.Setup(x => x.GetUserId()).Returns(userId);

            var repositoryMock = new Mock<IRepository<Game>>();
            repositoryMock.Setup(x => x.All()).Returns(() =>
                new List<Game> { game, }
                .AsQueryable());
            repositoryMock.Setup(x => x.Find(It.IsAny<Guid>())).Returns(game);

            var uowData = new Mock<ITicTacToeData>();
            uowData.SetupGet(x => x.Games).Returns(repositoryMock.Object);

            var controller = new GamesController(
                        uowData.Object,
                        new GameResultValidator(),
                        userIdProviderMock.Object);



            var result = controller.Play(
                new PlayRequestDataModel
                {
                    GameId = gameId.ToString(),
                    Col = 1,
                    Row = 1,
                });

            Assert.AreEqual(GameState.TurnO, game.State);
        }
    }
}