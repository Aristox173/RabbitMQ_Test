using System;
using System.Linq;
using System.Text;
using System.Threading; // Librería para utilizar Thread.Sleep
using RabbitMQ.Client;

class EmailPublisher
{
    static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Declara el intercambio (exchange) de tipo 'topic'
            channel.ExchangeDeclare(exchange: "email_topic", type: ExchangeType.Topic);

            while (true)
            {
                // Interfaz de usuario para enviar correo
                Console.WriteLine("=== Enviar correo electrónico ===");
                Console.WriteLine("Seleccione una opción:");
                Console.WriteLine("1. Enviar a una sola dirección.");
                Console.WriteLine("2. Enviar a múltiples direcciones.");
                Console.WriteLine("Escriba 'exit' para salir.");
                Console.Write("Opción: ");

                string option = Console.ReadLine();

                if (option.ToLower() == "exit")
                    break;

                switch (option)
                {
                    case "1":
                        EnviarAUnaDireccion(channel);
                        break;
                    case "2":
                        EnviarAMultiplesDirecciones(channel);
                        break;
                    default:
                        Console.WriteLine("Opción no válida. Por favor, seleccione 1 o 2.");
                        break;
                }
            }
        }
    }

    static void EnviarAUnaDireccion(IModel channel)
    {
        // Obtener dirección de correo y validarla
        Console.Write("Ingrese la dirección de correo electrónico: ");
        string recipientEmail = Console.ReadLine();

        if (IsValidEmail(recipientEmail.Trim()))
        {
            Console.Write("Ingrese el cuerpo del correo electrónico: ");
            string body = Console.ReadLine();

            // Mostrar borrador antes de enviar
            MostrarBorrador(recipientEmail.Trim(), "RabbitMQ Test", body, channel);
        }
        else
        {
            Console.WriteLine("La dirección de correo electrónico no es válida.");
        }
    }

    static void EnviarAMultiplesDirecciones(IModel channel)
    {
        // Obtener múltiples direcciones de correo y validarlas
        Console.Write("Ingrese direcciones de correo electrónico separadas por comas: ");
        string recipientEmailsInput = Console.ReadLine();

        string[] recipientEmails = recipientEmailsInput.Split(',');
        var validEmails = recipientEmails.Where(email => IsValidEmail(email.Trim())).ToList();

        if (validEmails.Any())
        {
            Console.Write("Ingrese el cuerpo del correo electrónico: ");
            string body = Console.ReadLine();

            // Mostrar borrador antes de enviar
            MostrarBorrador(string.Join(", ", validEmails), "RabbitMQ Test", body, channel);
        }
        else
        {
            Console.WriteLine("No se proporcionaron direcciones de correo electrónico válidas.");
        }
    }

    static void MostrarBorrador(string recipientEmails, string subject, string body, IModel channel)
    {
        // Mostrar borrador del correo antes de enviar
        Console.WriteLine("\n=== Borrador del correo ===");
        Console.WriteLine($"Para: {recipientEmails}");
        Console.WriteLine($"Asunto: {subject}");
        Console.WriteLine("--------");
        Console.WriteLine(body);
        Console.WriteLine("--------");

        Console.Write("¿Desea enviar este correo? (s/n): ");
        string confirmacion = Console.ReadLine().ToLower();

        if (confirmacion == "s")
        {
            Console.WriteLine("Enviando correo...");

            // Simulación de envío con un retraso de 3 segundos (3000 milisegundos)
            Thread.Sleep(3000);

            Console.WriteLine("Correo enviado correctamente.");
            EnviarCorreo(channel, recipientEmails, subject, body);
        }
        else
        {
            Console.WriteLine("Correo no enviado.");
        }
    }

    static void EnviarCorreo(IModel channel, string recipientEmails, string subject, string body)
    {
        // Envío real del correo electrónico utilizando RabbitMQ
        foreach (var recipientEmail in recipientEmails.Split(','))
        {
            string emailMessage = $"{subject}\n\n{body}";
            var bodyBytes = Encoding.UTF8.GetBytes(emailMessage);
            channel.BasicPublish(exchange: "email_topic", routingKey: recipientEmail.Trim(), basicProperties: null, body: bodyBytes);
            Console.WriteLine($"Correo electrónico enviado a {recipientEmail.Trim()}");
        }
    }

    static bool IsValidEmail(string email)
    {
        // Validar la dirección de correo electrónico
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}