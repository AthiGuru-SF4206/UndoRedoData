using Bunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Syncfusion.Blazor;
using Newtonsoft.Json.Linq;

namespace Syncfusion.Blazor.Tests.Grids.MediaQuery
{ 
    
    public class MediaQuery : BunitTestContext
    {
        [Fact(DisplayName = "Initial Rendereing With MediaQuery")]
        public void ComponentRendering()
        {
            List<MediaBreakpoint> value = new();
            JSInterop.Setup<string>("window.sfBlazor.MediaQuery.initialize",e => {
                ((Dictionary<string, object>)e.Arguments[0]).TryGetValue("mediaBreakpoints", out object val);
                value = (List<MediaBreakpoint>)val;
                return true; 
            }).SetResult("Large");
            var mediaQuery = RenderComponent<SfMediaQuery>();
            //Values passed to interop
            Assert.Equal("(max-width: 768px)", value[0].MediaQuery);
            Assert.Equal("Small", value[0].Breakpoint);
            Assert.Equal("(min-width: 1024px)", value[1].MediaQuery);
            Assert.Equal("(min-width: 768px)", value[2].MediaQuery);
            Assert.Equal("Large", mediaQuery.Instance.ActiveBreakpoint);
        }
        [Fact(DisplayName = "Checking default property values of MediaQuery")]
        public void DefaultQuery()
        {
            Assert.Equal("(max-width: 768px)", SfMediaQuery.Small.MediaQuery);
            Assert.Equal("Small", SfMediaQuery.Small.Breakpoint);
            Assert.Equal("(min-width: 768px)", SfMediaQuery.Medium.MediaQuery);
            Assert.Equal("Medium", SfMediaQuery.Medium.Breakpoint);
            Assert.Equal("(min-width: 1024px)", SfMediaQuery.Large.MediaQuery);
            Assert.Equal("Large", SfMediaQuery.Large.Breakpoint);
        }
        [Fact(DisplayName = "Change MediaQuery and BreakPoint Inside Oninitialized Method")]
        public void UpdateMediaBreakPoint()
        {
            List<MediaBreakpoint> value = new();
            JSInterop.Setup<string>("window.sfBlazor.MediaQuery.initialize", e =>
            {
                ((Dictionary<string, object>)e.Arguments[0]).TryGetValue("mediaBreakpoints", out object val);
                value = (List<MediaBreakpoint>)val;
                return true;
            }).SetResult("CustomSmall");
            var mediaQuery = RenderComponent<MediaBreakPointOninitialize>();
            //Values passed to interop
            Assert.Equal("(max-width: 600px)", value[0].MediaQuery);
            Assert.Equal("CustomSmall", value[0].Breakpoint);
            Assert.Equal("(min-width: 600px)", value[1].MediaQuery);
            Assert.Equal("CustomLarge", value[1].Breakpoint);
        }
        [Fact(DisplayName = "OnBreakpointChanged Event Testing")]
        public void OnBreakpointChanged()
        {
            var OnBreakpointChanged = 0;
            var mediaQuery = RenderComponent<SfMediaQuery>(parameters => parameters.Add(s => s.OnBreakpointChanged, (BreakpointChangedEventArgs e) =>
            {
                OnBreakpointChanged++;
                Assert.Equal("Large", e.ActiveBreakpoint);
            }));
            mediaQuery.Instance.UpdateActiveBreakpoint("Large");
            Assert.Equal(1, OnBreakpointChanged);
        }
        [Fact(DisplayName = "ActiveBreakpointChanged Event Testing")]
        public void ActiveBreakpointChanged()
        {
            var ActiveBreakpointChanged = 0;
            var mediaQuery = RenderComponent<SfMediaQuery>(parameters => parameters.Add(s => s.ActiveBreakpointChanged, (string e) =>
            {
                Assert.Equal("Large", e);
                ActiveBreakpointChanged++;
            }));
            mediaQuery.Instance.UpdateActiveBreakpoint("Large");
            Assert.Equal(1, ActiveBreakpointChanged);
        }
    }
}
