using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace GameManagerActor
{
    internal static class Program
    {
        /// <summary>
        /// Este es el punto de entrada del proceso de host del servicio.
        /// </summary>
        private static void Main()
        {
            try
            {
                // Esta línea registra un servicio de actor para hospedar la clase de actor en el runtime de Service Fabric.
                // El contenido de los archivos ServiceManifest.xml y ApplicationManifest.xml
                // se rellena automáticamente cuando se compila el proyecto.
                // Para obtener más información, vea https://aka.ms/servicefabricactorsplatform.

                ActorRuntime.RegisterActorAsync<GameManagerActor>(
                   (context, actorType) => new ActorService(context, actorType)).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
