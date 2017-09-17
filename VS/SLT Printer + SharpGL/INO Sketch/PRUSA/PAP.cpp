/***************************************************
  This is a library for the PAP Motor

  Written by Jorge Belenguer.
  BSD license, all text above must be included in any redistribution
 ****************************************************/

#include "PAP.h"
#include <avr/pgmspace.h>
#include <util/delay.h>
#include <stdlib.h>
//#include <SPI.h>

PAP::PAP(void)
{
  _Invertido = false;
  _Enabled = false;

  _msPulso = 2;
  //_msParada = 1;
  //_Steps = 0;
  //_Modo = 1;
  ClearSteps();

  _Attached = false;
}

bool PAP::Attach(byte Enabled, byte Step, byte Direction, byte M0, byte M1, byte M2, bool Invertir)
{
  return Attach(Enabled, Step, Direction, M0, M1, M2, Invertir, 0);
}

bool PAP::Attach(byte Enabled, byte Step, byte Direction, byte M0, byte M1, byte M2, bool Invertir, byte End)
{
  _Invertido = Invertir;
  _PinEnabled = Enabled;
  _PinStep = Step;
  _PinDirection = Direction;
  _PinM0 = M0;
  _PinM1 = M1;
  _PinM2 = M2;
  _PinEnd = End;

  //define pin modes
  pinMode(_PinEnabled, OUTPUT);
  pinMode(_PinStep, OUTPUT);
  pinMode(_PinDirection, OUTPUT);
  pinMode(_PinM0, OUTPUT);
  pinMode(_PinM1, OUTPUT);
  pinMode(_PinM2, OUTPUT);
  if (_PinEnd > 0)
  {
    Serial.println("INFAttachPAP con Fin de carrera");
    pinMode(_PinEnd, INPUT);
  }
  else
  {
    Serial.println("INFAttachPAP sin Fin de carrera");
  }

  SetEnable(true);

  _msPulso = 2;
  //_msParada = 1;
  //_Steps = 0;
  //_Modo = 1;
  ClearSteps();

  _Attached = true;

  return _Attached;
}

bool PAP::Detach(void)
{
  _Enabled = false;

  _msPulso = 2;
  //_msParada = 1;
  //_Steps = 0;
  //_Modo = 1;
  ClearSteps();

  _Attached = false;

  return _Attached;
}

bool PAP::Attached(void)
{
  return _Attached;
}

PAPModes PAP::GetModo(void)//Modo = Fine, Normal, Draft, Faster
{
  return _Modo;
}

void PAP::SetModo(PAPModes Modo)//Modo = Fine, Normal, Draft, Faster
{
  /*if(Modo != _Modo)
    {
      _Steps = 0;
    }*/

  switch (Modo)
  {
    case PAPModes::Fine:
      _Modo = Modo;
      _msParadaPaso = 0;
      //_msParadaFin = 50;
      digitalWrite(_PinM0, LOW);
      digitalWrite(_PinM1, LOW);
      digitalWrite(_PinM2, LOW);
      break;
    case PAPModes::Normal:
      _Modo = Modo;
      _msParadaPaso = 0;
      //_msParadaFin = 50;
      digitalWrite(_PinM0, LOW);
      digitalWrite(_PinM1, HIGH);
      digitalWrite(_PinM2, LOW);
      break;
    case PAPModes::Draft:
      _Modo = Modo;
      _msParadaPaso = 0;
      //_msParadaFin = 50;
      digitalWrite(_PinM0, HIGH);
      digitalWrite(_PinM1, LOW);
      digitalWrite(_PinM2, LOW);
      break;
    case PAPModes::Faster:
      _Modo = Modo;
      _msParadaPaso = 5;
      //_msParadaFin = 50;
      digitalWrite(_PinM0, LOW);
      digitalWrite(_PinM1, HIGH);
      digitalWrite(_PinM2, LOW);
      break;
    default:
      //Modo no válido
      SetModo(PAPModes::Fine);
      break;
  }

}

void PAP::ClearSteps(void)
{
  _Steps = 0;
  SetModo(PAPModes::Fine);
}

void PAP::SetSteps(long Steps, PAPModes Modo)
{
  SetModo(Modo);
  _Steps = Steps;
}

long PAP::RemainSteps(void)
{
  return _Steps;
}

int PAP::DoStep(void)
{
  bool END = false;
  int Res = 0;

  if (_PinEnd > 10)
  {
    int EstadoFin = analogRead(_PinEnd);
    //Evalua cual es el final de carrera
    if (EstadoFin > 500) //elimino el posible ruido
    {
      if (_Steps < 0 && EstadoFin < 989)
      {
        //Serial.begin(115200);
        Serial.println("----Sentido -");
        END = true;
      }
      else if (_Steps > 0 && EstadoFin > 989)
      {
        //Serial.begin(115200);
        Serial.println("----Sentido +");
        END = true;
      }

      //Solo para DEBUG
      if (END)
      {
        Serial.print("----Final de carrera con valor: ");
        Serial.println(EstadoFin);
        //Serial.end();
      }
    }
  }

  if (END == false)
  {
    if (_Steps == 0)
    {
      //Salta por ser 0 los pasos restantes
      Res = 0;
    }
    else
    {
      if (_Steps < 0)
      {
        _Steps++;

        if (_Invertido)
        {
          digitalWrite(_PinDirection, HIGH);
        }
        else
        {
          digitalWrite(_PinDirection, LOW);
        }

        Res = -1;
      }
      else
      {
        _Steps--;

        if (_Invertido)
        {
          digitalWrite(_PinDirection, LOW);
        }
        else
        {
          digitalWrite(_PinDirection, HIGH);
        }

        Res = 1;
      }

      digitalWrite(_PinStep, HIGH);
      delay(_msPulso);
      digitalWrite(_PinStep, LOW);
      delay(_msParadaPaso);

      digitalWrite(_PinDirection, LOW);
    }
  }
  else
  {
    _Steps = 0;
    Res = 0;
  }

  return Res;
}

long PAP::DoSteps(long Steps)
{
  long Pasos = 0;
  for (long i = 0; i < Steps; i++)
  {
    switch (DoStep())
    {
      /*case 0:
        //delay(_msParadaFin);
        Steps = 0;
        return Pasos;
        break;*/
      case 1:
        Pasos++;
        break;
      case -1:
        Pasos--;
        break;
      default:
        delay(_msParadaPaso);
        return Pasos;
        break;
    }
  }
  //delay(_msParadaFin);
  return Pasos;
}

long PAP::AllSteps(void)
{
  long Pasos = 0;
  while (_Steps != 0)
  {
    int ResStep = DoStep();
    if (ResStep > 0)
    {
      Pasos++;
    }
    else if (ResStep < 0)
    {
      Pasos--;
    }
    else
    {
      delay(_msParadaPaso);
      return Pasos;
    }
  }
  //delay(_msParadaFin);
  return Pasos;
}

void PAP::SetEnable(bool Enabled)
{
  if (Enabled)
  {
    digitalWrite(_PinEnabled, LOW);
  }
  else
  {
    digitalWrite(_PinEnabled, HIGH);
  }

  _Enabled = Enabled;
}

bool PAP::IsEnabled(void)
{
  return _Enabled;
}


