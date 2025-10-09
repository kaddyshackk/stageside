using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Processing;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ComedyPull.Application.Modules.DataProcessing.Steps.Complete;
using ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces;

namespace ComedyPull.Application.Tests.Modules.DataProcessing.Processors.SubProcessors
{
    [TestClass]
    public class CompleteStateGenericSubProcessorTests
    {
        private ICompleteStateRepository _mockRepository = null!;
        private ILogger<CompleteStateGenericSubProcessor> _mockLogger = null!;
        private CompleteStateGenericSubProcessor _subProcessor = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = A.Fake<ICompleteStateRepository>();
            _mockLogger = A.Fake<ILogger<CompleteStateGenericSubProcessor>>();

            _subProcessor = new CompleteStateGenericSubProcessor(_mockRepository, _mockLogger);
        }

        [TestMethod, TestCategory("Unit")]
        public void Key_ShouldBeNull()
        {
            // Assert
            _subProcessor.Key.Should().BeNull();
        }

        [TestMethod, TestCategory("Unit")]
        public void FromState_ShouldBeTransformed()
        {
            // Assert
            _subProcessor.FromState.Should().Be(ProcessingState.Transformed);
        }

        [TestMethod, TestCategory("Unit")]
        public void ToState_ShouldBeCompleted()
        {
            // Assert
            _subProcessor.ToState.Should().Be(ProcessingState.Completed);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithActEntity_CreatesComedianAndEvents()
        {
            // Arrange
            var processedData = new
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Comedian bio",
                Events = new[]
                {
                    new
                    {
                        Title = "Comedy Night",
                        StartDateTime = DateTimeOffset.UtcNow.AddDays(1),
                        EndDateTime = (DateTimeOffset?)null,
                        Location = "NYC",
                        Venue = "Comedy Club",
                        TicketLink = "http://tickets.com"
                    }
                }
            };

            var record = CreateSourceRecord(EntityType.Act, JsonSerializer.Serialize(processedData));

            A.CallTo(() => _mockRepository.GetComedianBySlugAsync("john-doe", A<CancellationToken>._))
                .Returns(Task.FromResult<Comedian?>(null));

            A.CallTo(() => _mockRepository.GetVenueBySlugAsync("comedy-club", A<CancellationToken>._))
                .Returns(Task.FromResult<Venue?>(null));

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.AddComedian(A<Comedian>.That.Matches(c =>
                c.Name == "John Doe" &&
                c.Slug == "john-doe" &&
                c.Bio == "Comedian bio")))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _mockRepository.AddVenue(A<Venue>.That.Matches(v =>
                v.Name == "Comedy Club" &&
                v.Slug == "comedy-club")))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _mockRepository.AddEvent(A<Event>.That.Matches(e =>
                e.Title == "Comedy Night" &&
                e.Status == EventStatus.Scheduled)))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _mockRepository.AddComedianEvent(A<ComedianEvent>._))
                .MustHaveHappenedOnceExactly();

            record.State.Should().Be(ProcessingState.Completed);
            record.Status.Should().Be(ProcessingStatus.Completed);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithExistingComedian_ReusesComedian()
        {
            // Arrange
            var existingComedian = new Comedian
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Existing bio",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "System"
            };

            var processedData = new
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Comedian bio",
                Events = Array.Empty<object>()
            };

            var record = CreateSourceRecord(EntityType.Act, JsonSerializer.Serialize(processedData));

            A.CallTo(() => _mockRepository.GetComedianBySlugAsync("john-doe", A<CancellationToken>._))
                .Returns(Task.FromResult<Comedian?>(existingComedian));

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.AddComedian(A<Comedian>._))
                .MustNotHaveHappened();

            record.State.Should().Be(ProcessingState.Completed);
            record.Status.Should().Be(ProcessingStatus.Completed);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithExistingVenue_ReusesVenue()
        {
            // Arrange
            var existingVenue = new Venue
            {
                Name = "Comedy Club",
                Slug = "comedy-club",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "System"
            };

            var processedData = new
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Comedian bio",
                Events = new[]
                {
                    new
                    {
                        Title = "Comedy Night",
                        StartDateTime = DateTimeOffset.UtcNow.AddDays(1),
                        EndDateTime = (DateTimeOffset?)null,
                        Location = "NYC",
                        Venue = "Comedy Club",
                        TicketLink = "http://tickets.com"
                    }
                }
            };

            var record = CreateSourceRecord(EntityType.Act, JsonSerializer.Serialize(processedData));

            A.CallTo(() => _mockRepository.GetComedianBySlugAsync("john-doe", A<CancellationToken>._))
                .Returns(Task.FromResult<Comedian?>(null));

            A.CallTo(() => _mockRepository.GetVenueBySlugAsync("comedy-club", A<CancellationToken>._))
                .Returns(Task.FromResult<Venue?>(existingVenue));

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.AddVenue(A<Venue>._))
                .MustNotHaveHappened();

            A.CallTo(() => _mockRepository.AddEvent(A<Event>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithMultipleEvents_CreatesAllEvents()
        {
            // Arrange
            var processedData = new
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Comedian bio",
                Events = new[]
                {
                    new
                    {
                        Title = "Comedy Night 1",
                        StartDateTime = DateTimeOffset.UtcNow.AddDays(1),
                        EndDateTime = (DateTimeOffset?)null,
                        Location = "NYC",
                        Venue = "Comedy Club 1",
                        TicketLink = "http://tickets1.com"
                    },
                    new
                    {
                        Title = "Comedy Night 2",
                        StartDateTime = DateTimeOffset.UtcNow.AddDays(2),
                        EndDateTime = (DateTimeOffset?)null,
                        Location = "LA",
                        Venue = "Comedy Club 2",
                        TicketLink = "http://tickets2.com"
                    }
                }
            };

            var record = CreateSourceRecord(EntityType.Act, JsonSerializer.Serialize(processedData));

            A.CallTo(() => _mockRepository.GetComedianBySlugAsync(A<string>._, A<CancellationToken>._))
                .Returns(Task.FromResult<Comedian?>(null));

            A.CallTo(() => _mockRepository.GetVenueBySlugAsync(A<string>._, A<CancellationToken>._))
                .Returns(Task.FromResult<Venue?>(null));

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.AddEvent(A<Event>._))
                .MustHaveHappened(2, Times.Exactly);

            A.CallTo(() => _mockRepository.AddComedianEvent(A<ComedianEvent>._))
                .MustHaveHappened(2, Times.Exactly);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithEmptyProcessedData_MarksRecordAsFailed()
        {
            // Arrange
            var record = CreateSourceRecord(EntityType.Act, "");

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            record.Status.Should().Be(ProcessingStatus.Failed);
            record.State.Should().NotBe(ProcessingState.Completed);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithNullProcessedData_MarksRecordAsFailed()
        {
            // Arrange
            var record = CreateSourceRecord(EntityType.Act, null);

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            record.Status.Should().Be(ProcessingStatus.Failed);
            record.State.Should().NotBe(ProcessingState.Completed);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithInvalidJson_MarksRecordAsFailed()
        {
            // Arrange
            var record = CreateSourceRecord(EntityType.Act, "invalid json {");

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            record.Status.Should().Be(ProcessingStatus.Failed);

            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Error)
                .MustHaveHappened();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithMissingComedianName_MarksRecordAsFailed()
        {
            // Arrange
            var processedData = new
            {
                Name = (string?)null,
                Slug = "john-doe",
                Bio = "Comedian bio",
                Events = Array.Empty<object>()
            };

            var record = CreateSourceRecord(EntityType.Act, JsonSerializer.Serialize(processedData));

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            record.Status.Should().Be(ProcessingStatus.Failed);
            record.State.Should().NotBe(ProcessingState.Completed);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithEventEntity_MarksRecordAsFailed()
        {
            // Arrange
            var record = CreateSourceRecord(EntityType.Event, "{}");

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            record.Status.Should().Be(ProcessingStatus.Failed);

            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Warning)
                .MustHaveHappened();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithVenueEntity_MarksRecordAsFailed()
        {
            // Arrange
            var record = CreateSourceRecord(EntityType.Venue, "{}");

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            record.Status.Should().Be(ProcessingStatus.Failed);

            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Warning)
                .MustHaveHappened();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithInvalidEventData_SkipsEvent()
        {
            // Arrange
            var processedData = new
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Comedian bio",
                Events = new[]
                {
                    new
                    {
                        Title = (string?)null, // Invalid - missing title
                        StartDateTime = DateTimeOffset.UtcNow.AddDays(1),
                        EndDateTime = (DateTimeOffset?)null,
                        Location = "NYC",
                        Venue = "Comedy Club",
                        TicketLink = "http://tickets.com"
                    }
                }
            };

            var record = CreateSourceRecord(EntityType.Act, JsonSerializer.Serialize(processedData));

            A.CallTo(() => _mockRepository.GetComedianBySlugAsync("john-doe", A<CancellationToken>._))
                .Returns(Task.FromResult<Comedian?>(null));

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.AddEvent(A<Event>._))
                .MustNotHaveHappened();

            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Warning)
                .MustHaveHappened();

            // Comedian should still be created
            A.CallTo(() => _mockRepository.AddComedian(A<Comedian>._))
                .MustHaveHappenedOnceExactly();

            record.State.Should().Be(ProcessingState.Completed);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WithMultipleRecords_ProcessesAllRecords()
        {
            // Arrange
            var processedData1 = new
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Bio 1",
                Events = Array.Empty<object>()
            };

            var processedData2 = new
            {
                Name = "Jane Smith",
                Slug = "jane-smith",
                Bio = "Bio 2",
                Events = Array.Empty<object>()
            };

            var record1 = CreateSourceRecord(EntityType.Act, JsonSerializer.Serialize(processedData1));
            var record2 = CreateSourceRecord(EntityType.Act, JsonSerializer.Serialize(processedData2));

            A.CallTo(() => _mockRepository.GetComedianBySlugAsync(A<string>._, A<CancellationToken>._))
                .Returns(Task.FromResult<Comedian?>(null));

            // Act
            await _subProcessor.ProcessAsync(new[] { record1, record2 }, CancellationToken.None);

            // Assert
            A.CallTo(() => _mockRepository.AddComedian(A<Comedian>._))
                .MustHaveHappened(2, Times.Exactly);

            record1.State.Should().Be(ProcessingState.Completed);
            record2.State.Should().Be(ProcessingState.Completed);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAsync_WhenRepositoryThrows_MarksRecordAsFailed()
        {
            // Arrange
            var processedData = new
            {
                Name = "John Doe",
                Slug = "john-doe",
                Bio = "Comedian bio",
                Events = Array.Empty<object>()
            };

            var record = CreateSourceRecord(EntityType.Act, JsonSerializer.Serialize(processedData));

            A.CallTo(() => _mockRepository.GetComedianBySlugAsync("john-doe", A<CancellationToken>._))
                .Throws<Exception>();

            // Act
            await _subProcessor.ProcessAsync(new[] { record }, CancellationToken.None);

            // Assert
            record.Status.Should().Be(ProcessingStatus.Failed);

            A.CallTo(_mockLogger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Error)
                .MustHaveHappened();
        }

        private static SourceRecord CreateSourceRecord(EntityType entityType, string? processedData)
        {
            return new SourceRecord
            {
                BatchId = Guid.NewGuid().ToString(),
                EntityType = entityType,
                RecordType = RecordType.PunchupTicketsPage,
                RawData = "{}",
                ProcessedData = processedData,
                State = ProcessingState.Transformed,
                Status = ProcessingStatus.Processing,
                Source = DataSource.Punchup,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Test",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "Test"
            };
        }
    }
}
