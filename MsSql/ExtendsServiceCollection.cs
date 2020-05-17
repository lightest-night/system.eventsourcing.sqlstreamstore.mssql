using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;

namespace LightestNight.System.EventSourcing.SqlStreamStore.MSSql
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
            if (!(serviceProvider.GetService<IStreamStore>() is MsSqlStreamStore))
            {
                services.AddSingleton<IStreamStore>(sp =>
                {
                    var streamStore = new MsSqlStreamStore(new MsSqlStreamStoreSettings(msSqlOptions.ConnectionString)
                    {
                        Schema = msSqlOptions.Schema
                    });

                    if (msSqlOptions.CreateSchemaIfNotExists)
                        Task.Run(async () => await streamStore.CreateSchema().ConfigureAwait(false));

                    return streamStore;
                });
            }

            return services;
        }
    }
}