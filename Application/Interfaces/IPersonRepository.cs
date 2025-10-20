using Domain.Entities.Person;


namespace Application.Interfaces
{
    public interface IPersonRepository
    {
        Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Person?> GetByNationalCodeAsync(string nationalCode, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<Person>?> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Person> CreateAsync(Person person, CancellationToken cancellationToken = default);
        Task<Person?> UpdateAsync(Person person, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
