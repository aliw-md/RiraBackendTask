using Application.Interfaces;
using Application.Validation;
using Domain.Entities.Person;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IValidator<Person> _validator;

        public PersonService(IPersonRepository personRepository, IValidator<Person> validator)
        {
            _personRepository = personRepository;
            _validator = validator;
        }

        public async Task<Person> CreatePerson(Person person, CancellationToken ct = default)
        {
            ValidationResult validationResult = await _validator.ValidateAsync(person);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            if (await _personRepository.GetByNationalCodeAsync(person.NationalCode) != null)
                throw new InvalidOperationException("Another person with this national code already exists.");

            return await _personRepository.CreateAsync(person, ct);
        }

        public async Task<Person?> GetPerson(Guid id, CancellationToken ct = default)
        {
            return await _personRepository.GetByIdAsync(id,ct);
        }

        public async Task<IReadOnlyCollection<Person>?> GetAll(CancellationToken ct = default)
        {
            return await _personRepository.GetAllAsync(ct);
        }

        public async Task<Person?> UpdatePerson(Person person, CancellationToken ct = default)
        {
            ValidationResult validationResult = await _validator.ValidateAsync(person);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var existing = await _personRepository.GetByIdAsync(person.Id, ct);
            if (existing == null)
                throw new InvalidOperationException("Person not found.");

            if (existing.NationalCode != person.NationalCode)
            {
                var all = await _personRepository.GetAllAsync(ct);

                if (all!.Where(x => x.NationalCode == person.NationalCode).Any())
                    throw new InvalidOperationException("Another person with this national code already exists.");
            }

            return await _personRepository.UpdateAsync(person, ct);
        }

        public async Task<bool> DeletePerson(Guid id, CancellationToken ct = default)
        {
            var existing = await _personRepository.GetByIdAsync(id, ct);
            if (existing == null)
                throw new InvalidOperationException("Person not found.");

            return await _personRepository.DeleteAsync(id, ct);
        }
    }
}
