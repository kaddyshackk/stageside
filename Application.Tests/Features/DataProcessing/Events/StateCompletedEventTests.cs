using ComedyPull.Application.Features.DataProcessing.Events;
using ComedyPull.Domain.Models.Processing;
using FluentAssertions;

namespace ComedyPull.Application.Tests.Features.DataProcessing.Events
{
    [TestClass]
    public class StateCompletedEventTests
    {
        [TestMethod]
        public void StateCompletedEvent_ShouldCreateWithCorrectProperties()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var completedState = ProcessingState.Ingested;

            // Act
            var stateCompletedEvent = new StateCompletedEvent(batchId, completedState);

            // Assert
            stateCompletedEvent.BatchId.Should().Be(batchId);
            stateCompletedEvent.CompletedState.Should().Be(completedState);
        }

        [TestMethod, TestCategory("Unit")]
        public void StateCompletedEvent_WithTransformedState_ShouldCreateCorrectly()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var completedState = ProcessingState.Transformed;

            // Act
            var stateCompletedEvent = new StateCompletedEvent(batchId, completedState);

            // Assert
            stateCompletedEvent.BatchId.Should().Be(batchId);
            stateCompletedEvent.CompletedState.Should().Be(completedState);
        }

        [TestMethod, TestCategory("Unit")]
        public void StateCompletedEvent_WithDifferentBatchIds_ShouldNotBeEqual()
        {
            // Arrange
            var batchId1 = Guid.NewGuid();
            var batchId2 = Guid.NewGuid();
            var completedState = ProcessingState.Ingested;

            // Act
            var event1 = new StateCompletedEvent(batchId1, completedState);
            var event2 = new StateCompletedEvent(batchId2, completedState);

            // Assert
            event1.Should().NotBe(event2);
            event1.BatchId.Should().NotBe(event2.BatchId);
        }

        [TestMethod, TestCategory("Unit")]
        public void StateCompletedEvent_WithDifferentStates_ShouldNotBeEqual()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var state1 = ProcessingState.Ingested;
            var state2 = ProcessingState.Transformed;

            // Act
            var event1 = new StateCompletedEvent(batchId, state1);
            var event2 = new StateCompletedEvent(batchId, state2);

            // Assert
            event1.Should().NotBe(event2);
            event1.CompletedState.Should().NotBe(event2.CompletedState);
        }

        [TestMethod, TestCategory("Unit")]
        public void StateCompletedEvent_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var completedState = ProcessingState.Ingested;

            // Act
            var event1 = new StateCompletedEvent(batchId, completedState);
            var event2 = new StateCompletedEvent(batchId, completedState);

            // Assert
            event1.Should().Be(event2);
            event1.BatchId.Should().Be(event2.BatchId);
            event1.CompletedState.Should().Be(event2.CompletedState);
        }

        [TestMethod, TestCategory("Unit")]
        public void StateCompletedEvent_WithAllProcessingStates_ShouldCreateCorrectly()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var allStates = new[]
            {
                ProcessingState.Ingested,
                ProcessingState.Transformed,
                ProcessingState.DeDuped,
                ProcessingState.Enriched,
                ProcessingState.Linked,
                ProcessingState.Completed,
                ProcessingState.Failed
            };

            // Act & Assert
            foreach (var state in allStates)
            {
                var stateCompletedEvent = new StateCompletedEvent(batchId, state);
                stateCompletedEvent.BatchId.Should().Be(batchId);
                stateCompletedEvent.CompletedState.Should().Be(state);
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void StateCompletedEvent_ToString_ShouldContainBatchIdAndState()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var completedState = ProcessingState.Ingested;
            var stateCompletedEvent = new StateCompletedEvent(batchId, completedState);

            // Act
            var stringRepresentation = stateCompletedEvent.ToString();

            // Assert
            stringRepresentation.Should().Contain(batchId.ToString());
            stringRepresentation.Should().Contain(completedState.ToString());
        }
    }
}