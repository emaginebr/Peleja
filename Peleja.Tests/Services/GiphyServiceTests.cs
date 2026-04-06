namespace Peleja.Tests.Services;

using FluentAssertions;
using Moq;
using Peleja.Domain.Models.DTOs;
using Peleja.Domain.Services;
using Peleja.Infra.Interfaces.AppServices;

public class GiphyServiceTests
{
    private readonly Mock<IGiphyAppService> _giphyAppServiceMock;
    private readonly GiphyService _service;

    public GiphyServiceTests()
    {
        _giphyAppServiceMock = new Mock<IGiphyAppService>();
        _service = new GiphyService(_giphyAppServiceMock.Object);
    }

    [Fact]
    public async Task SearchAsync_ReturnsResults()
    {
        // Arrange
        var expectedResult = new GiphySearchResult
        {
            Items = new List<GiphyItemInfo>
            {
                new GiphyItemInfo { Id = "abc123", Title = "Funny cat", Url = "https://giphy.com/cat.gif", PreviewUrl = "https://giphy.com/cat_preview.gif", Width = 200, Height = 200 }
            },
            TotalCount = 1,
            Offset = 0,
            Limit = 20
        };

        _giphyAppServiceMock
            .Setup(s => s.SearchAsync("cat", 20, 0))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.SearchAsync("cat", 20, 0);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be("abc123");
        result.Items[0].Title.Should().Be("Funny cat");
        _giphyAppServiceMock.Verify(s => s.SearchAsync("cat", 20, 0), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.SearchAsync("", 20, 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*busca*obrigatório*");
    }

    [Fact]
    public async Task SearchAsync_WithWhitespaceQuery_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.SearchAsync("   ", 20, 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*busca*obrigatório*");
    }

    [Fact]
    public async Task SearchAsync_ClampsLimitToMinimum1()
    {
        // Arrange
        var expectedResult = new GiphySearchResult
        {
            Items = new List<GiphyItemInfo>(),
            TotalCount = 0,
            Offset = 0,
            Limit = 1
        };

        _giphyAppServiceMock
            .Setup(s => s.SearchAsync("cat", 1, 0))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.SearchAsync("cat", -5, 0);

        // Assert
        _giphyAppServiceMock.Verify(s => s.SearchAsync("cat", 1, 0), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_ClampsLimitToMaximum50()
    {
        // Arrange
        var expectedResult = new GiphySearchResult
        {
            Items = new List<GiphyItemInfo>(),
            TotalCount = 0,
            Offset = 0,
            Limit = 50
        };

        _giphyAppServiceMock
            .Setup(s => s.SearchAsync("cat", 50, 0))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.SearchAsync("cat", 100, 0);

        // Assert
        _giphyAppServiceMock.Verify(s => s.SearchAsync("cat", 50, 0), Times.Once);
    }
}
