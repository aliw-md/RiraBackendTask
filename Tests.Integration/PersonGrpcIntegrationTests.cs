
using EndPoint.Grpc;
using EndPoint.Grpc.Protos;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests.Integration
{
    public class PersonGrpcIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly GrpcChannel _channel;
        private readonly PersonProtoService.PersonProtoServiceClient _client;
        private readonly string _dataFilePath;

        #region Constructor
        public PersonGrpcIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureKestrel(options =>
                {
                    // Force HTTP/2 without TLS on localhost
                    options.ListenLocalhost(5001, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
                });
            });

            var httpClient = _factory.CreateDefaultClient(new Uri("http://localhost:5001"));
            _channel = GrpcChannel.ForAddress("http://localhost:5001", new GrpcChannelOptions { HttpClient = httpClient });
            _client = new PersonProtoService.PersonProtoServiceClient(_channel);
            _dataFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "persons.json");

        }
        #endregion

        #region GetPerson test
        [Fact]
        public async Task GetPerson_ShouldReturnExistingPerson()
        {
            //Arrange
            var person = new PersonMessage
            {
                FirstName = "Kaveh",
                LastName = "Ahmadi",
                NationalCode = "1234555555",
                BirthDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddYears(-22))
            };

            //Act
            var created = await _client.CreatePersonAsync(new CreatePersonRequest { Person = person });
            var fetched = await _client.GetPersonAsync(new GetPersonRequest { Id = created.Person.Id });

            //Assert
            fetched.Person.Id.Should().Be(created.Person.Id);
            fetched.Person.FirstName.Should().Be("Sara");
        }
        #endregion

        #region DeletePerson Test
        [Fact]
        public async Task DeletePerson_ShouldRemoveSuccessfully()
        {
            //Arrange
            var person = new PersonMessage
            {
                FirstName = "DeleteMe",
                LastName = "Test",
                NationalCode = "1234555555",
                BirthDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddYears(-40))
            };

            //Act
            var created = await _client.CreatePersonAsync(new CreatePersonRequest { Person = person });
            var deleted = await _client.DeletePersonAsync(new DeletePersonRequest { Id = created.Person.Id });

            //Assert
            deleted.Success.Should().BeTrue();
        }
        #endregion

        #region CreatePerson tests
        [Fact]
        public async Task CreatePerson_ShouldSucceed()
        {
            //Arrange
            var person = new PersonMessage
            {
                FirstName = "Ali",
                LastName = "Test",
                NationalCode = "1234555555",
                BirthDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddYears(-25))
            };

            //ACt
            var response = await _client.CreatePersonAsync(new CreatePersonRequest { Person = person });

            //Assert
            response.Should().NotBeNull();
            response.Person.FirstName.Should().Be("Ali");
        }

        [Theory]
        [InlineData("", "ValidLastName", "1234567890")]   // FirstName خالی
        [InlineData("Kambiz", "", "1234567890")]           // LastName خالی
        [InlineData("Jay", "Son", "12345")]            // NationalCode کمتر از 10 رقم
        [InlineData("Amir", "mmd", "1234567890123")]    // NationalCode بیشتر از 10 رقم
        [InlineData("Mahi", "mmd", "aaabbbcccd")]       // NationalCode حروف
        public async Task CreatePerson_ShouldThrowInvalidArgument_WhenInputInvalid(
                    string firstName, string lastName, string nationalCode)
        {
            // Arrange
            var personMessage = new PersonMessage
            {
                FirstName = firstName,
                LastName = lastName,
                NationalCode = nationalCode,
                BirthDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddYears(-25))
            };

            //Act
            Func<Task> act = async () =>
            {
                await _client.CreatePersonAsync(new CreatePersonRequest { Person = personMessage });
            };

            // Assert
            var ex = await Assert.ThrowsAsync<RpcException>(act);
            ex.StatusCode.Should().Be(StatusCode.InvalidArgument);
        }

        #endregion

        #region UpdatePerson tests
        [Fact]
        public async Task UpdatePerson_ShouldUpdateSuccessfully()
        {
            //Arrange
            var person = new PersonMessage
            {
                FirstName = "Reza",
                LastName = "Miri",
                NationalCode = "1234555555",
                BirthDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddYears(-28))
            };

            //Act
            var created = await _client.CreatePersonAsync(new CreatePersonRequest { Person = person });

            created.Person.FirstName = "RezaUpdated";
            var updated = await _client.UpdatePersonAsync(new UpdatePersonRequest { Person = created.Person });

            //Assert
            updated.Person.FirstName.Should().Be("RezaUpdated");
        }

        [Fact]
        public async Task UpdatePerson_ShouldThrowNotFound_WhenPersonNotExists()
        {
            //Arrange
            var person = new PersonMessage
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Ghost",
                LastName = "User",
                NationalCode = "9999999999",
                BirthDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
            };
            //Act
            Func<Task> act = async () =>
                await _client.UpdatePersonAsync(new UpdatePersonRequest { Person = person });
            //Assert
            var ex = await Assert.ThrowsAsync<RpcException>(act);
            ex.StatusCode.Should().Be(StatusCode.NotFound);
        }

        #endregion

        public void Dispose()
        {
            _channel?.Dispose();
            _factory?.Dispose();

            // حذف فایل دیتا بعد از پایان تست
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    File.Delete(_dataFilePath);
                }

                // اگر پوشه Data خالی شد، می‌توانیم حذفش کنیم
                var dataDir = Path.GetDirectoryName(_dataFilePath);
                if (dataDir != null && Directory.Exists(dataDir) && Directory.GetFiles(dataDir).Length == 0)
                {
                    Directory.Delete(dataDir);
                }
            }
            catch
            {
                // اگر نتوانستیم حذف کنیم، صرفاً نادیده بگیریم تا تست کرش نکند
            }
        }
    }
}
