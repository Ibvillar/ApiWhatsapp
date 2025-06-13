namespace ApiWhatsapp.Helpers
{
    public class FicherosHelper
    {
        public FicherosHelper() {}

        /// <summary>
        /// Copia un archivo de una ruta origen a una ruta destino.
        /// </summary>
        /// <param name="rutaOrigen">Ruta completa del archivo de origen.</param>
        /// <param name="rutaDestino">Ruta completa del archivo destino (incluye nombre del archivo).</param>
        /// <param name="sobrescribir">Indica si debe sobrescribir el archivo destino si ya existe.</param>
        public static async Task CopiarArchivoDesdeFormFile(IFormFile file, string rutaDestino, bool sobrescribir = true)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("El archivo está vacío o es nulo.", nameof(file));

            string? carpetaDestino = Path.GetDirectoryName(rutaDestino);

            if (string.IsNullOrWhiteSpace(carpetaDestino))
                throw new ArgumentException("La ruta de destino no contiene una carpeta válida.", nameof(rutaDestino));

            if (!Directory.Exists(carpetaDestino))
                Directory.CreateDirectory(carpetaDestino);

            // Guardar el contenido del IFormFile en la ruta de destino
            using var stream = new FileStream(rutaDestino, sobrescribir ? FileMode.Create : FileMode.CreateNew);
            await file.CopyToAsync(stream);
        }

    }
}
