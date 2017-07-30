using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace make2vc
{
    public class Variables
    {
        private Dictionary<string, string> _variables = new Dictionary<string, string>();

        public void Set(string name, string value)
        {
            _variables[name] = value;
        }

        public string Expand(string text)
        {
            int dummy;
            return Expand(text, '\0', 0, out dummy);
        }

        public string Expand(string text, char terminator, int startPos, out int endPos)
        {
            var sb = new StringBuilder();
            endPos = startPos;
            while (endPos < text.Length)
            {
                if (text[endPos] == terminator)
                {
                    endPos++;
                    break;
                }
                else if (text[endPos] == '$')
                {
                    if (endPos + 1 < text.Length && 
                        text[endPos + 1] == '$')
                    {
                        sb.Append('$');
                        endPos += 2;
                    }
                    else
                    {
                        var name = GetExpandedVariableName(text, endPos + 1, out endPos);
                        sb.Append(ValueOrEmptyString(name));
                    }
                }
                else
                {
                    sb.Append(text[endPos]);
                    endPos++;
                }
            }

            return sb.ToString();
        }

        private string GetExpandedVariableName(string text, int startPos, out int endPos)
        {
            if (startPos >= text.Length)
            {
                endPos = startPos;
                return string.Empty;
            }

            if (text[startPos] == '(')
            {
                return Expand(text, ')', startPos + 1, out endPos);
            }
            else if (text[startPos] == '{')
            {
                return Expand(text, '}', startPos + 1, out endPos);
            }
            else
            {
                endPos = startPos + 1;
                return text.Substring(startPos, 1);
            }
        }

        private string ValueOrEmptyString(string name)
        {
            var value = string.Empty;
            _variables.TryGetValue(name, out value);
            return value;
        }
    }
}
