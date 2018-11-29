using System;
using System.Timers;

//List
using System.Collections.Generic;

//UDP protocol
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
        public float headX, headY;
        float posX, posY;
        float speed;

        //TODO: Add a sensor class. Within the sensor class add a list of errors.
        
        public Ship(int id, int port)
        {
            //Ship needs an ID and port number for UDP 
            this.id = id;
            this.port = port;
            this.udpS = new UdpSender(port); 

            //Head and position initialized randomly
            Random r = new Random();
            this.headX = (float)r.NextDouble() * 2 - 1;
            this.headY = (float)r.NextDouble() * 2 - 1;
            this.speed = 1;

            this.posX = (float)r.NextDouble() * 1000 - 1000;
            this.posY = (float)r.NextDouble() * 1000 - 1000;
            Console.WriteLine("Created ship number " + id + " with port " + port + ". Initial heading " + headX + "," + headY + ". Initial position: " + posX + "," + posY);

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
            return "$XXHDT," + HeadToDegrees(headX, headY).ToString("0.00") + ",T*1F";
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
        static float HeadToDegrees(float headX, float headY)
        {
            return (float)Math.Atan2(headY, headX) * 180.0f / (float)Math.PI; //X axis is north in this case 
        }
    }

    //This implementation is not mine!
    public class UdpSender
    {
        private string IP;
        public int port;

        IPEndPoint remoteEndPoint;
        UdpClient client;

        public UdpSender(int port)
        {
            IP = "127.0.0.1";
            this.port = port;

            remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
            client = new UdpClient();
        }

        public void sendString(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                client.Send(data, data.Length, remoteEndPoint);
                Console.WriteLine("Sending message: " + message);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }
    }

    //
    public abstract class Error
    {
        public System.Timers.Timer timer;
        public float value;

        public void InitializeTimer(double interval = 1000)
        {
            timer = new System.Timers.Timer();
            timer.Interval = interval;
            timer.Elapsed += new ElapsedEventHandler(UpdateValue);
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        public virtual void UpdateValue(object source, ElapsedEventArgs e)
        {
            return;
        }
    }

    public class EOffset : Error
    {
        public EOffset(float value)
        {
            this.value = value;
        }
    }

    public class EDrift : Error
    {
        float drift;

        public EDrift(float drift)
        {
            //Error starts with 0, then begins drifting
            this.value = 0;
            this.drift = drift;
            InitializeTimer();
        }

        public override void UpdateValue(object source, ElapsedEventArgs e)
        {
            value += drift; 
        }
    }

    public class EFreq : Error
    {
        float frequency, amplitude, offset, time;

        public EFreq(float frequency, float amplitude, float offset)
        {
            this.frequency = frequency;
            this.amplitude = amplitude;
            this.offset = offset;
            time = 0;
            InitializeTimer(100);
        }

        public override void UpdateValue(object source, ElapsedEventArgs e)
        {
            value = amplitude * (float) Math.Sin((double)(frequency * time)) + offset;
            time += (float)timer.Interval; 
        }
    }
}
