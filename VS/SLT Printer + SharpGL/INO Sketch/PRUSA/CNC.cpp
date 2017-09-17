/***************************************************
  This is a library for the CNC Operations

  Written by Jorge Belenguer.
  BSD license, all text above must be included in any redistribution
 ****************************************************/

#include "CNC.h"
#include "PAP.h"
//#include <SPI.h>
//#include "Adafruit_MAX31855.h"
#include <avr/pgmspace.h>
#include <util/delay.h>
#include <stdlib.h>
//#include <SPI.h>
#include <avr/pgmspace.h>

static PAP PAPMotorX;
static PAP PAPMotorY;
static PAP PAPMotorZ;
static PAP PAPMotorE;
static PAP PAPMotorF;

//static Adafruit_MAX31855 _TermoPar;

CNC::CNC() {}

void CNC::Attach(byte EnabledX, byte StepX, byte DirectionX, byte M0X, byte M1X, byte M2X, bool InvertirX, byte EndX, byte TempDriverX,
                 byte EnabledY, byte StepY, byte DirectionY, byte M0Y, byte M1Y, byte M2Y, bool InvertirY, byte EndY, byte TempDriverY,
                 byte EnabledZ, byte StepZ, byte DirectionZ, byte M0Z, byte M1Z, byte M2Z, bool InvertirZ, byte EndZ, byte TempDriverZ,
                 byte PWMVentDrivers,
                 byte EnabledE, byte StepE, byte DirectionE, byte M0E, byte M1E, byte M2E, bool InvertirE, byte TempDriverE,
                 byte TempExtrusor, byte PWMVentE, byte TempHotEnd, byte TRIACFusor
                )
{
  PAPMotorX.Attach(EnabledX, StepX, DirectionX, M0X, M1X, M2X, InvertirX, EndX);
  PAPMotorY.Attach(EnabledY, StepY, DirectionY, M0Y, M1Y, M2Y, InvertirY, EndY);
  PAPMotorZ.Attach(EnabledZ, StepZ, DirectionZ, M0Z, M1Z, M2Z, InvertirZ, EndZ);
  PAPMotorE.Attach(EnabledE, StepE, DirectionE, M0E, M1E, M2E, InvertirE);

  _PinTempDriverX = TempDriverX;
  _PinTempDriverY = TempDriverY;
  _PinTempDriverZ = TempDriverZ;
  _PinTempDriverE = TempDriverE;
  _PinPWMVentDrivers = PWMVentDrivers;

  _PinTempExtrusor = TempExtrusor;
  _PinPWMVentE = PWMVentE;
  _PinHotEnd = TempHotEnd;

  //define pin modes
  pinMode(_PinTempDriverX, INPUT);
  pinMode(_PinTempDriverY, INPUT);
  pinMode(_PinTempDriverZ, INPUT);
  pinMode(_PinTempDriverE, INPUT);

  pinMode(_PinPWMVentDrivers, OUTPUT);
  pinMode(_PinPWMVentE, OUTPUT);
  pinMode(_PinHotEnd, INPUT);

  _PinTRIACFusor = TRIACFusor;
  pinMode(_PinTRIACFusor, OUTPUT);

  SetTempFusor(100, true);//Cambiar a true

  PAPMotorX.SetModo(PAPModes::Fine);
  PAPMotorY.SetModo(PAPModes::Fine);
  PAPMotorZ.SetModo(PAPModes::Fine);
  PAPMotorE.SetModo(PAPModes::Normal);
}

