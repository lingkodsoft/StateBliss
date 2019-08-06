using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StateBliss.SampleApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            RegisterServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
        
        private void RegisterServices(IServiceCollection services)
        {
            var stateDefinitionType = typeof(IStateDefinition);
            var stateDefinitionTypes = this.GetType().Assembly.GetTypes()
                .Where(a => typeof(IStateDefinition).IsAssignableFrom(a) && a.IsClass && !a.IsAbstract);
                
            foreach (var type in stateDefinitionTypes)
            {
                services.AddSingleton(stateDefinitionType, type);
            }

            services.AddSingleton<OrdersRepository>(provider =>
            {
                var orderRepository = new OrdersRepository();
                PopulateInitialDataForTesting(orderRepository);
                return orderRepository;
            });
            services.AddSingleton<StateProvider>();
            services.AddSingleton<IStateMachineManager>(provider =>
            {
                var stateProvider = provider.GetService<StateProvider>();
                StateMachineManager.Default.SetStateFactory(stateProvider.StatesProvider);
                return StateMachineManager.Default;
            });
            
            var stateDefinitionHandlerTypes = this.GetType().Assembly.GetTypes()
                .Where(a => typeof(IStateDefinitionHandler).IsAssignableFrom(a) && a.IsClass && !a.IsAbstract);
                
            foreach (var type in stateDefinitionHandlerTypes)
            {
                services.AddSingleton(type);
            }
        }

        private void PopulateInitialDataForTesting(OrdersRepository ordersRepository)
        {
            var order = new Order
            {
                Id = 1,
                Uid = Order.TestUid,
                State = OrderState.Initial
            };

            ordersRepository.InsertOrder(order);
        }
    }
}
