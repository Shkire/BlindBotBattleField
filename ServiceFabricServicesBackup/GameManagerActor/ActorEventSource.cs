using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace GameManagerActor
{
    [EventSource(Name = "MyCompany-BlindBotBattleField-GameManagerActor")]
    internal sealed class ActorEventSource : EventSource
    {
        public static readonly ActorEventSource Current = new ActorEventSource();

        static ActorEventSource()
        {
            // Esta es una solución para el problema por el que no se hace el seguimiento de la actividad de ETW hasta que se inicializa la infraestructura Tasks.
            // Este problema se corregirá en .NET Framework 4.6.2.
            Task.Run(() => { });
        }

        // El constructor de instancia es privado para exigir la semántica singleton
        private ActorEventSource() : base() { }

        #region Palabras clave
        // Las palabras clave de eventos se pueden usar para categorizar eventos. 
        // Cada palabra clave es una marca de bits. Un solo evento se puede asociar a varias palabras clave (con la propiedad EventAttribute.Keywords).
        // Las palabras clave deben definirse como una clase pública denominada 'Keywords' dentro del elemento EventSource que las usa.
        public static class Keywords
        {
            public const EventKeywords HostInitialization = (EventKeywords)0x1L;
        }
        #endregion

        #region Eventos
        // Defina un método de instancia para cada evento que desee registrar y aplíquele un atributo [Event].
        // El nombre del método es el nombre del evento.
        // Pase los parámetros que desee registrar con el evento (solo se permiten tipos enteros primitivos, DateTime, GUID y string).
        // La implementación de cada método de evento debe comprobar si el origen del evento está habilitado y, si lo está, llamar al método WriteEvent() para generar el evento.
        // El número y el tipo de los argumentos pasados a cada método de evento deben coincidir exactamente con los que se pasen a WriteEvent().
        // Ponga el atributo [NonEvent] en todos los métodos que no definen un evento.
        // Para obtener más información, vea https://msdn.microsoft.com/es-es/library/system.diagnostics.tracing.eventsource.aspx.

        [NonEvent]
        public void Message(string message, params object[] args)
        {
            if (this.IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                Message(finalMessage);
            }
        }

        private const int MessageEventId = 1;
        [Event(MessageEventId, Level = EventLevel.Informational, Message = "{0}")]
        public void Message(string message)
        {
            if (this.IsEnabled())
            {
                WriteEvent(MessageEventId, message);
            }
        }

        [NonEvent]
        public void ActorMessage(Actor actor, string message, params object[] args)
        {
            if (this.IsEnabled()
                && actor.Id != null
                && actor.ActorService != null
                && actor.ActorService.Context != null
                && actor.ActorService.Context.CodePackageActivationContext != null)
            {
                string finalMessage = string.Format(message, args);
                ActorMessage(
                    actor.GetType().ToString(),
                    actor.Id.ToString(),
                    actor.ActorService.Context.CodePackageActivationContext.ApplicationTypeName,
                    actor.ActorService.Context.CodePackageActivationContext.ApplicationName,
                    actor.ActorService.Context.ServiceTypeName,
                    actor.ActorService.Context.ServiceName.ToString(),
                    actor.ActorService.Context.PartitionId,
                    actor.ActorService.Context.ReplicaId,
                    actor.ActorService.Context.NodeContext.NodeName,
                    finalMessage);
            }
        }

        // En el caso de eventos muy frecuentes, puede ser más útil generarlos con la API WriteEventCore.
        // Esto permite un control más eficaz de los parámetros, pero requiere la asignación explícita de una estructura EventData y código inseguro.
        // Para habilitar esta ruta de código, defina el símbolo de compilación condicional UNSAFE y active la admisión de código inseguro en las propiedades del proyecto.
        private const int ActorMessageEventId = 2;
        [Event(ActorMessageEventId, Level = EventLevel.Informational, Message = "{9}")]
        private
#if UNSAFE
            unsafe
#endif
            void ActorMessage(
            string actorType,
            string actorId,
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string nodeName,
            string message)
        {
#if !UNSAFE
            WriteEvent(
                    ActorMessageEventId,
                    actorType,
                    actorId,
                    applicationTypeName,
                    applicationName,
                    serviceTypeName,
                    serviceName,
                    partitionId,
                    replicaOrInstanceId,
                    nodeName,
                    message);
#else
                const int numArgs = 10;
                fixed (char* pActorType = actorType, pActorId = actorId, pApplicationTypeName = applicationTypeName, pApplicationName = applicationName, pServiceTypeName = serviceTypeName, pServiceName = serviceName, pNodeName = nodeName, pMessage = message)
                {
                    EventData* eventData = stackalloc EventData[numArgs];
                    eventData[0] = new EventData { DataPointer = (IntPtr) pActorType, Size = SizeInBytes(actorType) };
                    eventData[1] = new EventData { DataPointer = (IntPtr) pActorId, Size = SizeInBytes(actorId) };
                    eventData[2] = new EventData { DataPointer = (IntPtr) pApplicationTypeName, Size = SizeInBytes(applicationTypeName) };
                    eventData[3] = new EventData { DataPointer = (IntPtr) pApplicationName, Size = SizeInBytes(applicationName) };
                    eventData[4] = new EventData { DataPointer = (IntPtr) pServiceTypeName, Size = SizeInBytes(serviceTypeName) };
                    eventData[5] = new EventData { DataPointer = (IntPtr) pServiceName, Size = SizeInBytes(serviceName) };
                    eventData[6] = new EventData { DataPointer = (IntPtr) (&partitionId), Size = sizeof(Guid) };
                    eventData[7] = new EventData { DataPointer = (IntPtr) (&replicaOrInstanceId), Size = sizeof(long) };
                    eventData[8] = new EventData { DataPointer = (IntPtr) pNodeName, Size = SizeInBytes(nodeName) };
                    eventData[9] = new EventData { DataPointer = (IntPtr) pMessage, Size = SizeInBytes(message) };

                    WriteEventCore(ActorMessageEventId, numArgs, eventData);
                }
#endif
        }

        private const int ActorHostInitializationFailedEventId = 3;
        [Event(ActorHostInitializationFailedEventId, Level = EventLevel.Error, Message = "Actor host initialization failed", Keywords = Keywords.HostInitialization)]
        public void ActorHostInitializationFailed(string exception)
        {
            WriteEvent(ActorHostInitializationFailedEventId, exception);
        }
        #endregion

        #region Métodos privados
#if UNSAFE
            private int SizeInBytes(string s)
            {
                if (s == null)
                {
                    return 0;
                }
                else
                {
                    return (s.Length + 1) * sizeof(char);
                }
            }
#endif
        #endregion
    }
}