void CNC::GoToOrigen(bool X, bool Y, bool Z)
{
  PAPMotorX.SetModo(PAPModes::Fine);
  PAPMotorY.SetModo(PAPModes::Fine);
  PAPMotorZ.SetModo(PAPModes::Fine);

  RegulaVentDrivers();
  //Separo el plano Z;
  PAPMotorZ.SetSteps(_PasosSepTraslacion, PAPModes::Fine);
  PAPMotorZ.AllSteps();

  //Serial.begin(115200);

  if (Y == true)
  {
    Serial.println("INFGoToOrigen Y");
    PAPMotorY.SetEnable(false);
    PAPMotorY.SetEnable(true);
    PAPMotorY.SetModo(PAPModes::Fine);
    do
    {
      PAPMotorY.SetSteps(-200, PAPModes::Fine);
    }
    while (PAPMotorY.AllSteps() == -200);
    _CurrentY = 0;
    _RestoPasosY = 0;
  }

  if (X == true)
  {
    Serial.println("INFGoToOrigen X");
    PAPMotorX.SetEnable(false);
    PAPMotorX.SetEnable(true);
    PAPMotorX.SetModo(PAPModes::Fine);
    do
    {
      PAPMotorX.SetSteps(-200, PAPModes::Fine);
    }
    while (PAPMotorX.AllSteps() == -200);
    _CurrentX = 0;
    _RestoPasosX = 0;
  }

  if (Z == true)
  {
    Serial.println("INFGoToOrigen Z");
    PAPMotorZ.SetEnable(false);
    PAPMotorZ.SetEnable(true);
    PAPMotorZ.SetModo(PAPModes::Fine);
    do
    {
      PAPMotorZ.SetSteps(-200, PAPModes::Fine);
    }
    while (PAPMotorZ.AllSteps() == -200);
    _CurrentZ = 0;
    _RestoPasosZ = 0;
  }
  else
  { //Retorna a la posición original
    PAPMotorZ.SetSteps(-_PasosSepTraslacion, PAPModes::Fine);
    PAPMotorZ.AllSteps();
  }
  //Serial.end();
  XYZSerial();
  RegulaVentDrivers();
}

void CNC::DefineDestino(float X, float Y, float Z, float E, PAPModes Modo)
{
  if (PAPMotorX.GetModo() != Modo)
  {
    //Si cambia de impresión 1 o 2 a traslación, Baja el plato 400 pasos
    if ((PAPMotorX.GetModo() == PAPModes::Normal || PAPMotorX.GetModo() == PAPModes::Draft || PAPMotorX.GetModo() == PAPModes::Faster) &&
        (Modo == PAPModes::Fine))
    {
      PAPMotorE.SetSteps(-_PasosExtTraslacion, PAPModes::Fine);//recoge filamento
      PAPMotorE.AllSteps();

      PAPMotorZ.SetSteps(-_PasosSepTraslacion, PAPModes::Fine);//baja plataforma
      PAPMotorZ.AllSteps();

      //Termina los pasos pendientes en el modo actual
      PAPMotorX.SetSteps(_RestoPasosX, PAPMotorX.GetModo());
      PAPMotorY.SetSteps(_RestoPasosY, PAPMotorY.GetModo());
      //Falta liquidar el resto de Pasos; sobrar�a el resto
      PAPMotorZ.SetSteps(_RestoPasosZ, PAPMotorZ.GetModo());

      Run(true);
    }
    //Si cambia de traslaci�n a impresi�n 1 o 2, Sube el plato 400 pasos
    else if ((Modo == PAPModes::Normal || Modo == PAPModes::Draft || Modo == PAPModes::Faster) && (PAPMotorX.GetModo() == PAPModes::Normal))
    {
      //Falta liquidar el resto de Pasos
      PAPMotorZ.SetSteps(_PasosSepTraslacion, PAPModes::Fine);//sube plataforma
      PAPMotorZ.AllSteps();
      PAPMotorE.SetSteps(_PasosExtTraslacion, PAPModes::Fine);//extrusiona
      PAPMotorE.AllSteps();
    }
  }

  PAPMotorX.SetModo(Modo);
  PAPMotorY.SetModo(Modo);

  //Calcula Pasos o semipasos hasta el punto (se parte desde un paso completo)
  float MicropasoX = _PasoX / (float)Modo;//tama�o del micropaso en mm
  long Pasos = (long)((X - _CurrentX) / MicropasoX);//micropasos hasta la posici�n m�s cercana
  PAPMotorX.SetSteps(Pasos, Modo);
  /*Serial.print("----PAPMotorX Modo = ");
    Serial.println(Modo);
    Serial.print("----PAPMotorX MicropasoX = ");
    Serial.println(MicropasoX);
    Serial.print("----PAPMotorX _PasoX = ");
    Serial.println(_PasoX);
    Serial.print("----PAPMotorX Pasos = ");
    Serial.println(Pasos);*/
  //Calcula los semipasos hasta el paso completo
  _RestoPasosX = _RestoPasosX - (Pasos % (long)Modo);//Resto para un paso completo

  float MicropasoY = _PasoY / (float)Modo;//tama�o del micropaso en mm
  Pasos = (long)((Y - _CurrentY) / MicropasoY);//micropasos hasta la posici�n m�s cercana
  PAPMotorY.SetSteps(Pasos, Modo);
  //Calcula los semipasos hasta el paso completo
  _RestoPasosY = _RestoPasosY - (Pasos % (long)Modo);//Resto para un paso completo

  float MicropasoZ = _PasoZ;// / (float)1;//tama�o del micropaso en mm
  Pasos = (long)((Z - _CurrentZ) / MicropasoZ);//micropasos hasta la posici�n m�s cercana
  PAPMotorZ.SetSteps(Pasos, PAPModes::Fine);
  //Serial.print("PAPMotorZ Pasos = ");
  //Serial.println(Pasos);
  //Calcula los semipasos hasta el paso completo
  _RestoPasosZ = _RestoPasosZ - (Pasos % (long)1);//Resto para un paso completo
}

