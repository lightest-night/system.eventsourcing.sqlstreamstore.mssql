using System;
using System.Reflection;
using System.Threading.Tasks;
using LightestNight.System.EventSourcing.Checkpoints;
using LightestNight.System.EventSourcing.SqlStreamStore.MsSql.Checkpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            services.AddEventStore(eventSourcingOptions => eventSourcingOptions = msSqlOptions, eventAssemblies)
                .Configure(optionsAccessor)
                .AddSingleton<MsSqlCheckpointManager>();
            
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
                        Task.WhenAll(
                            streamStore.CreateSchemaIfNotExists(),
                            sp.GetRequiredService<MsSqlCheckpointManager>().CreateSchemaIfNotExists()
                        ).Wait();

                    return streamStore;
                });
            }

            services.TryAddSingleton<GetGlobalCheckpoint>(sp =>
                sp.GetRequiredService<MsSqlCheckpointManager>().GetGlobalCheckpoint);

            services.TryAddSingleton<SetGlobalCheckpoint>(sp =>
                sp.GetRequiredService<MsSqlCheckpointManager>().SetGlobalCheckpoint);

            return services;
        }
    }
}