﻿using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace WebApiPluginTests
{
    public class TestBase
    {
        protected ITestOutputHelper Output { get; private set; }

        protected readonly ServiceCollection Services = new ServiceCollection();
        private IServiceProvider? _serviceProvider = null;
        protected IServiceProvider ServiceProvider => _serviceProvider ??= Services.BuildServiceProvider();

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupDependencyInjection();
        }

        protected HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:9000/api/");
            return client;
        }

        protected virtual void SetupDependencyInjection()
        {
            Services.AddLogging();
        }
    }
}
