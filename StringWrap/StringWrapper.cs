using System;
using System.Collections.Generic;
using System.Text;

namespace StringWrap
{
    enum TextType 
    {
        Word,
        Separator,
        NewLine
    }

    class TextPart
    {
        public TextType TextType;
        public string Text;

        public int Length
        {
            get  { return TextType == TextType.NewLine ? 0 : Text.Length; }
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public class StringWrapper
    {
        public string NewLine { get; set; }
        public int TabSize { get; set; }

        public StringWrapper(string newLine = null, int tabSize = 4)
        {
            NewLine = newLine ?? Environment.NewLine;
            TabSize = tabSize;
        }

        public string Wrap(string text, int maxLength)
        {
            var lines = GetLines(text, maxLength);
            return string.Join(NewLine, lines);
        }

        static IEnumerable<string> GetLines(string text, int maxLength)
        {
            text = text.Replace("\t", "    ");

            var line = new StringBuilder();
            bool endsWithNewLine = false;
            bool wordNeedsProcessing = true;
            var words = GetTextParts(text);
            foreach (var word in words)
            {
                do
                {
                    wordNeedsProcessing = false;
                    endsWithNewLine = false;

                    if (word.TextType == TextType.NewLine)
                    {
                        endsWithNewLine = true;
                        yield return line.ToString();
                        line.Clear();
                    }
                    else if (line.Length + word.Length <= maxLength)
                    {
                        line.Append(word);
                    }
                    else if (word.TextType == TextType.Separator || word.TextType == TextType.Word)
                    {
                        // try to not split words if possible
                        if (word.TextType == TextType.Word && word.Length < maxLength) {
                            yield return line.ToString();
                            line.Clear();
                        }
                        else
                        {
                            int freeSpace = maxLength - line.Length;
                            string firstPart = word.Text.Substring(0, freeSpace);

                            line.Append(firstPart);
                            yield return line.ToString();
                            line.Clear();

                            // rest part of word will be processed in next cicle
                            word.Text = word.Text.Substring(freeSpace);
                        }
                        wordNeedsProcessing = true;
                    }
                } while (wordNeedsProcessing);
            }

            if (line.Length > 0 || endsWithNewLine)
                yield return line.ToString();
        }

        static IEnumerable<TextPart> GetTextParts(string text)
        {
            TextPart part = null;
            TextType textType;
            char prevC = '\n';
            foreach (char c in text)
            {
                textType = DetectTextType(c);
                // only one new line per TextPart
                if (prevC != '\n' && part.TextType == textType)
                {
                    part.Text += c;
                }
                else 
                {
                    if (part != null)
                        yield return part;
                
                    part = new TextPart
                    {
                        TextType = textType,
                        Text = c.ToString()
                    };
                }
                prevC = c;
            }

            if (part != null)
                yield return part;
        }

        static TextType DetectTextType(char c)
        {
            if (char.IsLetterOrDigit(c))
                return TextType.Word;
            else if (c == '\r' || c == '\n')
                return TextType.NewLine;
            else
                return TextType.Separator;
        }
    }
}
