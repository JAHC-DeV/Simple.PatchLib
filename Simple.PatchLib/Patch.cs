using Simple.PatchLib.Converters;
using System.Text.Json;

namespace Simple.PatchLib;
public sealed class Patch<TModel> : Dictionary<string, object> where TModel : class
{
    public TModel Apply(TModel model)
    {
        var typeModel = typeof(TModel);

        foreach (var item in this)
        {
            var property = typeModel.GetProperties()
                        .FirstOrDefault(p => string.Equals(p.Name, item.Key, StringComparison.OrdinalIgnoreCase)); ;
            if (property is not null)
            {
                var propertyType = property.PropertyType;                
                var jsonEle = (JsonElement)item.Value;
                var deserializedValue = JsonSerializer.Deserialize(jsonEle.GetRawText(), propertyType, new JsonSerializerOptions
                {
                    Converters = { new DateTimeConverter("d/M/yyyy HH:mm:ss") },
                    PropertyNameCaseInsensitive = true,
                });
                object convertedValue;

                // Manejar el caso de tipos nullable
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Obtener el tipo base de Nullable
                    var underlyingType = Nullable.GetUnderlyingType(propertyType);
                    // Convertir el valor a Nullable
                    convertedValue = Convert.ChangeType(deserializedValue, underlyingType);
                }
                else
                    convertedValue = Convert.ChangeType(deserializedValue, propertyType);
                // Manejar la conversión de tipos correctamente


                // Asignar el valor convertido a la propiedad
                property.SetValue(model, convertedValue);
            }
        }

        return model;
    }
}




