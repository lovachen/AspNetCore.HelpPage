using HelpPage.Gen;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HelpPageGenServiceCollectionExtensions
    {
        public static IServiceCollection AddHelpPageGen(
            this IServiceCollection services,
            Action<HelpPageGenOptions> action)
        {
            //添加Mvc约定，以确保所有操作都启用了ApiExplorer
            services.Configure<MvcOptions>(c =>
               c.Conventions.Add(new HelpPageApplicationConvention()));



            services.AddSingleton<IHelpPageProvider, HelpPageGenerator>();

            if (action != null) services.ConfigureHelpPageGen(action);

            return services;
        }

        public static void ConfigureHelpPageGen(
            this IServiceCollection services,
            Action<HelpPageGenOptions> setupAction)
        {
            services.Configure(setupAction);
        }
    }
}
