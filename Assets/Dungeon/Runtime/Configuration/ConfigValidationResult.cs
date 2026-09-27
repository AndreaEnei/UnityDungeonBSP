using System.Collections.Generic;

namespace Tesi.Dungeon
{
    /// <summary>Descrive l'esito della validazione di una configurazione.</summary>
    public sealed class ConfigValidationResult
    {
        /// <summary>Indica se non sono stati rilevati errori.</summary>
        public bool IsValid => Errors.Count == 0;

        /// <summary>Elenco degli errori trovati durante la validazione.</summary>
        public IReadOnlyList<string> Errors { get; }

        public ConfigValidationResult(IReadOnlyList<string> errors)
        {
            Errors = new List<string>(errors).AsReadOnly();
        }
    }
}
