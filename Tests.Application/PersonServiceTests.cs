using Application.Interfaces;
using Application.Services;
using Domain.Entities.Person;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Xunit;

namespace Tests.Application
{
    public class PersonServiceTests
    {
        private readonly Mock<IPersonRepository> _repoMock;
        private readonly Mock<IValidator<Person>> _validatorMock;
        private readonly PersonService _service;

        public PersonServiceTests()
        {
            _repoMock = new Mock<IPersonRepository>();
            _validatorMock = new Mock<IValidator<Person>>();
            _service = new PersonService(_repoMock.Object, _validatorMock.Object);
        }

        private Person CreateSamplePerson(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = "Ali",
            LastName = "Md",
            NationalCode = "1234567890",
            BirthDate = DateTime.UtcNow.AddYears(-30)
        };

        [Fact]
        public async Task CreatePerson_Should_Add_Person_When_Valid()
        {
            var person = CreateSamplePerson();

            _validatorMock
                .Setup(v => v.ValidateAsync(person, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _repoMock
                .Setup(r => r.GetByNationalCodeAsync(person.NationalCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Person?)null);

            _repoMock
                .Setup(r => r.CreateAsync(person, It.IsAny<CancellationToken>()))
                .ReturnsAsync(person);

            var result = await _service.CreatePerson(person, CancellationToken.None);

            result.Should().BeEquivalentTo(person);
            _repoMock.Verify(r => r.CreateAsync(person, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePerson_ShouldThrowValidationException_WhenValidationFails()
        {
            var person = new Person { FirstName = "", LastName = "Md", NationalCode = "1234567890" };
            var failures = new List<ValidationFailure> { new ValidationFailure("FirstName", "Required") };
            _validatorMock.Setup(v => v.ValidateAsync(person, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            await Assert.ThrowsAsync<ValidationException>(() => _service.CreatePerson(person));
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreatePerson_ShouldThrowInvalidOperationException_WhenNationalCodeExists()
        {
            var person = new Person { FirstName = "Ali", LastName = "Md", NationalCode = "1234567890" };
            _validatorMock.Setup(v => v.ValidateAsync(person, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _repoMock.Setup(r => r.GetByNationalCodeAsync(person.NationalCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Person { Id = Guid.NewGuid(), NationalCode = "1234567890" });

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePerson(person));
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
        }


        [Fact]
        public async Task UpdatePerson_ShouldReturnUpdatedPerson_WhenPersonExists()
        {
            var person = new Person { Id = Guid.NewGuid(), FirstName = "Ali", LastName = "Md", NationalCode = "1234567890" };
            _validatorMock.Setup(v => v.ValidateAsync(person, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repoMock.Setup(r => r.GetByIdAsync(person.Id, It.IsAny<CancellationToken>())).ReturnsAsync(person);
            _repoMock.Setup(r => r.UpdateAsync(person, It.IsAny<CancellationToken>())).ReturnsAsync(person);

            var result = await _service.UpdatePerson(person);

            result.Should().BeEquivalentTo(person);
        }


        [Fact]
        public async Task DeletePerson_ShouldReturnTrue_WhenPersonExists()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new Person { Id = id });
            _repoMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _service.DeletePerson(id);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeletePerson_ShouldThrowInvalidOperationException_WhenPersonDoesNotExist()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Person?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeletePerson(id));
        }


        [Fact]
        public async Task GetPerson_ShouldReturnPerson_WhenExists()
        {
            var person = new Person { Id = Guid.NewGuid(), FirstName = "Ali" };
            _repoMock.Setup(r => r.GetByIdAsync(person.Id, It.IsAny<CancellationToken>())).ReturnsAsync(person);

            var result = await _service.GetPerson(person.Id);
            result.Should().BeEquivalentTo(person);
        }

        [Fact]
        public async Task GetAll_ShouldReturnEmptyList_WhenNoPersons()
        {
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Person>());

            var result = await _service.GetAll();
            result.Should().BeEmpty();
        }

    }
}
