using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenamerLogic
{
    /// <summary>
    /// Realiza un reemplazo de texto 
    /// en un lote de archivos.
    /// </summary>
    public class TextReplacementBatch
    {
        /// <summary>
        /// Representa una entrada del batch.
        /// </summary>
        private class BatchEntry
        {
            public string Path { get; set; }
            public string From { get; set; }
            public string To { get; set; }
        }

        private List<string> _filePaths;
        private string _from;
        private string _to;

        /// <summary>
        /// Crea una nueva instancia de TextReplacementBatch.
        /// </summary>
        /// <param name="filePaths">Rutas de los archivos
        /// en los que se va a realizar el reemplazo.</param>
        /// <param name="from">Cadena a reemplazar.</param>
        /// <param name="to">Cadena que reemplaza.</param>
        public TextReplacementBatch(IEnumerable<string> filePaths, string from, string to)
        {
            _filePaths = new List<string>(filePaths);
            _from = from;
            _to = to;
        }

        /// <summary>
        /// Lleva a cabo el reemplazo de texto.
        /// </summary>
        public void Perform()
        {
            var tasks = new Task[_filePaths.Count];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew((object entry) =>
                {
                    var batchEntry = entry as BatchEntry;
                    if (batchEntry == null)
                    {
                        return;
                    }
                    else
                    {
                        var replacement = new TextReplacement(batchEntry.Path, batchEntry.From, batchEntry.To);
                        replacement.Perform();
                    }
                }, 
                new BatchEntry() { Path = _filePaths[i], From = _from, To = _to });
            }

            Task.WaitAll(tasks);
        }
    }
}
