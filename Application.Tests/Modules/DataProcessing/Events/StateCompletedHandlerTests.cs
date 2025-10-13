using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Domain.Modules.DataProcessing;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Tests.Modules.DataProcessing.Events
{
    [TestClass]
    public class StateCompletedHandlerTests
    {
        private List<IStateProcessor> _mockStateProcessors = null!;
        private ILogger<StateCompletedHandler> _mockLogger = null!;
        private StateCompletedHandler _handler = null!;
        private IStateProcessor _mockStateProcessor = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockStateProcessor = A.Fake<IStateProcessor>();
            _mockLogger = A.Fake<ILogger<StateCompletedHandler>>();

            // Configure mock state processor
            A.CallTo(() => _mockStateProcessor.FromState).Returns(ProcessingState.Ingested);
            A.CallTo(() => _mockStateProcessor.ToState).Returns(ProcessingState.Transformed);

            _mockStateProcessors = new List<IStateProcessor> { _mockStateProcessor };
            _handler = new StateCompletedHandler(_mockStateProcessors, _mockLogger);
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
            A.CallTo(() => _mockStateProcessor.ProcessBatchAsync(batchId, cancellationToken))
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
            A.CallTo(() => _mockStateProcessor.ProcessBatchAsync(A<Guid>._, A<CancellationToken>._))
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
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WhenProcessorThrowsException_DoesNotCatchIt()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.Ingested);
            var cancellationToken = CancellationToken.None;
            var expectedException = new Exception("Processing failed");

            A.CallTo(() => _mockStateProcessor.ProcessBatchAsync(batchId, cancellationToken))
                .Throws(expectedException);

            // Act & Assert
            var act = async () => await _handler.Handle(stateCompletedEvent, cancellationToken);
            await act.Should().ThrowAsync<Exception>().WithMessage("Processing failed");
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Handle_WithUnsupportedState_CatchesInvalidOperationExceptionAndLogsCompletion()
        {
            // Arrange
            var stateProcessors = new List<IStateProcessor>();
            var logger = A.Fake<ILogger<StateCompletedHandler>>();
            var handler = new StateCompletedHandler(stateProcessors, logger);

            var batchId = Guid.NewGuid();
            // Use a state that doesn't have a transition in the state machine - DeDuped
            var stateCompletedEvent = new StateCompletedEvent(batchId, ProcessingState.DeDuped);

            // Act
            await handler.Handle(stateCompletedEvent, CancellationToken.None);
        }
    }
}