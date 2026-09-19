README
Start RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.13-management
Run Consumer
dotnet run --project Consumer
Run Producer
dotnet run --project Producer
Build Solution
dotnet build
