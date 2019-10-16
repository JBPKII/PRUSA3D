/***************************************************
  This is a library for the comunication with Windows application

  Written by Jorge Belenguer.
  BSD license, all text above must be included in any redistribution
 ****************************************************/

#include <SPI.h>
//#include "Adafruit_MAX31855.h"
#include "PAP.h"
#include "CNC.h"

static String SistemaID = "3D-PRINTER_001";
static CNC CNCRouter;

float X = 0.0f;
float Y = 0.0f;
float Z = 0.0f;
float E = 0.0f;
PAPModes Modo = PAPModes::Traslation;//PAPModes = Traslation, Fill, Rim, Other

boolean SendWait = false;
boolean SendWARNBuffer = false;

void setup()
{
  //Configuro el PWM a 30 kHz.
  TCCR3B = TCCR3B & (0b11111000 | 0x05); //timer 3 (controls pin 5, 3, 2)
  TCCR4B = TCCR4B & (0b11111000 | 0x05); //timer 4 (controls pin 8, 7, 6)

  // initialize serial:
  Serial.begin(115200);

  delay(500);
  //SistemaID
  Serial.print("INF");
  Serial.println(SistemaID);
  delay(500);

  Serial.println("INFArranque de maquina:");
  /*CNCRouter.Attach(byte EnabledX, byte StepX, byte DirectionX, byte M0X, byte M1X, byte M2X, bool InvertirX, byte EndX, byte TempDriverX,
    byte EnabledY, byte StepY, byte DirectionY, byte M0Y, byte M1Y, byte M2Y, bool InvertirY, byte EndY, byte TempDriverY,
    byte EnabledZ, byte StepZ, byte DirectionZ, byte M0Z, byte M1Z, byte M2Z, bool InvertirZ, byte EndZ, byte TempDriverZ,
    byte PWMVentDrivers,
    byte EnabledE, byte StepE, byte DirectionE, byte M0E, byte M1E, byte M2E, bool InvertirE, byte TempDriverE,
          byte TempExtrusor, byte PWMVentE, byte TempHotEnd, byte TRIACFusor
    );*/

  //

  CNCRouter.Attach(25, 22, 23, 24, 27, 26, false, A15, A11,
                   31, 28, 29, 30, 33, 32, false, A14, A10,//29
                   37, 34, 35, 36, 39, 38, true, A13, A9,//35
                   6,
                   /*43, 40, 41, 42, 45, 44, true, A8, */
                   49, 46, 47, 48, 51, 50, true, A8,
                   A7, 7, A6, 10
                  );

  //
  //Inicializa el CNC
  Serial.println("INFCalentando Fusor ...");
  CNCRouter.SetTempFusor(180, true);
  Serial.println("INFInicializando coordenadas ...");
  CNCRouter.GoToOrigen(true, true, true);
  Serial.println("INFArranque de maquina finalizado.");
}

void loop()
{
  //Espera Comando
  if (!SendWait)
  {
    Serial.println("BFREMPTY");
    SendWait = true;
  }

  CNCRouter.XYZSerial();
  
  //Tareas de mantenimiento
  CNCRouter.RegulaVentExtrusor();
  CNCRouter.RegulaVentDrivers();
  delay(300);
  CNCRouter.RegulaFusor(false);
}

String CurrentReadingCommand = ""; 
void serialEvent()
{
  char inChar;
  while (Serial.available())
  {
    inChar = (char)Serial.read();
    if (inChar == '\n' || inChar == '\r')
    {
      if(CurrentReadingCommand != "")
      {
        ProcessCommand(CurrentReadingCommand);
      }
      CurrentReadingCommand = "";
    }
    else
    {
      CurrentReadingCommand += inChar;
    }
  }
}

void ProcessCommand(String inputCommand)
{
  String CommandType = inputCommand.substring(0, 3);
  String Command = inputCommand.substring(3);
  //Serial.print("INF" + CommandType);
  //Serial.println("|" + Command);
  
  if (CommandType == "X =")
  {
    char carray[Command.length() + 1];
    Command.toCharArray(carray, sizeof(carray));
    X = atof(carray); //convert the array into an Float
  }
  else if (CommandType == "Y =")
  {
    char carray[Command.length() + 1];
    Command.toCharArray(carray, sizeof(carray));
    Y = atof(carray); //convert the array into an Float
  }
  else if (CommandType == "Z =")
  {
    char carray[Command.length() + 1];
    Command.toCharArray(carray, sizeof(carray));
    Z = atof(carray); //convert the array into an Float
  }
  else if (CommandType == "E =")
  {
    char carray[Command.length() + 1];
    Command.toCharArray(carray, sizeof(carray));
    E = atof(carray); //convert the array into an Float
  }
  else if (CommandType == "M =")
  {
    char carray[Command.length() + 1];
    Command.toCharArray(carray, sizeof(carray));
    int Temp = atoi(carray); //convert the array into an Integer
    if (Temp < 255 && (Temp == 1 || Temp == 2 || Temp == 4 || Temp == 8))
    {
      Modo = (PAPModes)Temp;
    }
    else
    {
      Serial.println("WRNEl valor de 'M =' debe ser 1,2,4 u 8");
      Modo = PAPModes::Traslation;
    }
  }
  else if (CommandType == "RUN")
  {
    CNCRouter.DefineDestino(X, Y, Z, E, Modo);

    Serial.println("BFREMPTY");
    SendWait = true;

    CNCRouter.Run(false);
  }
  else if (CommandType == "ST=")
  {
    char carray[Command.length() + 1];
    Command.toCharArray(carray, sizeof(carray));
    int Temp = atoi(carray); //convert the array into an Integer
    CNCRouter.SetTempFusor(Temp, true);
  }
  else if (CommandType == "ID?")
  {
    Serial.println(SistemaID);
  }
  else
  {
    Serial.println("WRNNo se ha reconocido el comando: '" + inputCommand + "'.");
  }
}
