using System.Reflection;
using UIPooc.Attributes;
using UIPooc.Models;

namespace UIPooc.Helpers;

/// <summary>
/// Configuration metadata for property mapping
/// Defines how to map from source data to target entity properties
/// </summary>
public class PropertyMetadata
{
    /// <summary>
    /// The name of the field in the source data (e.g., JSON key, API field name)
    /// </summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// The name of the column/property in the target entity (e.g., database column name)
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this field is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Whether to ignore this field during mapping
    /// </summary>
    public bool Ignore { get; set; }

    /// <summary>
    /// Default value to use if source data is null or empty
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Format string for parsing (e.g., date format: "yyyy-MM-ddTHH:mm:ss.fffZ")
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Custom converter type for complex conversions
    /// </summary>
    public Type? ConverterType { get; set; }

    /// <summary>
    /// Method name in the converter type to call
    /// </summary>
    public string? ConverterMethodName { get; set; }
}

/// <summary>
/// Provides reflection-based mapping using DbEntity attributes or PropertyMetadata dictionaries
/// </summary>
public static class DbEntityMapperDepr
{



    /// <summary>
    /// Populates an entity from a dictionary using DbProperty attributes
    /// </summary>
    private static void PopulateFromDictionary<T>(T entity, Dictionary<string, object> data) where T : struct
    {
        var type = typeof(T);
        var entityAttr = type.GetCustomAttribute<DbEntityAttribute>();

        if (entityAttr == null)
        {
            throw new InvalidOperationException($"Type {type.Name} is not marked with [DbEntity] attribute");
        }

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propAttr = property.GetCustomAttribute<DbPropertyAttribute>();
            
            // Skip if property is marked to ignore
            if (propAttr?.Ignore == true)
                continue;

            // Determine the source key name
            string sourceKey = propAttr?.SourceName ?? property.Name;

            if (data.TryGetValue(sourceKey, out var value) && value != null)
            {
                try
                {
                    // Check for custom converter
                    var converterAttr = property.GetCustomAttribute<DbConverterAttribute>();
                    if (converterAttr != null && converterAttr.ConverterType != null && converterAttr.MethodName != null)
                    {
                        var converterMethod = converterAttr.ConverterType.GetMethod(
                            converterAttr.MethodName,
                            BindingFlags.Public | BindingFlags.Static
                        );

                        if (converterMethod != null)
                        {
                            var convertedValue = converterMethod.Invoke(null, new[] { value });
                            if (convertedValue != null)
                            {
                                property.SetValue(entity, convertedValue);
                            }
                            continue;
                        }
                    }

                    // Default conversion
                    var stringValue = value.ToString();
                    if (!string.IsNullOrEmpty(stringValue) && property.CanWrite)
                    {
                        property.SetValue(entity, stringValue);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting property {property.Name}: {ex.Message}");
                }
            }
            else if (propAttr?.IsRequired == true)
            {
                // Use default value if provided
                if (propAttr.DefaultValue != null && property.CanWrite)
                {
                    property.SetValue(entity, propAttr.DefaultValue.ToString());
                }
                else
                {
                    Console.WriteLine($"Warning: Required property {property.Name} (source: {sourceKey}) not found in data");
                }
            }
        }
    }

