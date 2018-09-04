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
                    var configuration = CreateConfigurationFromArguments(args);
                    var entries = ReportSearch(configuration.PreparedSearch);
                    if (AskIfSure())
                    {
                        PerformRenaming(entries, configuration.From, configuration.To, configuration.EmitBom);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Crea una configuración de ejecución a partir
        /// de los argumentos introducidos por el usuario.
        /// </summary>
        /// <param name="args">Argumentos introducidos.</param>
        /// <returns>Configuración creada.</returns>
        private static RenamingConfiguration CreateConfigurationFromArguments(string[] args)
        {
            var argumentParser = new ArgumentParser(args);
            var configuration = argumentParser.Parse();
            return configuration;
        }

        /// <summary>
        /// Lleva a cabo el renombrado de archivos
        /// y directorios.
        /// </summary>
        /// <param name="entries">Entradas a parsear.</param>
        private static void PerformRenaming(IEnumerable<FileSystemEntry> entries, string from, string to, bool emitBom)
        {
            Console.WriteLine("Renaming...");

            var rename = new Renaming(entries, from, to, emitBom);
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

            Console.WriteLine();
            Console.WriteLine("{0} elements will be parsed.", entries.Count());

            return entries;
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
            Console.WriteLine("path\\to\\directory --change nameToReplace // newName [--exclude \"nameToExclude,...\"][--emitbom]");
            Console.WriteLine();
            Console.WriteLine("Subdirectories will be explored too.");
        }
    }
}
