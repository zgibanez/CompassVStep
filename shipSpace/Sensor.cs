using System;
using System.Collections.Generic;
using System.Text;

namespace shipSpace
{
    class Sensor
    {
        public List<Error> errorList;
        private Ship ship;

        //Ideally I would like to pass a reference to the physical property of the ship.
        //This way, the sensor would be completely independent of the property.
        public enum SensorType { HEAD, VELOCITY};
        public SensorType sensorType;

        public Sensor(Ship ship, SensorType sensorType)
        {
            this.ship = ship;
            this.sensorType = sensorType;

            errorList = new List<Error>();
        }

        public double GetSensorValue()
        {
            double sensorValue = 0;

            //Get the physical property
            switch (sensorType)
            {
                case SensorType.HEAD:
                    sensorValue = Ship.HeadToDegrees(ship.headX, ship.headY);
                    break;
                case SensorType.VELOCITY:
                    sensorValue = ship.speed;
                    break;
                default:
                    break;
            }

            //Add the sensor errors
            foreach (Error error in errorList)
            {
                sensorValue += error.GetErrorValue();
            }

            return sensorValue;
        }

    }
}
