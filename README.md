# API de Envío de Correos

Esta aplicación es una **API REST** desarrollada en **.NET 8.0** para el envío de correos electrónicos a uno o más destinatarios. 

## Descripción

La API permite el envío de correos electrónicos con contenido dinámico y soporte para múltiples destinatarios. El mensaje se puede personalizar a través del cuerpo del correo y los destinatarios. Además, la configuración de la aplicación se puede ajustar para adaptarse a tus necesidades, como el puerto de despliegue y la configuración de la base de datos.

## Características

- **Envío de correos electrónicos a 1 o más destinatarios**. (ver el Postman agregado para la plantilla del body)
- **Configuración flexible** para el Remitente a travez de los endpoints.
- **Base de datos SQL Server** para la configuración del servidor SMTP.
- **Desarrollado en .NET 8.0**.

## Requisitos

- **.NET 8.0**: Asegúrate de tener instalada la versión correcta de .NET en tu máquina. Puedes descargarla desde [aquí](https://dotnet.microsoft.com/download/dotnet).
- **SQL Server**: La aplicación usa una base de datos SQL Server para almacenar la configuración del servidor SMTP. Puedes descargarla desde [aquí](https://www.microsoft.com/es-es/sql-server/sql-server-downloads)
- **Nugget packages** en la configuracion del proyecto podras observar la paqueteria requerida para su correcto funcionamiento.
- **JSONApiResponses** La aplicacion maneja el standard JsonApiResponses lo que permite al Client-APP standarizar el consumo del API.

```xml
 <ItemGroup>
   <PackageReference Include="MailKit" Version="4.11.0" />
   <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.14">
     <PrivateAssets>all</PrivateAssets>
     <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
   </PackageReference>
   <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.14" />
   <PackageReference Include="Serilog" Version="4.2.0" />
   <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
 </ItemGroup>
```

## Configuración

### Base de Datos

La aplicación utiliza una base de datos SQL Server (v20.2) para almacenar la configuración relacionada con el servidor SMTP (como el host, puerto, usuario y contraseña). Asegúrate de tener una instancia de SQL Server disponible y realizar la configuración adecuada en la base de datos.

La descripcion de la tabla es :

```sql
CREATE TABLE [SmtpSettings] (
    [Id] int NOT NULL IDENTITY,
    [Host] nvarchar(max) NOT NULL,
    [Port] int NOT NULL,
    [Username] nvarchar(max) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [UseSSL] bit NOT NULL,
    [FromEmail] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SmtpSettings] PRIMARY KEY ([Id])
);
```

Asi como tambien contiene una tabla para administrar las extensiones de correo a las cuales se permitira enviar correos. la descripcion de la tabla es : 

```sql
CREATE TABLE email_sender_db.dbo.EmailExtensions (
	Id int IDENTITY(1,1) NOT NULL,
	Extension nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Status bit NOT NULL,
	CONSTRAINT PK_EmailExtensions PRIMARY KEY (Id)
);
 CREATE UNIQUE NONCLUSTERED INDEX IX_EmailExtensions_Extension ON email_sender_db.dbo.EmailExtensions (  Extension ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
```

### AppSettings.json


el appsettings  contiene la configuracion inicial de la aplicacion, donde se debe indicar la ubicacion de la base de datos, y el puerto donde despliega la aplicacion:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "email_db": "Server=localhost\\SQLEXPRESS;Database=email_sender_db;TrustServerCertificate=True;Trusted_Connection=True;"
  },
  "Urls": "http://localhost:8081"     //Puerto de la app es lo unico que se configura. Manteniendo el http:servidor:puerto . 
}
```
**PD SOLO MODIFICAR EL CAMPO PUERTO** 


### Manejo de excepciones.

La aplicacion maneja el standard JsonApiResponse para el manejo de errores, por lo que todas las excepciones son capturadas a nivel de servicio y el middleware ExceptionHandlerMiddleware se encarga de gestionar el retorno de las respuestas.

por defecto se configura una lista de errores en el archivo "ErrorMessagesConfig.json" que se encuentra en la carpeta "{{root}}\MailSenderAPI\Utils\Exceptions"  y tiene la siguiente configuracion: 
```json
{
  "Errors": [
    {
      "Code": 404,
      "Message": "Recurso no encontrado",
      "Detail": "El recurso solicitado no pudo ser encontrado."
    },
    {
      "Code": 400,
      "Message": "Solicitud incorrecta",
      "Detail": "Los datos enviados no son válidos"
    },
    {
      "Code": 409,
      "Message": "Ya registrado",
      "Detail": "El recurso que intentas crear ya existe en la base de datos."
    },
    {
      "Code": 500,
      "Message": "Error interno del servidor",
      "Detail": "Se produjo un error inesperado"
    }
  ]
}
```

De igual forma, se pueden agregar mas detalles en el campo Detail desde la implementacion del servicio de excepciones:


```csharp
Sin estilo: 

