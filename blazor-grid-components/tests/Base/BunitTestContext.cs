using Syncfusion.Blazor.Tests.Grids.Base;
using System.Reflection;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Globalization;

namespace Syncfusion.Blazor.Tests.Grids
{
    public class BunitTestContext : BaseTestContext
    {
        public BunitTestContext()
        {
            // Create a new culture based on en-US and set the ShortDatePattern
            var cultureInfo = new CultureInfo("en-US");
            cultureInfo.DateTimeFormat.ShortDatePattern = "M/d/yyyy";
            cultureInfo.DateTimeFormat.ShortestDayNames = new[] { "S", "M", "T", "W", "T", "F", "S" };

            // Apply this culture globally
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            this.BeforeEachRun();
            this.UpdateRequiredMockJSRuntime();
#if NETCOREAPP
            SyncfusionBlazorService syncfusionBlazorService = (SyncfusionBlazorService)Services.GetService(typeof(SyncfusionBlazorService));
            syncfusionBlazorService.GetType().GetProperty("IsScriptRendered", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(syncfusionBlazorService, true);
#endif
        }

        public virtual void BeforeEachRun()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddSyncfusionBlazor().Replace(ServiceDescriptor.Transient<IComponentActivator, SfComponentActivator>());
            Services.AddOptions();
        }

        public virtual void UpdateRequiredMockJSRuntime()
        {
            JSInterop.Setup<bool>("sfBlazor.isRendered", new { }).SetResult(true);
        }

        public void Dispose()
        {
            base.Dispose();
            this.AfterEachRun();
        }

        public virtual void AfterEachRun() { }
    }
}
