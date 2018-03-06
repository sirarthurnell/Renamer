using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenamerLogic
{
    /// <summary>
    /// Lleva a cabo el proceso de renombrado.
    /// </summary>
    public class Rename
    {
        private IEnumerable<FileSystemEntry> _entriesToRename;
        private string _from;
        private string _to;

        /// <summary>
        /// Crea una nueva instancia de Rename.
        /// </summary>
        /// <param name="entriesToRename">Lista de archivos
        /// y directorios sobre los que realizar el renombrado.</param>
        /// <param name="from">Cadena a sustituir.</param>
        /// <param name="to">Cadena que sustituye.</param>
        public Rename(IEnumerable<FileSystemEntry> entriesToRename, string from, string to)
        {
            _entriesToRename = entriesToRename;
            _from = from;
            _to = to;
        }

        /// <summary>
        /// Lleva a cabo el renombrado.
        /// </summary>
        public void Perform()
        {
            var files = _entriesToRename
                .Where(e => e.Type == FileSystemEntryType.File)
                .Select(e => e.Path);

            ProcessAllFiles(files);

            var directories = _entriesToRename
                .Where(e => e.Type == FileSystemEntryType.Directory)
                .Select(e => e.Path);

            ProcessAllDirectories(directories);
        }

        /// <summary>
        /// Procesa todos los archivos.
        /// </summary>
        /// <param name="filePaths">Rutas a los archivos
        /// a procesar.</param>
        private void ProcessAllFiles(IEnumerable<string> filePaths)
        {
            var inverseOrderedFilePaths = filePaths.OrderByDescending(f => f.Length);

            var batch = new TextReplacementBatch(inverseOrderedFilePaths, _from, _to);
            batch.Perform();

            inverseOrderedFilePaths
                .ToList()
                .ForEach(f => RenameFile(f));
        }
        
        /// <summary>
        /// Reemplaza el nombre del archivo.
        /// </summary>
        /// <param name="filePath">Ruta al archivo.</param>
        private void RenameFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            if (IsMatch(fileName))
            {
                var newFileName = fileName.Replace(_from, _to);
                var newPath = ReplaceLastSegment(filePath, fileName, newFileName);
                File.Move(filePath, newPath);
            }
        }

        /// <summary>
        /// Procesa el directorio.
        /// </summary>
        /// <param name="directoryPath">Ruta del directorio
        /// a procesar.</param>
        private void ProcessAllDirectories(IEnumerable<string> directoryPaths)
        {
            directoryPaths
                .OrderByDescending(d => d.Length)
                .ToList()
                .ForEach(directoryPath => {

                var directoryName = Path.GetFileName(directoryPath);

                if (IsMatch(directoryName))
                {
                    var newDirectoryName = directoryName.Replace(_from, _to);
                    var newPath = ReplaceLastSegment(directoryPath, directoryName, newDirectoryName);
                    Directory.Move(directoryPath, newPath);
                }

            });            
        }

        /// <summary>
        /// Reemplaza el último segmento coincidente de
        /// la ruta indicada por uno nuevo.
        /// </summary>
        /// <param name="path">Ruta indicada.</param>
        /// <param name="oldValue">Valor a reemplazar.</param>
        /// <param name="newValue">Valor reemplazante.</param>
        /// <returns>Ruta con el valor reemplazado.</returns>
        private string ReplaceLastSegment(string path, string oldValue, string newValue)
        {
            var newPath = path.Substring(0, path.LastIndexOf(oldValue)) + newValue;
            return newPath;
        }

        /// <summary>
        /// Comprueba si la ruta contiene la cadena a
        /// sustituir.
        /// </summary>
        /// <param name="path">Ruta a comprobar.</param>
        /// <returns>True si la contiene. False en caso
        /// contrario.</returns>
        private bool IsMatch(string path)
        {
            return path.IndexOf(_from, StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }
}
