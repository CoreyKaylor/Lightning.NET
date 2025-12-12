namespace LightningDB;

/// <summary>
/// Represents the configuration options for a LightningEnvironment instance.
/// </summary>
/// <remarks>
/// This class enables customization of various environment parameters such as the size of the memory map,
/// the maximum number of databases, and the maximum number of reader slots. These configurations are applied
/// to a LightningEnvironment during its creation or initialization.
/// </remarks>
public class EnvironmentConfiguration
{
    /// <summary>
    /// Gets or sets the size of the memory map (in bytes) for a LightningEnvironment.
    /// </summary>
    /// <remarks>
    /// This property specifies the maximum size of the database file that can be mapped into memory. Adjusting this value
    /// is critical when working with databases expected to grow over time, as it defines the capacity available for storing data.
    /// - Increasing the map size allows the database to accommodate more data, but it may consume more virtual memory.
    /// - Decreasing the map size can restrict database growth or limit memory consumption.
    /// Changes to the <c>MapSize</c> might require restarting the environment or re-creating it with the new configuration,
    /// depending on the implementation of the underlying storage environment.
    /// Use caution when setting this value in applications running in 32-bit processes, as the effective addressable memory
    /// space is limited. For such scenarios, auto-adjustments may be applied to ensure compatibility.
    /// </remarks>
    public long MapSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of reader slots available for a LightningEnvironment instance.
    /// </summary>
    /// <remarks>
    /// This property defines the maximum number of simultaneous read transactions that can be performed on the environment.
    /// - Increasing the value allows for more concurrent read operations, but it may increase resource usage.
    /// - Decreasing the value limits the number of concurrent reads but may reduce memory consumption.
    /// Changes to the <c>MaxReaders</c> property must be set before the environment is opened. Attempting to modify this
    /// property after the environment is already initialized will result in an exception. Adjust this parameter to match
    /// the requirements of your application's workload and concurrency needs.
    /// </remarks>
    public int MaxReaders { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of databases that can be opened within a LightningEnvironment instance.
    /// </summary>
    /// <remarks>
    /// This property defines the upper limit on the number of named databases that can be created or opened within the environment.
    /// - Increasing the maximum database count may allow complex applications to organize data across multiple logical databases.
    /// - Decreasing the maximum database count can help reduce overhead in environments with constrained resources or simpler use cases.
    /// Changes to the <c>MaxDatabases</c> property must be configured before the environment is opened. Attempting to modify this
    /// value after the environment has been initialized will result in an exception. This limitation ensures consistency across
    /// resources allocated for the environment.
    /// Configuring the appropriate <c>MaxDatabases</c> value is particularly relevant for applications requiring concurrency or
    /// multiple named databases.
    /// </remarks>
    public int MaxDatabases { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the map size should be automatically reduced
    /// in 32-bit processes to ensure compatibility with limited addressable memory space.
    /// </summary>
    /// <remarks>
    /// In 32-bit processes, the addressable memory space is constrained, which may lead to issues
    /// when working with large map sizes. When this property is set to <c>true</c>, the environment
    /// will automatically adjust the map size to a suitable value (e.g., <c>int.MaxValue</c>) during
    /// configuration to prevent memory allocation errors or crashes. This adjustment is only applied
    /// on 32-bit systems and has no effect on 64-bit processes.
    /// This setting is critical for maintaining stability in environments with constrained memory
    /// while allowing applications to utilize an appropriate map size without manual intervention.
    /// </remarks>
    public bool AutoReduceMapSizeIn32BitProcess { get; set; }

    internal void Configure(LightningEnvironment env)
    {
        if (MapSize > 0)
            env.MapSize = MapSize;

        if (MaxDatabases > 0)
            env.MaxDatabases = MaxDatabases;

        if (MaxReaders > 0)
            env.MaxReaders = MaxReaders;
    }
}