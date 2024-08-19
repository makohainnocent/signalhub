﻿namespace Api.Core.Services
{
    public static class ServiceProviderAccessor
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }

}
