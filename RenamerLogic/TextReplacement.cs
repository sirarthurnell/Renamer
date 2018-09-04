using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenamerLogic
{
    /// <summary>
    /// Reemplaza texto en un archivo de texto.
    /// </summary>
    public class TextReplacement
    {
        private string _filePath;
        private string _from;
        private string _to;
        private bool _emitBom;

        /// <summary>
        /// Crea una nueva instancia de TextReplacement.
        /// </summary>
        /// <param name="filePath">Ruta al archivo a procesar.</param>
        /// <param name="from">Texto a cambiar.</param>
        /// <param name="to">Texto que sustituye al anterior.</param>
        /// <param name="emitBom">Indica si se ha de emitir BOM.</param>
        public TextReplacement(string filePath, string from, string to, bool emitBom)
        {
            _filePath = filePath;
            _from = from;
            _to = to;
            _emitBom = emitBom;
        }

        /// <summary>
        /// Procesa el cambio de texto.
        /// </summary>
        public void Perform()
        {
            var isBinary = GuessIfBinary();

            if (isBinary)
            {
                return;
            }
            else
            {
                PerformSustitution();
            }
        }

        /// <summary>
        /// Lleva a cabo el reemplazo de texto.
        /// </summary>
        private void PerformSustitution()
        {
            var contents = string.Empty;
            var newContents = string.Empty;
            var utf8WithoutBom = new UTF8Encoding(_emitBom);

            using (var reader = new StreamReader(_filePath))
            {
                contents = reader.ReadToEnd();
            }

            newContents = contents.Replace(_from, _to);

            using (var writer = new StreamWriter(_filePath, false, utf8WithoutBom))
            {
                writer.Write(newContents);
            }
        }

        /// <summary>
        /// Comprueba si un archivo es binario o no.
        /// </summary>
        /// <returns>True si el archivo es binario.
        /// False en caso contrario.</returns>
        /// <remarks>Se realizan dos tipos de test:
        /// Uno comprueba si existe una cadena de carácter 0
        /// consecutivos.
        /// El otro comprueba si existe un número determinado
        /// de carácteres de control.
        /// Si alguno de los dos test da positivo, el
        /// archivo se considera binario.</remarks>
        public bool GuessIfBinary()
        {
            int consecutiveZerosCount = 0;
            int consecutiveZerosThreshold = 4;
            long controlCharsThreshold = 100;
            long controlCharsCount = 0;

            using (StreamReader stream = new StreamReader(_filePath))
            {
                int character;
                while ((character = stream.Read()) != -1)
                {
                    if (IsControlChar(character))
                    {
                        controlCharsCount++;

                        if (IsZeroChar(character))
                        {
                            consecutiveZerosCount++;
                            if (consecutiveZerosCount >= consecutiveZerosThreshold)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            consecutiveZerosCount = 0;
                        }
                    }
                }
            }

            return controlCharsCount >= controlCharsThreshold;
        }

        /// <summary>
        /// Comprueba de si se trata de carácter 0.
        /// </summary>
        /// <param name="character">Carácter a comprobar.</param>
        /// <returns>True si es el carácter 0. False
        /// en caso contrario.</returns>
        public bool IsZeroChar(int character)
        {
            return character == (char)0;
        }

        /// <summary>
        /// Comprueba si un carácter es un carácter
        /// de control.
        /// </summary>
        /// <param name="character">Carácter a comprobar.</param>
        /// <returns>True si es un carácter de control.
        /// False en caso contrario.</returns>
        public bool IsControlChar(int character)
        {
            return (character > (char)0 && character < (char)8)
                || (character > (char)13 && character < (char)26);
        }
    }
}
