using Domain.Entities.Person;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPersonService
    {
        public Task<Person?> GetPerson(Guid id, CancellationToken cancellationToken = default);
        public Task<Person> CreatePerson(Person person, CancellationToken cancellationToken = default);
        public Task<Person?> UpdatePerson(Person person, CancellationToken cancellationToken = default);
        public Task<bool> DeletePerson(Guid id, CancellationToken cancellationToken = default);
        public Task<IReadOnlyCollection<Person>?> GetAll(CancellationToken cancellationToken = default);

    }
}
