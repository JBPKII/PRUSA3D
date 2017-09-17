/***************************************************
  This is a library for the PAP Motor

  Written by Jorge Belenguer.
  BSD license, all text above must be included in any redistribution
 ****************************************************/

#ifndef PAP_H
#define PAP_H

#if (ARDUINO >= 100)
#include "Arduino.h"
#else
#include "WProgram.h"
#endif

enum class PAPModes { Fine = 1, Normal = 2, Draft = 4, Faster = 8 };

class PAP {
  public:
    PAP();
    bool Attach(byte Enabled, byte Step, byte Direction, byte M0, byte M1, byte M2, bool Invertir, byte End);
    bool Attach(byte Enabled, byte Step, byte Direction, byte M0, byte M1, byte M2, bool Invertir);
    bool Detach(void);
    bool Attached(void);

    void SetModo(PAPModes Modo);//Modo = Fine, Normal, Draft, Faster
    PAPModes GetModo(void);
    void ClearSteps(void);
    void SetSteps(long Steps, PAPModes Modo);
    long RemainSteps(void);
    int DoStep(void);
    long DoSteps(long Steps);
    long AllSteps(void);
    void SetEnable(bool Enabled);
    bool IsEnabled(void);


  private:
    byte _PinEnabled, _PinStep, _PinDirection, _PinM0, _PinM1, _PinM2, _PinEnd;
    bool _Enabled, _Attached, _Invertido;
    long _Steps;
    byte _msPulso, _msParadaPaso/*, _msParadaFin*/;
    PAPModes _Modo;


};

#endif