void CNC::Run(bool Resto)
{
  RegulaVentDrivers();

  float PasosX;
  float PasosY;
  float PasosZ;

  RegulaFusor(false);
  RegulaVentExtrusor();

  if (Resto || PAPMotorX.GetModo() == PAPModes::Fine)
  {
    //Serial.println("Modo sin extrusi�n");
    //Sin extrusi�n
    //Pasos y Modo en Y
    PasosY = PAPMotorY.AllSteps();
    //Modifico current
    _CurrentY += ((_PasoY / (float)PAPMotorY.GetModo()) * (float)PasosY);

    //Pasos y Modo en X
    PasosX = PAPMotorX.AllSteps();
    //Modifico current
    _CurrentX += ((_PasoX / (float)PAPMotorX.GetModo()) * (float)PasosX);

    //Pasos y Modo en Z
    PasosZ = PAPMotorZ.AllSteps();
    //Modifico current
    _CurrentZ += ((_PasoZ / (float)PAPMotorZ.GetModo()) * (float)PasosZ);

    //Salida por el puerto serie
    XYZSerial();
  }
  else
  {
    //Serial.println("Modo con extrusi�n");
    //Con extrusi�n
    float ContX = 0;
    float ContY = 0;
    float ContZ = 0;

    //Bucle hasta que se ha alcanzado el destino
    long DestX = 0;
    long DestY = 0;
    long DestZ = 0;

    long TempX = 0;
    long TempY = 0;
    long TempZ = 0;

    long StepsMax = max(
                      max(
                        abs(PAPMotorX.RemainSteps()), abs(PAPMotorY.RemainSteps())
                      )
                      , abs(PAPMotorZ.RemainSteps())
                    );
    //Serial.print("----StepsMax = ");
    //Serial.println(StepsMax);

    //noInterrupts();
    const float RelSetpX = (float)abs(PAPMotorX.RemainSteps()) / (float)StepsMax;
    //Serial.print("----RelSetpX = ");
    //Serial.println(RelSetpX);
    const float RelSetpY = (float)abs(PAPMotorY.RemainSteps()) / (float)StepsMax;
    //Serial.print("----RelSetpY = ");
    //Serial.println(RelSetpY);
    const float RelSetpZ = (float)abs(PAPMotorZ.RemainSteps()) / (float)StepsMax;
    //Serial.print("----RelSetpZ = ");
    //Serial.println(RelSetpZ);
    //interrupts();

    for (long i = 1; i <= StepsMax; i++)
    {
      //Serial.print("Bucle For i = ");
      //Serial.println(i);
      if (((i - 1) % 10) == 0)
      {
        XYZSerial();
        if (((i - 1) % 1000) == 0)
        {
          RegulaFusor(false);
        }
      }

      //noInterrupts();
      {
        //Calculo X
        if (PAPMotorX.RemainSteps() != 0)
        {
          //Serial.println("----Calculo X:");
          TempX = (long)(RelSetpX * (float)i);//Distancia desde el origen
          DestX = TempX - ContX;//Distancia desde la posici�n actual
          ContX = TempX;//Posici�n anterior
        }
        else
        {
          //Serial.println("----Omito X:");
          DestX = 0;
        }
        //Calculo Y
        if (PAPMotorY.RemainSteps() != 0)
        {
          //Serial.println("----Calculo Y:");
          TempY = (long)(RelSetpY * (float)i);//Distancia desde el origen
          DestY = (long)(TempY - ContY);//Distancia desde la posici�n actual
          ContY = TempY;//Posici�n anterior
        }
        else
        {
          //Serial.println("----Omito Y:");
          DestY = 0;
        }
        //Calculo Z
        if (PAPMotorZ.RemainSteps() != 0)
        {
          //Serial.println("----Calculo Z:");
          TempZ = (long)(RelSetpZ * (float)i);//Distancia desde el origen
          DestZ = (long)(TempZ - ContZ);//Distancia desde la posici�n actual
          ContZ = TempZ;//Posici�n anterior
        }
        else
        {
          //Serial.println("----Omito Z:");
          DestZ = 0;
        }
      }
      //interrupts();

      //Extrusi�n
      if (((i - 1) % (long)PAPMotorX.GetModo()) == 0)
      {
        //Extruye
        //Serial.println("!!!!!!!!!!!!!!!Extruye!!!!!!!!!!!!!");
        TODO: Calcular la extrusión
        PAPMotorE.SetSteps((long)1, PAPModes::Normal);
        PAPMotorE.AllSteps();
      }

      //Muevo X
      //if(PAPMotorX.RemainSteps() != 0)
      {
        TempX = PAPMotorX.DoSteps(DestX);
      }
      //Muevo Y
      //if(PAPMotorY.RemainSteps() != 0)
      {
        TempY = PAPMotorY.DoSteps(DestY);
      }
      //Muevo Z
      //if(PAPMotorZ.RemainSteps() != 0)
      {
        TempZ = PAPMotorZ.DoSteps(DestZ);
      }

      //noInterrupts();
      {
        //Ubico X
        //if(PAPMotorX.RemainSteps()==0)
        {
          _CurrentX += (float)(_PasoX / (float)PAPMotorX.GetModo()) * (float)TempX;
        }
        //Ubico Y
        //if(PAPMotorY.RemainSteps()==0)
        {
          _CurrentY += (float)(_PasoY / (float)PAPMotorY.GetModo()) * (float)TempY;
        }
        //Ubico Z
        //if(PAPMotorZ.RemainSteps()==0)
        {
          _CurrentZ += (float)(_PasoZ / (float)PAPMotorZ.GetModo()) * (float)TempZ;
        }
      }
      //interrupts();

      //Salida por el puerto serie
      //XYZSerial();
    }
    //Salida por el puerto serie
    XYZSerial();
  }
  Serial.println("DONE");
  RegulaFusor(false);
}

