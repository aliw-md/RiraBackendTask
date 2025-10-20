using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using EndPoint.Grpc.Protos;
using System.Globalization;
using Grpc.Core;
using System;

namespace EndPoint.ConsoleApp.Services
{
    public class PersonGrpcClient
    {
        private readonly PersonProtoService.PersonProtoServiceClient _client;

        public PersonGrpcClient(string grpcServerAddress)
        {
            var channel = GrpcChannel.ForAddress(grpcServerAddress);

            _client = new PersonProtoService.PersonProtoServiceClient(channel);
        }

        public async Task GetAllPersonsAsync()
        {
            var response = await _client.GetAllPersonsAsync(new GetAllPersonsRequest());

            if (response.Persons.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("No registered persons found.");
                return;
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("_______________________________________________________________________________________________");
            Console.WriteLine($"{"ID",-36} | {"First Name",-12} | {"Last Name",-12} | {"National Code",-12}");
            Console.WriteLine("_______________________________________________________________________________________________");

            foreach (var person in response.Persons)
            {
                Console.WriteLine($"{person.Id,-36} | {person.FirstName,-12} | {person.LastName,-12} | {person.NationalCode,-12}");
            }

            Console.WriteLine("_______________________________________________________________________________________________");
        }

        public async Task GetPersonByIdAsync()
        {
            Console.Write("Enter Person ID: ");
            var id = Console.ReadLine();
            var request = new GetPersonRequest { Id = id };

            var response = await _client.GetPersonAsync(request);
            var person = response.Person;

            if (person == null)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("No person found with the given ID.");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"{"Field",-20} | {"Value",-30}");
            Console.WriteLine("------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"{"ID",-20} | {person.Id,-30}");
            Console.WriteLine($"{"First Name",-20} | {person.FirstName,-30}");
            Console.WriteLine($"{"Last Name",-20} | {person.LastName,-30}");
            Console.WriteLine($"{"National Code",-20} | {person.NationalCode,-30}");
            Console.WriteLine($"{"Birth Date",-20} | {person.BirthDate.ToDateTime():yyyy-MM-dd,-30}");
            Console.WriteLine("------------------------------------------------------------------------------------------------------");
        }

        public async Task CreatePersonAsync()
        {
            Console.Write("First Name: ");
            var firstName = Console.ReadLine();
            Console.Write("Last Name: ");
            var lastName = Console.ReadLine();
            Console.Write("National Code: ");
            var nationalCode = Console.ReadLine();
            Console.Write("Birth Date (yyyy-MM-dd): ");
            var birthDateStr = Console.ReadLine();

            if (!DateTime.TryParseExact(birthDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var birthDate))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Failed to convert BirthDate field!!"));
            }
            else
            {
                var request = new CreatePersonRequest
                {
                    Person = new PersonMessage
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        NationalCode = nationalCode,
                        BirthDate = birthDate.ToUniversalTime().ToTimestamp()
                    }
                };

                var response = await _client.CreatePersonAsync(request);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Person created successfully with ID: {response.Person.Id}");
            }
        }

        public async Task UpdatePersonAsync()
        {
            Console.Write("Enter Person ID to update: ");
            var id = Console.ReadLine();
            Console.Write("New First Name: ");
            var firstName = Console.ReadLine();
            Console.Write("New Last Name: ");
            var lastName = Console.ReadLine();
            Console.Write("New National Code: ");
            var nationalCode = Console.ReadLine();
            Console.Write("New Birth Date (yyyy-MM-dd): ");
            var birthDateStr = Console.ReadLine();

            if(!DateTime.TryParseExact(birthDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture,DateTimeStyles.AdjustToUniversal,out var birthDate))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Failed to convert BirthDate field!!"));
            }

            var request = new UpdatePersonRequest
            {
                Person = new PersonMessage
                {
                    Id = id,
                    FirstName = firstName,
                    LastName = lastName,
                    NationalCode = nationalCode,
                    BirthDate = birthDate.ToUniversalTime().ToTimestamp()
                }
            };

            var response = await _client.UpdatePersonAsync(request);
            Console.WriteLine("Person has been successfully updated.");
        }

        public async Task DeletePersonAsync()
        {
            Console.Write("Enter Person ID to delete: ");
            var id = Console.ReadLine();

            var response = await _client.DeletePersonAsync(new DeletePersonRequest { Id = id });

            if (response.Success)
                Console.WriteLine("Person has been successfully deleted.");
            else
                Console.WriteLine($"Failed to delete person. Details: {response.Message}");
        }
    }
}
