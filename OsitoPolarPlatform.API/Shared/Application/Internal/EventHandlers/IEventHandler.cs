using Cortex.Mediator.Notifications;
using OsitoPolarPlatform.API.Shared.Domain.Model.Events;

namespace OsitoPolarPlatform.API.Shared.Application.Internal.EventHandlers;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
    
}