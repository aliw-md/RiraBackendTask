
# Rira Backend Task
Hello!

## Project Structure

The project follows a **Clean Architecture** structure and includes the following layers:

<img width="340" height="340" alt="image" src="https://github.com/user-attachments/assets/02ae5d4d-d8b0-49bd-ae17-cca73b347c27" />

### Domain

Contains the main domain entity `Person`.

This layer holds the core business model and has no dependencies on other layers.

### Application

Contains the application services and validation logic.

Uses **FluentValidation** to validate entities and business rules before persistence.

### Persistence

Implements data storage using a local JSON file as the data source.

Includes the `FilePersonRepository`, which provides CRUD operations for the `Person` entity.

### Presentation (gRPC)

Contains the gRPC server endpoint (`PersonProtoService`) which is responsible for exposing data through gRPC.

This is the main API layer of the system.

### Console Application

A console client that communicates with the gRPC server.

It provides a simple interactive menu to perform CRUD operations through gRPC calls.

### Tests

Includes unit and integration tests for validating the application logic and gRPC endpoints.

---

## How to Run

To run the system correctly, configure **Multiple Startup Projects** in Visual Studio:

Select both **EndPoint.Grpc** and **EndPoint.ConsoleApp** as startup projects.

This ensures that the gRPC server runs before the console client connects.