void CNC::XYZSerial(void)
{

  //Salida por el puerto serie
  //Serial.begin(115200);
  Serial.print("XYZ");
  Serial.print(_CurrentX);
  Serial.print(';');
  Serial.print(_CurrentY);
  Serial.print(';');
  Serial.println(_CurrentZ);
  //Serial.end();
}

float Map2(int val, int val0, float temp0, int val1, float temp1)
{
  return (((temp1 - temp0) * (val - val0)) / (val1 - val0)) + temp0;
}

void CNC::RegulaVentDrivers(void)
{
  //Serial.println("INFInicio Regula Ventilaci�n Drivers PAP");
  float TempDrivers = -1;

  float TempX = Map2(analogRead(_PinTempDriverX), 275, 0, 480, 22.2);
  Serial.print("INFTemperatura Driver X = ");
  Serial.print(TempX);
  Serial.println("ºC");
  if (TempDrivers < TempX)
  {
    TempDrivers = TempX;
  }

  float TempY = Map2(analogRead(_PinTempDriverY), 275, 0, 480, 22.2);
  Serial.print("INFTemperatura Driver Y = ");
  Serial.print(TempY);
  Serial.println("ºC");
  if (TempDrivers < TempY)
  {
    TempDrivers = TempY;
  }

  float TempZ = Map2(analogRead(_PinTempDriverZ), 275, 0, 480, 22.2);
  Serial.print("INFTemperatura Driver Z = ");
  Serial.print(TempZ);
  Serial.println("ºC");
  if (TempDrivers < TempZ)
  {
    TempDrivers = TempZ;
  }

  float TempE1 = Map2(analogRead(_PinTempDriverE), 275, 0, 480, 22.2);
  Serial.print("INFTemperatura Driver E1 = ");
  Serial.print(TempE1);
  Serial.println("ºC");
  if (TempDrivers < TempE1)
  {
    TempDrivers = TempE1;
  }

  {
    /*Serial.print("INFTemperatura Max Drivers = ");
      Serial.print(TempDrivers);
      Serial.println("�C");*/

    byte PotVentDriv;

    if (TempDrivers < 0)
    {
      //PWM=0%;
      PotVentDriv = 0;
    }
    else if (TempDrivers >= 0 and TempDrivers < 10)
    {
      //PWM=0%;
      PotVentDriv = 0;
    }
    else if (TempDrivers >= 10 and TempDrivers < 40)
    {
      //MAP (PWM)
      PotVentDriv = (byte)map(TempDrivers, 10, 40, 1, 255);
    }
    else
    {
      //PWM=100%;
      PotVentDriv = 255;
    }

    analogWrite(_PinPWMVentDrivers, PotVentDriv);

    Serial.print("INFPotencia Ventilador Drivers = ");
    Serial.print(PotVentDriv * 100.0 / 255.0);
    Serial.println("%");
  }
  //Serial.println("INFFin Regula Ventilaci�n Drivers PAP");
}

