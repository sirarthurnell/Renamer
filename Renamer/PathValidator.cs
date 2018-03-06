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
    /// Resultado de la validación de ruta.
    /// </summary>
    public class PathValidatorResult
    {
        /// <summary>
        /// Indica si la ruta es válida.
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// Contiene un mensaje informativo en
        /// el caso en el que la ruta no sea válida.
        /// </summary>
        public string Reason { get; set; }
    }

    /// <summary>
    /// Valida una ruta en el sistema de archivos.
    /// </summary>
    public class PathValidator
    {
        /// <summary>
        /// Valida una ruta en el sistema de archivos.
        /// </summary>
        /// <param name="path">Ruta a validar.</param>
        /// <param name="enforceExistence">True para validar
        /// la existencia de la ruta indicada en el sistema
        /// de archivos. False en caso contrario.</param>
        /// <returns>Resultado de la validación.</returns>
        public PathValidatorResult ValidatePath(string path, bool enforceExistence)
        {
            var regexSearch = new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            var cleanPath = r.Replace(path, string.Empty);

            if (path.CompareTo(cleanPath) != 0)
            {
                return new PathValidatorResult
                {
                    Valid = false,
                    Reason = "Invalid path."
                };
            }
            else
            {
                if (enforceExistence && !(Directory.Exists(path) || File.Exists(path)))
                {
                    return new PathValidatorResult
                    {
                        Valid = false,
                        Reason = "File or directory doesn't exist."
                    };
                }
            }

            return new PathValidatorResult
            {
                Valid = true,
                Reason = string.Empty
            };
        }

        /// <summary>
        /// Valida un nombre de archivo.
        /// </summary>
        /// <param name="fileName">Nombre de archivo
        /// a validar.</param>
        /// <returns>Resultado de la validación.</returns>
        public PathValidatorResult ValidateFileName(string fileName)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            var cleanFileName = r.Replace(fileName, string.Empty);

            if (fileName.CompareTo(cleanFileName) != 0)
            {
                return new PathValidatorResult
                {
                    Valid = false,
                    Reason = "Invalid filename."
                };
            }
            else
            {
                return new PathValidatorResult
                {
                    Valid = true,
                    Reason = string.Empty
                };
            }
        }
    }
}
