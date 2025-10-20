
using EndPoint.ConsoleApp.Services;
using Microsoft.Extensions.Configuration;
using System.Drawing;
using System.Reflection;


namespace EndPoint.ConsoleApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {


            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config = new ConfigurationBuilder()
                .SetBasePath(exePath!)
                .AddJsonFile("appsettings.console.json", optional: false, reloadOnChange: true)
                .Build();

            var grpcAddress = config["GrpcSettings:PersonServiceUrl"];

            var client = new PersonGrpcClient(grpcAddress!);


            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("=============================================================");
            Console.WriteLine(" Rira Backend Task - Console Application");
            Console.WriteLine("=============================================================");
            Console.WriteLine("This application communicates with the Person gRPC Service.");
            Console.WriteLine("-------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;


            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.WriteLine("-------------------------------------------------------------");
                Console.WriteLine("Operations Menu:");
                Console.WriteLine();
                Console.WriteLine("1. View all persons");
                Console.WriteLine("2. View person by ID");
                Console.WriteLine("3. Add a new person");
                Console.WriteLine("4. Update an existing person");
                Console.WriteLine("5. Delete a person");
                Console.WriteLine("6. Clear Console");
                Console.WriteLine("7. Exit");
                Console.WriteLine("-------------------------------------------------------------");
                Console.Write("Select an option (1-7): ");
                var choice = Console.ReadLine();
                Console.WriteLine();


                try
                {
                    switch (choice)
                    {
                        case "1":
                            await client.GetAllPersonsAsync();
                            break;

                        case "2":
                            await client.GetPersonByIdAsync();
                            break;

                        case "3":
                            await client.CreatePersonAsync();
                            break;

                        case "4":
                            await client.UpdatePersonAsync();
                            break;

                        case "5":
                            await client.DeletePersonAsync();
                            break;

                        case "6":
                            Console.Clear();
                            break;

                        case "7":
                            Console.WriteLine("Bye Bye!");
                            return;

                        default:
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("Invalid option. Please select a number between 1 and 6.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("-------------------------------------------------------------");
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine("-------------------------------------------------------------");
                }
            }
        }
    }
}