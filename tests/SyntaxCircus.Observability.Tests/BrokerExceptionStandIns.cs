// Stand-ins named like the real types: BrokerUnreachableNoiseFilter matches by full name and takes no broker dependency.
namespace MassTransit
{
    public sealed class RabbitMqConnectionException(string message) : Exception(message);
}

namespace RabbitMQ.Client.Exceptions
{
    public sealed class BrokerUnreachableException(string message) : Exception(message);

    public sealed class ConnectFailureException(string message) : Exception(message);
}
