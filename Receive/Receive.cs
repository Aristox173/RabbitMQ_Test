using System;
using System.Net.Mail;
using System.Net;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class EmailConsumer
{
    static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchange: "email_topic", type: ExchangeType.Topic);
            var queueName = "common_email_queue";
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queue: queueName, exchange: "email_topic", routingKey: "#");

            Console.WriteLine("Esperando correos electrónicos. Para salir, presione Ctrl+C");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var emailMessage = Encoding.UTF8.GetString(body);
                var recipientEmail = ea.RoutingKey;

                EnviarCorreo(recipientEmail, emailMessage);
                Console.WriteLine($"Correo electrónico enviado a {recipientEmail}");
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            Console.ReadLine(); // Mantiene la aplicación en ejecución
        }
    }

    static void EnviarCorreo(string recipientEmail, string emailMessage)
    {
        if (MailAddress.TryCreate(recipientEmail, out MailAddress validAddress))
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("", ""),//Poner las respectivas credenciales
                EnableSsl = true,
            };

            var mail = new MailMessage("correo@gmail.com", recipientEmail)
            {
                Subject = "Correo electrónico desde RabbitMQ",
                Body = emailMessage,
                IsBodyHtml = false
            };

            smtpClient.Send(mail);
        }
        else
        {
            Console.WriteLine("La dirección de correo electrónico no es válida.");
        }
    }
}