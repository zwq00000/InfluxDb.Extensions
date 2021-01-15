using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using InfluxData.Net.InfluxDb.Models.Responses;

namespace InfluxDb.Extensions.Tests {
    internal static class InfluxJsonUtf8Extensions {

        public static void ReadFromJson (string json) {
            var bytes = Encoding.UTF8.GetBytes (json);
            var reader = new Utf8JsonReader (bytes, false, new JsonReaderState ());
            while (reader.Read ()) {

                Debug.WriteLine ("->"+ reader.TokenType);

                switch (reader.TokenType) {
                    case JsonTokenType.PropertyName:
                        Console.WriteLine (reader.GetString ());
                        break;
                    case JsonTokenType.StartArray:
                        ReadColumns (reader);
                        break;
                }
            }
        }

        private static void ReadColumns (this Utf8JsonReader reader) {
            if (reader.TokenType == JsonTokenType.StartArray) {
                while (reader.Read ()) {
                    if (reader.TokenType == JsonTokenType.EndArray) {
                        break;
                    }
                    switch (reader.TokenType) {
                        case JsonTokenType.String:
                            Debug.Write (reader.GetString ());
                            break;
                    }
                }
            }
        }

        public static void WriteToJsonUtf8 (this IEnumerable<Serie> series, Utf8JsonWriter writer, Func<string, string> naming = null) {
            writer.WriteStartArray ();
            foreach (var serie in series) {
                serie.WriteToJsonUtf8Internal (writer, naming);
            }
            writer.WriteEndArray ();
        }

        /// <summary>
        /// 使用 <see cref="Utf8JsonWriter" /> 写入 Serie Json
        /// </summary>
        /// <param name="serie"></param>
        /// <param name="writer"></param>
        /// <param name="naming"></param>
        public static void WriteToJsonUtf8 (this Serie serie, Utf8JsonWriter writer, Func<string, string> naming = null) {
            writer.WriteStartArray ();
            serie.WriteToJsonUtf8Internal (writer, naming);
            writer.WriteEndArray ();
        }

        private static string[] GetColumnsNames (this Serie serie, Func<string, string> naming = null) {
            if (naming == null) {
                return serie.Columns.ToArray ();
            } else {
                return serie.Columns.Select (n => naming (n)).ToArray ();
            }
        }

        private static void WriteToJsonUtf8Internal (this Serie serie, Utf8JsonWriter writer, Func<string, string> naming = null) {
            string[] names = serie.GetColumnsNames (naming);

            var cache = new SerieWriteHelper (serie);
            foreach (var value in serie.Values) {
                writer.WriteStartObject ();

                foreach (var tag in serie.Tags) {
                    writer.WritePropertyName (tag.Key);
                    writer.WriteStringValue (tag.Value);
                }

                cache.WriteProperties (writer, names);

                writer.WriteEndObject ();
            }
        }

        private class SerieWriteHelper {
            private readonly Serie serie;

            public SerieWriteHelper (Serie serie) {
                this.serie = serie;
            }

            private TypeCode[] GetValueTypes () {
                var value = this.serie.Values.First ();
                var types = new TypeCode[value.Count];
                for (var i = 0; i < value.Count; i++) {
                    if (value[i] == null) {
                        continue;
                    }
                    types[i] = Type.GetTypeCode (value[i].GetType ());
                }
                return types;
            }

            public void WriteProperties (Utf8JsonWriter writer, string[] names) {
                // var types = GetValueTypes ();
                foreach (var value in serie.Values) {
                    WriteProperty (writer, names, value);
                }
            }

            private void WriteProperty (Utf8JsonWriter writer, string[] names, IList<object> value) {
                for (int i = 0; i < value.Count; i++) {
                    var name = names[i];
                    var val = value[i];
                    if (val == null) {
                        continue;
                    }
                    var typeCode = Type.GetTypeCode (val.GetType ());
                    switch (typeCode) {
                        case TypeCode.String:
                            writer.WriteString (name, val.ToString ());
                            break;
                        case TypeCode.Boolean:
                            writer.WriteBoolean (name, (bool) val);
                            break;
                        case TypeCode.DateTime:
                            writer.WriteString (name, (DateTime) val);
                            break;
                        case TypeCode.Int16:
                            writer.WriteNumber (name, (Int16) val);
                            break;
                        case TypeCode.UInt16:
                            writer.WriteNumber (name, (UInt16) val);
                            break;
                        case TypeCode.Int32:
                            writer.WriteNumber (name, (int) val);
                            break;
                        case TypeCode.UInt32:
                            writer.WriteNumber (name, (UInt32) val);
                            break;
                        case TypeCode.Int64:
                            writer.WriteNumber (name, (Int64) val);
                            break;
                        case TypeCode.UInt64:
                            writer.WriteNumber (name, (UInt64) val);
                            break;
                        case TypeCode.Single:
                            writer.WriteNumber (name, (short) val);
                            break;
                        case TypeCode.Double:
                            writer.WriteNumber (name, (double) val);
                            break;
                        case TypeCode.Decimal:
                            writer.WriteNumber (name, (decimal) val);
                            break;
                        case TypeCode.Object:
                            writer.WriteString (name, val.ToString ());
                            break;
                        default:
                            throw new NotSupportedException ($"Not support typeCode:{typeCode}, {val}");
                    }
                }
            }
        }
    }
}