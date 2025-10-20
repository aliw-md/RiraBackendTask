using Application.Interfaces;
using Domain.Entities.Person;
using System.Text.Json;

namespace Persistence.Repository
{
    public class FilePersonRepository : IPersonRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public FilePersonRepository()
        {
            string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "persons.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (!File.Exists(_filePath))
            {
                var seedData = GetSeedData();
                var jsonSeed = JsonSerializer.Serialize(seedData, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllTextAsync(_filePath, "[]");
            }
        }

        private async Task<List<Person>> LoadDataAsync(CancellationToken cancellationToken = default)
        {
            string json = await File.ReadAllTextAsync(_filePath, cancellationToken);
            return JsonSerializer.Deserialize<List<Person>>(json, _jsonOptions) ?? new List<Person>();
        }

        private async Task SaveDataAsync(List<Person> persons, CancellationToken cancellationToken = default)
        {
            string json = JsonSerializer.Serialize(persons, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json, cancellationToken);
        }

        public async Task<Person> CreateAsync(Person person, CancellationToken cancellationToken = default)
        {
            var persons = await LoadDataAsync(cancellationToken);
            persons.Add(person);
            await SaveDataAsync(persons, cancellationToken);
            return person;
        }

        public async Task<IReadOnlyCollection<Person>?> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var persons = await LoadDataAsync(cancellationToken);
            return persons;
        }

        public async Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var persons = await LoadDataAsync(cancellationToken);
            var person = persons.FirstOrDefault(p => p.Id == id);
            return person;
        }

        public async Task<Person?> UpdateAsync(Person person, CancellationToken cancellationToken = default)
        {
            var persons = await LoadDataAsync(cancellationToken);
            var existing = persons.FirstOrDefault(p => p.Id == person.Id);
            if (existing is null)
                return null;

            existing.FirstName = person.FirstName;
            existing.LastName = person.LastName;
            existing.NationalCode = person.NationalCode;
            existing.BirthDate = person.BirthDate;

            await SaveDataAsync(persons, cancellationToken);
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var persons = await LoadDataAsync(cancellationToken);
            var person = persons.FirstOrDefault(p => p.Id == id);
            if (person is null)
                return false;

            persons.Remove(person);
            await SaveDataAsync(persons, cancellationToken);
            return true;
        }

        public async Task<Person?> GetByNationalCodeAsync(string nationalCode, CancellationToken cancellationToken = default)
        {
            var persons = await LoadDataAsync(cancellationToken);
            var person = persons.FirstOrDefault(p => p.NationalCode == nationalCode);
            return person;

        }

        private static List<Person> GetSeedData()
        {
            return new List<Person>
        {
            new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "ali",
                LastName = "mmdoost",
                NationalCode = "2110866666",
                BirthDate = DateTime.Parse("2000-08-31T19:30:00Z").ToUniversalTime()
            },
            new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "nazanain",
                LastName = "akbari",
                NationalCode = "2120866666",
                BirthDate = DateTime.Parse("2002-08-31T19:30:00Z").ToUniversalTime()
            }
        };
        }
    }
}
