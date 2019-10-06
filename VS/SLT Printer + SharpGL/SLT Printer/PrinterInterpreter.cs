using SLT_Printer.SLT;
using System;
using System.IO.Ports;

namespace SLT_Printer
{
    public class PrinterInterpreter
    {
        private SerialPort SerialPort { get; set; }
        public bool IsTest { private get; set; }

        public PrinterInterpreter (ref SerialPort serialPort)
        {
            SerialPort = serialPort;

            serialPort.DtrEnable = true;
            serialPort.ReceivedBytesThreshold = 1;
            serialPort.DataReceived += Port_DataReceived;
        }

        internal bool SendStroke(StrokeSLT Trazo, double mmMaterial = 0.0)
        {
            bool Res = false;

            if (IsTest)
            {
                mmMaterial = 0.0;
            }

            Trazo.E = mmMaterial;

            try
            {
                SerialPort.WriteLine(Trazo.ToPrinter());

                OnLog("SEND: " + Trazo.ToPrinter());

                Res = true;
            }
            catch (System.Exception sysEx)
            {
                System.Windows.Forms.MessageBox.Show(sysEx.ToString());
            }

            return Res;
        }

        public bool SendTemperature(double temperature = 100.0)
        {
            if (IsTest)
            {
                return true;
            }

            return SendTemperature(temperature.ToString("F1").Replace('.', ','));
        }

        private bool SendTemperature(string temperature = "100,0")
        {
            bool Res = false;

            try
            {
                SerialPort.WriteLine("ST=" + temperature);

                OnLog("SEND: " + "ST=" + temperature);

                Res = true;
            }
            catch (System.Exception sysEx)
            {
                System.Windows.Forms.MessageBox.Show(sysEx.ToString());
            }

            return Res;
        }

        private static string _LastCommand = "";//not nullable, requires lock

        private void Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            while (SerialPort.BytesToRead > 0)
            {
                char TempChar = Convert.ToChar(Convert.ToByte(SerialPort.ReadByte()));

                if (TempChar == '\n' || TempChar == '\r')
                {
                    //Interpreta el comando
                    if (_LastCommand != "" && ReadCommand())
                    {
                        _LastCommand = "";
                    }
                }
                else if (TempChar != 'Â')
                {
                    _LastCommand += TempChar;
                }
            }
        }

        Punto lastPoint = new Punto(0.0, 0.0, 0.0);
        private bool ReadCommand()
        {
            bool Executed = false;

            lock (_LastCommand)
            {
                if (_LastCommand != null && _LastCommand.Length > 3)
                {
                    string Tipo = _LastCommand.Substring(0, 3);
                    string Comando = _LastCommand.Substring(3);

                    OnLog("READ: " + Tipo + Comando);

                    Executed = true;

                    switch (Tipo)
                    {
                        case "XYZ":
                            //Muestra la posición
                            string[] temp = Comando.Split(';');
                            try
                            {
                                OnChangeXYZ(Convert.ToSingle(temp[0]), Convert.ToSingle(temp[1]), Convert.ToSingle(temp[2]));
                            }
                            catch (System.Exception)
                            {
                                OnChangeXYZ(double.NaN, double.NaN, double.NaN);
                            }
                            break;
                        case "INF":
                            //Información del funcionamiento
                            OnInformation(Comando);
                            break;
                        case "WRN":
                            //Advertencia
                            OnWarning(Comando);
                            break;
                        /*case "DON"://E
                            if (Comando == "E")
                            {
                                _SendNBextStroke = true;
                            }
                            OnLog("Comando: " + Tipo);
                            break;*/
                        /*case "WAI"://TING
                            if (Comando == "TING")
                            {
                                _SendNextStroke = true;
                            }
                            OnLog("Comando: " + Tipo);
                            break;*/
                        case "BFR":
                            if (Comando == "FULL")
                            {
                                // _SendNextStroke = false;
                            }
                            else if (Comando == "EMPTY")
                            {
                                // _SendNextStroke = true;
                                OnRequireCommand();
                                /*StrokeSLT strokeSLT = NextStroke();

                                if (IsTrazando && strokeSLT != null)
                                {
                                    double distance = lastPoint.Distancia(new Punto(strokeSLT.Destino.X, strokeSLT.Destino.Y, ZTrazado));
                                    lastPoint = new Punto(strokeSLT.Destino.X, strokeSLT.Destino.Y, ZTrazado);

                                    SendStroke(strokeSLT, distance);
                                }
                                else
                                {
                                    IsTrazando = false;
                                }*/
                            }
                            OnLog("Comando: " + Tipo + Comando);
                            break;
                        default:
                            OnInformation("El tipo de comando '" + Tipo + "' con valor '" + Comando + "' no se reconoce.");
                            OnLog("Comando no reconocido: " + _LastCommand);
                            break;
                    }
                }
                else
                {
                    Executed = false;
                    OnLog("Comando re-encolado: " + _LastCommand);
                }
            }

            return Executed;
        }

        protected virtual void OnRequireCommand()
        {
            RequireCommand?.Invoke();
        }

        protected virtual void OnChangeXYZ(double X, double Y, double Z)
        {
            ChangeXYZ?.Invoke(X, Y, Z);
        }

        protected virtual void OnInformation(string Info)
        {
            Information?.Invoke(Info);
        }

        protected virtual void OnWarning(string Warn)
        {
            Warning?.Invoke(Warn);
        }

        protected virtual void OnLog(string Logging)
        {
            Log?.Invoke(Logging);
        }

        public event RequireCommandEventHandler RequireCommand;
        public event ChangedXYZEventHandler ChangeXYZ;
        public event InformationEventHandler Information;
        public event WarningEventHandler Warning;
        public event LogEventHandler Log;
    }


    public delegate void RequireCommandEventHandler();

    public delegate void ChangedXYZEventHandler(double X, double Y, double Z);

    public delegate void InformationEventHandler(string Info);

    public delegate void WarningEventHandler(string Warning);

    public delegate void LogEventHandler(string Log);
}
