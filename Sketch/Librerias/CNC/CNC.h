/*************************************************** 
  This is a library for the CNC Operations

  Written by Jorge Belenguer.  
  BSD license, all text above must be included in any redistribution
 ****************************************************/

#ifndef CNC_H
#define CNC_H

#if (ARDUINO >= 100)
 #include "Arduino.h"
#else
 #include "WProgram.h"
#endif

#include <avr/pgmspace.h>
#include "PAP.h"
//#include "Adafruit_MAX31855.h"

//typedef enum { Traslacion, Impresion1, Impresion2} mode ;

class CNC {
 public:
 // constructors:
  CNC();
  
  void Attach(byte EnabledX, byte StepX, byte DirectionX, byte M0X, byte M1X, byte M2X, bool InvertirX, byte EndX, byte TempDriverX,
         byte EnabledY, byte StepY, byte DirectionY, byte M0Y, byte M1Y, byte M2Y, bool InvertirY, byte EndY, byte TempDriverY,
         byte EnabledZ, byte StepZ, byte DirectionZ, byte M0Z, byte M1Z, byte M2Z, bool InvertirZ, byte EndZ, byte TempDriverZ,
		 byte PWMVentDrivers,
         byte EnabledE, byte StepE, byte DirectionE, byte M0E, byte M1E, byte M2E, bool InvertirE, byte TempDriverE, 
		 byte TempExtrusor, byte PWMVentE, byte TempHotEnd, byte TRIACFusor
		 );
		 
  void GoToOrigen(bool X, bool Y, bool Z);
  
  void DefineDestino(float X, float Y, float Z, byte Modo);//Modo=1, 2, 4 o 8
  
  void Run(bool Resto);
  
  void XYZSerial(void);
  
  void RegulaVentDrivers(void);
  void RegulaVentExtrusor(void);
  void SetTempFusor(int Temperatura, bool Esperar);
  void RegulaFusor(bool Esperar);

 private:
  byte _PinTempDriverX, _PinTempDriverY, _PinTempDriverZ, _PinTempDriverE, _PinTempExtrusor,
       _PinPWMVentDrivers, _PinPWMVentE, _PinHotEnd, _PinTRIACFusor;
  int _TargetTempFusor;//ºC
  float _CurrentX, _CurrentY , _CurrentZ ;//mm
  //byte _Modo;
  
  //Modificar para calibrar el dispositivo
  static const float _PasoX = 0.20;//mm
  static const float _PasoY = 0.20;//mm
  static const float _PasoZ = 0.005;//mm
  
  //Modificar para establecer los márgenes de seguridad
  static const long _PasosSepTraslacion = -200;
  static const long _PasosExtTraslacion = 5;
  
  int _RestoPasosX, _RestoPasosY, _RestoPasosZ;
};

#endif