throw _errorService.GetApiException(ErrorCodes.NotFound, "Algun mensaje adicional");

Con estilo: 
string AdditionalErrorMessage = "Algun mensaje adicional";
throw _errorService.GetApiException(ErrorCodes.NotFound, AdditionalErrorMessage;
);
```


Como el campo Detail es una List<string> se pueden agregar varios mensajes.

el estandar de respuesta en Json seria: 
```json
{
  "code": 400, //codigo de la excepcion
  "message": "Solicitud incorrecta", //mensaje preconfigurado en el archivo Json
  "detail": [
    "Los datos enviados no son válidos", //detalle preconfigurado en el Json
    "Algun mensaje adicional." //detalle agregado en la implementacion del metodo
  ]
}
```


## Funcionamiento del API
En princpio es importante configurar las extenciones que seran permitidas para el envio de correos. Posteriormente se podra registrar los datos del SMTP que enviara los correos a cada destinatario. Finalmente, podra hacer uso del servicio de envio de correos. 

La coleccion de postman esta disponible en la carpeta DOC del proyecto. sin embargo puedes acceder a ella directamente desde aca: [PostmanCollection](https://raw.githubusercontent.com/aalejog09/emailSenderAPI/main/doc/EmailSenderAPI.postman_collection.json)


### Gestionar Extensiones Permitidas.

En este apartado se gestionan las extensiones a las cuales podra enviarse correo electronicos. 

#### Registrar Extensiones de correo.
HTTP POST [CrearEmailSender]({{server}}/api/email/extension/add) 

```json
{
  "extensionName": "correo.com" // no agregar el @, es suficiente con el nombre de dominio : correo.com
}
```

Response OK:
```json
{
  "status": "Success",
  "code": 201,
  "message": "Extensión registrada con éxito.",
  "data": {
    "extensionName": "correo.com",
    "status": true
  }
}
```
Response 400:
```json
{
  "code": 400,
  "message": "Solicitud incorrecta",
  "detail": [
    "Los datos enviados no son válidos",
    "La extensión [ásdas] no es válida."
  ]
}
```



#### Listar Extensiones de correo.
HTTP GET [Listar Extenciones]({{server}}/api/email/extension/list) 

Response OK:

```json
{
  "status": "Success",
  "code": 200,
  "message": "Extensión encontrada.",
  "data": [
    {
      "extensionName": "correo.com",
      "status": true
    },
    {
      "extensionName": "correo1.com",
      "status": true
    },
    {
      "extensionName": "correo2.com",
      "status": true
    }
  ]
}
```

#### Buscar Extension por nombre

HTTP GET [Buscar Extencion por nombre]({{server}}/api/email/extension/find/{extension})  

Response OK:
```json
{
  "status": "Success",
  "code": 200,
  "message": "Extensión encontrada.",
  "data": {
    "extensionName": "correo.com",
    "status": true
  }
}
```

```json
{
  "code": 404,
  "message": "Recurso no encontrado",
  "detail": [
    "El recurso solicitado no pudo ser encontrado.",
    "La extensión 'correo3.com' no está registrada."
  ]
}
```




#### Activar/desactivar Extension por nombre

HTTP PUT [Cambiar Status por nombre]({{server}}/api/email/extension/change-status?extension={extensionName}&status={status})  

Se envian parametros en la solicitud (queryParam), suponiendo status=false

Response OK:
```json
{
  "status": "Success",
  "code": 200,
  "message": "Extensión actualizada con éxito",
  "data": {
    "extensionName": "correo.com",
    "status": false
  }
}
```
#### eliminar Extension por nombre
HTTP DELETE [Cambiar Status por nombre]({{server}}/api/email/extension/delete?extension={extesionName})  

Response OK:
```json
{
  "status": "Success",
  "code": 200,
  "message": "Extensión eliminada con éxito",
  "data": null
}
```

### Gestionar SMTP 
El Api cuenta con rutas para realizar las operaciones de CRUD para el  **EMAIL SENDER** que son los datos de configuracion del SMTP. 
se puede crear email sender, listar y eliminar por identificador unico. Importante destacar que **LA API SELECCIONA EL ULTIMO REGISTRO DE LA TABLA SMTPSETTINGS PARA SELECCIONAR EL REMITENTE**

Los datos de los Json estan especificados en el servicio de **SWAGGER** configurado.

puedes configurar un smtp a travez de los endpoints de settings:

#### Crear un smtp

HTTP POST [CrearEmailSender]({{server}}/api/email/settings/create) 
```json
{
    "host": "smtp.correo.com",
    "port": 465, //si es ssl usar el puerto seguro del proveedor de correo.
    "username": "correo@correo.com",
    "password": "clave_de_usuario_o_aplicacion",
    "useSSL": true, //esto depende del puerto, si es SSL dejar en true.
    "fromEmail": "correo@correo.com"
}
```

Luego de la validacion a los campos correspondiente, el response OK seria:

```json
{
  "status": "Success",
  "code": 201,
  "message": "Smtp registrado con éxito.",
  "data": {
    "host": "smtp.correo.com",
    "port": 465,
    "username": "correo@correo.com",
    "password": "clave_de_usuario_o_aplicacion_CIFRADA",
    "useSSL": true,
    "fromEmail": "correo@correo.com",
    "createdAt": "09-05-2025 09:48:46"
  }
}
```

PD: siempre que se cree un nuevo SMTP settigns se usara el ultimo registrado.

#### Listar los SMTP configurados
HTTP GET [listadeEmailSenders]({{server}}/api/email/settings/list]) 

Se registra una lista de SMTP para llevar un historial de los smtp configurados. ya que el servicio de envio de correos tomara el ultimo configurado. (el campo Password se muestra cifrado)
El response OK seria:


```json
{
  "status": "Success",
  "code": 200,
  "message": "Lista de SMTP",
  "data": [
    {
      "host": "smtp.gmail.com",
      "port": 465,
      "username": "correo@gmail.com",
      "password": "u8BcoQrdtPPV6khU2TZCdh3+8BcoQrdtPPV6khU2=",
      "useSSL": true,
      "fromEmail": "correo@gmail.com",
      "createdAt": "2025-03-01 11:11:19"
    },
    {
      "host": "smtp.correo.com",
      "port": 123,
      "username": "user",
      "password": "+8BcoQrdtPPV6khU2+mphw==",
      "useSSL": true,
      "fromEmail": "correo@correo.com",
      "createdAt": "2025-03-02 10:59:40"
    }
  ]
}
```

#### buscar SMTP por correo.
HTTP GET [buscarSmtp]({{server}}/api/email/settings/{FromEmail}]) 

se puede ubicar un smtp configurado por su valor FromEmail (el campo Password se muestra cifrado)
El response OK seria:

``` Json
{
  "status": "Success",
  "code": 200,
  "message": "Smtp encontrado con éxito.",
  "data": {
    "host": "smtp.correo.com",
    "port": 465,
    "username": "correo@correo.com",
    "password": "clave_de_usuario_o_aplicacion_CIFRADA",
    "useSSL": true,
    "fromEmail": "correo@correo.com",
    "createdAt": "09-05-2025 09:48:46"
  }
}
```

#### Eliminar un SMTP
HTTP: DELETE  [EliminarEmailSender]({{server}}/api/email/settings/delete/{fromEmail}) 

Se utiliza el campo de remitente (fromEmail) para ubicar en la base de datos y eliminarlo.

respuesta OK :
```json
{
  "status": "Success",
  "code": 200,
  "message": "Eliminado.",
  "data": "Smtp eliminado exitosamente."
}
```



### Envio de correo

Finalmente podra consumir el servicio de envio de correos. debera tener en cuenta que :


- Existe la extension del correo del destinatario? (correo.com)
- Esta habilitada la extencion de correo?
- el correo cumple con las caracteristicas para ser enviado (correo@correo.com)
- Si almenos 1 de los correos no es valido, no se envia a ningun destinatario. 


#### Enviar de correo

[enviar correo]({{server}}/api/email/sendMail) el cuerpo de esta peticion es :

``` json
{
  "to": "correo@correo.com", // debe indicar solo correos validos, y si es una lista deberia ser indicada en formato : "Correo1@correo.com;correo2@correo.com" (separados por ";")
  "subject": "Test API Email Sender", // el Asunto del correo
  "body": "<!DOCTYPE html><html lang='es'><head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'><title>Correo de Prueba</title><style>body {font-family: Arial, sans-serif;line-height: 1.6;background-color: #f4f4f4;margin: 0;padding: 0;} .email-container {width: 100%;background-color: #ffffff;padding: 20px;margin: 20px auto;max-width: 600px;border-radius: 8px;box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);} .header {text-align: center;font-size: 24px;margin-bottom: 20px;} .content {font-size: 16px;color: #333333;margin-bottom: 20px;} .footer {font-size: 14px;text-align: center;color: #888888;} .footer a {color: #888888;text-decoration: none;}</style></head><body><div class='email-container'><div class='header'><h2>Saludos,</h2></div><div class='content'><p>Está recibiendo esta información: <strong>Informacion</strong> desde el API de envío de correos.</p><p><strong>Otro contenido dinamico</strong></p></div><div class='footer'><p><strong>No-Reply</strong></p><p>Este es un correo automatizado, por favor no responda.</p></div></div></body></html>"
}
```

reponse 200:
``` json
{
  "status": "Success",
  "code": 200,
  "message": "Correo Enviado.",
  "data": "Correo enviado con éxito"
}
```

<div style="margin-top: 50px; text-align: center; font-size: 12px;">
    <hr>
    <h2>CREDITOS</h2>
    <p><strong>Desarrollado por Andrés Alejo</strong></p>
    <p>Marzo de 2025</p>
    <hr>
    <p><a href="https://github.com/aalejog09" target="_blank">Developer GitHub.</a></p>
</div>