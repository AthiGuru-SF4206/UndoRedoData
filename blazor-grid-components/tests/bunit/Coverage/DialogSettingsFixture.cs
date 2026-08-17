using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Popups;
using System;
using System.Collections.Generic;

namespace Syncfusion.Blazor.Tests.Grids.Coverage
{
    /// <summary>
    /// Fixture class for DialogSettings test data and configurations
    /// </summary>
    public class DialogSettingsFixture
    {
        /// <summary>
        /// Gets a DialogSettings instance with Height property set
        /// </summary>
        public static DialogSettings GetDialogSettingsWithHeight(string height = "400px")
        {
            return new DialogSettings
            {
                Height = height
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with Width property set
        /// </summary>
        public static DialogSettings GetDialogSettingsWithWidth(string width = "500px")
        {
            return new DialogSettings
            {
                Width = width
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with MinHeight property set
        /// </summary>
        public static DialogSettings GetDialogSettingsWithMinHeight(string minHeight = "250px")
        {
            return new DialogSettings
            {
                MinHeight = minHeight
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with AllowDragging enabled
        /// </summary>
        public static DialogSettings GetDialogSettingsWithDragging(bool allowDragging = true)
        {
            return new DialogSettings
            {
                AllowDragging = allowDragging
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with ShowCloseIcon enabled
        /// </summary>
        public static DialogSettings GetDialogSettingsWithCloseIcon(bool showCloseIcon = true)
        {
            return new DialogSettings
            {
                ShowCloseIcon = showCloseIcon
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with CloseOnEscape enabled
        /// </summary>
        public static DialogSettings GetDialogSettingsWithCloseOnEscape(bool closeOnEscape = true)
        {
            return new DialogSettings
            {
                CloseOnEscape = closeOnEscape
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with EnableResize enabled
        /// </summary>
        public static DialogSettings GetDialogSettingsWithResize(bool enableResize = true)
        {
            return new DialogSettings
            {
                EnableResize = enableResize
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with CssClass set
        /// </summary>
        public static DialogSettings GetDialogSettingsWithCssClass(string cssClass = "custom-dialog-class")
        {
            return new DialogSettings
            {
                CssClass = cssClass
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with Target set
        /// </summary>
        public static DialogSettings GetDialogSettingsWithTarget(string target = "#dialogTarget")
        {
            return new DialogSettings
            {
                Target = target
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with XValue (offset left) set
        /// </summary>
        public static DialogSettings GetDialogSettingsWithXValue(string xValue = "100px")
        {
            return new DialogSettings
            {
                XValue = xValue
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with YValue (offset top) set
        /// </summary>
        public static DialogSettings GetDialogSettingsWithYValue(string yValue = "50px")
        {
            return new DialogSettings
            {
                YValue = yValue
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with AnimationDelay set
        /// </summary>
        public static DialogSettings GetDialogSettingsWithAnimationDelay(double animationDelay = 200)
        {
            return new DialogSettings
            {
                AnimationDelay = animationDelay
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with AnimationDuration set
        /// </summary>
        public static DialogSettings GetDialogSettingsWithAnimationDuration(double animationDuration = 600)
        {
            return new DialogSettings
            {
                AnimationDuration = animationDuration
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with ZIndex set
        /// </summary>
        public static DialogSettings GetDialogSettingsWithZIndex(int zIndex = 1500)
        {
            return new DialogSettings
            {
                ZIndex = zIndex
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with AnimationEffect set
        /// </summary>
        public static DialogSettings GetDialogSettingsWithAnimationEffect(Syncfusion.Blazor.Popups.DialogEffect? animationEffect = Syncfusion.Blazor.Popups.DialogEffect.Zoom)
        {
            return new DialogSettings
            {
                AnimationEffect = animationEffect
            };
        }

        /// <summary>
        /// Gets a comprehensive DialogSettings instance with all properties configured
        /// </summary>
        public static DialogSettings GetCompleteDialogSettings()
        {
            return new DialogSettings
            {
                Height = "450px",
                Width = "550px",
                MinHeight = "300px",
                AllowDragging = true,
                ShowCloseIcon = true,
                CloseOnEscape = true,
                EnableResize = true,
                CssClass = "e-custom-dialog",
                Target = null,
                XValue = "50px",
                YValue = "75px",
                AnimationDelay = 100,
                AnimationDuration = 500,
                ZIndex = 1200,
                AnimationEffect = DialogEffect.Fade
            };
        }

        /// <summary>
        /// Gets a DialogSettings instance with default values
        /// </summary>
        public static DialogSettings GetDefaultDialogSettings()
        {
            return new DialogSettings();
        }

        /// <summary>
        /// Gets multiple animation effect samples for testing
        /// </summary>
        public static List<Syncfusion.Blazor.Popups.DialogEffect?> GetAllAnimationEffects()
        {
            return new List<Syncfusion.Blazor.Popups.DialogEffect?>
            {
                Syncfusion.Blazor.Popups.DialogEffect.Fade,
                Syncfusion.Blazor.Popups.DialogEffect.FadeZoom,
                Syncfusion.Blazor.Popups.DialogEffect.FlipLeftDown,
                Syncfusion.Blazor.Popups.DialogEffect.FlipLeftUp,
                Syncfusion.Blazor.Popups.DialogEffect.FlipRightDown,
                Syncfusion.Blazor.Popups.DialogEffect.FlipRightUp,
                Syncfusion.Blazor.Popups.DialogEffect.FlipXDown,
                Syncfusion.Blazor.Popups.DialogEffect.FlipXUp,
                Syncfusion.Blazor.Popups.DialogEffect.FlipYLeft,
                Syncfusion.Blazor.Popups.DialogEffect.FlipYRight,
                Syncfusion.Blazor.Popups.DialogEffect.SlideBottom,
                Syncfusion.Blazor.Popups.DialogEffect.SlideLeft,
                Syncfusion.Blazor.Popups.DialogEffect.SlideRight,
                Syncfusion.Blazor.Popups.DialogEffect.SlideTop,
                Syncfusion.Blazor.Popups.DialogEffect.Zoom,
                Syncfusion.Blazor.Popups.DialogEffect.None
            };
        }

        /// <summary>
        /// Gets a collection of various dialog configurations for comprehensive testing
        /// </summary>
        public static List<DialogSettings> GetVariousDialogConfigurations()
        {
            var dialogWithHeightAndWidth = GetDialogSettingsWithHeight("500px");
            dialogWithHeightAndWidth.Width = "600px";

            return new List<DialogSettings>
            {
                GetDialogSettingsWithHeight("350px"),
                GetDialogSettingsWithWidth("450px"),
                dialogWithHeightAndWidth,
                GetDialogSettingsWithMinHeight("200px"),
                GetDialogSettingsWithDragging(true),
                GetDialogSettingsWithDragging(false),
                GetDialogSettingsWithCloseIcon(true),
                GetDialogSettingsWithCloseIcon(false),
                GetDialogSettingsWithCloseOnEscape(true),
                GetDialogSettingsWithCloseOnEscape(false),
                GetDialogSettingsWithResize(true),
                GetDialogSettingsWithResize(false),
                GetDialogSettingsWithCssClass("custom-1"),
                GetDialogSettingsWithCssClass("custom-2"),
                GetDialogSettingsWithXValue("50px"),
                GetDialogSettingsWithXValue("150px"),
                GetDialogSettingsWithYValue("25px"),
                GetDialogSettingsWithYValue("100px"),
                GetDialogSettingsWithAnimationDelay(100),
                GetDialogSettingsWithAnimationDelay(300),
                GetDialogSettingsWithAnimationDuration(400),
                GetDialogSettingsWithAnimationDuration(800),
                GetDialogSettingsWithZIndex(1000),
                GetDialogSettingsWithZIndex(2000),
                GetDialogSettingsWithAnimationEffect(Syncfusion.Blazor.Popups.DialogEffect.Zoom),
                GetDialogSettingsWithAnimationEffect(Syncfusion.Blazor.Popups.DialogEffect.Fade),
                GetCompleteDialogSettings(),
                GetDefaultDialogSettings()
            };
        }

        /// <summary>
        /// Validates if a DialogSettings instance has all expected properties set
        /// </summary>
        public static bool ValidateCompleteDialogSettings(DialogSettings settings)
        {
            if (settings == null)
                return false;

            return !string.IsNullOrEmpty(settings.Height) &&
                   !string.IsNullOrEmpty(settings.Width) &&
                   !string.IsNullOrEmpty(settings.MinHeight) &&
                   settings.AllowDragging.HasValue &&
                   settings.ShowCloseIcon.HasValue &&
                   settings.CloseOnEscape.HasValue &&
                   settings.EnableResize.HasValue &&
                   !string.IsNullOrEmpty(settings.CssClass) &&
                   !string.IsNullOrEmpty(settings.XValue) &&
                   !string.IsNullOrEmpty(settings.YValue) &&
                   settings.AnimationDelay > 0 &&
                   settings.AnimationDuration > 0 &&
                   settings.ZIndex > 0 &&
                   settings.AnimationEffect.HasValue;
        }

        /// <summary>
        /// Gets expected string representation values for testing
        /// </summary>
        public static Dictionary<string, string> GetExpectedPropertyValues()
        {
            return new Dictionary<string, string>
            {
                { "Height", "400px" },
                { "Width", "500px" },
                { "MinHeight", "300px" },
                { "XValue", "100px" },
                { "YValue", "50px" },
                { "CssClass", "custom-dialog" },
                { "AnimationDelay", "200" },
                { "AnimationDuration", "600" },
                { "ZIndex", "1500" },
                { "AllowDragging", "true" },
                { "ShowCloseIcon", "true" },
                { "CloseOnEscape", "true" },
                { "EnableResize", "true" },
                { "AnimationEffect", "Zoom" }
            };
        }
    }
}
