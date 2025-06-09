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
        public static void CopiarArchivo(string rutaOrigen, string rutaDestino, bool sobrescribir = true)
        {
            if (!File.Exists(rutaOrigen))
            {
                throw new FileNotFoundException("El archivo de origen no existe.", rutaOrigen);
            }

            string? carpetaDestino = Path.GetDirectoryName(rutaDestino);

            if (string.IsNullOrWhiteSpace(carpetaDestino))
            {
                throw new ArgumentException("La ruta de destino no contiene una carpeta válida.", nameof(rutaDestino));
            }

            // Crear el directorio destino si no existe
            if (!Directory.Exists(carpetaDestino))
            {
                Directory.CreateDirectory(carpetaDestino);
            }

            // Copiar el archivo
            File.Copy(rutaOrigen, rutaDestino, sobrescribir);
        }
    }
}
