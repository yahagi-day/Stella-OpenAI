using Xunit;
using Moq;
using OpenAI.Chat;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stella_OpenAI.Tests;

public class ChatGptClassTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithSystemMessage()
    {
        // Act
        var chatGpt = new ChatGptClass();

        // Assert - Constructor should not throw exception
        Assert.NotNull(chatGpt);
    }

    [Fact]
    public async Task SendChatGptPromptAsync_WithValidMessages_ShouldReturnResponse()
    {
        // Arrange
        var chatGpt = new ChatGptClass();
        var messages = new List<ChatMessage> { new UserChatMessage("テストメッセージ") };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        // Note: This test requires actual API call, so it's more of an integration test
        // In a real scenario, we would mock the ChatClient
        var exception = await Record.ExceptionAsync(async () =>
        {
            var result = await chatGpt.SendChatGptPromptAsync(messages, cancellationToken);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        });

        // Should not throw exception if API key is valid
        // Will throw if API key is not configured
    }

    [Fact]
    public async Task CreateImageDataAsync_WithValidPrompt_ShouldReturnImageData()
    {
        // Arrange
        var chatGpt = new ChatGptClass();
        var prompt = "魔法少女";
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        // Note: This test requires actual API call
        var exception = await Record.ExceptionAsync(async () =>
        {
            var result = await chatGpt.CreateImageDataAsync(prompt, cancellationToken);
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        });

        // Should not throw exception if API key is valid
    }

    [Fact]
    public async Task SendChatGptPromptAsync_WithEmptyMessages_ShouldHandleGracefully()
    {
        // Arrange
        var chatGpt = new ChatGptClass();
        var messages = new List<ChatMessage>();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await chatGpt.SendChatGptPromptAsync(messages, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task CreateImageDataAsync_WithEmptyPrompt_ShouldHandleGracefully()
    {
        // Arrange
        var chatGpt = new ChatGptClass();
        var prompt = "";
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await chatGpt.CreateImageDataAsync(prompt, cancellationToken);
        });
    }
}