using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using InfluxData.Net.InfluxDb.Models.Responses;

namespace InfluxDb.Extensions.Tests {
    internal static class InfluxJsonUtf8Extensions {

        /// <summary>
        /// 使用 <see cref="Utf8JsonWriter" /> 写入 Serie Json
        /// </summary>
        /// <param name="serie"></param>
        /// <param name="writer"></param>
        /// <param name="naming"></param>
        public static void WriteToJsonUtf8 (this Serie serie, Utf8JsonWriter writer, Func<string, string> naming = null) {
            // var names = serie.Columns.Select (n => naming (n)).ToArray ();
            string[] names;
            if (naming == null) {
                names = serie.Columns.ToArray ();
            } else {
                names = serie.Columns.Select (n => naming (n)).ToArray ();
            }

            writer.WriteStartArray ();
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
            writer.WriteEndArray ();
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