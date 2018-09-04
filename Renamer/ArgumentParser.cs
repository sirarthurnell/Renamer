using RenamerLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renamer
{
    /// <summary>
    /// Configuración para el renombrado.
    /// </summary>
    class RenamingConfiguration
    {
        public Search PreparedSearch { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool EmitBom { get; set; }
    }

    /// <summary>
    /// Excepción ocurrida en el parseo de argumentos.
    /// </summary>
    class ArgumentParseException : ApplicationException
    {
        public ArgumentParseException() { }
        public ArgumentParseException(string message) : base(message) { }
        public ArgumentParseException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Parsea los argumentos de entrada.
    /// </summary>
    class ArgumentParser
    {
        /// <summary>
        /// Clase base para resultados de parseo.
        /// </summary>
        private class ParseResult
        {
            public int NextIndex { get; set; }
        }

        /// <summary>
        /// Resultado de parsear la ruta al directorio.
        /// </summary>
        private class PathParseResult : ParseResult
        {
            public string Path { get; set; }
        }

        /// <summary>
        /// Resultado de parsear el argumento
        /// change.
        /// </summary>
        private class ChangeParseResult : ParseResult
        {
            public string From { get; set; }
            public string To { get; set; }
        }

        /// <summary>
        /// Resultado de parsear el argumento
        /// exclude.
        /// </summary>
        private class ExcludeParseResult : ParseResult
        {
            public IEnumerable<string> Exclude { get; set; }
        }

        /// <summary>
        /// Resultado de parsear el argumento
        /// emitBom.
        /// </summary>
        private class EmitBomParseResult : ParseResult
        {
            public bool EmitBom { get; set; }
        }

        private string[] _args;
        private PathValidator _pathValidator;

        /// <summary>
        /// Crea una nueva instancia de ArgumentParser.
        /// </summary>
        /// <param name="args">Argumentos a parsear.</param>
        public ArgumentParser(string[] args)
        {
            _args = args;
            _pathValidator = new PathValidator();
        }

        /// <summary>
        /// Parsea los argumentos de entrada y configura
        /// una búsqueda acorde con ellos.
        /// </summary>
        /// <param name="args">Argumentos de entrada.</param>
        /// <returns>Configuración de renombrado.</returns>
        public RenamingConfiguration Parse()
        {
            int i = 0;
            PathParseResult pathResult = ParsePathArgument(i);
            i = pathResult.NextIndex;

            ChangeParseResult changeResult = null;
            ExcludeParseResult excludeResult = null;
            EmitBomParseResult emitBomResult = null;

            for (; i < _args.Length; i++)
            {
                var arg = _args[i];

                if (arg.StartsWith("--"))
                {
                    var parsingArgument = arg
                        .Substring(2)
                        .ToLowerInvariant();

                    switch (parsingArgument)
                    {
                        case "change":
                            changeResult = ParseChangeArgument(i);
                            i = changeResult.NextIndex;
                            break;

                        case "exclude":
                            excludeResult = ParseExcludeArgument(i);
                            i = excludeResult.NextIndex;
                            break;

                        case "emitbom":
                            emitBomResult = ParseEmitBomArgument(i);
                            i = emitBomResult.NextIndex;
                            break;

                        default:
                            throw new ArgumentParseException(string.Format("Argument \"{0}\" not recognized", arg));
                    }
                }
                else
                {
                    throw new ArgumentParseException("Badformed argument list");
                }
            }

            if (changeResult == null)
            {
                throw new ArgumentParseException("--change argument is required");
            }

            Search search;

            if (excludeResult == null)
            {
                search = new Search(pathResult.Path);
            }
            else
            {
                search = new Search(pathResult.Path, excludeResult.Exclude);
            }

            bool emitBom = false;
            if (emitBomResult != null)
            {
                emitBom = true;
            }

            return new RenamingConfiguration
            {
                PreparedSearch = search,
                From = changeResult.From,
                To = changeResult.To,
                EmitBom = emitBom
            };
        }

        /// <summary>
        /// Parsea la ruta al directorio.
        /// </summary>
        /// <param name="startIndex">Índice en el que
        /// comienza el argumento.</param>
        /// <returns>Resultados del parseo.</returns>
        private PathParseResult ParsePathArgument(int startIndex)
        {
            var path = _args[startIndex];

            if (!path.StartsWith("--"))
            {
                var pathValidation = _pathValidator.ValidatePath(path, true);
                if (pathValidation.Valid)
                {
                    return new PathParseResult
                    {
                        Path = path,
                        NextIndex = startIndex + 1
                    };
                }
                else
                {
                    throw new ArgumentParseException(pathValidation.Reason);
                }
            }
            else
            {
                return new PathParseResult
                {
                    Path = Directory.GetCurrentDirectory(),
                    NextIndex = startIndex
                };
            }
        }

        /// <summary>
        /// Parsea el argumento de change.
        /// </summary>
        /// <param name="startIndex">Índice en el que
        /// comienza el argumento.</param>
        /// <returns>Resultados del parseo.</returns>
        private ChangeParseResult ParseChangeArgument(int startIndex)
        {
            var header = startIndex;
            var from = _args[++header];
            var delimiter = _args[++header];
            var to = _args[++header];

            if (delimiter == "//")
            {
                var areValidNames = IsValidName(from) && IsValidName(to);
                if (areValidNames)
                {
                    var areValidFileNames =
                        _pathValidator.ValidateFileName(from).Valid &&
                        _pathValidator.ValidateFileName(to).Valid;

                    if (areValidFileNames)
                    {
                        var results = new ChangeParseResult
                        {
                            From = from,
                            To = to,
                            NextIndex = header
                        };

                        return results;
                    }
                }
            }

            throw new ArgumentParseException("Badformed --change argument. It should have this format: FROM // TO, where FROM and TO must be valid filenames.");
        }

        /// <summary>
        /// Parsea el argumento de exclude.
        /// </summary>
        /// <param name="startIndex">Índice en el que
        /// comienza el argumento.</param>
        /// <returns>Resultados del parseo.</returns>
        private ExcludeParseResult ParseExcludeArgument(int startIndex)
        {
            var header = startIndex;
            var exclude = _args[++header];

            var exclusions = exclude.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (exclusions.Length > 0)
            {
                var results = new ExcludeParseResult
                {
                    Exclude = exclusions.Select(e => e.Trim()),
                    NextIndex = header
                };

                return results;
            }

            throw new ArgumentParseException("Badformed --exclude argument. It should have this format: \"EXCLUDE1, EXCLUDE2...\"");
        }

        /// <summary>
        /// Parsea el argumento de emitBom.
        /// </summary>
        /// <param name="startIndex">Índice en el que
        /// comienza el argumento.</param>
        /// <returns>Resultado del parseo.</returns>
        private EmitBomParseResult ParseEmitBomArgument(int startIndex)
        {
            var header = startIndex;
            var emitBom = _args[++header];

            var results = new EmitBomParseResult
            {
                EmitBom = true,
                NextIndex = header
            };

            return results;
        }

        /// <summary>
        /// Comprueba si se trata de un nombre válido.
        /// </summary>
        /// <param name="name">Nombre a comprobar.</param>
        /// <returns>True si es un nombre válido.
        /// False en caso contrario.</returns>
        private bool IsValidName(string name)
        {
            return (name != string.Empty) && (!name.StartsWith("--"));
        }

        /// <summary>
        /// Parsea los argumentos introducidos por el usuario.
        /// </summary>
        /// <param name="args">Argumentos introducidos.</param>
        /// <returns>Búsqueda configurada acorde con los
        /// parámetros.</returns>
        private static Search ParseArguments(string[] args)
        {
            if (args.Length != 3 && args.Length != 4)
            {
                throw new ArgumentException("Arguments not specified correctly.");
            }

            var path = args[0];
            var pathValidator = new PathValidator();
            var pathResult = pathValidator.ValidatePath(path, true);
            if (!pathResult.Valid)
            {
                throw new ArgumentException(pathResult.Reason);
            }

            var from = args[1];
            var fromResult = pathValidator.ValidateFileName(from);
            if (!fromResult.Valid)
            {
                throw new ArgumentException(pathResult.Reason);
            }

            var to = args[2];
            var toResult = pathValidator.ValidateFileName(to);
            if (!toResult.Valid)
            {
                throw new ArgumentException(pathResult.Reason);
            }

            Search search = null;

            if (args.Length == 4)
            {
                var exclusions = args[3].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var exclusionsTrimmed = new List<string>();
                foreach (var exclusion in exclusions)
                {
                    var exclusionTrimmed = exclusion.Trim();
                    var exclusionResult = pathValidator.ValidatePath(exclusionTrimmed, false);
                    if (!exclusionResult.Valid)
                    {
                        throw new ArgumentException(string.Format("Exclusion \"{0}\" is not a valid filename.", exclusion));
                    }
                    else
                    {
                        exclusionsTrimmed.Add(exclusionTrimmed);
                    }
                }

                search = new Search(path, exclusionsTrimmed);
            }
            else
            {
                search = new Search(path);
            }

            return search;
        }
    }
}
