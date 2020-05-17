# Lightest Night
## Event Sourcing > Sql Stream Store > Microsoft Sql Server

The libraries for using the Microsoft Sql Server implementation of Sql Stream Store. For more information see the Core repository's [README.md](https://github.com/lightest-night/system.eventsourcing.sqlstreamstore/blob/master/README.md "README.md")

#### How To Use
##### Registration
* Asp.Net Standard/Core Dependency Injection
  * Use the provided `services.AddMSSqlEventStore(Action<MSSqlEventSourcingOptions>? optionsAccessor = null, params Assembly[] eventAssemblies)` method

* Other Containers
  * Register all instances required for `IEventSourceProjection` as Transient dependencies
  * Register an instance of `GetEventTypes` as a Singleton. `EventCollection.GetEventTypes` has been provided for this use case
  * Register an instance of `IPersistentSubscriptionManager` as a Singleton. `PersistentSubscriptionManager` has been provided for this use case
  * Register an instance of `IEventPersistence` as a Singleton. `SqlEventStore` has been provided for this use case
  * Register `EventSubscription` as a hosted service
  * Register `IStreamStore` as a Singleton with the concrete type of `MSSqlStreamStore`.
  
##### Usage
###### Aggregates
* `Task<T> GetById<T>(Guid id, CancellationToken cancellationToken = default) where T : class, IEventSourceAggregate`
  * An asynchronous function to call when retrieving events from a stream in the shape of an Aggregate

* `Task Save(IEventSourceAggregate aggregate, CancellationToken cancellationToken = default)`
  * An asynchronous function to call when saving new events that have been applied to an Aggregate
  
###### Persistent Subscriptions
* `Task<Guid> CreateCategorySubscription(string categoryName, Func<object, CancellationToken, Task> eventReceived, CancellationToken cancellationToken = default)`
  * An asynchronous function to call when creating a subscription to a Category
  
* `Task CloseSubscription(Guid subscriptionId, CancellationToken cancellationToken = new CancellationToken())`
  * An asynchronous function to call when closing a subscription
  
* `Task<int> CatchSubscriptionUp(string streamName, int checkpoint, Func<object, CancellationToken, Task> eventReceived, CancellationToken cancellationToken = default)`
  * An asynchronous function to call when a stream has moved forward from a subscription, facilitates fast catch up.