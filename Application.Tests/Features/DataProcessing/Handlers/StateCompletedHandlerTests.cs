using ComedyPull.Application.Features.DataProcessing.Events;
using ComedyPull.Application.Features.DataProcessing.Handlers;
using ComedyPull.Application.Features.DataProcessing.Interfaces;
using ComedyPull.Domain.Models.Processing;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Tests.Features.DataProcessing.Handlers
{
    [TestClass]
    public class StateCompletedHandlerTests
    {
        private IServiceProvider _mockServiceProvider = null!;
        private ILogger<StateCompletedHandler> _mockLogger = null!;
        private StateCompletedHandler _handler = null!;
        private ITransformProcessor _mockTransformProcessor = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockServiceProvider = A.Fake<IServiceProvider>();
            _mockLogger = A.Fake<ILogger<StateCompletedHandler>>();
            _mockTransformProcessor = A.Fake<ITransformProcessor>();
            _handler = new StateCompletedHandler(_mockServiceProvider, _mockLogger);

            // Configure the service provider to return the mock processor
            A.CallTo(() => _mockServiceProvider.GetService(typeof(ITransformProcessor)))
                .Returns(_mockTransformProcessor);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WithIngestedState_GetsTransformProcessorAndCallsProcessBatch()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.Ingested);
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(stateCompletedEvent, cancellationToken);

            // Assert
            A.CallTo(() => _mockServiceProvider.GetService(typeof(ITransformProcessor)))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _mockTransformProcessor.ProcessBatchAsync(batchId, cancellationToken))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WithIngestedState_LogsStartingProcessing()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.Ingested);
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(stateCompletedEvent, cancellationToken);

            // Assert
            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Information)
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WithTransformedState_CatchesInvalidOperationExceptionAndLogsCompletion()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.Transformed);
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(stateCompletedEvent, cancellationToken);

            // Assert
            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Information)
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _mockServiceProvider.GetService(typeof(ITransformProcessor)))
                .MustNotHaveHappened();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WithDeDupedState_CatchesInvalidOperationExceptionAndLogsCompletion()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.DeDuped);
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(stateCompletedEvent, cancellationToken);

            // Assert
            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Information)
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WithEnrichedState_CatchesInvalidOperationExceptionAndLogsCompletion()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.Enriched);
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(stateCompletedEvent, cancellationToken);

            // Assert
            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Information)
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WithLinkedState_CatchesInvalidOperationExceptionAndLogsCompletion()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.Linked);
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(stateCompletedEvent, cancellationToken);

            // Assert
            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Information)
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WithCompletedState_CatchesInvalidOperationExceptionAndLogsCompletion()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.Completed);
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(stateCompletedEvent, cancellationToken);

            // Assert
            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Information)
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WithFailedState_CatchesInvalidOperationExceptionAndLogsCompletion()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.Failed);
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(stateCompletedEvent, cancellationToken);

            // Assert
            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Information)
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WhenProcessorThrowsException_DoesNotCatchIt()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.Ingested);
            var cancellationToken = CancellationToken.None;
            var expectedException = new Exception("Processing failed");

            A.CallTo(() => _mockTransformProcessor.ProcessBatchAsync(batchId, cancellationToken))
                .Throws(expectedException);

            // Act & Assert
            var act = async () => await _handler.Handle(stateCompletedEvent, cancellationToken);
            await act.Should().ThrowAsync<Exception>().WithMessage("Processing failed");
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WithUnsupportedState_CatchesInvalidOperationExceptionAndLogsCompletion()
        {
            // Arrange
            var serviceProvider = A.Fake<IServiceProvider>();
            var logger = A.Fake<ILogger<StateCompletedHandler>>();
            var handler = new StateCompletedHandler(serviceProvider, logger);

            var batchId = Guid.NewGuid();
            // Use a state that doesn't have a transition in the state machine - DeDuped
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.DeDuped);

            // Act
            await handler.Handle(stateCompletedEvent, CancellationToken.None);

            // Assert - Should log completion message when no transition found
            A.CallTo(logger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Information)
                .MustHaveHappenedOnceExactly();
        }
    }
}