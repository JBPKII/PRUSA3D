
const int PasoMotor = 23;
const int DirMotor = 22;
const int HalfStepMotor = 24;
const int EnableStepMotor = 25;

String inputString = "";
boolean stringComplete = false;

void setup() 
{
  // initialize serial:
  Serial.begin(9600);
  // reserve 200 bytes for the inputString:
  inputString.reserve(200);

  pinMode(PasoMotor, OUTPUT);
  pinMode(DirMotor, OUTPUT);
  pinMode(HalfStepMotor, OUTPUT);
  pinMode(EnableStepMotor, OUTPUT);
  digitalWrite(PasoMotor, LOW);
  digitalWrite(DirMotor, HIGH);
  digitalWrite(HalfStepMotor, LOW);
  digitalWrite(EnableStepMotor, LOW);
  
  Serial.println("NumeroDePasos;DelayEntrePasos");
}

void loop() 
{
  if(stringComplete == true)
  {
    String Pasos = "";
    int iPasos = 0;
    String Delay = "";
    int iDelay = 0;
    Pasos = inputString.substring(0,inputString.indexOf(';'));
    Delay = inputString.substring(inputString.indexOf(';') + 1);

    if(Pasos == "")
    {
      iPasos = 0; 
    }
    else
    {
      iPasos = Pasos.toInt();
    }

    if(Delay == "")
    {
      iDelay = 1; 
    }
    else
    {
      iDelay = Delay.toInt();
    }

    for(int i = 0; i < iPasos; i++)
    {
      digitalWrite(PasoMotor, HIGH);
      delay(1);
      digitalWrite(PasoMotor, LOW);
      delay(iDelay);
    }

    inputString = "";
    stringComplete = false;
    Serial.println(Pasos + " Pasos; Cada " + Delay + " ms");
  }
  else
  {
    
  }
}

void serialEvent() 
{
  while (Serial.available()) 
  {
    // get the new byte:
    char inChar = (char)Serial.read(); 
    // add it to the inputString:
    inputString += inChar;
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') 
    {
      stringComplete = true;
    } 
  }
}

