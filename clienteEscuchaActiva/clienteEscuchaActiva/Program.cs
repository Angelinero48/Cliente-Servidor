using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IronPdf;
using System.Linq.Expressions;

namespace ClienteEscuchaActiva
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
            else
            {
                try
                {
                    dynamic configObject = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(baseDir, "config.json")));

                    int port = configObject.port;
                    string ip = configObject.ip;
                    string path = configObject.path;
                    string accion = configObject.accion;
                    var printers = configObject.printers;
                    bool deleteFiles = configObject.deleteFiles;

                    while (true)
                    {
                        using (TcpClient cliente = new TcpClient())
                        {
                            try
                            {
                                await cliente.ConnectAsync(ip, port);
                                Console.WriteLine("Conectado al servidor.");

                                var stream = cliente.GetStream();

                                while (cliente.Connected)
                                {   
                                        byte[] longitudNombreBytes = new byte[sizeof(int)];
                                        int bytesRead = await stream.ReadAsync(longitudNombreBytes, 0, longitudNombreBytes.Length);

                                        int longitudNombre = BitConverter.ToInt32(longitudNombreBytes, 0);

                                        byte[] nombreBytes = new byte[longitudNombre];
                                        bytesRead = await stream.ReadAsync(nombreBytes, 0, nombreBytes.Length);


                                        string nombreArchivo = Encoding.UTF8.GetString(nombreBytes);


                                        using (MemoryStream memoryStream = new MemoryStream())
                                        {
                                            byte[] buffer = new byte[1024];
                                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                            {
                                                await memoryStream.WriteAsync(buffer, 0, bytesRead);
                                                if (bytesRead < buffer.Length)
                                                {
                                                    break;
                                                }
                                            }
                                            
                                            byte[] pdfBytes = memoryStream.ToArray();

                                            string pdfSavePath = Path.Combine(baseDir, path, $"{nombreArchivo}");

                                            Directory.CreateDirectory(Path.Combine(baseDir, path));

                                            File.WriteAllBytes(pdfSavePath, pdfBytes);


                                            string tipoArchivo = nombreArchivo.Substring(0, 3);

                                            switch (accion)
                                            {
                                                case "open":
                                                    Process.Start(new ProcessStartInfo
                                                    {
                                                        FileName = pdfSavePath,
                                                        UseShellExecute = true
                                                    });
                                                    break;

                                                case "print":
                                                    foreach (var impresiones in printers)
                                                    {
                                                        if (tipoArchivo == impresiones.type.ToString())
                                                        {
                                                            string impresora = impresiones.printer;
                                                            short quantity = impresiones.quantity;
                                                            bool duplex = impresiones.duplex;

                                                            var networkPrinterName = impresora;
                                                            PdfDocument pdf = new PdfDocument(pdfSavePath);
                                                            PrinterSettings settings = new PrinterSettings()
                                                            {
                                                                PrinterName = networkPrinterName,
                                                                Copies = quantity,
                                                                Duplex = duplex ? Duplex.Horizontal : Duplex.Simplex,
                                                            };
                                                            PrintDocument document = pdf.GetPrintDocument(settings);

                                                            foreach (string printer2 in PrinterSettings.InstalledPrinters)
                                                            {
                                                                if (printer2 == networkPrinterName)
                                                                {
                                                                    document.Print();
                                                                    Console.WriteLine($"{nombreArchivo} enviado a imprimir. \nImpresora: {impresora}\nNº copias: {quantity}");
                                                                    break;
                                                                }
                                                            }

                                                            if (deleteFiles)
                                                            {
                                                                File.Delete(pdfSavePath);
                                                            }
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                    
                                }
                            }
                            catch (IOException ex)
                            {
                                Console.WriteLine($"Error de E/S: {ex.Message}");
                            }
                            catch (SocketException ex)
                            {
                                Console.WriteLine($"Error de socket: {ex.Message}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
