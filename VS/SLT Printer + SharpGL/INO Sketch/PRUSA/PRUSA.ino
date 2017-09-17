#include <SPI.h>
//#include "Adafruit_MAX31855.h"
#include "PAP.h" 
#include "CNC.h"

static String SistemaID = "3D-PRINTER_001";
static CNC CNCRouter;

static const int LongBuffer = 20;
static String LastCommand = "";
static String Commands[LongBuffer];
static int InputIndex = 0;
static int CurrentIndex = 0;

float X = 0.0f;
float Y = 0.0f;
float Z = 0.0f;
float E = 0.0f;
PAPModes Modo = PAPModes::Fine;//PAPModes = Fine, Normal, Draft, Faster

boolean SendWait = false;

void setup()
{
  //Configuro el PWM a 30 kHz.
  TCCR3B = TCCR3B & (0b11111000 | 0x05); //timer 3 (controls pin 5, 3, 2)
  TCCR4B = TCCR4B & (0b11111000 | 0x05); //timer 4 (controls pin 8, 7, 6)

  //Inicio el Buffer de Comandos
  for (int i = 0; i > LongBuffer; i++)
  {
    Commands[i] = "";
  }

  // initialize serial:
  Serial.begin(115200);

  delay(500);
  //SistemaID
  Serial.println(SistemaID);
  delay(1000);

  Serial.println("INFArranque de maquina:");
  //Serial.end();
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
  Serial.println("INFInicializando coordenadas ...");
  //Serial.end();
  CNCRouter.GoToOrigen( true, true, true);
  //Serial.begin(115200);
  Serial.println("INFCalentando Fusor ...");
  //Serial.end();
  CNCRouter.SetTempFusor(150, false);
  //Serial.begin(115200);
  Serial.println("INFArranque de maquina finalizado.");
  //Serial.end();
}

void loop()
{
  if (RunCommand())
  {
    String CommandType = Commands[CurrentIndex].substring(0, 3);
    String Command = Commands[CurrentIndex].substring(3);
    //Serial.begin(115200);
    //Serial.print("INF" + CommandType);
    //Serial.println("|" + Command);
    //Serial.end();

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
        Modo = PAPModes::Fine;
      }
    }
    else if (CommandType == "RUN")
    {
      CNCRouter.DefineDestino(X, Y, Z, E, Modo);
      E = 0.0f;//es incremental no absoluta como X, Y y Z
      CNCRouter.Run(false);
    }
    else if (CommandType == "LIN")
    {
      CNCRouter.SetTempFusor(341, true);
      
      float const IncLayer = 0.2;
      float CurrLayer = 0.0;
      
      for(int lay = 0; lay < 20; lay++)
      {
        CNCRouter.DefineDestino(5.0f, 5.0f, CurrLayer, 0.0f, PAPModes::Fine);
        CNCRouter.Run(false);
        CNCRouter.DefineDestino(20.0f, 20.0f, CurrLayer, 40.0f, PAPModes::Draft);
        CNCRouter.Run(false);
        CurrLayer += IncLayer;
      }
      
      CNCRouter.SetTempFusor(100, false);
      
      CNCRouter.DefineDestino(0.0f, 0.0f, CurrLayer + 50.0f, 0.0f, PAPModes::Fine);
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
      //No se contempla el comando
      //Serial.begin(115200);
      Serial.print("WRNNo se ha reconocido el comando: '");
      Serial.print(Commands[CurrentIndex]);
      Serial.println("'");
      //Serial.end();
    }

    Commands[CurrentIndex] = "";
    CurrentIndex = NextIndex(CurrentIndex);
    SendWait = false;
  }
  else
  {
    //Espera Comando
    if (!SendWait)
    {
      Serial.println("WAITING...");
      SendWait = true;
    }

    //Tareas de mantenimiento
    CNCRouter.RegulaVentExtrusor();
    CNCRouter.RegulaVentDrivers();
    delay(500);
    CNCRouter.RegulaFusor(false);

    //...
  }
}

boolean RunCommand()
{
  if (InputIndex == CurrentIndex && Commands[CurrentIndex] == "")
  {
    return false;
  }
  else
  {
    return true;
  }
}

int NextIndex(int Index)
{
  int Temp;
  //Serial.print("NextIndex: ");
  //Serial.print(Index);
  //Serial.print(" --> ");
  if (Index == (LongBuffer - 1))
  {
    Temp = 0;
  }
  else
  {
    Temp = Index;
    Temp++;
  }
  //Serial.println(Temp);
  return Temp;
}

void serialEvent()
{
  char inChar;
  while (Serial.available())
  {
    // get the new byte:
    inChar = (char)Serial.read();
    // add it to the inputString:
    LastCommand += inChar;
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n')
    {
      if (Commands[InputIndex] != "")
      {
        Serial.println("WRNBuffer Serial Completo.");
      }
      else
      {
        Commands[InputIndex] = LastCommand;
        //Serial.print("InputIndex: ");
        //Serial.print(InputIndex);
        //Serial.print(" --> ");
        InputIndex = NextIndex(InputIndex);
        //Serial.println(InputIndex);
        LastCommand = "";
      }
    }
  }
}
