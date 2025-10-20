using Application.Interfaces;
using Domain.Entities.Person;
using EndPoint.Grpc.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EndPoint.Grpc.Services
{
    public class PersonGrpcService : PersonProtoService.PersonProtoServiceBase
    {
        private readonly IPersonService _service;

        public PersonGrpcService(IPersonService service)
        {
            _service = service;
        }

        #region Mapping

        private static PersonMessage ToMessage(Person p)
        {
            return new PersonMessage
            {
                Id = p.Id.ToString(),
                FirstName = p.FirstName,
                LastName = p.LastName,
                NationalCode = p.NationalCode,
                BirthDate = Timestamp.FromDateTime(p.BirthDate.ToUniversalTime())
            };
        }

        private static Person FromMessage(PersonMessage msg)
        {
            return new Person
            {
                Id = string.IsNullOrWhiteSpace(msg.Id) ? Guid.NewGuid() : Guid.Parse(msg.Id),
                FirstName = msg.FirstName,
                LastName = msg.LastName,
                NationalCode = msg.NationalCode,
                BirthDate = msg.BirthDate.ToDateTime()
            };
        }

        #endregion

        #region CRUD Operations

        public override async Task<CreatePersonResponse> CreatePerson(CreatePersonRequest request, ServerCallContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (request.Person == null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Person data is required"));

            var person = FromMessage(request.Person);

            var created = await _service.CreatePerson(person);
            return new CreatePersonResponse { Person = ToMessage(created) };

        }

        public override async Task<GetPersonResponse> GetPerson(GetPersonRequest request, ServerCallContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!Guid.TryParse(request.Id, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Person ID"));

            var p = await _service.GetPerson(id);
            if (p == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Person not found"));

            return new GetPersonResponse { Person = ToMessage(p) };
        }

        public override async Task<UpdatePersonResponse> UpdatePerson(UpdatePersonRequest request, ServerCallContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (request.Person == null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Person data is required"));

            var person = FromMessage(request.Person);
            var existing = await _service.GetPerson(person.Id);

            if (existing == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Person not found"));

            // Apply changes (mapping)
            existing.FirstName = person.FirstName;
            existing.LastName = person.LastName;
            existing.NationalCode = person.NationalCode;
            existing.BirthDate = person.BirthDate;

            var updated = await _service.UpdatePerson(existing);

            return new UpdatePersonResponse { Person = ToMessage(updated) };
        }

        public override async Task<DeletePersonResponse> DeletePerson(DeletePersonRequest request, ServerCallContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!Guid.TryParse(request.Id, out var id))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Person ID"));

            var existing = await _service.GetPerson(id);
            if (existing == null)
                return new DeletePersonResponse { Success = false };

            await _service.DeletePerson(id);
            return new DeletePersonResponse { Success = true };
        }

        public override async Task<GetAllPersonsResponse> GetAllPersons(GetAllPersonsRequest request, ServerCallContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var list = await _service.GetAll();
            var resp = new GetAllPersonsResponse();
            if (list != null)
                resp.Persons.AddRange(list.Select(ToMessage));

            return resp;
        }

        #endregion
    }
}
