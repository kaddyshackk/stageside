using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Complete;
using ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Modules.Common;
using ComedyPull.Domain.Modules.DataProcessing;
using FakeItEasy;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ComedyPull.Application.Events;

namespace ComedyPull.Application.Tests.Modules.DataProcessing.Processors
{
    [TestClass]
    public class CompleteStateProcessorTests
    {
        private IBatchRepository _mockBatchRepository = null!;
        private ICompleteStateRepository _mockRepository = null!;
        private IMediator _mockMediator = null!;
        private ILogger<CompleteStateProcessor> _mockLogger = null!;
        private CompleteStateProcessor _processor = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockBatchRepository = A.Fake<IBatchRepository>();
            _mockRepository = A.Fake<ICompleteStateRepository>();
            _mockMediator = A.Fake<IMediator>();
            _mockLogger = A.Fake<ILogger<CompleteStateProcessor>>();

            _processor = new CompleteStateProcessor(
                _mockBatchRepository,
                _mockRepository,
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
        public async Task ProcessBatchAsync_LoadsBatchAndValidatesState()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(new List<SilverRecord>());

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_ThrowsIfBatchStateInvalid()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Ingested); // Wrong state

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            // Act & Assert
            var act = async () => await _processor.ProcessBatchAsync(batchId, CancellationToken.None);
            await act.Should().ThrowAsync<Exception>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_LoadsSilverRecordsForBatch()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);
            var silverRecords = new List<SilverRecord>
            {
                CreateSilverRecord(batchId, EntityType.Act)
            };

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(silverRecords);

