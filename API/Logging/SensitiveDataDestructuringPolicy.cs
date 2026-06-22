using Serilog.Core;
using Serilog.Events;
using System.Reflection;

namespace API.Logging
{
    public class SensitiveDataDestructuringPolicy : IDestructuringPolicy
    {
        private readonly string[] _sensitiveProperties = { "Password", "Token", "HashSecret", "CardNumber", "ClientSecret" };

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            if (value == null)
            {
                result = null!;
                return false;
            }

            var type = value.GetType();
            
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum || type == typeof(decimal) || type == typeof(DateTime))
            {
                result = null!;
                return false;
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (properties.Length == 0)
            {
                result = null!;
                return false;
            }

            var structureProperties = new List<LogEventProperty>();

            foreach (var property in properties)
            {
                if (!property.CanRead) continue;

                try
                {
                    var propValue = property.GetValue(value);
                    if (propValue == null)
                    {
                        structureProperties.Add(new LogEventProperty(property.Name, new ScalarValue(null)));
                        continue;
                    }

                    if (_sensitiveProperties.Any(p => property.Name.Contains(p, StringComparison.OrdinalIgnoreCase)))
                    {
                        structureProperties.Add(new LogEventProperty(property.Name, new ScalarValue("***MASKED***")));
                    }
                    else
                    {
                        structureProperties.Add(new LogEventProperty(property.Name, propertyValueFactory.CreatePropertyValue(propValue, true)));
                    }
                }
                catch
                {
                }
            }

            result = new StructureValue(structureProperties, type.Name);
            return true;
        }
    }
}
