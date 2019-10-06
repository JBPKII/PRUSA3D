using SLT_Printer.SLT;
using System;
using System.IO.Ports;

namespace SLT_Printer
{
    public enum PrinterStatus
    {
        Stoped,
        Printing,
        Pause
    }

    public class PrintController
    {
        internal ModelSLT Model { get; private set; }
        public PrinterInterpreter Interpreter { get; private set; }
        private Punto lastPoint { get; set; }
        public PrinterStatus Status { get; private set; } = PrinterStatus.Stoped;

        public PrintController (ref SerialPort serialPort)
        {
            Interpreter = new PrinterInterpreter(ref serialPort);
            Model = new ModelSLT();
            Model.ModelChange += Stop;
        }

        public void Print(bool isTest = false)
        {
            if (Status != PrinterStatus.Printing)
            {
                Interpreter.IsTest = isTest;
                Interpreter.RequireCommand += PrinterInterpreter_RequireCommand;

                /*if (_TImprimir == null || !_TImprimir.IsAlive)
                {
                    _TImprimir = new Thread(new ThreadStart(_Draw));
                    _TImprimir.Start();
                }
                else
                {
                    //avisa de que ya está en ejecución
                    System.Windows.Forms.MessageBox.Show("Proceso de impresión ejecutándose.");
                }*/

                StrokeSLT _Trazo = null;

                //Printing = true;

                /*_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
                _TGenerarLayer.Start();
                _GenerandoLayer = true;*/

                Interpreter.SendTemperature(160.0);

                Model.GenerateLayer();

                /*while(_GenerandoLayer )
                {
                    System.Threading.Thread.Sleep(500);
                }*/

                Model.ChangeLayer();

                /*_TGenerarLayer = new Thread(new ThreadStart(_GeneraLayer));
                _TGenerarLayer.Start();*/

                Model.GenerateLayer();


                //if (ZCalculo <= Solido.Top)

                //IsTrazando = true;

                _Trazo = Model.FirstStroke();

                //Para calcular la distancia del trazado
                lastPoint = new Punto(_Trazo.Destino.X, _Trazo.Destino.Y, _Trazo.Destino.Z);

                //Envía el trazo
                Interpreter.SendStroke(_Trazo);
            }
        }

        private void PrinterInterpreter_RequireCommand()
        {
            // _SendNextStroke = true;

            StrokeSLT strokeSLT = Model.NextStroke();

            if (strokeSLT != null)
            {
                Punto current = new Punto(strokeSLT.Destino.X, strokeSLT.Destino.Y, strokeSLT.Destino.Z);
                double distance = lastPoint.Distancia(current);
                lastPoint = current;

                Interpreter.SendStroke(strokeSLT, distance);
            }
            else
            {
                OnStop();
            }
        }

        public void Pause()
        {
            try
            {
                Interpreter.RequireCommand -= PrinterInterpreter_RequireCommand;
            }
            catch (Exception)
            { }
            Status = PrinterStatus.Pause;
        }

        public void Resume()
        {
            if (Status == PrinterStatus.Pause)
            {
                Interpreter.RequireCommand += PrinterInterpreter_RequireCommand;
                PrinterInterpreter_RequireCommand();
                Status = PrinterStatus.Printing;
            }
        }

        public void Stop()
        {
            if (Status == PrinterStatus.Pause || Status == PrinterStatus.Printing)
            {
                OnStop();
            }
        }

        private void OnStop()
        {
            Status = PrinterStatus.Stoped;

            double gapInMilimetres = 50.0/*mm*/;

            try
            {
                Interpreter.RequireCommand -= PrinterInterpreter_RequireCommand;
            }
            catch (Exception)
            { }
            
            Interpreter.SendTemperature();

            // Presenta el trazado
            StrokeSLT trzPresent = new StrokeSLT();
            trzPresent.Destino = new VertexSLT(lastPoint.X, lastPoint.Y, lastPoint.Z + gapInMilimetres/*mm*/);
            trzPresent.E = 0.0;
            trzPresent.Mode = Modes.ModeTraslation;
            trzPresent.Pendiente = true;
            Interpreter.SendStroke(trzPresent);

            trzPresent.Destino = new VertexSLT(0.0, 0.0, lastPoint.Z + gapInMilimetres/*mm*/);
            trzPresent.E = 0.0;
            trzPresent.Mode = Modes.ModeTraslation;
            trzPresent.Pendiente = true;
            Interpreter.SendStroke(trzPresent);
        }
    }
}