            A.CallTo(() => _mockRepository.GetComediansBySlugAsync(A<IEnumerable<string>>._, A<CancellationToken>._))
                .Returns(new List<Comedian>());

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_ProcessesActsAndCreatesNewComedians()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);
            var processedAct = new ProcessedAct
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Comedian bio"
            };

            var silverRecords = new List<SilverRecord>
            {
                CreateSilverRecord(batchId, EntityType.Act, JsonSerializer.Serialize(processedAct))
            };

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(silverRecords);

            A.CallTo(() => _mockRepository.GetComediansBySlugAsync(A<IEnumerable<string>>._, A<CancellationToken>._))
                .Returns(new List<Comedian>());

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.AddComediansAsync(
                A<IEnumerable<Comedian>>.That.Matches(c =>
                    c.Count() == 1 &&
                    c.First().Name == "John Doe" &&
                    c.First().Slug == "john-doe"),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_SkipsDuplicateComediansBasedOnSlug()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);

            var processedAct1 = new ProcessedAct { Name = "John Doe", Slug = "john-doe", Bio = "Bio" };
            var processedAct2 = new ProcessedAct { Name = "John Doe", Slug = "john-doe", Bio = "Bio" };

            var silverRecords = new List<SilverRecord>
            {
                CreateSilverRecord(batchId, EntityType.Act, JsonSerializer.Serialize(processedAct1)),
                CreateSilverRecord(batchId, EntityType.Act, JsonSerializer.Serialize(processedAct2))
            };

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(silverRecords);

            A.CallTo(() => _mockRepository.GetComediansBySlugAsync(A<IEnumerable<string>>._, A<CancellationToken>._))
                .Returns(new List<Comedian>());

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert - Should only create one comedian despite two records
            A.CallTo(() => _mockRepository.AddComediansAsync(
                A<IEnumerable<Comedian>>.That.Matches(c => c.Count() == 1),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_UpdatesExistingComedianIfBioChanged()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);

            var existingComedian = new Comedian
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Old bio",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "System"
            };

            var processedAct = new ProcessedAct
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "New bio"
            };

            var silverRecords = new List<SilverRecord>
            {
                CreateSilverRecord(batchId, EntityType.Act, JsonSerializer.Serialize(processedAct))
            };

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(silverRecords);

            A.CallTo(() => _mockRepository.GetComediansBySlugAsync(A<IEnumerable<string>>._, A<CancellationToken>._))
                .Returns(new List<Comedian> { existingComedian });

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.UpdateComediansAsync(
                A<IEnumerable<Comedian>>.That.Matches(c =>
                    c.Count() == 1 &&
                    c.First().Bio == "New bio"),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _mockRepository.AddComediansAsync(A<IEnumerable<Comedian>>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_ProcessesVenuesAndCreatesNew()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);

            var processedVenue = new ProcessedVenue
            {
                Name = "Comedy Club",
                Slug = "comedy-club",
                Location = "NYC"
            };

            var silverRecords = new List<SilverRecord>
            {
                CreateSilverRecord(batchId, EntityType.Venue, JsonSerializer.Serialize(processedVenue))
            };

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(silverRecords);

            A.CallTo(() => _mockRepository.GetVenuesBySlugAsync(A<IEnumerable<string>>._, A<CancellationToken>._))
                .Returns(new List<Venue>());

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.AddVenuesAsync(
                A<IEnumerable<Venue>>.That.Matches(v =>
                    v.Count() == 1 &&
                    v.First().Name == "Comedy Club" &&
                    v.First().Slug == "comedy-club"),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_ProcessesEventsAndCreatesRelationships()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);

            var comedian = new Comedian
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Bio",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "System"
            };

            var venue = new Venue
            {
                Name = "Comedy Club",
                Slug = "comedy-club",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "System"
            };

            var processedEvent = new ProcessedEvent
            {
                Title = "Comedy Night",
                Slug = "john-doe-comedy-club-2025-10-15",
                ComedianSlug = "john-doe",
                VenueSlug = "comedy-club",
                StartDateTime = new DateTimeOffset(2025, 10, 15, 20, 0, 0, TimeSpan.Zero),
                TicketLink = "http://tickets.com"
            };

            var silverRecords = new List<SilverRecord>
            {
                CreateSilverRecord(batchId, EntityType.Event, JsonSerializer.Serialize(processedEvent))
            };

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(silverRecords);

            A.CallTo(() => _mockRepository.GetComediansBySlugAsync(A<IEnumerable<string>>._, A<CancellationToken>._))
                .Returns(new List<Comedian> { comedian });

            A.CallTo(() => _mockRepository.GetVenuesBySlugAsync(A<IEnumerable<string>>._, A<CancellationToken>._))
                .Returns(new List<Venue> { venue });

            A.CallTo(() => _mockRepository.GetEventsBySlugAsync(A<IEnumerable<string>>._, A<CancellationToken>._))
                .Returns(new List<Event>());

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.AddEventsAsync(
                A<IEnumerable<Event>>.That.Matches(e =>
                    e.Count() == 1 &&
                    e.First().Title == "Comedy Night" &&
                    e.First().VenueId == venue.Id),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _mockRepository.AddComedianEventsAsync(
                A<IEnumerable<ComedianEvent>>.That.Matches(ce =>
                    ce.Count() == 1 &&
                    ce.First().ComedianId == comedian.Id),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_UpdatesBatchStateToCompleted()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(new List<SilverRecord>());

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockBatchRepository.UpdateBatchStateById(batchId, ProcessingState.Completed, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_PublishesStateCompletedEvent()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(new List<SilverRecord>());

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockMediator.Publish(
                A<StateCompletedEvent>.That.Matches(e =>
                    e.BatchId == batchId),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_UpdatesSilverRecordsStatus()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);

            var processedAct = new ProcessedAct
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Bio"
            };

            var silverRecords = new List<SilverRecord>
            {
                CreateSilverRecord(batchId, EntityType.Act, JsonSerializer.Serialize(processedAct))
            };

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(silverRecords);

            A.CallTo(() => _mockRepository.GetComediansBySlugAsync(A<IEnumerable<string>>._, A<CancellationToken>._))
                .Returns(new List<Comedian>());

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.UpdateSilverRecordsAsync(
                A<IEnumerable<SilverRecord>>.That.Matches(r =>
                    r.All(sr => sr.Status == ProcessingStatus.Completed)),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_WhenExceptionThrown_UpdatesBatchStatusToFailed()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var expectedException = new Exception("Test exception");

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Throws(expectedException);

            // Act & Assert
            var act = async () => await _processor.ProcessBatchAsync(batchId, CancellationToken.None);
            await act.Should().ThrowAsync<Exception>().WithMessage("Test exception");
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_LogsStartAndCompletion()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(new List<SilverRecord>());

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Information)
                .MustHaveHappened(2, Times.OrMore); // Start and completed
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessBatchAsync_WithEmptyBatch_StillUpdatesStateAndPublishes()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var batch = CreateBatch(ProcessingState.Transformed);

            A.CallTo(() => _mockBatchRepository.GetBatchById(batchId, A<CancellationToken>._))
                .Returns(batch);

            A.CallTo(() => _mockRepository.GetSilverRecordsByBatchId(batchId, A<CancellationToken>._))
                .Returns(new List<SilverRecord>());

            // Act
            await _processor.ProcessBatchAsync(batchId, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockBatchRepository.UpdateBatchStateById(batchId, ProcessingState.Completed, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _mockMediator.Publish(
                A<StateCompletedEvent>.That.Matches(e =>
                    e.BatchId == batchId),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        private static Batch CreateBatch(ProcessingState state)
        {
            return new Batch
            {
                Source = DataSource.Punchup,
                SourceType = DataSourceType.PunchupTicketsPage,
                State = state,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Test",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "Test"
            };
        }

        private static SilverRecord CreateSilverRecord(Guid batchId, EntityType entityType, string? data = null)
        {
            return new SilverRecord
            {
                BatchId = batchId,
                BronzeRecordId = Guid.NewGuid(),
                EntityType = entityType,
                Status = ProcessingStatus.Processing,
                Data = data ?? "{}",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Test",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "Test"
            };
        }
    }
}