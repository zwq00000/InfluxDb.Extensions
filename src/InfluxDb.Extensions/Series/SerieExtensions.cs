using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using InfluxData.Net.InfluxDb.Models.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace InfluxDb.Extensions {
    public static class SerieExtensions {

        public static IEnumerable<TModel> As<TModel> (this IEnumerable<Serie> series) where TModel : new () {
            foreach (var serie in series) {
                foreach (var item in serie.As<TModel> ()) {
                    yield return item;
                }
            }
        }

        public static IEnumerable<TModel> As<TModel> (this Serie serie) where TModel : new () {
            var setters = PropertiesCache<TModel>.GetSetters ();
            var matched = serie.Columns.Select (c => setters.TryGetValue (c, out var s) ? s : null).ToArray ();

            foreach (var value in serie.Values) {
                var instance = new TModel ();
                foreach (var tag in serie.Tags) {
                    if (setters.TryGetValue (tag.Key, out var setter)) {
                        setter (instance, tag.Value);
                    }
                }

                for (var i = 0; i < serie.Columns.Count; i++) {
                    var col = serie.Columns[i];
                    var setter = matched[i];
                    if (setter != null) {
                        setter (instance, value[i]);
                    }
                }
                yield return instance;
            }
        }

        #region Write To Json

        public static async Task WriteToJsonAsync (this PageResult<Serie> pageResult, JsonWriter writer, NamingStrategy naming = null) {
            await writer.WriteStartObjectAsync ();
            await writer.WritePropertyNameAsync (naming.GetName (nameof (pageResult.Page)));
            await pageResult.Page.WriteToJsonAsync (writer, naming);
            await writer.WritePropertyNameAsync (naming.GetName (nameof (pageResult.Values)));
            await pageResult.Values.WriteToJsonAsync (writer, naming);
            await writer.WriteEndObjectAsync ();
            await writer.FlushAsync ();
        }

        private static string GetName (this NamingStrategy naming, string name) {
            return naming != null? naming.GetPropertyName (name, false) : name;
        }

        private static async Task WriteToJsonAsync (this Paging pageing, JsonWriter writer, NamingStrategy naming = null) {
            await writer.WriteStartObjectAsync ();

            await writer.WritePropertyNameAsync (naming.GetName (nameof (Paging.Page)));
            await writer.WriteValueAsync (pageing.Page);

            await writer.WritePropertyNameAsync (naming.GetName (nameof (Paging.Pages)));
            await writer.WriteValueAsync (pageing.Pages);

            await writer.WritePropertyNameAsync (naming.GetName (nameof (Paging.PageSize)));
            await writer.WriteValueAsync (pageing.PageSize);

            await writer.WritePropertyNameAsync (naming.GetName (nameof (Paging.Total)));
            await writer.WriteValueAsync (pageing.Total);

            await writer.WriteEndObjectAsync ();
        }

        public static async Task WriteToJsonAsync (this IEnumerable<Serie> series, JsonWriter writer, NamingStrategy naming = null) {
            await writer.WriteStartArrayAsync ();
            foreach (var serie in series) {
                var names = serie.GetColumnsNames (naming);
                await writer.WriteSerieValuesAsync (serie, names);
            }
            await writer.WriteEndArrayAsync ();
        }

        private static string[] GetColumnsNames (this Serie serie, NamingStrategy naming = null) {
            if (naming == null) {
                return serie.Columns.ToArray ();
            } else {
                return serie.Columns.Select (n => naming.GetName (n)).ToArray ();
            }
        }

        public static void WriteToJson (this Serie serie, JsonWriter writer, NamingStrategy naming = null) {
            var names = serie.GetColumnsNames (naming);

            writer.WriteStartArray ();

            foreach (var value in serie.Values) {
                writer.WriteStartObject ();
                for (var i = 0; i < value.Count; i++) {
                    writer.WritePropertyName (names[i]);
                    writer.WriteValue (value[i]);
                }

                writer.WriteEndObject ();
            }
            writer.WriteEndArray ();
        }

        public static async Task WriteToJsonAsync (this Serie serie, JsonWriter writer, NamingStrategy naming = null) {
            var names = serie.GetColumnsNames (naming);
            await writer.WriteStartArrayAsync ();
            await writer.WriteSerieValuesAsync (serie, names);
            await writer.WriteEndArrayAsync ();
        }

        private static async Task WriteSerieValuesAsync (this JsonWriter writer, Serie serie, string[] names) {
            if (serie == null) {
                throw new ArgumentNullException (nameof (serie));
            }
            if (!serie.Values.Any ()) {
                return;
            }
            if (names.Length != serie.Values.First ().Count) {
                throw new ArgumentException ("names length must equal values count");
            }
            foreach (var value in serie.Values) {
                await writer.WriteStartObjectAsync ();

                foreach (var tag in serie.Tags) {
                    writer.WritePropertyName (tag.Key);
                    writer.WriteValue (tag.Value);
                }

                for (var i = 0; i < value.Count; i++) {
                    var val = value[i];
                    if (val != null) {
                        await writer.WritePropertyNameAsync (names[i]);
                        await writer.WriteValueAsync (val);
                    }
                }
                await writer.WriteEndObjectAsync ();
            }
        }

        #endregion
    }

    /// <summary>
    /// 类型属性和Setter 缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class PropertiesCache<T> {
        private static PropertyInfo[] _cache;

        private static IDictionary<string, Action<T, object>> _setters;

        public static PropertyInfo[] GetProperties () {
            if (_cache == null) {
                var type = typeof (T);
                var props = GetProperties (type);
                if (type.IsInterface) {
                    props = props.Concat (type.GetInterfaces ().SelectMany (i => GetProperties (i)));
                }
                _cache = props.ToArray ();
            }

            return _cache;
        }

        public static IDictionary<string, Action<T, object>> GetSetters () {
            if (_setters == null) {
                var properties = GetProperties ();
                _setters = properties.ToDictionary (p => p.Name, p => GetSetter (p), StringComparer.OrdinalIgnoreCase);
            }

            return _setters;
        }

        private static IEnumerable<PropertyInfo> GetProperties (Type type) {
            return type.GetRuntimeProperties ()
                .Where (p => p.CanRead && CanConverted (p.PropertyType));
        }

        private static Action<T, object> GetSetter (PropertyInfo property) {
            if (property.PropertyType.IsEnum) {
                return GetEnumSetter (property);
            }
            var param_instance = Expression.Parameter (typeof (T));
            var param_value = Expression.Parameter (typeof (object));
            var body_value = Expression.Convert (param_value, property.PropertyType);
            var body_call = Expression.Call (param_instance, property.GetSetMethod (), body_value);
            return Expression.Lambda<Action<T, object>> (body_call, param_instance, param_value).Compile ();
        }

        private static MethodInfo enumParseMethod = typeof (Enum).GetMethod (nameof (Enum.Parse), new [] { typeof (string) });
        /// <summary>
        /// 生成枚举类型赋值Action
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static Action<T, object> GetEnumSetter (PropertyInfo property) {
            var param_instance = Expression.Parameter (typeof (T));
            var param_value = Expression.Parameter (typeof (object));
            var body_instance = Expression.Convert (param_instance, property.DeclaringType);
            var param_Converter = Expression.Convert (param_value, typeof (string));
            var parseMethod = enumParseMethod.MakeGenericMethod (property.PropertyType);
            var exp_right = Expression.Call (null, parseMethod, param_Converter);
            var body_call = Expression.Call (body_instance, property.GetSetMethod (), exp_right);
            return Expression.Lambda<Action<T, object>> (body_call, param_instance, param_value).Compile ();
        }

        /// <summary>
        /// 可以被转换的属性类型
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        private static bool CanConverted (Type propertyType) {
            return propertyType.IsValueType ||
                propertyType == typeof (string) ||
                propertyType.IsEnum ||
                propertyType == typeof (byte[]);
        }
    }
}