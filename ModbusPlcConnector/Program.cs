
// Uncomment following line if you have a ModBus device.
// #define I_DO_HAVE_MODBUS_DEVICE

using System;
using System.Linq;
using Vortex.Adapters.Connector.Tc3.Adapter;

namespace ModbusPlcConnector
{
    class Program
    {

        const string AMS_ID = "172.20.10.2.1.1"; // set your ams id
        const int AMS_PORT = 851;

        const string modbusIP = "172.20.10.55"; // set your ModBus end point IP
        const int modbusPort = 502;             // set your ModBus end point port

        static void Main(string[] args)
        {
            // Connects to the twin of PLC
            _connector = new ModbusPlc.ModbusPlcTwinController(Tc3ConnectorAdapter.Create(AMS_ID, AMS_PORT, true));
            // Kicks off twin operations
            _connector.Connector.BuildAndStart();
            // Sets cyclic access inter-loop delay
            _connector.Connector.ReadWriteCycleDelay = 10;

#if I_DO_HAVE_MODBUS_DEVICE
            // Connect to modbus client
            _modbusClient.Connect(modbusIP, modbusPort);
#endif
            // With this we just initialize which method will execute on PLC remote call.
            _connector.MAIN._context._touchModbus.Initialize(() => ModBusComTask());
   
            // Will just wait. End the application when enter is pressed.
            Console.Read();
        }

        static ModbusPlc.ModbusPlcTwinController _connector;
        static short dummy = 0;

#if I_DO_HAVE_MODBUS_DEVICE            
        static EasyModbus.ModbusClient _modbusClient = new EasyModbus.ModbusClient();
#endif

        // This will be called each time remote task is invoked from the PLC
        static void ModBusComTask()
        {
            
#if I_DO_HAVE_MODBUS_DEVICE
            // Writes PLC variable 'MAIN._context.modbusvar1';
            _modbusClient.WriteSingleRegister(1000, _connector.MAIN._context.modbusvar1.Synchron);

            // Reads PLC variable 'MAIN._context.modbusvar1';
            _connector.MAIN._context.modbusvar1.Synchron = (short)_modbusClient.ReadInputRegisters(1000, 1).FirstOrDefault();
#else
            _connector.MAIN._context.modbusvar1.Synchron = dummy++;
            _connector.MAIN._context.modbusvar2.Synchron = (short)(dummy / 2);
#endif
            Console.WriteLine($"{_connector.MAIN._context.modbusvar1.Symbol} : {_connector.MAIN._context.modbusvar1.Synchron}");
            Console.WriteLine($"{_connector.MAIN._context.modbusvar2.Symbol} : {_connector.MAIN._context.modbusvar2.Synchron}");
        }
    }
}
