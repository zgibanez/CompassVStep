using System;

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

 



}
