using System.Reflection;
using UIPooc.Attributes;

namespace UIPooc.Helpers;

/// <summary>
/// Provides reflection-based mapping using DbEntity attributes
/// </summary>
public static class EntityMapperDepr
{
    /// <summary>
    /// Populates an entity from a dictionary using DbProperty attributes
    /// </summary>
    public static void PopulateFromDictionary<T>(T entity, Dictionary<string, object> data) where T : struct
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
    /// Gets the database table name from DbEntity attribute
    /// </summary>
    public static string? GetTableName<T>()
    {
        var type = typeof(T);
        var entityAttr = type.GetCustomAttribute<DbEntityAttribute>();
        return entityAttr?.TableName;
    }

    /// <summary>
    /// Gets the data source from DbEntity attribute
    /// </summary>
    public static string? GetSource<T>()
    {
        var type = typeof(T);
        var entityAttr = type.GetCustomAttribute<DbEntityAttribute>();
        return entityAttr?.Source;
    }

    /// <summary>
    /// Gets all properties marked as required
    /// </summary>
    public static List<PropertyInfo> GetRequiredProperties<T>()
    {
        var type = typeof(T);
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<DbPropertyAttribute>()?.IsRequired == true)
            .ToList();
    }

    /// <summary>
    /// Gets all properties NOT marked to ignore
    /// </summary>
    public static List<PropertyInfo> GetMappableProperties<T>()
    {
        var type = typeof(T);
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<DbPropertyAttribute>()?.Ignore != true)
            .ToList();
    }

    /// <summary>
    /// Gets the database column name for a property
    /// </summary>
    public static string GetColumnName(PropertyInfo property)
    {
        var propAttr = property.GetCustomAttribute<DbPropertyAttribute>();
        return propAttr?.ColumnName ?? property.Name;
    }

    /// <summary>
    /// Gets the source property name for API mapping
    /// </summary>
    public static string GetSourceName(PropertyInfo property)
    {
        var propAttr = property.GetCustomAttribute<DbPropertyAttribute>();
        return propAttr?.SourceName ?? property.Name;
    }

    /// <summary>
    /// Validates an entity based on DbValidation attributes
    /// </summary>
    public static (bool IsValid, List<string> Errors) ValidateEntity<T>(T entity)
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
    public static Dictionary<string, string> GetPropertyToColumnMap<T>()
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
    public static Dictionary<string, string> GetSourceToPropertyMap<T>()
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
