using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedMandelbrotGraphics.Classes {

	// An object for parsing and creating JSON data.
	public static class Json {

		private static JsonValue _null = new JsonNullValue();

		public static JsonValue Null => _null;
		public static JsonObject Object => new JsonObject();
		public static JsonArray Array => new JsonArray();
		public static JsonLiteralValue Literal(string value) { return new JsonLiteralValue(value); }

		public static string JSEncode(string value) {
			return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\b", "\\b").Replace("\f", "\\f").Replace("\t", "\\t");
		}

		public abstract class JsonValue {

			public static implicit operator JsonValue(double value) { return new JsonFloatValue(value); }
			public static implicit operator JsonValue(float value) { return new JsonFloatValue(value); }
			public static implicit operator JsonValue(decimal value) { return new JsonFloatValue(value); }
			public static implicit operator JsonValue(bool value) { return new JsonBooleanValue(value); }
			public static implicit operator JsonValue(long value) { return new JsonFloatValue((double)value); }
			public static implicit operator JsonValue(int value) { return new JsonFloatValue((double)value); }
			public static implicit operator JsonValue(short value) { return new JsonFloatValue((double)value); }
			public static implicit operator JsonValue(byte value) { return new JsonFloatValue((double)value); }

			public static implicit operator JsonValue(string value) { return value != null ? new JsonStringValue(value) : Null; }
			public static implicit operator JsonValue(double? value) { return value.HasValue ? new JsonFloatValue(value.Value) : Null; }
			public static implicit operator JsonValue(decimal? value) { return value.HasValue ? new JsonFloatValue(value.Value) : Null; }
			public static implicit operator JsonValue(float? value) { return value.HasValue ? new JsonFloatValue(value.Value) : Null; }
			public static implicit operator JsonValue(bool? value) { return value.HasValue ? new JsonBooleanValue(value.Value) : Null; }
			public static implicit operator JsonValue(long? value) { return value.HasValue ? new JsonFloatValue((double)value.Value) : Null; }
			public static implicit operator JsonValue(int? value) { return value.HasValue ? new JsonFloatValue((double)value.Value) : Null; }
			public static implicit operator JsonValue(short? value) { return value.HasValue ? new JsonFloatValue((double)value.Value) : Null; }
			public static implicit operator JsonValue(byte? value) { return value.HasValue ? new JsonFloatValue((double)value.Value) : Null; }

			public bool IsArray => this is JsonArray;
			public bool IsObject => this is JsonObject;
			public bool IsBoolean => this is JsonBooleanValue;
			public bool IsString => this is JsonStringValue;
			public bool IsNumber => this is JsonFloatValue;
			public bool IsNull => this is JsonNullValue;

			public bool IsNullableBoolean => this.IsBoolean || this.IsNull;
			public bool IsNullableNumber => this.IsNumber || this.IsNull;

			private T As<T>() where T : JsonValue {
				T result = this as T;
				if (result == null) {
					throw new Exception("Attempt to get '" + this.GetType().Name + "' as '" + typeof(T).Name + "'.");
				}
				return result;
			}

			public JsonArray AsArray => As<JsonArray>();
			public JsonObject AsObject => As<JsonObject>();
			public bool AsBoolean => As<JsonBooleanValue>().Value;
			public string AsString => As<JsonStringValue>().Value;
			public double AsNumber => As<JsonFloatValue>().Value;
			public int AsInteger => (int)Math.Round(As<JsonFloatValue>().Value);

			public bool? AsNullableBoolean => this.IsNull ? (bool?)null : this.AsBoolean;
			public string AsNullableString => this.IsNull ? null : this.AsString;
			public double? AsNullableNumber => this.IsNull ? (double?)null : this.AsNumber;
			public int? AsNullableInteger => this.IsNull ? (int?)null : this.AsInteger;

			public JsonValue this[string name] => AsObject[name];
			public JsonValue this[int index] => AsArray[index];

		}

		public abstract class JsonValue<T> : JsonValue {

			public T Value { get; private set; }
			private Func<T, string> _getString;

			public JsonValue(T value, Func<T, string> getString) {
				Value = value;
				_getString = getString;
			}

			public override string ToString() {
				return _getString(Value);
			}

		}

		public class JsonFloatValue : JsonValue<double> {
			public JsonFloatValue(double value) : base(value, v => v.ToString(CultureInfo.InvariantCulture)) { }
			public JsonFloatValue(decimal value) : base((double)value, v => v.ToString(CultureInfo.InvariantCulture)) { }
		}

		public class JsonStringValue : JsonValue<string> {
			public JsonStringValue(string value) : base(value, v => "\"" + JSEncode(v) + "\"") { }
		}

		public class JsonBooleanValue : JsonValue<bool> {
			public JsonBooleanValue(bool value) : base(value, v => v ? "true" : "false") { }
		}

		public class JsonNullValue : JsonValue<int> {
			public JsonNullValue() : base(0, v => "null") { }
		}

		public class JsonLiteralValue : JsonValue<string> {
			public JsonLiteralValue(string value) : base(value, v => v) { }
		}

		public class JsonObject : JsonValue, IEnumerable<KeyValuePair<string, JsonValue>> {

			private Dictionary<string, JsonValue> _nodes;

			public JsonObject() {
				_nodes = new Dictionary<string, JsonValue>();
			}

			public bool HasKey(string name) {
				return _nodes.ContainsKey(name);
			}

			public new JsonValue this[string name] {
				get {
					JsonValue value;
					if (_nodes.TryGetValue(name, out value)) {
						return value;
					} else {
						throw new KeyNotFoundException("The key '" + name + "' was not present in the object.");
					}
					//return _nodes[name];
				}
				set {
					if (_nodes.ContainsKey(name)) {
						_nodes[name] = value;
					} else {
						Add(name, value);
					}
				}
			}

			public JsonObject Add(string name, JsonValue value) {
				_nodes.Add(name, value);
				return this;
			}

			public JsonObject Add<T>(IEnumerable<T> items, Func<T, string> getName, Func<T, JsonValue> getValue) {
				foreach (T item in items) {
					_nodes.Add(getName(item), getValue(item));
				}
				return this;
			}

			public JsonObject Add<T>(IEnumerable<KeyValuePair<string, T>> items) where T : JsonValue {
				foreach (KeyValuePair<string, T> item in items) {
					_nodes.Add(item.Key, item.Value);
				}
				return this;
			}

			public override string ToString() {
				return "{" + String.Join(",", _nodes.Select(n => "\"" + JSEncode(n.Key) + "\":" + n.Value.ToString())) + "}";
			}


			public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator() {
				return _nodes.GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

		}

		public class JsonArray : JsonValue, IEnumerable<JsonValue> {

			private List<JsonValue> _values;

			public JsonArray() {
				_values = new List<JsonValue>();
			}

			public int Length => _values.Count;

			public JsonArray Add(JsonValue value) {
				_values.Add(value);
				return this;
			}

			public JsonArray Add(params JsonValue[] values) {
				foreach (JsonValue value in values) {
					_values.Add(value);
				}
				return this;
			}

			public JsonArray Add<T>(IEnumerable<T> items, Func<T, JsonValue> get) {
				foreach (T item in items) {
					Add(get(item));
				}
				return this;
			}

			public new JsonValue this[int index] => _values[index];

			public override string ToString() {
				return "[" + String.Join(",", _values.Select(v => v.ToString())) + "]";
			}

			public IEnumerator<JsonValue> GetEnumerator() {
				return _values.GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			public IEnumerable<JsonObject> GetObjects() {
				foreach (JsonValue value in _values) {
					yield return value.AsObject;
				}
			}

			public IEnumerable<int> GetInts() {
				foreach (JsonValue value in _values) {
					yield return value.AsInteger;
				}
			}

			public IEnumerable<string> GetStrings() {
				foreach (JsonValue value in _values) {
					yield return value.AsString;
				}
			}

		}

		private class Parser {

			private string _text;
			private int _offset, _end;

			public Parser(string text) {
				_text = text;
				_offset = 0;
				_end = text.Length;
			}

			private static bool IsIdentifier(char c) {
				return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_';
			}

			private static bool Is(char c, string valid) {
				return valid.IndexOf(c) != -1;
			}

			private char Peek() {
				if (_offset == _end) throw new ApplicationException("Unexpected end of string.");
				return _text[_offset];
			}

			private char Read() {
				char c = Peek();
				_offset++;
				return c;
			}

			private void SkipWhiteSpace() {
				while (_offset < _end && Is(_text[_offset], " \r\n\t")) {
					_offset++;
				}
			}

			public string ParseName() {
				StringBuilder name = new StringBuilder();
				SkipWhiteSpace();
				Ensure("\"");
				char c;
				while (_offset < _end && (c = Read()) != '"') {
					name.Append(c);
				}
				return name.ToString();
			}

			private Exception CreateException(string message) {
				return new ApplicationException(message + " at position " + (_offset - 1) + ".");
			}

			private Exception CreateExpectedException(string expected, char found) {
				return CreateException("Expected " + expected + " but found " + found);
			}

			public JsonValue ParseValue(bool allowPrimitives) {
				SkipWhiteSpace();
				char c = Read();
				switch (c) {
					case '[': {
							JsonArray array = new JsonArray();
							SkipWhiteSpace();
							if (Peek() != ']') {
								bool cont = true;
								while (cont) {
									array.Add(ParseValue(true));
									SkipWhiteSpace();
									c = Read();
									switch (c) {
										case ',': SkipWhiteSpace(); break;
										case ']': cont = false; break;
										default: throw CreateExpectedException(", or ]", c);
									}
								}
							} else {
								Read();
							}
							return array;
						}
					case '{': {
							JsonObject obj = new JsonObject();
							SkipWhiteSpace();
							if (Peek() != '}') {
								bool cont = true;
								while (cont) {
									string name = ParseName();
									SkipWhiteSpace();
									Ensure(":");
									SkipWhiteSpace();
									JsonValue value = ParseValue(true);
									obj.Add(name, value);
									SkipWhiteSpace();
									c = Read();
									switch (c) {
										case ',': SkipWhiteSpace(); break;
										case '}': cont = false; break;
										default: throw CreateExpectedException(", or }", c);
									}
								}
							} else {
								Read();
							}
							return obj;
						}
					case '"': {
							if (!allowPrimitives) throw CreateException("String not allowed here");
							StringBuilder s = new StringBuilder();
							char x;
							while ((x = Read()) != '"') {
								if (x == '\\') {
									x = Read();
									switch (x) {
										case 'b': x = '\b'; break;
										case 'f': x = '\f'; break;
										case 'n': x = '\n'; break;
										case 'r': x = '\r'; break;
										case 't': x = '\t'; break;
										case 'u': {
												x = (char)Convert.ToInt64(new String(new char[] { Read(), Read(), Read(), Read() }), 16);
											}
											break;
									}
								}
								s.Append(x);
							}
							return new JsonStringValue(s.ToString());
						}
					case 't':
						if (!allowPrimitives) throw CreateException("Boolean not allowed here");
						Ensure("rue");
						return new JsonBooleanValue(true);
					case 'f':
						if (!allowPrimitives) throw CreateException("Boolean not allowed here");
						Ensure("alse");
						return new JsonBooleanValue(false);
					case 'n':
						if (!allowPrimitives) throw CreateException("Null not allowed here");
						Ensure("ull");
						return new JsonNullValue();
					default:
						if (!allowPrimitives) throw CreateException("Number not allowed here");
						if (Is(c, "0123456789.-")) {
							bool neg = false, fraction = false;
							double n = 0, frac = 0.1;
							if (c == '-') {
								neg = true;
							} else if (c == '.') {
								fraction = true;
							} else {
								n = c - '0';
							}
							while (Is(Peek(), fraction ? "0123456789Ee" : "0123456789.Ee")) {
								c = Read();
								if (c == '.') {
									fraction = true;
								} else if (c == 'E' || c == 'e') {
									bool eneg = false, first = true;
									int exp = 0;
									while (Is(Peek(), first ? "0123456789+-" : "0123456879")) {
										first = false;
										c = Read();
										if (c == '-') {
											eneg = true;
										} else if (c != '+') {
											exp = exp * 10 + (c - '0');
										}
									}
									n *= Math.Pow(10, eneg ? -exp : exp);
									break;
								} else if (fraction) {
									n = n + (c - '0') * frac;
									frac /= 10.0;
								} else {
									n = n * 10.0 + (c - '0');
								}
							}
							return new JsonFloatValue(neg ? -n : n);
						} else {
							throw CreateExpectedException(allowPrimitives ? "{, [, string, boolean, number or null" : "{ or [", c);
						}
				}
			}

			public void Ensure(string text) {
				SkipWhiteSpace();
				foreach (char t in text) {
					char c = Read();
					if (c != t) throw CreateExpectedException(t.ToString(), c);
				}
			}

		}

		public static JsonValue Parse(string text) {
			Parser parser = new Parser(text);
			return parser.ParseValue(false);
		}

	}

}
