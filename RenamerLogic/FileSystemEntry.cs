using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenamerLogic
{
    /// <summary>
    /// Representa una entrada del sistema de archivos.
    /// </summary>
    public class FileSystemEntry
    {
        public FileSystemEntryType Type { get; private set; }
        public string Path { get; private set; }

        /// <summary>
        /// Crea una nueva instancia de entrada de sistema
        /// de archivos.
        /// </summary>
        /// <param name="path">Ruta de la entrada del sistema
        /// de archivos.</param>
        /// <param name="type">Tipo de entrada.</param>
        public FileSystemEntry(string path, FileSystemEntryType type)
        {
            Path = path;
            Type = type;
        }
    }
}