void CNC::RegulaVentExtrusor(void)
{
  //Serial.println("INFInicio Regula Ventilaci�n Extrusor");
  float TempExtrusor = 0;

  float TempE = Map2(analogRead(_PinTempDriverE), 275, 0, 480, 22.2);
  if (TempExtrusor < TempE)
  {
    TempExtrusor = TempE;
  }

  {
    if (TempExtrusor < 20)
    {
      //PWM = 0;
      analogWrite(_PinPWMVentE, 0);
    }
    else if (TempExtrusor >= 20 and TempExtrusor < 50)
    {
      //MAP (PWM)
      analogWrite(_PinPWMVentE, (byte)map(TempExtrusor, 20, 50, 1, 255));
    }
    else
    {
      //PWM = 255;
      analogWrite(_PinPWMVentE, 255);
    }
  }
  //Serial.println("INFFin Regula Ventilaci�n Extrusor");
}

void CNC::SetTempFusor(int TempFusor, bool Esperar)
{
  _TargetTempFusor = TempFusor;
  RegulaFusor(Esperar);
}

void CNC::RegulaFusor(bool Esperar)
{
  //Serial.println("INFInicio Regula Fusor");
  //Adafruit_MAX31855 _TermoPar(_PinTP_CLK, _PinTP_CS, _PinTP_DO);
  float c = 0;
  do
  {
    /*Serial.print("AnalogRead HotEnd: ");
      Serial.println(analogRead(_PinHotEnd));*/
    float c = Map2(analogRead(_PinHotEnd), 56, 20, 743, 250);
    Serial.print("INFTemperatura Extrusor1 = ");
    Serial.print(c);
    Serial.println("ºC");

    /*Serial.print("Internal Temp = ");
      Serial.println(_TermoPar.readInternal());*/

    //c = _TermoPar.readCelsius();
    /*if (isnan(c))
      {
      //Reintento
      c = _TermoPar.readCelsius();
      if (isnan(c))
      {
        c = 500;
      }
      }*/

    if (c < (_TargetTempFusor * 0.99))
    {
      //Enciendo el Fusor
      digitalWrite(_PinTRIACFusor, 255);
    }
    else if (c >= (_TargetTempFusor * 1.01))
    {
      //Apago el fusor
      digitalWrite(_PinTRIACFusor, 0);
      Esperar = false;
    }
  } while (Esperar);
}
//TermoPar = NULL;
//Serial.println("INFFin Regula Fusor");

PAPModes PAPToCNCMode(CNCModes cncMode)
{
  PAPModes Res = PAPModes::Fine;
  //PAPModes = Fine, Normal, Draft, Faster
  //CNCModes = Print, Traslation
  switch (cncMode)
  {
    case CNCModes::Print:
      Res = PAPModes::Fine;
      break;
    case CNCModes::Traslation:
      Res = PAPModes::Normal;
      break;
    default:
      //Modo no válido
      Serial.print("WRNPAPToCNCMode no reconoce el tipo: '");
      Serial.print((int)cncMode);
      Serial.println("'");
      break;
  }

  return Res;
}

CNCModes CNCToPAPMode(PAPModes papMode)
{
  CNCModes Res = CNCModes::Print;
  //PAPModes = Fine, Normal, Draft, Faster
  //CNCModes = Print, Traslation
  switch (papMode)
  {
    case PAPModes::Fine:
      Res = CNCModes::Print;
      break;
    case PAPModes::Normal:
    case PAPModes::Draft:
    case PAPModes::Faster:
      Res = CNCModes::Traslation;
      break;
    default:
      //Modo no válido
      Serial.print("WRNCNCToPAPMode no reconoce el tipo: '");
      Serial.print((int)papMode);
      Serial.println("'");
      break;
  }

  return Res;
}
