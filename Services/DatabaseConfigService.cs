using System.Text.Json;
using AngularGenerator.Core.Models;

namespace AngularGenerator.Services
{
    /// <summary>
    /// Service for managing database configurations
    /// Stores configurations in appsettings.json or a separate config file
    /// </summary>
    public class DatabaseConfigService
    {
        private readonly string _configFilePath;
        private DatabaseConfigCollection _configs = new DatabaseConfigCollection();

        public DatabaseConfigService(IWebHostEnvironment env)
        {
            _configFilePath = Path.Combine(env.ContentRootPath, "database-configs.json");
            LoadConfigurations();
        }

        /// <summary>
        /// Load all saved configurations
        /// </summary>
        public DatabaseConfigCollection LoadConfigurations()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    _configs = JsonSerializer.Deserialize<DatabaseConfigCollection>(json) ?? new DatabaseConfigCollection();
                }
                else
                {
                    _configs = new DatabaseConfigCollection();
                }

                return _configs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading database configs: {ex.Message}");
                _configs = new DatabaseConfigCollection();
                return _configs;
            }
        }

        /// <summary>
        /// Save a new configuration or update existing one
        /// </summary>
        public bool SaveConfiguration(DatabaseConfig config)
        {
            try
            {
                // Check if config with same name exists
                var existing = _configs.Configurations.FirstOrDefault(c => c.Name == config.Name);
                if (existing != null)
                {
                    // Update existing
                    existing.ConnectionString = config.ConnectionString;
                    existing.DbType = config.DbType;
                    existing.Description = config.Description;
                    existing.IsDefault = config.IsDefault;
                    existing.LastUsed = DateTime.Now;
                }
                else
                {
                    // Add new
                    config.LastUsed = DateTime.Now;
                    _configs.Configurations.Add(config);
                }

                // If this is set as default, unset others
                if (config.IsDefault)
                {
                    foreach (var c in _configs.Configurations.Where(c => c.Name != config.Name))
                    {
                        c.IsDefault = false;
                    }
                    _configs.DefaultConfigName = config.Name;
                }

                // Save to file
                SaveToFile();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a configuration
        /// </summary>
        public bool DeleteConfiguration(string name)
        {
            try
            {
                var config = _configs.Configurations.FirstOrDefault(c => c.Name == name);
                if (config != null)
                {
                    _configs.Configurations.Remove(config);
                    
                    // If this was default, clear default
                    if (_configs.DefaultConfigName == name)
                    {
                        _configs.DefaultConfigName = null;
                    }

                    SaveToFile();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get a specific configuration by name
        /// </summary>
        public DatabaseConfig? GetConfiguration(string name)
        {
            return _configs.Configurations.FirstOrDefault(c => c.Name == name);
        }

        /// <summary>
        /// Get the default configuration
        /// </summary>
        public DatabaseConfig? GetDefaultConfiguration()
        {
            return _configs.Configurations.FirstOrDefault(c => c.IsDefault);
        }

        /// <summary>
        /// Get all configurations
        /// </summary>
        public List<DatabaseConfig> GetAllConfigurations()
        {
            return _configs.Configurations.OrderByDescending(c => c.IsDefault).ThenByDescending(c => c.LastUsed).ToList();
        }

        /// <summary>
        /// Update last used timestamp
        /// </summary>
        public void UpdateLastUsed(string name)
        {
            var config = _configs.Configurations.FirstOrDefault(c => c.Name == name);
            if (config != null)
            {
                config.LastUsed = DateTime.Now;
                SaveToFile();
            }
        }

        /// <summary>
        /// Save configurations to file
        /// </summary>
        private void SaveToFile()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(_configs, options);
                File.WriteAllText(_configFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to file: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Export configurations (without passwords for security)
        /// </summary>
        public string ExportConfigurations(bool includePasswords = false)
        {
            var exportConfigs = _configs.Configurations.Select(c => new
            {
                c.Name,
                c.DbType,
                ConnectionString = includePasswords ? c.ConnectionString : MaskPassword(c.ConnectionString),
                c.Description,
                c.IsDefault
            }).ToList();

            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(exportConfigs, options);
        }

        /// <summary>
        /// Mask password in connection string for display
        /// </summary>
        private string MaskPassword(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return connectionString;

            // Simple password masking
            var patterns = new[] { "Password=", "Pwd=", "password=", "pwd=" };
            foreach (var pattern in patterns)
            {
                var index = connectionString.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    var start = index + pattern.Length;
                    var end = connectionString.IndexOfAny(new[] { ';', ' ' }, start);
                    if (end < 0) end = connectionString.Length;

                    var beforePassword = connectionString.Substring(0, start);
                    var afterPassword = connectionString.Substring(end);
                    return beforePassword + "****" + afterPassword;
                }
            }

            return connectionString;
        }
    }
}
