using System.Collections.Generic;
using System.Text.Json;
using SharedKernel.Domain.Events;

namespace SharedKernel.Infrastructure.Events
{
    /// <summary>
    /// 
    /// </summary>
    public class DomainEventJsonSerializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="domainEvent"></param>
        /// <returns></returns>
        public string Serialize(DomainEvent domainEvent)
        {
            if (domainEvent == null) return "";

            var attributes = domainEvent.ToPrimitives();

            attributes.Add("id", domainEvent.AggregateId);

            return JsonSerializer.Serialize(new Dictionary<string, Dictionary<string, object>>
            {
                {"data", new Dictionary<string,object>
                {
                    {"id" , domainEvent.EventId},
                    {"type", domainEvent.GetEventName()},
                    {"occurred_on", domainEvent.OccurredOn},
                    {"attributes", attributes}
                }},
                {"meta", new Dictionary<string,object>()}
            });
        }
    }
}