using Bunit;
using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Syncfusion.Blazor.Tests.Grids
{
    public class SfFixture : Fixture, IDisposable
    {
        protected bool DisableScriptManager = false;
       
        public SfFixture()
        {
            this.BeforeEachRun();
            this.UpdateRequiredMockJSRuntime();
            //Setup = (Fixture fixture) => { 
            //    Services.AddSyncfusionBlazor(); SfSetUp.Invoke(this);
            //    Services.AddMockJSRuntime(this.JSRuntimeMockMode);
            //};
        }

        //[Parameter]
        //public Action<Fixture> SfSetUp { get; set; }

        public virtual void BeforeEachRun()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSyncfusionBlazor().Replace(ServiceDescriptor.Transient<IComponentActivator, SfComponentActivator>()); 
            var options = new GlobalOptions();
            SyncfusionBlazorService serv = new SyncfusionBlazorService(options, JSInterop.JSRuntime);
            serv.GetType().GetProperty("IsScriptRendered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(serv, true);
            Services.AddScoped((IServiceProvider provider) => serv);
        }

        public virtual void UpdateRequiredMockJSRuntime()
        {
            JSInterop.Setup<bool>("sfBlazor.isRendered", new { }).SetResult(true);
        }

        void IDisposable.Dispose()
        {
            this.AfterEachRun();
        }

        public virtual void AfterEachRun() { }
    }
}
