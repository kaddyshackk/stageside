using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.DataProcessing.Services.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Complete;
using ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Models.Processing;
using FakeItEasy;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Tests.Modules.DataProcessing.Processors
{
    [TestClass]
    public class CompleteStateProcessorTests
    {
        private ICompleteStateRepository _mockRepository = null!;
        private ISubProcessorResolver _mockSubProcessorResolver = null!;
        private IMediator _mockMediator = null!;
        private ILogger<CompleteStateProcessor> _mockLogger = null!;
        private CompleteStateProcessor _processor = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = A.Fake<ICompleteStateRepository>();
            _mockSubProcessorResolver = A.Fake<ISubProcessorResolver>();
            _mockMediator = A.Fake<IMediator>();
            _mockLogger = A.Fake<ILogger<CompleteStateProcessor>>();

            _processor = new CompleteStateProcessor(
                _mockRepository,
                _mockSubProcessorResolver,
                _mockMediator,
                _mockLogger);
        }

        [TestMethod, TestCategory("Unit")]
        public void FromState_ShouldBeTransformed()
        {
            // Assert
            _processor.FromState.Should().Be(ProcessingState.Transformed);
        }

        [TestMethod, TestCategory("Unit")]
        public void ToState_ShouldBeCompleted()
        {
            // Assert
            _processor.ToState.Should().Be(ProcessingState.Completed);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_LoadsRecordsFromRepository()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var records = new List<SourceRecord>
            {
                CreateSourceRecord(DataSource.Punchup)
            };

            A.CallTo(() => _mockRepository.GetRecordsByBatchAsync(batchId.ToString(), A<CancellationToken>._))
                .Returns(records);

            var mockSubProcessor = A.Fake<ISubProcessor<DataSource>>();
            A.CallTo(() => _mockSubProcessorResolver.Resolve(A<DataSource>._, ProcessingState.Transformed, ProcessingState.Completed))
                .Returns(mockSubProcessor);

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.GetRecordsByBatchAsync(batchId.ToString(), A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_GroupsRecordsBySource()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var records = new List<SourceRecord>
            {
                CreateSourceRecord(DataSource.Punchup),
                CreateSourceRecord(DataSource.Punchup)
            };

            A.CallTo(() => _mockRepository.GetRecordsByBatchAsync(batchId.ToString(), A<CancellationToken>._))
                .Returns(records);

            var mockSubProcessor = A.Fake<ISubProcessor<DataSource>>();
            A.CallTo(() => _mockSubProcessorResolver.Resolve(DataSource.Punchup, ProcessingState.Transformed, ProcessingState.Completed))
                .Returns(mockSubProcessor);

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockSubProcessorResolver.Resolve(DataSource.Punchup, ProcessingState.Transformed, ProcessingState.Completed))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => mockSubProcessor.ProcessAsync(
                A<IEnumerable<SourceRecord>>.That.Matches(r => r.Count() == 2),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_ResolvesSubProcessorForEachSource()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var records = new List<SourceRecord>
            {
                CreateSourceRecord(DataSource.Punchup)
            };

            A.CallTo(() => _mockRepository.GetRecordsByBatchAsync(batchId.ToString(), A<CancellationToken>._))
                .Returns(records);

            var mockSubProcessor = A.Fake<ISubProcessor<DataSource>>();
            A.CallTo(() => _mockSubProcessorResolver.Resolve(DataSource.Punchup, ProcessingState.Transformed, ProcessingState.Completed))
                .Returns(mockSubProcessor);

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockSubProcessorResolver.Resolve(DataSource.Punchup, ProcessingState.Transformed, ProcessingState.Completed))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_CallsSubProcessorProcessAsync()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var records = new List<SourceRecord>
            {
                CreateSourceRecord(DataSource.Punchup)
            };

            A.CallTo(() => _mockRepository.GetRecordsByBatchAsync(batchId.ToString(), A<CancellationToken>._))
                .Returns(records);

            var mockSubProcessor = A.Fake<ISubProcessor<DataSource>>();
            A.CallTo(() => _mockSubProcessorResolver.Resolve(DataSource.Punchup, ProcessingState.Transformed, ProcessingState.Completed))
                .Returns(mockSubProcessor);

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => mockSubProcessor.ProcessAsync(A<IEnumerable<SourceRecord>>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_SavesChangesAfterProcessing()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var records = new List<SourceRecord>
            {
                CreateSourceRecord(DataSource.Punchup)
            };

            A.CallTo(() => _mockRepository.GetRecordsByBatchAsync(batchId.ToString(), A<CancellationToken>._))
                .Returns(records);

            var mockSubProcessor = A.Fake<ISubProcessor<DataSource>>();
            A.CallTo(() => _mockSubProcessorResolver.Resolve(DataSource.Punchup, ProcessingState.Transformed, ProcessingState.Completed))
                .Returns(mockSubProcessor);

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_PublishesStateCompletedEvent()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var records = new List<SourceRecord>
            {
                CreateSourceRecord(DataSource.Punchup)
            };

            A.CallTo(() => _mockRepository.GetRecordsByBatchAsync(batchId.ToString(), A<CancellationToken>._))
                .Returns(records);

            var mockSubProcessor = A.Fake<ISubProcessor<DataSource>>();
            A.CallTo(() => _mockSubProcessorResolver.Resolve(DataSource.Punchup, ProcessingState.Transformed, ProcessingState.Completed))
                .Returns(mockSubProcessor);

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockMediator.Publish(
                A<StateCompletedEvent>.That.Matches(e =>
                    e.BatchId == batchId &&
                    e.CompletedState == ProcessingState.Completed),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_LogsStartAndCompletion()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var records = new List<SourceRecord>
            {
                CreateSourceRecord(DataSource.Punchup)
            };

            A.CallTo(() => _mockRepository.GetRecordsByBatchAsync(batchId.ToString(), A<CancellationToken>._))
                .Returns(records);

            var mockSubProcessor = A.Fake<ISubProcessor<DataSource>>();
            A.CallTo(() => _mockSubProcessorResolver.Resolve(DataSource.Punchup, ProcessingState.Transformed, ProcessingState.Completed))
                .Returns(mockSubProcessor);

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Information)
                .MustHaveHappened(4, Times.OrMore); // Start, per-source, saved, completed
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_WhenExceptionThrown_LogsErrorAndRethrows()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var expectedException = new Exception("Test exception");

            A.CallTo(() => _mockRepository.GetRecordsByBatchAsync(batchId.ToString(), A<CancellationToken>._))
                .Throws(expectedException);

            // Act & Assert
            var act = async () => await _processor.ProcessBatchAsync(batchId, CancellationToken.None);
            await act.Should().ThrowAsync<Exception>().WithMessage("Test exception");

            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Error)
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_WithEmptyBatch_StillSavesAndPublishes()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var records = new List<SourceRecord>();

            A.CallTo(() => _mockRepository.GetRecordsByBatchAsync(batchId.ToString(), A<CancellationToken>._))
                .Returns(records);

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _mockMediator.Publish(
                A<StateCompletedEvent>.That.Matches(e =>
                    e.BatchId == batchId &&
                    e.CompletedState == ProcessingState.Completed),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        private static SourceRecord CreateSourceRecord(DataSource source)
        {
            return new SourceRecord
            {
                BatchId = Guid.NewGuid().ToString(),
                EntityType = EntityType.Act,
                RecordType = RecordType.PunchupTicketsPage,
                RawData = "{}",
                ProcessedData = "{}",
                State = ProcessingState.Transformed,
                Status = ProcessingStatus.Processing,
                Source = source,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Test",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "Test"
            };
        }
    }
}
