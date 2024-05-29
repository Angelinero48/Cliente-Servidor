using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServidorEscuchaActiva
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            if (!File.Exists(Path.Combine(baseDir, "config.json")))
            {
                throw new FileNotFoundException("No se encontró el archivo config.json");
            }

            dynamic configObject = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(baseDir, "config.json")));

            int port = configObject.port;
            bool deleteFiles = configObject.deleteFiles;
            var files = configObject.files;

            

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            try
            {
                listener.Start();
                Console.WriteLine("Escuchando conexiones entrantes...");

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("Conectado al cliente.");

                    using (FileSystemWatcher watcher = new FileSystemWatcher())
                    {

                        foreach (var file in files)
                        {
                            string path = file.path;
                            string pathCompleto;
                            if (path.StartsWith("\\"))
                            {
                                path = path.TrimStart('\\');
                                pathCompleto = Path.Combine(baseDir, path);
                            }
                            else
                            {
                                pathCompleto = path;
                            }


                            watcher.Path = pathCompleto;
                            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                            watcher.Filter = "*.*";
                            watcher.EnableRaisingEvents = true;

                            watcher.Created += async (sender, e) =>
                            {
                                foreach (var file in files)
                                {
                                    string path = file.path;
                                    string prefix = file.prefix;
                                    string ext = file.ext;

                                    string pathCompleto;
                                    if (path.StartsWith("\\"))
                                    {
                                        path = path.TrimStart('\\');
                                        pathCompleto = Path.Combine(baseDir, path);
                                    }
                                    else
                                    {
                                        pathCompleto = path;
                                    }

                                    string[] archivos;
                                    if (string.IsNullOrEmpty(prefix))
                                    {
                                        archivos = Directory.GetFiles(pathCompleto, "*.*", SearchOption.TopDirectoryOnly)
                                            .Where(s => Path.GetExtension(s).Equals(ext, StringComparison.OrdinalIgnoreCase))
                                            .ToArray();
                                    }
                                    else
                                    {
                                        archivos = Directory.GetFiles(pathCompleto, "*.*", SearchOption.TopDirectoryOnly)
                                            .Where(s => Path.GetExtension(s).Equals(ext, StringComparison.OrdinalIgnoreCase) &&
                                                        Path.GetFileName(s).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                            .ToArray();
                                    }

                                    foreach (var archivo in archivos)
                                    {
                                        Console.WriteLine("Enviando archivo: " + archivo);

                                        byte[] archivoArray = File.ReadAllBytes(archivo);
                                        string nombreConExtension = Path.GetFileName(archivo);
                                        byte[] nombreBytes = Encoding.UTF8.GetBytes(nombreConExtension);
                                        byte[] longitudNombreBytes = BitConverter.GetBytes(nombreBytes.Length);

                                        try
                                        {
                                            if (client.Connected)
                                            {
                                                NetworkStream ns = client.GetStream();

                                                // Enviar la longitud del nombre del archivo
                                                await ns.WriteAsync(longitudNombreBytes, 0, longitudNombreBytes.Length);

                                                // Enviar el nombre del archivo
                                                await ns.WriteAsync(nombreBytes, 0, nombreBytes.Length);

                                                // Enviar los datos del archivo
                                                await ns.WriteAsync(archivoArray, 0, archivoArray.Length);

                                                Console.WriteLine($"{nombreConExtension} enviado al cliente");

                                                await Task.Delay(1000);

                                                if (deleteFiles)
                                                {
                                                    File.Delete(archivo);
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("El cliente se ha desconectado.");
                                                break; // Salir del bucle si el cliente se desconecta
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error al enviar archivo: {ex.Message}");
                                            await Task.Delay(TimeSpan.FromSeconds(1)); // Espera antes de reintentar
                                        }
                                    }
                                }

                            };

                            // Mantener la aplicación corriendo
                            await Task.Delay(Timeout.Infinite);
                        }
                    }
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
