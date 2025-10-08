using ComedyPull.Application.Modules.DataProcessing;
using ComedyPull.Domain.Models.Processing;
using FluentAssertions;

namespace ComedyPull.Application.Tests.Modules.DataProcessing
{
    [TestClass]
    public class ProcessingStateMachineTests
    {
        [TestMethod, TestCategory("Unit")]
        public void GetNextState_WithIngestedState_ReturnsTransformed()
        {
            // Act
            var nextState = ProcessingStateMachine.GetNextState(ProcessingState.Ingested);

            // Assert
            nextState.Should().Be(ProcessingState.Transformed);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetNextState_WithTransformedState_ReturnsCompleted()
        {
            // Act
            var nextState = ProcessingStateMachine.GetNextState(ProcessingState.Transformed);

            // Assert
            nextState.Should().Be(ProcessingState.Completed);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetNextState_WithDeDupedState_ThrowsInvalidOperationException()
        {
            // Act & Assert
            var act = () => ProcessingStateMachine.GetNextState(ProcessingState.DeDuped);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("No valid transition from state DeDuped");
        }

        [TestMethod, TestCategory("Unit")]
        public void GetNextState_WithEnrichedState_ThrowsInvalidOperationException()
        {
            // Act & Assert
            var act = () => ProcessingStateMachine.GetNextState(ProcessingState.Enriched);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("No valid transition from state Enriched");
        }

        [TestMethod, TestCategory("Unit")]
        public void GetNextState_WithLinkedState_ThrowsInvalidOperationException()
        {
            // Act & Assert
            var act = () => ProcessingStateMachine.GetNextState(ProcessingState.Linked);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("No valid transition from state Linked");
        }

        [TestMethod, TestCategory("Unit")]
        public void GetNextState_WithCompletedState_ThrowsInvalidOperationException()
        {
            // Act & Assert
            var act = () => ProcessingStateMachine.GetNextState(ProcessingState.Completed);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("No valid transition from state Completed");
        }

        [TestMethod, TestCategory("Unit")]
        public void GetNextState_WithFailedState_ThrowsInvalidOperationException()
        {
            // Act & Assert
            var act = () => ProcessingStateMachine.GetNextState(ProcessingState.Failed);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("No valid transition from state Failed");
        }

        [TestMethod, TestCategory("Unit")]
        public void CanTransition_FromIngestedToTransformed_ReturnsTrue()
        {
            // Act
            var canTransition = ProcessingStateMachine.CanTransition(ProcessingState.Ingested, ProcessingState.Transformed);

            // Assert
            canTransition.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        public void CanTransition_FromIngestedToDeDuped_ReturnsFalse()
        {
            // Act
            var canTransition = ProcessingStateMachine.CanTransition(ProcessingState.Ingested, ProcessingState.DeDuped);

            // Assert
            canTransition.Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        public void CanTransition_FromTransformedToCompleted_ReturnsTrue()
        {
            // Act
            var canTransition = ProcessingStateMachine.CanTransition(ProcessingState.Transformed, ProcessingState.Completed);

            // Assert
            canTransition.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        public void CanTransition_FromTransformedToInvalidStates_ReturnsFalse()
        {
            // Arrange
            var targetStates = new[]
            {
                ProcessingState.Ingested,
                ProcessingState.DeDuped,
                ProcessingState.Enriched,
                ProcessingState.Linked,
                ProcessingState.Failed
            };

            // Act & Assert
            foreach (var targetState in targetStates)
            {
                var canTransition = ProcessingStateMachine.CanTransition(ProcessingState.Transformed, targetState);
                canTransition.Should().BeFalse($"transition from Transformed to {targetState} should not be allowed");
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void CanTransition_FromInvalidStates_ReturnsFalse()
        {
            // Arrange
            var invalidStates = new[]
            {
                ProcessingState.DeDuped,
                ProcessingState.Enriched,
                ProcessingState.Linked,
                ProcessingState.Completed,
                ProcessingState.Failed
            };

            // Act & Assert
            foreach (var invalidState in invalidStates)
            {
                var canTransition = ProcessingStateMachine.CanTransition(invalidState, ProcessingState.Transformed);
                canTransition.Should().BeFalse($"transition from {invalidState} to Transformed should not be allowed");
            }
        }
    }
}