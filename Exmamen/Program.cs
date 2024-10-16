using MySql.Data.MySqlClient;
using System;

class Car
{
    public int CarID { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
    public DateTime DateAdded { get; set; }

    public override string ToString()
    {
        return $"{CarID}: {Make} {Model} ({Year}) - ${Price:F2} (Added on {DateAdded.ToShortDateString()})";
    }
}

class Program
{


    static void Main()
    {
        bool repeat = true;

        while (repeat)
        {
            Console.WriteLine(
                "\nWelcome to the Car Management App" +
                "\n1. View All Cars" +
                "\n2. Add a New Car" +
                "\n3. Update an Existing Car" +
                "\n4. Exit" +
                "\nPlease select an option:\n");

            try
            {
                int option = Convert.ToInt32(Console.ReadLine());

                switch (option)
                {
                    case 1:
                        Console.WriteLine("\nViewing all cars...");
                        viewAllCars();
                        break;

                    case 2:
                        addNewCar();
                        break;

                    case 3:
                        Console.Write("\nEnter the ID of the car you want to update: ");
                        int carId = int.Parse(Console.ReadLine());
                        getCar(carId);
                        UpdateCar(carId);
                        break;

                    case 4:
                        Console.WriteLine("\nExiting the application...");
                        repeat = false;
                        break;

                    default:
                        Console.WriteLine("\nInvalid option. Please select a valid number (1-4).");
                        break;
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("\nInvalid input. Please enter a number.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    // Function that establishes a connection to the database and accepts an action as a parameter to execute it.
    static void ExecuteWithConnection(Action<MySqlConnection> action)
    {
        string conn = "Server=localhost;Database=Car;UserId=root;Password=;";
        using (MySqlConnection cnx = new MySqlConnection(conn))
        {
            try
            {
                cnx.Open();
                action(cnx);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"\nMySQL error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
        }
    }

    static void viewAllCars()
    {
        ExecuteWithConnection(cnx =>
        {
            string query = "SELECT * FROM car";
            MySqlCommand cmd = new MySqlCommand(query, cnx);

            using MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int carID = reader.GetInt32("CarID");
                string make = reader.GetString("Make");
                string model = reader.GetString("Model");
                int year = reader.GetInt32("Year");
                decimal price = reader.GetDecimal("Price");
                DateTime dateAdded = reader.GetDateTime("DateAdded");

                Console.WriteLine($"\nCardID: {carID}, " +
                                  $"\nMake: {make}, " +
                                  $"\nModel: {model}, " +
                                  $"\nYear: {year}, " +
                                  $"\nPrice: {price:F2}, " +
                                  $"\nDateAdded: {dateAdded.ToShortDateString()}");
            }
        });
    }

    static void addNewCar()
    {
        bool adding = true;
        while (adding)
        {
            Console.Write("\nEnter the make of the car: ");
            string make = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(make))
            {
                Console.WriteLine("\nInput cannot be empty or whitespace. Please enter a valid value.");
                make = Console.ReadLine();
            }

            Console.Write("\nEnter the model of the car: ");
            string model = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(model))
            {
                Console.WriteLine("\nInput cannot be empty or whitespace. Please enter a valid value.");
                model = Console.ReadLine();
            }

            Console.Write("\nEnter the year of the car: ");
            int year;
            while (!int.TryParse(Console.ReadLine(), out year))
            {
                Console.WriteLine("\nInvalid year. Please enter a valid number.");
            }

            Console.Write("\nEnter the car price: ");
            decimal price;
            while (!decimal.TryParse(Console.ReadLine(), out price))
            {
                Console.WriteLine("\nInvalid price. Please enter a valid number.");
            }

            insertCar(make, model, year, price);

            Console.Write("\nDo you want to add another car? (y/n): ");
            string response = Console.ReadLine().ToLower();
            if (response != "y")
            {
                adding = false;
            }
        }
    }

    static void insertCar(string make, string model, int year, decimal price)
    {
        DateTime dateAdded = DateTime.Now;

        ExecuteWithConnection(cnx =>
        {
            string query = "INSERT INTO car (Make, Model, Year, Price, DateAdded) VALUES (@Make, @Model, @Year, @Price, @DateAdded)";
            using (MySqlCommand cmd = new MySqlCommand(query, cnx))
            {
                cmd.Parameters.AddWithValue("@Make", make);
                cmd.Parameters.AddWithValue("@Model", model);
                cmd.Parameters.AddWithValue("@Year", year);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@DateAdded", dateAdded);

                int rowsAffected = cmd.ExecuteNonQuery();
                Console.WriteLine(rowsAffected > 0 ? "\nCar added successfully!" : "\nError adding the car.");
            }
        });
    }
    static void getCar(int carID)
    {
        ExecuteWithConnection(cnx =>
        {
            string query = "SELECT * FROM Car WHERE CarID = @CarID";
            using (MySqlCommand command = new MySqlCommand(query, cnx))
            {
                command.Parameters.AddWithValue("@CarID", carID);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int id = reader.GetInt32("CarID");
                        string make = reader.GetString("Make");
                        string model = reader.GetString("Model");
                        int year = reader.GetInt32("Year");
                        decimal price = reader.GetDecimal("Price");
                        DateTime dateAdded = reader.GetDateTime("DateAdded");

                        Console.WriteLine("\nCAR FOUND:" +
                                          $"\nCardID: {id}, " +
                                          $"\nMake: {make}, " +
                                          $"\nModel: {model}, " +
                                          $"\nYear: {year}, " +
                                          $"\nPrice: {price}, " +
                                          $"\nDateAdded: {dateAdded.ToShortDateString()}");
                    }
                    else
                    {
                        Console.WriteLine("\nNo car found with the specified CarID.");
                    }
                }
            }
        });
    }

    static void UpdateCar(int carID)
    {
        ExecuteWithConnection(cnx =>
        {
            Console.Write("\nEnter the new Make (leave blank to keep current): ");
            string make = Console.ReadLine();

            Console.Write("\nEnter the new Model (leave blank to keep current): ");
            string model = Console.ReadLine();

            Console.Write("\nEnter the new Year (leave blank to keep current): ");
            string yearInput = Console.ReadLine();
            int? year = string.IsNullOrWhiteSpace(yearInput) ? (int?)null : int.Parse(yearInput);

            Console.Write("\nEnter the new Price (leave blank to keep current): ");
            string priceInput = Console.ReadLine();
            decimal? price = string.IsNullOrWhiteSpace(priceInput) ? (decimal?)null : decimal.Parse(priceInput);

            string query = "UPDATE car SET " +
                           "Make = COALESCE(NULLIF(@Make, ''), Make), " +
                           "Model = COALESCE(NULLIF(@Model, ''), Model), " +
                           "Year = COALESCE(@Year, Year), " +
                           "Price = COALESCE(@Price, Price) " +
                           "WHERE CarID = @CarID";

            using (MySqlCommand cmd = new MySqlCommand(query, cnx))
            {
                cmd.Parameters.AddWithValue("@CarID", carID);
                cmd.Parameters.AddWithValue("@Make", make);
                cmd.Parameters.AddWithValue("@Model", model);
                cmd.Parameters.AddWithValue("@Year", (object)year ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Price", (object)price ?? DBNull.Value);

                int rowsAffected = cmd.ExecuteNonQuery();
                Console.WriteLine(rowsAffected > 0 ? "\nCar updated successfully!" : "\nError updating the car. Ensure the CarID exists.");
            }
        });
    }
}
