using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Blockcore.AtomicSwaps.Server;
using Blockcore.AtomicSwaps.Server.Services;

namespace Blockcore.AtomicSwaps.Test
{
    using Xunit;
    using Moq;
    using System.Linq;
    using System.Threading.Tasks;
    using Blockcore.AtomicSwaps.Server.Controllers;
    using Blockcore.AtomicSwaps.Shared;
    using Microsoft.Extensions.Options;

    public class StorageServiceTests
    {
        //[Fact]
        public async Task Get_ShouldReturnAllAvailableAndInProgressSwaps()
        {
            // Arrange
            var options = new DataConfigOptions { DirectoryPath = "tests" };

            var op = new Mock<IOptions<DataConfigOptions>>();
            op.Setup(r => r.Value).Returns(options);

            var storageService = new StorageService(op.Object);

            // Act
            var result = await storageService.Get();

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains(result, s => s.Status == SwapsDataStatus.Available || s.Status == SwapsDataStatus.InProgress);
            Assert.Equal(result.OrderByDescending(s => s.Created), result);
        }

        //[Fact]
        public async Task Get_WithSessionId_ShouldReturnMatchingSwapSession()
        {
            // Arrange
            var data = new DataConfigOptions { DirectoryPath = "tests" };
            var options = new Mock<IOptions<DataConfigOptions>>();
            options.Setup(r => r.Value).Returns(data);

            var storageService = new StorageService(options.Object);
            var sessionId = "session123";

            // Act
            var result = await storageService.Get(sessionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sessionId, result.SwapSessionId);
        }

        //[Fact]
        public async Task Add_ShouldAddSwapSessionToDatabase()
        {
            // Arrange
            var data = new DataConfigOptions { DirectoryPath = "tests" };
            var options = new Mock<IOptions<DataConfigOptions>>();
            options.Setup(r => r.Value).Returns(data);

            var storageService = new StorageService(options.Object);
            var swap = new SwapSession { SwapSessionId = "session123" };

            // Act
            await storageService.Add(swap);

            // Assert
            // assert that swap session is in the database
        }

        //[Fact]
        public async Task Update_ShouldUpdateSwapSessionInDatabase()
        {
            // Arrange
            var data = new DataConfigOptions { DirectoryPath = "tests" };
            var options = new Mock<IOptions<DataConfigOptions>>();
            options.Setup(r => r.Value).Returns(data);

            var storageService = new StorageService(options.Object);
            var swap = new SwapSession { SwapSessionId = "session123" };

            // Act
            await storageService.Update(swap);

            // Assert
            // assert that swap session in the database has been updated
        }

        //[Fact]
        public async Task Complete_ShouldMarkSwapSessionAsCompleteInDatabase()
        {
            // Arrange
            var data = new DataConfigOptions { DirectoryPath = "tests" };
            var options = new Mock<IOptions<DataConfigOptions>>();
            options.Setup(r => r.Value).Returns(data);

            var storageService = new StorageService(options.Object);
            var swap = new SwapSession { SwapSessionId = "session123" };

            // Act
            await storageService.Complete(swap);

            // Assert
            // assert that swap session in the database has status of Complete
        }
    }
}
