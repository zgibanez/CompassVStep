using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using System.Text;

namespace shipSpace
{
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
            ReadSensorOutputs(out sensorSpeed, out sensorHead);

            string baseString = "XXVTG," + HeadToDegrees(headX, headY) + ",T,,M," + SpeedToKnots() + ",N," + SpeedToKmH() + ",K,S";


            return "$" + baseString + "*" + GetChecksum(baseString);
        }

        public string GetChecksum(string message)
        {
            //Encode message into bytes
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            byte[] byteArray = new byte[1];
            byteArray[0] = 0x00;

            //Perform XOR of each one
            for (int i = 0; i < bytes.Length; i++)
            {
                byteArray[0] ^= bytes[i];
            }

            //Take most and least significant 4 bits as characters
            return Encoding.ASCII.GetString(byteArray);
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
                        sensorHead += sensor.GetSensorValue(HeadToDegrees(headX, headY));
                        break;
                    case (Sensor.SensorType.VELOCITY):
                        sensorSpeed += sensor.GetSensorValue(speed);
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
