using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace Dash.Engine.IO
{
    public class ConfigFile : ConfigSection
    {
        static StringBuilder strBuilder;

        static ConfigFile()
        {
            strBuilder = new StringBuilder();
        }

        public ConfigFile(string filePath)
            : base(Path.GetFileName(filePath), null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Config file not found!", filePath);

            // Read file and parse it
            string rawText = File.ReadAllText(filePath);
            Parse(this, rawText);
        }

        public ConfigFile(string fileContents, string filePath)
            : base(filePath, null)
        {
            Parse(this, fileContents);
        }

        public static ConfigSection Parse(string rootName, string text)
        {
            ConfigSection rootSection = new ConfigSection(rootName, null);
            Parse(rootSection, text);

            return rootSection;
        }

        static void Parse(ConfigSection rootSection, string text)
        {
            bool inBlockComment = false, inLineComment = false;
            bool readingQuoteBlock = false, readingArray = false;

            strBuilder.Clear();
            char lastC = ' ';
            string lastKey = "";
            bool readingValue = false;
            bool lastCharEscaped = false;

            int trackerChar = 0, trackerLine = 0;

            List<string> array = new List<string>();

            ConfigSection currentSection = rootSection;

            for (int i = 0; i < text.Length; i++, trackerChar++)
            {
                char c = text[i];

                if (c == '\r')
                    continue; // Nope

                if (c == '\n')
                {
                    trackerChar = 0;
                    trackerLine++;
                }

                // Not in comment
                if (!inLineComment && !inBlockComment)
                {
                    if (!readingQuoteBlock)
                    {
                        // Enter block comment
                        if (lastC == '/' && c == '*')
                        {
                            inBlockComment = true;
                            strBuilder.Remove(strBuilder.Length - 1, 1);
                        }
                        // Enter inline comment
                        else if (lastC == '/' && c == '/')
                        {
                            inLineComment = true;
                            strBuilder.Remove(strBuilder.Length - 1, 1);
                        }
                        else if (c == ':')
                        {
                            if (readingArray)
                                throw new ConfigParseException("Unexpected symbol in array.",
                                    rootSection.Name, trackerLine, trackerChar);

                            lastKey = strBuilder.ToString();
                            strBuilder.Clear();

                            readingValue = true;
                        }
                        else if (c == ',' && readingValue && !readingArray)
                        {
                            if (lastKey != "")
                            {
                                string value = strBuilder.ToString();
                                strBuilder.Clear();
                                currentSection.Children.Add(lastKey, value); // Add keyvalue pair

                                readingValue = false;
                            }
                        }
                        else if (c == ',' && readingArray)
                        {
                            array.Add(strBuilder.ToString());
                            strBuilder.Clear();
                        }
                        else if (c == '\"')
                        {
                            // Enter quote block
                            readingQuoteBlock = true;
                        }
                        else if (c == '{')
                        {
                            if (readingArray)
                                throw new ConfigParseException("Unexpected symbol in array.",
                                    rootSection.Name, trackerLine, trackerChar);

                            // Enter section
                            ConfigSection newSection = new ConfigSection(lastKey, currentSection);
                            currentSection.Children.Add(lastKey, newSection);
                            currentSection = newSection;
                        }
                        else if (c == '}')
                        {
                            if (readingArray)
                                throw new ConfigParseException("Unexpected symbol in array.",
                                    rootSection.Name, trackerLine, trackerChar);

                            if (readingValue)
                            {
                                if (lastKey != "")
                                {
                                    string value = strBuilder.ToString();
                                    strBuilder.Clear();
                                    currentSection.Children.Add(lastKey, value); // Add keyvalue pair

                                    readingValue = false;
                                }
                            }

                            // Exit section
                            if (currentSection.Parent == null)
                                throw new ConfigParseException("Reached unexpected end of hierarchy.",
                                    rootSection.Name, trackerLine, trackerChar);

                            currentSection = currentSection.Parent;
                        }
                        else if (c == '[')
                        {
                            // Enter array
                            readingArray = true;
                        }
                        else if (c == ']')
                        {
                            // Exit array
                            if (strBuilder.Length > 0)
                            {
                                array.Add(strBuilder.ToString());
                                strBuilder.Clear();
                            }

                            // Add array
                            currentSection.Children.Add(lastKey, array.ToArray());

                            readingArray = false;
                            array.Clear();

                            lastKey = "";
                        }
                        else if (c != ' ' && c != '\n' && c != '\t' && c != ',')
                            strBuilder.Append(c);
                    }
                    else // In quote block
                    {
                        // Skip chars
                        if (c == '\"' && !lastCharEscaped)
                            readingQuoteBlock = false;
                        else if (c != '\\')
                            strBuilder.Append(c);
                    }
                }
                else
                {
                    // Exit inline comment
                    if (inLineComment && c == '\n')
                        inLineComment = false;

                    // Exit block comment
                    else if (inBlockComment && lastC == '*' && c == '/')
                        inBlockComment = false;
                }

                lastCharEscaped = c == '\\';
                lastC = c;
            }
        }

        /// <summary>
        /// Writes the whole file (in memory) to the console for debugging purposes.
        /// </summary>
        public void Debug()
        {
            PrintSection(this, 0);
        }

        static void PrintSection(ConfigSection section, int depth)
        {
            WriteLine(depth, String.Format("\"{0}\": ", section.Name) + "{");

            IDictionaryEnumerator enumerator = section.Children.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ConfigSection nestedSection = enumerator.Value as ConfigSection;
                if (nestedSection != null)
                    PrintSection(nestedSection, depth + 1);
                else
                {
                    string[] array = enumerator.Value as string[];
                    if (array != null)
                    {
                        Console.Write(new string('\t', depth + 1) + "\"{0}\": [ ", enumerator.Key);
                        for (int i = 0; i < array.Length; i++)
                            if (i < array.Length - 1)
                                Console.Write("{0}, ", array[i]);
                            else
                                Console.Write("{0} ", array[i]);
                        Console.Write("]\n");
                    }
                    else
                        WriteLine(depth + 1, "\"{0}\": \"{1}\"", enumerator.Key, enumerator.Value);
                }
            }

            WriteLine(depth, "}");
        }

        static void WriteLine(int depth, string text, params object[] args)
        {
            if (args.Length > 0)
                Console.WriteLine(new string('\t', depth) + text, args);
            else
                Console.WriteLine(new string('\t', depth) + text);
        }
    }
}