    /// <summary>
    /// Populates an entity from a dictionary using PropertyMetadata configuration (metadata-driven approach)
    /// This version doesn't require DbEntity attributes - uses runtime metadata instead
    /// Iterates over the data dictionary and maps fields based on metadata configuration
    /// </summary>
    /// <typeparam name="T">The entity type to populate</typeparam>
    /// <param name="entity">The entity instance to populate</param>
    /// <param name="data">Source data dictionary</param>
    /// <param name="metadata">PropertyMetadata dictionary defining the mapping rules</param>
    /// <returns>The populated entity (important for value types/structs)</returns>
    private static T PopulateDbEntityFromDictionary<T>(Dictionary<string, object> data, Dictionary<string, PropertyMetadata> metadata)
    {
        T dbEquity = Activator.CreateInstance<T>();

        var type = typeof(T);

        // Create reverse lookup: sourceName -> PropertyMetadata
        Dictionary<string, PropertyMetadata> sourceToMetadata = new Dictionary<string, PropertyMetadata>();
        foreach (PropertyMetadata metaEntry in metadata.Values)
        {
            if (!string.IsNullOrEmpty(metaEntry.SourceName))
            {
                sourceToMetadata[metaEntry.SourceName] = metaEntry;
            }
        }

        // Iterate over data dictionary
        foreach (var dataEntry in data)
        {
            string sourceFieldName = dataEntry.Key;
            object? sourceValue = dataEntry.Value;

            // Find matching metadata by SourceName
            if (!sourceToMetadata.TryGetValue(sourceFieldName, out var meta))
            {
                Console.WriteLine($"{sourceFieldName}");
                continue;
            }

            // Skip if marked as ignored
            if (meta.Ignore)
                continue;

            // Determine target property name (use ColumnName from metadata)
            string targetPropertyName = meta.ColumnName;
            if (string.IsNullOrEmpty(targetPropertyName))
                continue;

            // Find the property on the target entity
            PropertyInfo? property = type.GetProperty(targetPropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanWrite)
            {
                // Try finding property by the metadata key name as fallback
                foreach (var metaKvp in metadata)
                {
                    if (metaKvp.Value.SourceName == sourceFieldName)
                    {
                        property = type.GetProperty(metaKvp.Key, BindingFlags.Public | BindingFlags.Instance);
                        if (property != null && property.CanWrite)
                            break;
                    }
                }

                if (property == null || !property.CanWrite)
                    continue;
            }

            // Handle null or empty values
            if (sourceValue == null || (sourceValue is string s && string.IsNullOrWhiteSpace(s)))
            {
                // Use default value if specified in metadata
                if (meta.DefaultValue != null)
                {
                    sourceValue = meta.DefaultValue;
                }
                else if (meta.IsRequired)
                {
                    Console.WriteLine($"Warning: Required property {targetPropertyName} (source: {sourceFieldName}) has null/empty value");
                    continue;
                }
                else
                {
                    continue;
                }
            }

            try
            {
                // Check for custom converter
                if (meta.ConverterType != null && !string.IsNullOrEmpty(meta.ConverterMethodName))
                {
                    var converterMethod = meta.ConverterType.GetMethod(
                        meta.ConverterMethodName,
                        BindingFlags.Public | BindingFlags.Static
                    );

                    if (converterMethod != null)
                    {
                        var convertedValue = converterMethod.Invoke(null, new[] { sourceValue });
                        if (convertedValue != null)
                        {
                            property.SetValue(dbEquity, convertedValue);
                        }
                        continue;
                    }
                }

                // Convert value to target property type
                var convertedVal = ConvertValue(sourceValue, property.PropertyType, meta.Format);
                if (convertedVal != null)
                {
                    property.SetValue(dbEquity, convertedVal);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting property {targetPropertyName} from source {sourceFieldName}: {ex.Message}");
            }
        }

        // Check for required fields that weren't in the data
        foreach (var metaEntry in metadata)
        {
            var meta = metaEntry.Value;
            if (meta.IsRequired && !string.IsNullOrEmpty(meta.SourceName))
            {
                if (!data.ContainsKey(meta.SourceName))
                {
                    Console.WriteLine($"Warning: Required field {meta.SourceName} (maps to {meta.ColumnName}) not found in source data");
                }
            }
        }

        return dbEquity;
    }

    /// <summary>
    /// Populates an entity from a dictionary using PropertyMetadata configuration (metadata-driven approach)
    /// This version doesn't require DbEntity attributes - uses runtime metadata instead
    /// </summary>
    /// <typeparam name="T">The entity type to populate</typeparam>
    /// <param name="entity">The entity instance to populate</param>
    /// <param name="data">Source data dictionary</param>
    /// <param name="metadata">PropertyMetadata dictionary defining the mapping rules</param>
    /// <returns>The populated entity (important for value types/structs)</returns>
    private static T PopulateFromDictionaryBy2<T>(T entity, Dictionary<string, object> data, Dictionary<string, PropertyMetadata> metadata)
    {
        var type = typeof(T);

        foreach (KeyValuePair<string, PropertyMetadata> metaEntry in metadata)
        {
            string propertyName = metaEntry.Key;
            PropertyMetadata meta = metaEntry.Value;

            // Skip if marked as ignored
            if (meta.Ignore)
                continue;

            // Find the property on the target entity
            PropertyInfo? property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanWrite)
                continue;

            // Get value from source data
            object? sourceValue = null;
            if (!string.IsNullOrEmpty(meta.SourceName) && data.ContainsKey(meta.SourceName))
            {
                sourceValue = data[meta.SourceName];
            }

            // Use default value if source is null/empty
            if (sourceValue == null || (sourceValue is string s && string.IsNullOrWhiteSpace(s)))
            {
                if (meta.DefaultValue != null)
                {
                    sourceValue = meta.DefaultValue;
                }
                else if (meta.IsRequired)
                {
                    Console.WriteLine($"Warning: Required property {propertyName} (source: {meta.SourceName}) not found in data");
                    continue;
                }
                else
                {
                    continue;
                }
            }

            try
            {
                // Check for custom converter
                if (meta.ConverterType != null && !string.IsNullOrEmpty(meta.ConverterMethodName))
                {
                    var converterMethod = meta.ConverterType.GetMethod(
                        meta.ConverterMethodName,
                        BindingFlags.Public | BindingFlags.Static
                    );

                    if (converterMethod != null)
                    {
                        var convertedValue = converterMethod.Invoke(null, new[] { sourceValue });
                        if (convertedValue != null)
                        {
                            property.SetValue(entity, convertedValue);
                        }
                        continue;
                    }
                }

                // Convert value to target property type
                var convertedVal = ConvertValue(sourceValue, property.PropertyType, meta.Format);
                if (convertedVal != null)
                {
                    property.SetValue(entity, convertedVal);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting property {propertyName}: {ex.Message}");
            }
        }

        return entity;
    }



    /// <summary>
    /// Converts a value to the target type with optional format support
    /// </summary>
    private static object? ConvertValue(object? value, Type targetType, string? format = null)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        // Handle JsonElement from System.Text.Json
        if (value is System.Text.Json.JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Null)
                return null;

            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                value = jsonElement.GetString();
            else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                    return jsonElement.GetDecimal();
                if (targetType == typeof(int) || targetType == typeof(int?))
                    return jsonElement.GetInt32();
                if (targetType == typeof(long) || targetType == typeof(long?))
                    return jsonElement.GetInt64();
                if (targetType == typeof(double) || targetType == typeof(double?))
                    return jsonElement.GetDouble();
            }
            else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.True)
                return true;
            else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.False)
                return false;
        }

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // String type
        if (underlyingType == typeof(string))
        {
            return value!.ToString();
        }

        // Decimal type
        if (underlyingType == typeof(decimal))
        {
            if (value is string strVal && decimal.TryParse(strVal, out var decResult))
                return decResult;
            if (value is decimal dec)
                return dec;
            try { return Convert.ToDecimal(value); } catch { return null; }
        }

        // Integer types
        if (underlyingType == typeof(int))
        {
            if (value is string strVal && int.TryParse(strVal, out var intResult))
                return intResult;
            try { return Convert.ToInt32(value); } catch { return null; }
        }

        if (underlyingType == typeof(long))
        {
            if (value is string strVal && long.TryParse(strVal, out var longResult))
                return longResult;
            try { return Convert.ToInt64(value); } catch { return null; }
        }

        // Double type
        if (underlyingType == typeof(double))
        {
            if (value is string strVal && double.TryParse(strVal, out var dblResult))
                return dblResult;
            try { return Convert.ToDouble(value); } catch { return null; }
        }

        // Boolean type
        if (underlyingType == typeof(bool))
        {
            if (value is string strVal && bool.TryParse(strVal, out var boolResult))
                return boolResult;
            try { return Convert.ToBoolean(value); } catch { return null; }
        }

        // DateTime type
        if (underlyingType == typeof(DateTime))
        {
            if (value is string strVal)
            {
                // Try with format if provided
                if (!string.IsNullOrEmpty(format) && DateTime.TryParseExact(strVal, format, null, System.Globalization.DateTimeStyles.None, out var formattedResult))
                    return formattedResult;

                // Try standard parsing
                if (DateTime.TryParse(strVal, out var dateResult))
                    return dateResult;

                // Try Unix timestamp
                if (long.TryParse(strVal, out var timestamp))
                    return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }

            // Handle Unix timestamp as number
            if (value is long || value is int)
            {
                var timestamp = Convert.ToInt64(value);
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }
        }

        // Default: try direct conversion
        try
        {
            return Convert.ChangeType(value, underlyingType);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts PropertyMetadata from DbProperty attributes (for hybrid approach)
    /// Allows using attributes to define metadata that can then be modified at runtime
    /// </summary>
    private static Dictionary<string, PropertyMetadata> ExtractMetadata<T>()
    {
        var metadata = new Dictionary<string, PropertyMetadata>();
        var type = typeof(T);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propAttr = property.GetCustomAttribute<DbPropertyAttribute>();
            if (propAttr == null)
                continue;

            var converterAttr = property.GetCustomAttribute<DbConverterAttribute>();

            metadata[property.Name] = new PropertyMetadata
            {
                SourceName = propAttr.SourceName ?? property.Name,
                ColumnName = propAttr.ColumnName ?? property.Name,
                IsRequired = propAttr.IsRequired,
                Ignore = propAttr.Ignore,
                DefaultValue = propAttr.DefaultValue,
                Format = propAttr.Format,
                ConverterType = converterAttr?.ConverterType,
                ConverterMethodName = converterAttr?.MethodName
            };
        }

        return metadata;
    }

    /// <summary>
    /// Gets the database table name from DbEntity attribute
    /// </summary>
    private static string? GetTableName<T>()
    {
        var type = typeof(T);
        var entityAttr = type.GetCustomAttribute<DbEntityAttribute>();
        return entityAttr?.TableName;
    }

    /// <summary>
    /// Gets the data source from DbEntity attribute
    /// </summary>
    private static string? GetSource<T>()
    {
        var type = typeof(T);
        var entityAttr = type.GetCustomAttribute<DbEntityAttribute>();
        return entityAttr?.Source;
    }

    /// <summary>
    /// Gets all properties marked as required
    /// </summary>
    private static List<PropertyInfo> GetRequiredProperties<T>()
    {
        var type = typeof(T);
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<DbPropertyAttribute>()?.IsRequired == true)
            .ToList();
    }

    /// <summary>
    /// Gets all properties NOT marked to ignore
    /// </summary>
    private static List<PropertyInfo> GetMappableProperties<T>()
    {
        var type = typeof(T);
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<DbPropertyAttribute>()?.Ignore != true)
            .ToList();
    }

    /// <summary>
    /// Gets the database column name for a property
    /// </summary>
    private static string GetColumnName(PropertyInfo property)
    {
        var propAttr = property.GetCustomAttribute<DbPropertyAttribute>();
        return propAttr?.ColumnName ?? property.Name;
    }

    /// <summary>
    /// Gets the source property name for API mapping
    /// </summary>
    private static string GetSourceName(PropertyInfo property)
    {
        var propAttr = property.GetCustomAttribute<DbPropertyAttribute>();
        return propAttr?.SourceName ?? property.Name;
    }

    /// <summary>
    /// Validates an entity based on DbValidation attributes
    /// </summary>
    private static (bool IsValid, List<string> Errors) ValidateEntity<T>(T entity)
    {
        var errors = new List<string>();
        var type = typeof(T);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var validationAttrs = property.GetCustomAttributes<DbValidationAttribute>();
            var value = property.GetValue(entity);

            foreach (var validation in validationAttrs)
            {
                bool isValid = validation.ValidationType switch
                {
                    "Range" => ValidateRange(value, validation.MinValue, validation.MaxValue),
                    "StringLength" => ValidateStringLength(value?.ToString(), validation.MinValue, validation.MaxValue),
                    "Regex" => ValidateRegex(value?.ToString(), validation.Pattern),
                    _ => true
                };

                if (!isValid)
                {
                    errors.Add(validation.ErrorMessage ?? $"Validation failed for property {property.Name}");
                }
            }
        }

        return (errors.Count == 0, errors);
    }

    private static bool ValidateRange(object? value, object? minValue, object? maxValue)
    {
        if (value == null) return true;

        try
        {
            var numValue = Convert.ToDecimal(value);
            var min = minValue != null ? Convert.ToDecimal(minValue) : decimal.MinValue;
            var max = maxValue != null ? Convert.ToDecimal(maxValue) : decimal.MaxValue;

            return numValue >= min && numValue <= max;
        }
        catch
        {
            return false;
        }
    }

    private static bool ValidateStringLength(string? value, object? minValue, object? maxValue)
    {
        if (value == null) return true;

        var length = value.Length;
        var min = minValue != null ? Convert.ToInt32(minValue) : 0;
        var max = maxValue != null ? Convert.ToInt32(maxValue) : int.MaxValue;

        return length >= min && length <= max;
    }

    private static bool ValidateRegex(string? value, string? pattern)
    {
        if (value == null || pattern == null) return true;

        return System.Text.RegularExpressions.Regex.IsMatch(value, pattern);
    }

    /// <summary>
    /// Creates a mapping dictionary from entity properties to database columns
    /// </summary>
    private static Dictionary<string, string> GetPropertyToColumnMap2<T>()
    {
        var type = typeof(T);
        var map = new Dictionary<string, string>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propAttr = property.GetCustomAttribute<DbPropertyAttribute>();
            if (propAttr?.Ignore == true)
                continue;

            var columnName = propAttr?.ColumnName ?? property.Name;
            map[property.Name] = columnName;
        }

        return map;
    }

    /// <summary>
    /// Creates a mapping dictionary from API source names to entity properties
    /// </summary>
    private static Dictionary<string, string> GetSourceToPropertyMap2<T>()
    {
        var type = typeof(T);
        var map = new Dictionary<string, string>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propAttr = property.GetCustomAttribute<DbPropertyAttribute>();
            if (propAttr?.Ignore == true)
                continue;

            var sourceName = propAttr?.SourceName ?? property.Name;
            map[sourceName] = property.Name;
        }

        return map;
    }
}
