using System;
using System.Text;
using System.Text.Json;

namespace GComFuelManager.Client.Repositorios
{
    //Repositorio dedicado a uso de peticiones HTTP (GET, PUT, DELETE & POST)
    public class Repositorio : IRepositorio
    {
        private readonly HttpClient httpClient;

        public Repositorio(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        private JsonSerializerOptions OpcionesPorDefectoJson => new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        //Method Post
        //Encapsularemos el objeto para despues poderlo enviar, recibimos un string url con el objeto encapsulado y le daremos funcionalidad a partir de T
        public async Task<HttpResponseWrapper<object>> Post<T>(string url, T enviar)
        {
            //Serializamos el objeto que vamos a enviar
            var enviarJSON = JsonSerializer.Serialize(enviar);
            //Variable para enviar la informacion
            var enviarContent = new StringContent(enviarJSON, Encoding.UTF8, "application/json");
            //Enviamos la peticion
            var responseHttp = await httpClient.PostAsync(url, enviarContent);
            //Encapsulamos la respuesta de la peticion
            return new HttpResponseWrapper<object>(null!, !responseHttp.IsSuccessStatusCode, responseHttp);
        }
        //Method Post para deserializar la respuesta de un dato que se reciba del servidor
        //Con tal de recibir el ID de la URL y poderla enviar mediante POST
        //Encapsulamos el objeto para despues poderlo enviar, recibiendo un string de la URL y le damos funcionalidad(envio) mediante T
        public async Task<HttpResponseWrapper<TResponse>> Post<T, TResponse>(string url, T enviar)
        {
            //Serializamos el objeto que vamos a mandar
            var enviarJSON = JsonSerializer.Serialize(enviar);
            //Enviamos la informacion del objeto
            var enviarContent = new StringContent(enviarJSON, Encoding.UTF8, "application/json");
            //Enviamos la peticion por method Post
            var responseHttp = await httpClient.PostAsync(url, enviarContent);

            //Condicion, si el response is success, deserializamos la respuesta, sino encapsulamos la respuesta del servidor
            if (responseHttp.IsSuccessStatusCode)
            {
                var response = await DeserializarRespuesta<TResponse>(responseHttp, OpcionesPorDefectoJson);
                return new HttpResponseWrapper<TResponse>(response, Error: false, responseHttp);
            }
            return new HttpResponseWrapper<TResponse>(default!, !responseHttp.IsSuccessStatusCode, responseHttp);
        }
        private async Task<T> DeserializarRespuesta<T>(HttpResponseMessage httpResponse, JsonSerializerOptions jsonSerializerOptions)
		{
            var respuestaString = await httpResponse.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(respuestaString, jsonSerializerOptions)!;
        }
        //Method para Delete
        private async Task<HttpResponseWrapper<object>> Delete(string url)
        {
            //Enviamos la peticion por method Delete
            var responseHTTP = await httpClient.DeleteAsync(url);
            //Encapsulamos la respuesta de la peticion
            return new HttpResponseWrapper<object>(null!, !responseHTTP.IsSuccessStatusCode, responseHTTP);
        }
        //Method para obtencion de datos GET
        private async Task<HttpResponseWrapper<T>> Get<T>(string url)
        {
            //Variable HTTP en donde obtendremos la data
            var respuestaHTTP = await httpClient.GetAsync(url);
            //Condicional para cuando exista una respuesta exitosa, podremos deserializar la respuesta y retornarla
            if(respuestaHTTP.IsSuccessStatusCode)
            {
                //Mandamos llamar al method DeserializarRespuesta
                var respuesta = await DeserializarRespuesta<T>(respuestaHTTP, OpcionesPorDefectoJson);
                //Encapsulamos la respuesta de la peticion
                return new HttpResponseWrapper<T>(respuesta, Error: false, respuestaHTTP);
            }
            else
            {
                return new HttpResponseWrapper<T>(default!, Error: true, respuestaHTTP);
            }
        }
        //Method PUT
        private async Task<HttpResponseWrapper<object>> Put<T>(string url, T enviar)
        {
            //Serializamos el objeto que se va a enviar
            var enviarJson = JsonSerializer.Serialize(enviar);
            //Enviamos la informacion
            var enviarContent = new StringContent(enviarJson, Encoding.UTF8, "application/json");
            //Enviamos la peticion por el method Post
            var responseHttp = await httpClient.PutAsync(url, enviarContent);
            //Encapsulamos la respuesta de la peticion
            return new HttpResponseWrapper<object>(null!, !responseHttp.IsSuccessStatusCode, responseHttp);
        }
}
}

