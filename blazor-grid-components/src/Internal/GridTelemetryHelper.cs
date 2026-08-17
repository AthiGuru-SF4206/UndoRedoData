using System;
using System.Reflection;
using Syncfusion.Telemetry;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Helper utility class for managing telemetry logging across the Grid component and related modules.
    /// Provides centralized access to telemetry tracking functionality with assembly metadata caching.
    /// </summary>
    internal static class GridTelemetryHelper
    {
        // Dedicated lock that serializes every call to LogTelemetry.
        private static readonly object _telemetrySync = new object();

        // Memoization flag for Telemetry.Telemetry.Configure / assembly-name capture.
        private static bool _telemetryConfigured ;

        private static string _assemblyName = string.Empty;

        private static string _sdkVersion = string.Empty;

        internal static void LogTelemetry(bool isfeature, string name)
        {
            // Lock-free fast path when telemetry is disabled. Avoids contention for callers
            // that never opt-in to telemetry.
            if (!Telemetry.Telemetry.IsTelemetryEnabled)
            {
                return;
            }
            // Serializes the body so that static fields (assemblyName, sdkVersion, s_telemetryConfigured) and the process-wide Telemetry.Telemetry singleton
            // are not mutated concurrently by multiple threads. 
            lock (_telemetrySync)
            {
                if (!_telemetryConfigured)
                {
                    if (string.IsNullOrEmpty(_assemblyName) || string.IsNullOrEmpty(_sdkVersion))
                    {
                        Assembly assembly = typeof(SfGrid<>).Assembly;
                        if (assembly != null)
                        {
                            AssemblyName assemblyFileName = assembly.GetName();
                            if (assemblyFileName != null && assemblyFileName.Name != null && assemblyFileName.Version != null)
                            {
                                _assemblyName = assemblyFileName.Name.ToString();
                                _sdkVersion = assemblyFileName.Version.ToString();
                            }
                        }
                    }
                    _telemetryConfigured = true;
                }
                TelemetryOptions options = new TelemetryOptions
                {
                    AssemblyName = _assemblyName,
                    SdkVersion = _sdkVersion,
                    SdkName = "Grid SDK"
                };

                //if (isfeature)
                //    Telemetry.Telemetry.TrackFeature(name, ComponentName.GridBlazor, options);
                //else
                //    Telemetry.Telemetry.TrackComponent(ComponentName.GridBlazor, options);
            }
        }
    }
}
