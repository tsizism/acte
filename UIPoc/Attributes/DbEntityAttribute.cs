using System;

namespace UIPooc.Attributes;

/// <summary>
/// Marks a struct or class as a database entity that can be mapped to/from external APIs
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
public class DbEntityAttribute : Attribute
{
    /// <summary>
    /// The name of the database table this entity maps to
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// The source API or data provider (e.g., "YahooFinance", "AlphaVantage")
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Description of the entity
    /// </summary>
    public string? Description { get; set; }

    public DbEntityAttribute()
    {
    }

    public DbEntityAttribute(string tableName)
    {
        TableName = tableName;
    }
}

/// <summary>
/// Marks a property for database/API mapping with metadata
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DbPropertyAttribute : Attribute
{
    /// <summary>
    /// The source property name from the API response (if different from property name)
    /// </summary>
    public string? SourceName { get; set; }

    /// <summary>
    /// The database column name (if different from property name)
    /// </summary>
    public string? ColumnName { get; set; }

    /// <summary>
    /// Whether this property is required/mandatory
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Whether this property is a primary key
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// The data type in the database (e.g., "decimal(18,2)", "varchar(100)")
    /// </summary>
    public string? DbType { get; set; }

    /// <summary>
    /// Format string for parsing/formatting (e.g., "yyyy-MM-dd", "0.00")
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Default value if the source data is missing or null
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Whether to ignore this property during mapping
    /// </summary>
    public bool Ignore { get; set; }

    /// <summary>
    /// Description of the property
    /// </summary>
    public string? Description { get; set; }

    public DbPropertyAttribute()
    {
    }

    public DbPropertyAttribute(string sourceName)
    {
        SourceName = sourceName;
    }
}

/// <summary>
/// Specifies a conversion method for complex property mapping
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DbConverterAttribute : Attribute
{
    /// <summary>
    /// The type containing the conversion method
    /// </summary>
    public Type? ConverterType { get; set; }

    /// <summary>
    /// The name of the static method to use for conversion
    /// Example: "ParseDecimal", "ConvertToDateTime"
    /// </summary>
    public string? MethodName { get; set; }

    public DbConverterAttribute(Type converterType, string methodName)
    {
        ConverterType = converterType;
        MethodName = methodName;
    }
}

/// <summary>
/// Marks properties that should be validated
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class DbValidationAttribute : Attribute
{
    /// <summary>
    /// Validation type (e.g., "Range", "StringLength", "Regex")
    /// </summary>
    public string ValidationType { get; set; }

    /// <summary>
    /// Minimum value for range validation
    /// </summary>
    public object? MinValue { get; set; }

    /// <summary>
    /// Maximum value for range validation
    /// </summary>
    public object? MaxValue { get; set; }

    /// <summary>
    /// Regex pattern for string validation
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Error message if validation fails
    /// </summary>
    public string? ErrorMessage { get; set; }

    public DbValidationAttribute(string validationType)
    {
        ValidationType = validationType;
    }
}
