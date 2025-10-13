using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Modules.Punchup.Collectors;
using ComedyPull.Domain.Modules.DataProcessing;
using FakeItEasy;
using FluentAssertions;

namespace ComedyPull.Application.Tests.Modules.Punchup.Factories
{
    [TestClass]
    public class PunchupTicketsPageCollectorFactoryTests
    {
        [TestMethod, TestCategory("Unit")]
        public void CreateCollector_ReturnsPunchupTicketsPageCollectorWithQueue()
        {
            // Arrange
            var mockQueue = A.Fake<IQueue<BronzeRecord>>();
            var factory = new PunchupTicketsPageCollectorFactory(mockQueue);

            // Act
            var collector = factory.CreateCollector("test-batch-id");

            // Assert
            collector.Should().NotBeNull();
            collector.Should().BeOfType<PunchupTicketsPageCollector>();
        }

        [TestMethod, TestCategory("Unit")]
        public void CreateCollector_ReturnsNewInstanceEachTime()
        {
            // Arrange
            var mockQueue = A.Fake<IQueue<BronzeRecord>>();
            var factory = new PunchupTicketsPageCollectorFactory(mockQueue);

            // Act
            var collector1 = factory.CreateCollector("test-batch-id-1");
            var collector2 = factory.CreateCollector("test-batch-id-2");

            // Assert
            collector1.Should().NotBeSameAs(collector2);
        }
    }
}