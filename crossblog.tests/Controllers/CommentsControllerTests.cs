using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using crossblog.Controllers;
using crossblog.Domain;
using crossblog.Model;
using crossblog.Repositories;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace crossblog.tests.Controllers
{
    public class CommentsControllerTests
    {
        private CommentsController _commentsController;

        private Mock<IArticleRepository> _articleRepositoryMock = new Mock<IArticleRepository>();
        private Mock<ICommentRepository> _commentRepositoryMock = new Mock<ICommentRepository>();

        public CommentsControllerTests()
        {
            _commentsController = new CommentsController(_articleRepositoryMock.Object,_commentRepositoryMock.Object);
        }

        [Fact]
        public async Task GetArticleComments_ReturnsItem()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));

            var commentDbSetMock = Builder<Comment>.CreateListOfSize(2).Build().ToAsyncDbSetMock();
            _commentRepositoryMock.Setup(m => m.Query()).Returns(commentDbSetMock.Object);

            // Act
            var result = await _commentsController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as CommentListModel;
            Assert.NotNull(content);

            Assert.Equal(1, ((CommentListModel)objectResult.Value).Comments.Count());
        }

        [Fact]
        public async Task GetArticleComments_NotFound()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(null));

            // Act
            var result = await _commentsController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task GetArticleComment_ReturnsItem()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));

            var commentDbSetMock = Builder<Comment>.CreateListOfSize(1).Build().ToAsyncDbSetMock();
            _commentRepositoryMock.Setup(m => m.Query()).Returns(commentDbSetMock.Object);

            // Act
            var result = await _commentsController.Get(1,1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as CommentModel;
            Assert.NotNull(content);
        }

        [Fact]
        public async Task GetArticleComment_ArticleNotFound()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));

            var commentDbSetMock = Builder<Comment>.CreateListOfSize(1).Build().ToAsyncDbSetMock();
            _commentRepositoryMock.Setup(m => m.Query()).Returns(commentDbSetMock.Object);

            // Act
            var result = await _commentsController.Get(2, 1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task GetArticleComment_CommentNotFound()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));

            var commentDbSetMock = Builder<Comment>.CreateListOfSize(1).Build().ToAsyncDbSetMock();
            _commentRepositoryMock.Setup(m => m.Query()).Returns(commentDbSetMock.Object);

            // Act
            var result = await _commentsController.Get(1, 2);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task Create_Success()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));
            _commentRepositoryMock.Setup(m => m.InsertAsync(It.IsAny<Comment>())).Returns(Task.FromResult(true));

            // Act
            var model = Builder<CommentModel>.CreateNew().Build();
            var result = await _commentsController.Post(1, model);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as CreatedResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as CommentModel;
            Assert.NotNull(content);

            Assert.Equal("Title1", content.Title);
        }

        [Fact]
        public async Task Create_Fail()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.InsertAsync(It.IsAny<Article>())).Returns(Task.FromResult(true));

            // Act
            var model = Builder<CommentModel>.CreateNew().Build();
            model.Title = "";

            _commentsController.ValidateViewModel(model);
            var result = await _commentsController.Post(1, model);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as BadRequestObjectResult;
            Assert.NotNull(objectResult);    
        }

        [Fact]
        public async Task Create_ArticleNotFound()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(null));

            // Act
            var result = await _commentsController.Post(1, Builder<CommentModel>.CreateNew().Build());

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }
    }
}
