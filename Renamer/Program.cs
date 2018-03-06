using RenamerLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Renamer
{
    /// <summary>
    /// Comando para cambiar una cadena dentro del nombre
    /// de directorios, archivos y de su contenido.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entrada principal.
        /// </summary>
        /// <param name="args">Argumentos de
        /// la línea de comandos.</param>
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
            }
            else
            {
                try
                {
                    var search = ParseArguments(args);                    
                    var entries = ReportSearch(search);
                    if (AskIfSure())
                    {
                        var from = args[1];
                        var to = args[2];

                        PerformRename(entries, from, to);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }            
        }

        /// <summary>
        /// Lleva a cabo el renombrado de archivos
        /// y directorios.
        /// </summary>
        /// <param name="entries">Entradas a parsear.</param>
        private static void PerformRename(IEnumerable<FileSystemEntry> entries, string from, string to)
        {
            Console.WriteLine("Renaming...");

            var rename = new Rename(entries, from, to);
            rename.Perform();

            Console.WriteLine("Rename completed.");
        }

        /// <summary>
        /// Muestra al usuario información de la búsqueda.
        /// </summary>
        /// <param name="search">Búsqueda.</param>
        private static IEnumerable<FileSystemEntry> ReportSearch(Search search)
        {
            Console.WriteLine("Searching files...");

            var entries = search.Perform();
            foreach (var entry in entries)
            {
                Console.WriteLine(entry.Path);
            }

            Console.WriteLine("{0} elements will be parsed.", entries.Count());

            return entries;
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
                var exclusions = args[3].Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var exclusion in exclusions)
                {
                    var exclusionResult = pathValidator.ValidatePath(exclusion, false);
                    if (!exclusionResult.Valid)
                    {
                        throw new ArgumentException(string.Format("Exclusion \"{0}\" is not a valid filename.", exclusion));
                    }
                }

                search = new Search(path, exclusions.ToList());
            }
            else
            {
                search = new Search(path);
            }

            return search;
        }

        /// <summary>
        /// Pregunta al usuario si quiere continuar.
        /// </summary>
        /// <returns>True si quiere continuar. False
        /// en caso contrario.</returns>
        private static bool AskIfSure()
        {
            Console.Write("Are you sure? (Y/N): ");

            var answer = Console
                .ReadLine()
                .Trim()
                .ToUpperInvariant();

            if (string.Compare(answer, "Y") == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Imprime el uso del comando.
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("Specify the path to explore and the name to replace in this way:");
            Console.WriteLine();
            Console.WriteLine(@"<path\to\directory> <nameToReplace> <newName> [excludeName...]");
            Console.WriteLine();
            Console.WriteLine("Subdirectories will be explored too.");
        }
    }
}
