using System;
using System.Text;
using SimpleCollections.Hash;
using SimpleCollections.Lists;
using SimpleCollections.Util;

namespace hash
{
    /// <summary>
    /// Defines a set of configurations for validating command line arguments.
    /// </summary>
    public class CommandLineArgValidationOption
    {
        public string ArgumentName;
        public ArgRequirement Requirements;

        public ArgValidationResult ValidationResult;
    }

    /// <summary>
    /// Enumerates all possible argument validations.
    /// </summary>
    [Flags]
    public enum ArgRequirement
    {
        None = 1 << 0,
        Unique = 1 << 1,
        Required = 1 << 2,
        ValueRequired = 1 << 3,
    }

    /// <summary>
    /// Enumerates the possible result of the command line validation.
    /// </summary>
    [Flags]
    public enum ArgValidationResult
    {
        EverythingOk = 1 << 0,
        NotFound = 1 << 1,
        EmptyValue = 1 << 2,
        Duplicated = 1 << 3,
    }
    
    public static class Shell
    {
        #region Properties

        public const char ArgumentPrefix = '-';

        #endregion

        /// <summary>
        /// Returns the name of the command on the given commandLine.
        /// The command name is the text from the start of the command line all the way to the first special character.
        /// If command line is empty or null, empty is returned.
        /// </summary>
        public static string GetCommandName(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine))
                return string.Empty;

            var builder = new StringBuilder(commandLine.Length);
            for (int i = 0; i < commandLine.Length; i++)
            {
                var letter = commandLine[i];

                if (char.IsLetter(letter))
                    builder.Append(letter);
                else
                    break;
            }

