using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenamerLogic
{
    /// <summary>
    /// Representa una búsqueda de reemplazo de cadenas.
    /// </summary>
    public class Search
    {
        private string _startPath;
        private IEnumerable<string> _exclude;

        /// <summary>
        /// Crea una nueva instancia de Search.
        /// </summary>
        /// <param name="startPath">Ruta desde la que empieza
        /// la búsqueda.</param>
        public Search(string startPath)
        {
            _startPath = startPath;
            _exclude = new List<string>();
        }

        /// <summary>
        /// Crea una nueva instancia de Search.
        /// </summary>
        /// <param name="startPath">Ruta desde la que empieza
        /// la búsqueda.</param>
        /// <param name="exclude">IEnumerable con cadenas que al
        /// ser contenidas dentro del nombre de archivo o directorio,
        /// excluyen al mismo del proceso.</param>
        public Search(string startPath, IEnumerable<string> exclude)
        {
            _startPath = startPath;
            _exclude = exclude;
        }
        
        /// <summary>
        /// Realiza el reemplazo.
        /// </summary>
        public IEnumerable<FileSystemEntry> Perform()
        {
            var entries = Directory.EnumerateFileSystemEntries(_startPath, "*", SearchOption.AllDirectories);
            var excluded = new List<string>();
            var results = new List<FileSystemEntry>();

            foreach (var entry in entries)
            {
                if (IsExcluded(entry))
                {
                    excluded.Add(entry);
                    continue;
                }
                else
                {
                    var attributes = File.GetAttributes(entry);
                    var isDirectory = (attributes & FileAttributes.Directory) == FileAttributes.Directory;
                    FileSystemEntry currentResult = new FileSystemEntry(entry, 
                        isDirectory ? 
                        FileSystemEntryType.Directory : FileSystemEntryType.File);                    

                    results.Add(currentResult);
                }
            }

            return results;
        }

        /// <summary>
        /// Comprueba si la entrada a procesar ha
        /// de ser excluida.
        /// </summary>
        /// <param name="entryPath">Entrada a procesar.</param>
        /// <returns>True si ha de ser excluida.
        /// False en caso contario.</returns>
        private bool IsExcluded(string entryPath)
        {
            foreach (var exclusion in _exclude)
            {
                var match = entryPath.IndexOf(exclusion, StringComparison.InvariantCultureIgnoreCase) > -1;
                if (match)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
