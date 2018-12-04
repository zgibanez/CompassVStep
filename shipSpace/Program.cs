using System;
using System.Timers;

//List
using System.Collections.Generic;

//UDP protocol
using System.Text;


namespace shipSpace
{
    
    class Program
    {
        static List<Ship> ships;
        static int shipCount = 0;
        static int initPort = 8051;

        static void Main(string[] args)
        {
            //Initialize ship list
            ships = new List<Ship>();

            Console.WriteLine("Welcome to the Ship application. \n " +
                              " Type \"new\" for creating a new ship. \n" +
                              " Type \"head ship_number headX headY\" to change a ship heading.");

            string command = "";
            do
            {
                command = Console.ReadLine();
                processInput(command);
            } while (command != "exit");

            Console.WriteLine("Exiting program...");
        }

        static public void processInput(string command)
        {
            String[] args = command.Split(" ");
            if (args[0] == "new")
            {
                Console.WriteLine("Initial head parameters not given, new ship will have random head direction.");
                Ship ship = new Ship(shipCount, initPort);
                ships.Add(ship);
                shipCount++;
                initPort++;
            }
            else if (args[0] == "head")
            {
                
                if (args.Length == 4)
                {
                    int ship_number;
                    float _headX, _headY;
                    if (Int32.TryParse(args[1], out ship_number) && float.TryParse(args[2], out _headX) && float.TryParse(args[3], out _headY))
                    {
                        if (ship_number < ships.Count)
                        {
                            //Convert into unitarian vector
                            float mod = (float) Math.Sqrt(_headX * _headX + _headY * _headY);
                            _headX = _headX / mod;
                            _headY = _headY / mod;
                            ships[ship_number].headX = _headX;
                            ships[ship_number].headY = _headY;
                            Console.WriteLine("Ship " + ship_number + " changed its head direction to " + _headX + "," + _headY);
                        }
                        else
                        {
                            Console.WriteLine("Ship " + ship_number + " does not exist.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input.");
                    }
                }
                else
                {
                    Console.WriteLine("Incorrect number of arguments for head parameter.");
                    return;
                }
            }
        }
    }

    class Ship
    {
        //Network variables
        int port;
        UdpSender udpS;

        //Physical variables
        int id;
        public double headX, headY;
        public double speed;
        double posX, posY;

        //TODO: Add a sensor class. Within the sensor class add a list of errors.
        private List<Sensor> sensorList;
        
        public Ship(int id, int port)
        {
            //Ship needs an ID and port number for UDP 
            this.id = id;
            this.port = port;
            this.udpS = new UdpSender(port); 

            //Head and position initialized randomly
            Random r = new Random();
            this.headX = r.NextDouble() * 2 - 1;
            this.headY = r.NextDouble() * 2 - 1;
            this.speed = 1;

            this.posX = r.NextDouble() * 1000 - 1000;
            this.posY = r.NextDouble() * 1000 - 1000;
            Console.WriteLine("Created ship number " + id + " with port " + port + ". Initial heading " + headX + "," + headY + ". Initial position: " + posX + "," + posY);

            //Start ship sensors. By default we add one for heading and one for velocity
            sensorList = new List<Sensor>();
            sensorList.Add(new Sensor(this, Sensor.SensorType.HEAD));
            sensorList.Add(new Sensor(this, Sensor.SensorType.VELOCITY));

            //Start ship movement thread
            System.Timers.Timer timer1 = new System.Timers.Timer();
            timer1.Interval = 1000;
            timer1.Elapsed += (sender, e) => ShipThread(sender, e, this);
            timer1.AutoReset = true;
            timer1.Enabled = true;

            //Start ship udp sender
            System.Timers.Timer timer2 = new System.Timers.Timer();
            timer2.Interval = 1000;
            timer2.Elapsed += (sender, e) => SendToCompass(sender, e, GetNMEAMessage(), udpS);
            timer2.AutoReset = true;
            timer2.Enabled = true;

        }

        string GetNMEAMessage()
        {
            //ADD each of the errors value to the heading measure 
            /*Create a speed sensor and output the speed to the compass application using the following sentence:
             * $XXVTG,a.a,T,,M,c.c,N,d.d,K,S*hh, where 
             * a.a is the heading in decimal degrees from North, 
             * c.c is the speed in knots,
             * d.d is the speed in Km/h and hh is the checksum (same rules as above). 
             * For example: $XXVTG,148.3,T,,M,1.5,N,2.8,K,S*08*/
            double sensorSpeed, sensorHead;
            ReadSensorOutputs(out sensorSpeed,out sensorHead);

            return "$XXHDT," + HeadToDegrees(headX, headY).ToString("0.00") + ",T*1F";
        }

        public void ReadSensorOutputs(out double sensorSpeed, out double sensorHead)
        {
            sensorSpeed = 0;
            sensorHead = 0;
            foreach (Sensor sensor in sensorList)
            {
                switch (sensor.sensorType)
                {
                    case (Sensor.SensorType.HEAD):
                        sensorHead += sensor.GetSensorValue();
                        break;
                    case (Sensor.SensorType.VELOCITY):
                        sensorSpeed += sensor.GetSensorValue();
                        break;
                    default:
                        break;
                }
            }
        }

        static void SendToCompass(object state, ElapsedEventArgs e, string message, UdpSender udpS)
        {
            udpS.sendString(message);
        }
        
        static void ShipThread(object state, ElapsedEventArgs e, Ship ship)
        {
            //Output state to console
            Console.WriteLine("I am a ship number " + ship.id + ". Heading: " + ship.headX + "," + ship.headY + ". Position: " + ship.posX + "," + ship.posY);
            //Change physical properties
            ship.Move();
        }

        void Move()
        {
            posX += speed * headX;
            posY += speed * headY;
        }

        //TODO: Convert to unit vec
        public static double HeadToDegrees(double headX, double headY)
        {
            return Math.Atan2(headY, headX) * 180.0 / Math.PI; //X axis is north in this case 
        }

        public double SpeedToKnots()
        {
            return speed * 1.94384;
        }

        public double SpeedToKmH()
        {
            return speed * 3600 / 1000;
        }
    }



}