            var builderText = builder.ToString();
            return builderText;
        }

        /// <summary>
        /// Removes the command from the command line and returns the result.
        /// If command line is null or empty, return empty.
        /// </summary>
        public static string RemoveCommandFromCommandLine(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine))
                return string.Empty;

            var commandName = GetCommandName(commandLine);
            if (commandName.Length == commandLine.Length)
                return string.Empty;
            else
                return commandLine.Replace(commandName, string.Empty).Trim();
        }

        /// <summary>
        /// Parses the given command line and returns the arguments form is.
        /// The command line is expected to be without the command name.
        /// </summary>
        public static SimpleList<Pair<string, string>> GetArgumentsFromCommandLine(string commandLineWithoutCommand)
        {
            commandLineWithoutCommand = commandLineWithoutCommand.Trim();

            var result = SList.Create<Pair<string, string>>(3);

            var builder = new StringBuilder(commandLineWithoutCommand.Length);

            var argName = string.Empty;
            var argValue = string.Empty;

            const int lookForArgState = 0;
            const int readArgNameState = 1;
            const int readArgValueState = 2;

            int currentState = lookForArgState;

            // If it don't start with the argument prefix, it means that the parameter has no name, just value
            if (!commandLineWithoutCommand.StartsWith(ArgumentPrefix.ToString()))
                currentState = readArgValueState;

            var onQuote = false;
            var ignoreNextSpecial = false;
            for (int i = 0; i < commandLineWithoutCommand.Length; i++)
            {
                var letter = commandLineWithoutCommand[i];

                // Specials
                var isEmpty = letter == ' ' && !ignoreNextSpecial;
                var isQuote = letter == '"' && !ignoreNextSpecial;
                var isBackSlash = letter == '\\' && !ignoreNextSpecial;
                var isArgPrefix = letter == ArgumentPrefix && !ignoreNextSpecial;

                ignoreNextSpecial = false;

                if (isQuote)
                {
                    onQuote = !onQuote;
                    continue;
                }
                else if (isBackSlash)
                {
                    ignoreNextSpecial = true;
                    continue;
                }

                if (onQuote)
                    ignoreNextSpecial = true;

                switch (currentState)
                {
                    case lookForArgState:
                        if (isArgPrefix)
                            currentState = readArgNameState;
                        break;
                    case readArgNameState:
                        // if there's a space after the start of a arg, ignore the space
                        if (isEmpty && builder.Length == 0)
                            continue;

                        if (isEmpty)
                        {
                            argName = builder.ToString();
                            builder.Clear();
                            currentState = readArgValueState;

                            argValue = string.Empty;
                        }
                        else
                            builder.Append(letter);
                        break;
                    case readArgValueState:
                        if (isArgPrefix)
                        {
                            argValue = builder.ToString();
                            builder.Clear();

                            var pair = CreateArgPair(argName, argValue);
                            SList.Add(result, pair);

                            argName = string.Empty;
                            argValue = string.Empty;

                            currentState = readArgNameState;
                        }
                        else
                            builder.Append(letter);
                        break;
                }
            }

            // Process what might have been left on the command line
            switch (currentState)
            {
                case lookForArgState:
                    argName = string.Empty;
                    argValue = string.Empty;
                    break;
                case readArgNameState:
                    argName = builder.ToString();
                    break;
                case readArgValueState:
                    argValue = builder.ToString();
                    break;
            }

            // We can arguments with empty names but values or with empty values but names
            if (!(string.IsNullOrEmpty(argName) && string.IsNullOrEmpty(argValue)))
            {
                var finalPair = CreateArgPair(argName, argValue);
                SList.Add(result, finalPair);
            }

            return result;
        }

        /// <summary>
        /// Shorthand for create an new pair with the given name and value.
        /// </summary>
        public static Pair<string, string> CreateArgPair(string argName, string argValue)
        {
            var pair = new Pair<string, string>();
            pair.Key = argName.Trim();
            pair.Value = argValue.Trim();
            return pair;
        }

        /// <summary>
        /// Search for the first argument with the given name. Returns true if found the parameter, false otherse.
        /// The resulting parameter will be on the out Pair argument.
        /// </summary>
        public static bool TryGetArgumentByName(SimpleList<Pair<string, string>> arguments, string parameterName, out Pair<string, string> parameter)
        {
            parameter = SList.Find(arguments, pair => string.Equals(pair.Key, parameterName, StringComparison.InvariantCultureIgnoreCase));

            // since pair is a struct (always not null), we need to validate if the key AND value is null
            return !(string.IsNullOrEmpty(parameter.Key) && string.IsNullOrEmpty(parameter.Value));
        }

        public static Pair<string, string> FindArgumentByName(SimpleList<Pair<string, string>> arguments, string parameterName)
        {
            Pair<string, string> arg;
            TryGetArgumentByName(arguments, parameterName, out arg);
            return arg;
        }

        public static bool ArgumentExists(SimpleList<Pair<string, string>> arguments, string parameterName)
        {
            Pair<string, string> arg;
            return TryGetArgumentByName(arguments, parameterName, out arg);
        }

        /// <summary>
        /// Search for the nth argument with the given name. Returns true if found the parameter, false otherse.
        /// The resulting parameter will be on the   out Pair argument.
        /// </summary>
        public static bool TryGetNthArgumentByName(SimpleList<Pair<string, string>> arguments, string parameterName, out Pair<string, string> parameter, int n)
        {
            var count = 0;
            parameter = SList.Find(arguments, pair =>
            {
                if (count++ == n)
                    return string.Equals(pair.Key, parameterName, StringComparison.InvariantCultureIgnoreCase);
                else
                    return false;
            });

            // since pair is a struct (always not null), we need to validate if the key AND value is null
            return !(string.IsNullOrEmpty(parameter.Key) && string.IsNullOrEmpty(parameter.Value));
        }

        /// <summary>
        /// Returns true if the given parameter name appears more than one on the list of arguments.
        /// </summary>
        public static bool IsArgumentDuplicated(SimpleList<Pair<string, string>> arguments, string parameterName)
        {
            var set = SSet.Create<string>(10, true);
            SSet.Clear(set);
            for (int i = 0; i < arguments.Count; i++)
            {
                var key = arguments[i].Key;
                if (string.Equals(key, parameterName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (SSet.Contains(set, key))
                        return true;
                    else
                        SSet.Add(set, key);
                }
            }

            return false;
        }

        /// <summary>
        /// Validates the given command line validation options and stores the validation result on the corresponding command line option.
        /// containing the result of the validation.
        /// This will validate all options whether one of them failed.
        /// This returns true of EVERY argument is EverythingOk.
        /// </summary>
        public static bool ValidateArguments(SimpleList<Pair<string, string>> arguments, CommandLineArgValidationOption[] options)
        {
            bool result = true;
            for (int i = 0; i < options.Length; i++)
            {
                var opt = options[i];
                
                if ((opt.Requirements & ArgRequirement.Unique) != 0)
                {
                    if (IsArgumentDuplicated(arguments, opt.ArgumentName))
                    {
                        opt.ValidationResult |= ArgValidationResult.Duplicated;
                        result = false;
                    }
                }

                Pair<string, string> arg;
                if (TryGetArgumentByName(arguments, opt.ArgumentName, out arg))
                {
                    if ((opt.Requirements & ArgRequirement.ValueRequired) != 0)
                    {
                        if (string.IsNullOrEmpty(arg.Value))
                        {
                            opt.ValidationResult |= ArgValidationResult.EmptyValue;
                            result = false;
                        }
                    }
                }
                else
                {
                    if ((opt.Requirements & ArgRequirement.Required) != 0)
                    {
                        opt.ValidationResult |= ArgValidationResult.NotFound;
                        result = false;
                    }
                }

                if (result)
                    opt.ValidationResult = ArgValidationResult.EverythingOk;
            }

            return result;
        }

        /// <summary>
        /// Returns true if EVERY argument on the arguments list is also on the known arguments list.
        /// Use GetUnkownArguments to see which arguments were unknown.
        /// </summary>
        public static bool AreAllArgumentsKnown(SimpleList<Pair<string, string>> arguments, string[] knownArguments)
        {
            for (int i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i].Key;
                if (Array.IndexOf(knownArguments, arg) == -1)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a list of all unknown arguments.
        /// An argument is unknown when it's present on the arguments list but not on the knownArguments list.
        /// </summary>
        public static SimpleList<Pair<string, string>> GetUnknownArguments(SimpleList<Pair<string, string>> arguments, string[] knownArguments)
        {
            var result = SList.Create<Pair<string, string>>(1);
            if (AreAllArgumentsKnown(arguments, knownArguments))
                return result;

            for (int i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i];
                var indexOf = Array.IndexOf(knownArguments, arg.Key);
                if (indexOf == -1)
                    SList.Add(result, arg);
            }

            return result;
        }

        /// <summary>
        /// Shorthand for calling ValidateArguments and HasUnknownArguments.
        /// Returns true if ValidateArguments return true and HasUnkoownArgument return false. Returns false otherwise.
        /// If this method returns true, every validating said that the arguments are ok. If returned false, there's something wrong.
        /// Use the hasUnknownArg and hasNotOkParameter to known the results of each operation.
        /// </summary>
        public static bool FullArgValidation(SimpleList<Pair<string, string>> arguments,
            CommandLineArgValidationOption[] options, string[] knownArguments, out bool areArgumentsKnown, out bool areArgumentsOk)
        {
            areArgumentsOk = ValidateArguments(arguments, options);
            areArgumentsKnown = AreAllArgumentsKnown(arguments, knownArguments);

            return areArgumentsOk & areArgumentsKnown;
        }
    }
}