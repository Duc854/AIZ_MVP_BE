using AIZ_MVP_Bussiness.Dtos.RequestDtos;
using AIZ_MVP_Bussiness.Services;
using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Entities;
using Moq;
using Shared.Models;
using Shared.Wrappers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AIZ_MVP_Bussiness.Tests
{
    /// <summary>
    /// Unit tests for InterviewAnswerService idempotent behavior.
    /// Tests that save-answer can be called multiple times without errors.
    /// </summary>
    public class InterviewAnswerServiceIdempotentTests
    {
        private readonly Mock<IInterviewAnswerRepository> _mockAnswerRepository;
        private readonly Mock<IInterviewTurnRepository> _mockTurnRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly InterviewAnswerService _service;

        public InterviewAnswerServiceIdempotentTests()
        {
            _mockAnswerRepository = new Mock<IInterviewAnswerRepository>();
            _mockTurnRepository = new Mock<IInterviewTurnRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _service = new InterviewAnswerService(
                _mockTurnRepository.Object,
                _mockAnswerRepository.Object,
                _mockUnitOfWork.Object);
        }

        [Fact]
        public async Task SaveAnswer_FirstCall_CreatesNewAnswer()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var turnId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userIdentity = new UserIdentity { UserId = userId.ToString() };

            var dto = new SaveInterviewAnswerDto
            {
                InterviewSessionId = sessionId,
                TurnIndex = 1,
                UserAnswer = "My first answer"
            };

            var interviewSession = new InterviewSession
            {
                Id = sessionId,
                UserId = userId
            };

            var turn = new InterviewTurn
            {
                Id = turnId,
                InterviewSessionId = sessionId,
                TurnIndex = 1,
                InterviewSession = interviewSession,
                Answer = null // No existing answer
            };

            _mockTurnRepository
                .Setup(r => r.GetBySessionAndTurnForUpdate(sessionId, 1))
                .ReturnsAsync(turn);

            _mockAnswerRepository
                .Setup(r => r.GetAnswerByTurnIdForUpdate(turnId))
                .ReturnsAsync((InterviewAnswer?)null); // No existing answer

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SaveAnswer(dto, userIdentity);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            _mockAnswerRepository.Verify(r => r.Add(It.IsAny<InterviewAnswer>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SaveAnswer_SecondCall_UpdatesExistingAnswer()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var turnId = Guid.NewGuid();
            var answerId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userIdentity = new UserIdentity { UserId = userId.ToString() };

            var dto = new SaveInterviewAnswerDto
            {
                InterviewSessionId = sessionId,
                TurnIndex = 1,
                UserAnswer = "My updated answer"
            };

            var interviewSession = new InterviewSession
            {
                Id = sessionId,
                UserId = userId
            };

            var existingAnswer = new InterviewAnswer
            {
                Id = answerId,
                InterviewTurnId = turnId,
                AnswerText = "My first answer",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                UpdatedAt = null
            };

            var turn = new InterviewTurn
            {
                Id = turnId,
                InterviewSessionId = sessionId,
                TurnIndex = 1,
                InterviewSession = interviewSession,
                Answer = existingAnswer
            };

            _mockTurnRepository
                .Setup(r => r.GetBySessionAndTurnForUpdate(sessionId, 1))
                .ReturnsAsync(turn);

            _mockAnswerRepository
                .Setup(r => r.GetAnswerByTurnIdForUpdate(turnId))
                .ReturnsAsync(existingAnswer); // Existing answer found

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SaveAnswer(dto, userIdentity);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(answerId, result.Value); // Returns same answer ID
            Assert.Equal("My updated answer", existingAnswer.AnswerText); // Answer text updated
            Assert.NotNull(existingAnswer.UpdatedAt); // UpdatedAt is set
            _mockAnswerRepository.Verify(r => r.Add(It.IsAny<InterviewAnswer>()), Times.Never); // No new answer created
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SaveAnswer_MultipleCalls_AlwaysSucceeds()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var turnId = Guid.NewGuid();
            var answerId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userIdentity = new UserIdentity { UserId = userId.ToString() };

            var interviewSession = new InterviewSession
            {
                Id = sessionId,
                UserId = userId
            };

            var existingAnswer = new InterviewAnswer
            {
                Id = answerId,
                InterviewTurnId = turnId,
                AnswerText = "Initial answer",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var turn = new InterviewTurn
            {
                Id = turnId,
                InterviewSessionId = sessionId,
                TurnIndex = 1,
                InterviewSession = interviewSession,
                Answer = existingAnswer
            };

            _mockTurnRepository
                .Setup(r => r.GetBySessionAndTurnForUpdate(sessionId, 1))
                .ReturnsAsync(turn);

            _mockAnswerRepository
                .Setup(r => r.GetAnswerByTurnIdForUpdate(turnId))
                .ReturnsAsync(existingAnswer);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act - Call multiple times with different answers
            var dto1 = new SaveInterviewAnswerDto
            {
                InterviewSessionId = sessionId,
                TurnIndex = 1,
                UserAnswer = "First update"
            };

            var dto2 = new SaveInterviewAnswerDto
            {
                InterviewSessionId = sessionId,
                TurnIndex = 1,
                UserAnswer = "Second update"
            };

            var result1 = await _service.SaveAnswer(dto1, userIdentity);
            var result2 = await _service.SaveAnswer(dto2, userIdentity);

            // Assert
            Assert.True(result1.IsSuccess);
            Assert.True(result2.IsSuccess);
            Assert.Equal(answerId, result1.Value);
            Assert.Equal(answerId, result2.Value); // Same ID returned
            Assert.Equal("Second update", existingAnswer.AnswerText); // Latest answer text
            _mockAnswerRepository.Verify(r => r.Add(It.IsAny<InterviewAnswer>()), Times.Never); // Never creates new
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Exactly(2)); // Called twice
        }
    }
}
