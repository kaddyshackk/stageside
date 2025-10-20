using ComedyPull.Application.Modules.DataProcessing;
using ComedyPull.Domain.Enums;
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
        public void GetNextState_WithCompletedState_ThrowsInvalidOperationException()
        {
            // Act
            var nextState = ProcessingStateMachine.GetNextState(ProcessingState.Completed);
            
            // Assert
            nextState.Should().BeNull();
        }

        [TestMethod, TestCategory("Unit")]
        public void GetNextState_WithFailedState_ThrowsInvalidOperationException()
        {
            // Act
            var nextState = ProcessingStateMachine.GetNextState(ProcessingState.Failed);
            
            // Assert
            nextState.Should().BeNull();
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