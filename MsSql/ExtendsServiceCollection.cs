using System;
using System.Reflection;
using LightestNight.System.EventSourcing.Checkpoints;
using LightestNight.System.EventSourcing.SqlStreamStore.MsSql.Checkpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlStreamStore;

namespace LightestNight.System.EventSourcing.SqlStreamStore.MsSql
{
    public static class ExtendsServiceCollection
    {
        public static IServiceCollection AddMsSqlEventStore(this IServiceCollection services,
            Action<MsSqlEventSourcingOptions>? optionsAccessor = null, params Assembly[] eventAssemblies)
        {
            var msSqlOptions = new MsSqlEventSourcingOptions();
            optionsAccessor?.Invoke(msSqlOptions);
            // ReSharper disable once RedundantAssignment
            services.AddEventStore(eventSourcingOptions => eventSourcingOptions = msSqlOptions, eventAssemblies);

            services.Configure(optionsAccessor);
            
            var serviceProvider = services.BuildServiceProvider();
            if (!(serviceProvider.GetService<IStreamStore>() is MsSqlStreamStoreV3))
            {
                services.AddSingleton<IStreamStore>(sp =>
                {
                    var streamStore = new MsSqlStreamStoreV3(
                        new MsSqlStreamStoreV3Settings(msSqlOptions.ConnectionString)
                        {
                            Schema = msSqlOptions.Schema
                        });

                    if (msSqlOptions.CreateSchemaIfNotExists)
                        streamStore.CreateSchemaIfNotExists().Wait();

                    return streamStore;
                });
            }

            if (!(serviceProvider.GetService<ICheckpointManager>() is MsSqlCheckpointManager))
            {
                services.AddSingleton<ICheckpointManager>(sp =>
                {
                    var checkpointManager = new MsSqlCheckpointManager(sp.GetRequiredService<IOptions<MsSqlEventSourcingOptions>>(), sp.GetRequiredService<ILogger<MsSqlCheckpointManager>>());
                    if (msSqlOptions.CreateSchemaIfNotExists)
                        checkpointManager.CreateSchemaIfNotExists().Wait();

                    return checkpointManager;
                });
            }

            return services;
        }
    }
}