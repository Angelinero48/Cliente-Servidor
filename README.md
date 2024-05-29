### Documentación Cliente Servidor. README.md

#### Resumen
Servidor en escucha activa, cuando se conecta un cliente, el servidor a través de un watcher, identifica cuando se crea un nuevo archivo en X carpeta, y manda los archivos pdf de esas carpetas al cliente. Una vez llegado al cliente, segun las opciones puestas en el config.json, podremos abrir este archivo o imprimirlo, previamente guardado en X carpeta, especificada en el config.

#### Parámetros Cliente

**acción** (string): Define que hacer con los archivos recibidos:
 - `open`: Abre el archivo recibido
 - `print`: Manda el archivo a imprimir

**ip** (string): Define la ip a la que conectarse (la del servidor):
 - `xxx.xxx.x.xxx`: Ip del servidor.
 
**port** (int): Define el puerto a través del cual conectarse al servidor.
 - `xxxx`: Puerto para conectarse al servidor.

**path** (string): Define la ruta donde guardar los archivos recibidos.
 - `carpetaDestino`: Ruta relativa donde guardar el archivo.
 - `c:\\X\xxxx\carpetaDestino`: Ruta absoluta donde guardar el archivo.

**deleteFiles** (boolean): Define si eliminar los archivos o no una vez enviados a imprimir.
 - `true`: Elimina los archivos.
 - `false`: No elimina los archivos.

**printers**: Printers es un array que contiene la informacion necesaria para las impresiones.
- **type** (string): Define el tipo de archivo que se recibe
  - `ALB`: Tipo de archivo, en este caso, albarán.
  - `FAC`: Tipo de archivo, en este caso, factura.
- **printers** (string): Define el nombre de la impresora.
  - `Microsoft Print to PDF`: Nombre de impresora.
- **quantity** (int): Define la cantidad de copias del archivo.
  - `1`: Realiza 1 única copia del archivo
  - `5`: Realiza 5 copias del archivo
- **duplex** (boolean): Define si las impresiones son a doble cara.
  - `true`: Realiza las impresiones a doble cara.
  - `false`: Realiza las impresiones a una cara.

 #### Ejemplo config.json (Cliente)

 ```
{
  "accion": "open",
  "ip": "192.188.4.20",
  "port": 7034,
  "path":"carpetaDestino",
  "deleteFiles":false,
  "printers": 
    [
      {
        "type": "ALB",
        "printer": "Canon G7000",
        "quantity": 2,
        "duplex": false
      },
      {
        "type": "FAC",
        "printer": "HP LaserJet",
        "quantity": 2,
        "duplex": true
      },
      {
        "type": "PRESUPUESTO",
        "printer": "impresora presupuesto",
        "quantity": 5,
        "duplex": false
      }
    ]
}
```
#### Parámetros servidor
**port** (int): Define el puerto a través del cual conectarse al cliente.
 - `xxxx`: Puerto para conectarse al cliente.

**deleteFiles** (boolean): Define si eliminar los archivos o no una vez enviados a imprimir.
 - `true`: Elimina los archivos.
 - `false`: No elimina los archivos.

**files**: Files es un array que contiene la informacion necesaria para la búsqueda de los archivos.
- **path** (string): Define la ruta donde se encuentra el archivo
  - `\\envio`: Ruta relativa.
  - `x:\\xxxx\\x\\envio`: Ruta absoluta.
- **prefix** (string): Define el prefijo del archivo a enviar.
  - `ALB`: Archivo de tipo albaran.
  - `FAC`: Archivo de tipo factura.
- **ext** (string): Define la extensión del archivo.
  - `pdf`: Archivo de extension PDF.
  - `xls`: Archivo de extension xls.
 
#### Ejemplo config.json (Servidor)
 
```
  {
  "port": 7034,
  "deleteFiles": true,
  "files": [
    {
      "path": "\\envioALB",
      "prefix": "",
      "ext": ".pdf"
    },
    {
      "path": "\\envioFAC",
      "prefix": "",
      "ext": ".pdf"
    }
   ]
  }
``` 
