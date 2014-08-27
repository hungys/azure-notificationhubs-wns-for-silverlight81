using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Messaging
{
    class ConnectionStringParser
    {
        private enum ParserState
        {
            EXPECT_KEY, EXPECT_ASSIGNMENT, EXPECT_VALUE, EXPECT_SEPARATOR
        }

        private String _value;
        private int _pos;
        private ParserState _state;

        public static Dictionary<String, String> Parse(String connectionString)
        {
            ConnectionStringParser connectionStringParser = new ConnectionStringParser(connectionString);
            return connectionStringParser.parse();
        }

        private ConnectionStringParser(String value)
        {
            this._value = value;
            this._pos = 0;
            this._state = ParserState.EXPECT_KEY;
        }

        private Dictionary<String, String> parse()
        {
            Dictionary<String, String> result = new Dictionary<String, String>();
            String key = null;
            String value = null;
            while (true)
            {
                this.skipWhitespaces();

                if (this._pos == this._value.Length && this._state != ConnectionStringParser.ParserState.EXPECT_VALUE)
                {
                    break;
                }

                switch (this._state)
                {
                    case ParserState.EXPECT_KEY:
                        key = this.extractKey();
                        this._state = ConnectionStringParser.ParserState.EXPECT_ASSIGNMENT;
                        break;

                    case ParserState.EXPECT_ASSIGNMENT:
                        this.skipOperator('=');
                        this._state = ConnectionStringParser.ParserState.EXPECT_VALUE;
                        break;

                    case ParserState.EXPECT_VALUE:
                        value = this.extractValue();
                        this._state = ConnectionStringParser.ParserState.EXPECT_SEPARATOR;
                        result.Add(key, value);
                        key = null;
                        value = null;
                        break;
                    default:
                        this.skipOperator(';');
                        this._state = ConnectionStringParser.ParserState.EXPECT_KEY;
                        break;
                }
            }

            if (this._state == ConnectionStringParser.ParserState.EXPECT_ASSIGNMENT)
            {
                throw this.createException(this._pos, "Missing character %s", "=");
            }

            return result;
        }

        private ArgumentException createException(int position, String errorString, params Object[] args)
        {
            errorString = String.Format(errorString, args);
            errorString = String.Format("Error parsing connection string: %s. Position %s", errorString, this._pos);

            errorString = String.Format("Invalid connection string: %s.", errorString);

            return new ArgumentException(errorString);
        }

        private void skipWhitespaces()
        {
            while (this._pos < this._value.Length && Char.IsWhiteSpace(this._value[this._pos]))
            {
                this._pos++;
            }
        }

        private String extractKey()
        {
            int pos = this._pos;
            char c = this._value[this._pos++];
            String text;

            if (c == '"' || c == '\'')
            {
                text = this.extractString(c);
            }
            else
            {
                if (c == ';' || c == '=')
                {
                    throw this.createException(pos, "Missing key");
                }
                while (this._pos < this._value.Length)
                {
                    c = this._value[this._pos];
                    if (c == '=')
                    {
                        break;
                    }
                    this._pos++;
                }
                text = this._value.Substring(pos, this._pos - pos).Trim();
            }
            if (text.Length == 0)
            {
                throw this.createException(pos, "Empty key");
            }
            return text;
        }

        private String extractString(char quote)
        {
            int pos = this._pos;
            while (this._pos < this._value.Length && this._value[this._pos] != quote)
            {
                this._pos++;
            }

            if (this._pos == this._value.Length)
            {
                throw this.createException(this._pos, "Missing character", quote);
            }

            return this._value.Substring(pos, this._pos++ - pos + 1);
        }

        private void skipOperator(char operatorChar)
        {
            if (this._value[this._pos] != operatorChar)
            {
                throw this.createException(this._pos, "Missing character", operatorChar);
            }

            this._pos++;
        }

        private String extractValue()
        {
            String result = "";

            if (this._pos < this._value.Length)
            {
                char c = this._value[this._pos];

                if (c == '\'' || c == '"')
                {
                    this._pos++;
                    result = this.extractString(c);
                }
                else
                {
                    int pos = this._pos;
                    bool flag = false;
                    while (this._pos < this._value.Length && !flag)
                    {
                        c = this._value[this._pos];
                        char c2 = c;
                        if (c2 == ';')
                        {
                            if (this.isStartWithKnownKey())
                            {
                                flag = true;
                            }
                            else
                            {
                                this._pos++;
                            }
                        }
                        else
                        {
                            this._pos++;
                        }
                    }
                    int len = this._value.Length;
                    result = this._value.Substring(pos, this._pos - pos).Trim();
                }
            }
            return result;
        }

        private bool isStartWithKnownKey()
        {
            return this._value.Length <= this._pos + 1 || this._value.Substring(this._pos + 1).ToLower().StartsWith("endpoint")
                    || this._value.Substring(this._pos + 1).ToLower().StartsWith("stsendpoint")
                    || this._value.Substring(this._pos + 1).ToLower().StartsWith("sharedsecretissuer")
                    || this._value.Substring(this._pos + 1).ToLower().StartsWith("sharedsecretvalue")
                    || this._value.Substring(this._pos + 1).ToLower().StartsWith("sharedaccesskeyname")
                    || this._value.Substring(this._pos + 1).ToLower().StartsWith("sharedaccesskey");
        }
    }
}
